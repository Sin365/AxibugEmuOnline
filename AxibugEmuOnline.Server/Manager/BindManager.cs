using AxibugEmuOnline.Server.Common;
using AxibugEmuOnline.Server.Manager.Client;
using AxibugEmuOnline.Server.NetWork;
using AxibugProtobuf;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Org.BouncyCastle.Ocsp;
using System.Net.Sockets;

namespace AxibugEmuOnline.Server.Manager
{
    public class BindManager
    {
        public BindManager()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdBindAcc, RecvBindAcc);
        }


        public void RecvBindAcc(Socket sk, byte[] reqData)
        {
            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(sk);
            AppSrv.g_Log.Debug("收到绑定消息");
            Protobuf_Bind msg = ProtoBufHelper.DeSerizlize<Protobuf_Bind>(reqData);

            // 更新客户端内存状态
            if (BindAcc(_c.UID, _c.ParentUID, msg, out ErrorCode err, out Protobuf_Bind_RESP resp))
            {
                _c.ParentUID = resp.ParentUID;
                _c.NickName = resp.MoveToNickName;
            }

            // 返回结果
            AppSrv.g_ClientMgr.ClientSend(
                _c,
                (int)CommandID.CmdBindAcc,
                (int)err,
                ProtoBufHelper.Serizlize(resp)
            );
        }
        private bool BindAcc(long UID, long currParentUID, Protobuf_Bind msg, out ErrorCode err, out Protobuf_Bind_RESP resp)
        {
            AppSrv.g_Log.DebugCmd("BindAcc");
            resp = new Protobuf_Bind_RESP();
            err = ErrorCode.ErrorOk;
            AppSrv.g_Log.DebugCmd($"BindCode => {msg.BindCode}");

            long codeId = -1;
            long targetUid = -1;
            DateTime codeCreateTime = default;
            bool codeIsUsed = false;

            using var conn = SQLRUN.GetConn("BindAcc");
            conn.Open();

            bool transOk = false;
            var toDeleteFiles = new List<(string Url, string ImgUrl)>();

            do
            {
                #region 1. 开启事务
                using (var cmd = new MySqlCommand("START TRANSACTION", conn))
                    cmd.ExecuteNonQuery();
                #endregion
                try
                {
                    #region 校验 bindcode（加行锁）
                    const string lockCodeSql = @"
SELECT id, uid, createtime, isused
FROM user_bindcode
WHERE `code` = @code
FOR UPDATE;";

                    using (var cmd = new MySqlCommand(lockCodeSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@code", msg.BindCode);
                        using var reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            codeId = reader.GetInt64(0);
                            targetUid = reader.GetInt64(1);
                            codeCreateTime = reader.IsDBNull(2) ? DateTime.Now : reader.GetDateTime(2);
                            codeIsUsed = reader.IsDBNull(3) ? false : reader.GetBoolean(3);
                        }
                    }

                    if (codeId <= 0)
                    {
                        err = ErrorCode.ErrorBindCodeNotExist;
                        AppSrv.g_Log.DebugCmd("BindCode 不存在");
                        break;
                    }

                    if (targetUid == currParentUID)
                    {
                        err = ErrorCode.ErrorBindCodeAccountAlreadyBindThisDev;
                        AppSrv.g_Log.DebugCmd("该设备已绑定该账户");
                        break;
                    }

                    if (codeIsUsed)
                    {
                        err = ErrorCode.ErrorBindCodeAlreadyInUse;
                        AppSrv.g_Log.DebugCmd("BindCode 已使用");
                        break;
                    }

                    if (Math.Abs((DateTime.Now - codeCreateTime).TotalMinutes) > 5)
                    {
                        err = ErrorCode.ErrorBindCodeTimeOut;
                        AppSrv.g_Log.DebugCmd("BindCode 超时");
                        break;
                    }
                    #endregion

                    #region 校验目标账户
                    const string checkUserSql = "SELECT uid, nikename FROM users WHERE uid = @target;";
                    using (var cmd = new MySqlCommand(checkUserSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@target", targetUid);
                        using var reader = cmd.ExecuteReader();

                        if (!reader.Read())
                        {
                            err = ErrorCode.ErrorBindCodeAccCant;
                            AppSrv.g_Log.DebugCmd("目标账户不存在");
                            break;
                        }
                        resp.ParentUID = targetUid;
                        resp.MoveToNickName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    }
                    #endregion

                    #region 防重入
                    if (currParentUID > 0)
                    {
                        err = ErrorCode.ErrorBindCodeAccountAlreadyHasParent;
                        AppSrv.g_Log.DebugCmd("该客户端已绑定过父账户");
                        break;
                    }
                    #endregion

                    #region 合并 rom_stars（收藏）
                    const string mergeStarsSql = @"
DELETE src FROM rom_stars src
INNER JOIN rom_stars tgt 
   ON tgt.uid = @target AND tgt.romid = src.romid
WHERE src.uid = @src AND src.logdate <= tgt.logdate;

UPDATE rom_stars SET uid = @target WHERE uid = @src;";

                    using (var cmd = new MySqlCommand(mergeStarsSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@target", targetUid);
                        cmd.Parameters.AddWithValue("@src", UID);
                        resp.MoveStarCount = cmd.ExecuteNonQuery();
                    }
                    #endregion

                    #region 合并 user_gamesavedata（游戏存档）
                    // 查询需要物理删除的文件（必须在事务内查询）
                    const string selectDeleteSql = @"
SELECT src.savUrl, src.savImgUrl
FROM user_gamesavedata src
JOIN user_gamesavedata tgt
  ON tgt.uid = @target 
 AND tgt.romid = src.romid 
 AND tgt.savidx = src.savidx
WHERE src.uid = @src 
  AND src.savDate <= tgt.savDate;";

                    using (var cmd = new MySqlCommand(selectDeleteSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@src", UID);
                        cmd.Parameters.AddWithValue("@target", targetUid);
                        using var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            string url = reader.GetString(0);
                            string imgUrl = reader.IsDBNull(1) ? null : reader.GetString(1);
                            toDeleteFiles.Add((url, imgUrl));
                        }
                    }

                    // 执行数据合并
                    const string mergeSaveSql = @"
-- 删除旧存档
DELETE src FROM user_gamesavedata src
JOIN user_gamesavedata tgt
  ON tgt.uid = @target 
 AND tgt.romid = src.romid 
 AND tgt.savidx = src.savidx
WHERE src.uid = @src 
  AND src.savDate <= tgt.savDate;

-- 覆盖更新为更新的存档
UPDATE user_gamesavedata src
JOIN user_gamesavedata tgt
  ON tgt.uid = @target 
 AND tgt.romid = src.romid 
 AND tgt.savidx = src.savidx
SET 
    tgt.savName = src.savName,
    tgt.savNote = src.savNote,
    tgt.savUrl = src.savUrl,
    tgt.savImgUrl = src.savImgUrl,
    tgt.savDate = src.savDate,
    tgt.savSequence = src.savSequence
WHERE src.uid = @src 
  AND src.savDate > tgt.savDate;

-- 迁移无冲突的记录
UPDATE user_gamesavedata
   SET uid = @target
 WHERE uid = @src 
   AND NOT EXISTS (
       SELECT 1 FROM user_gamesavedata tgt
       WHERE tgt.uid = @target 
         AND tgt.romid = user_gamesavedata.romid 
         AND tgt.savidx = user_gamesavedata.savidx
   );

DELETE FROM user_gamesavedata WHERE uid = @src;";

                    using (var cmd = new MySqlCommand(mergeSaveSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@src", UID);
                        cmd.Parameters.AddWithValue("@target", targetUid);
                        resp.MoveSavCount = cmd.ExecuteNonQuery();
                    }
                    #endregion

                    #region 更新账户绑定和验证码状态
                    using (var cmd = new MySqlCommand(
                        "UPDATE users SET parentuid = @target WHERE uid = @src", conn))
                    {
                        cmd.Parameters.AddWithValue("@target", targetUid);
                        cmd.Parameters.AddWithValue("@src", UID);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new MySqlCommand(@"
UPDATE user_bindcode 
SET isused = TRUE, 
    usedtime = NOW(), 
    useduid = @uid 
WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", UID);
                        cmd.Parameters.AddWithValue("@id", codeId);
                        cmd.ExecuteNonQuery();
                    }
                    #endregion

                    using (var cmd = new MySqlCommand("COMMIT", conn))
                        cmd.ExecuteNonQuery();
                    transOk = true;
                    AppSrv.g_Log.DebugCmd($"BindAcc SUCCESS | src={UID} target={targetUid} stars={resp.MoveStarCount} saves={resp.MoveSavCount}");
                }
                catch (Exception ex)
                {
                    err = ErrorCode.ErrorBindCodeException;
                    AppSrv.g_Log.DebugCmd($"BindAcc EXCEPTION | src={UID} target={targetUid} code={msg.BindCode} {ex}");
                }
                finally
                {
                    if (!transOk)
                    {
                        using var rb = new MySqlCommand("ROLLBACK", conn);
                        rb.ExecuteNonQuery();
                        AppSrv.g_Log.DebugCmd("BindAcc ROLLBACK");
                    }
                }
            } while (false);

            #region 事务外删除物理文件
            if (transOk && toDeleteFiles.Count > 0)
            {
                foreach (var (url, imgUrl) in toDeleteFiles)
                {
                    try
                    {
                        Helper.FileDelete(Path.Combine(Config.cfg.wwwRootPath, url));
                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            Helper.FileDelete(Path.Combine(Config.cfg.wwwRootPath, imgUrl));
                        }
                    }
                    catch (Exception ex)
                    {
                        AppSrv.g_Log.DebugCmd($"File delete failed: {url} | {ex.Message}");
                    }
                }
            }
            #endregion
            return err == ErrorCode.ErrorOk;
        }

        //        private void BindAcc(Socket socket, byte[] reqData)
        //        {
        //            AppSrv.g_Log.DebugCmd("BindAcc");
        //            Protobuf_Bind msg = ProtoBufHelper.DeSerizlize<Protobuf_Bind>(reqData);
        //            ClientInfo _c = AppSrv.g_ClientMgr.GetClientForSocket(socket);
        //            long code_id = -1;
        //            long code_targetuid = -1;
        //            DateTime code_createTime = default;
        //            bool code_isused = default;
        //            Protobuf_Bind_RESP _resp = new Protobuf_Bind_RESP();
        //            ErrorCode err = ErrorCode.ErrorOk;
        //            AppSrv.g_Log.DebugCmd($"BindCode => {msg.BindCode}");
        //            using (MySqlConnection conn = SQLRUN.GetConn("BindAcc"))
        //            {
        //                string query = "SELECT id,uid,createtime,isused from user_bindcode where user_bindcode.`code` = ?code ";
        //                using (var command = new MySqlCommand(query, conn))
        //                {
        //                    command.Parameters.AddWithValue("?code", msg.BindCode);
        //                    using (var reader = command.ExecuteReader())
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            code_id = reader.GetInt64(0);
        //                            code_targetuid = reader.GetInt64(1);
        //                            code_createTime = reader.IsDBNull(2) ? DateTime.Now : reader.GetDateTime(2);
        //                            code_isused = reader.IsDBNull(3) ? false : reader.GetBoolean(3);
        //                        }
        //                    }

        //                    if (code_id <= 0)
        //                    {
        //                        AppSrv.g_Log.DebugCmd($"BindCode 不存在");
        //                        err = ErrorCode.ErrorBindCodeNotExist;
        //                    }
        //                    else if (code_targetuid == _c.ParentUID)
        //                    {
        //                        AppSrv.g_Log.DebugCmd($"该设备已经绑定过该账户，请勿重复绑定");
        //                        err = ErrorCode.ErrorBindCodeAccountAlreadyBindThisDev;
        //                    }
        //                    else if (code_isused)
        //                    {
        //                        AppSrv.g_Log.DebugCmd($"BindCode 已被使用");
        //                        err = ErrorCode.ErrorBindCodeAlreadyInUse;
        //                    }
        //                    else if (Math.Abs((DateTime.Now - code_createTime).TotalMinutes) > 5)
        //                    {
        //                        AppSrv.g_Log.DebugCmd($"BindCode 超时");
        //                        err = ErrorCode.ErrorBindCodeTimeOut;
        //                    }
        //                }

        //                //检查用户是否存在
        //                if (err == ErrorCode.ErrorOk)
        //                {
        //                    try
        //                    {
        //                        query = "SELECT uid,nikename from users where uid = ?target";
        //                        using (var command = new MySqlCommand(query, conn))
        //                        {
        //                            command.Parameters.AddWithValue("?target", code_targetuid);
        //                            long tmp_targetuidcheck = -1;
        //                            using (var reader = command.ExecuteReader())
        //                            {
        //                                while (reader.Read())
        //                                {
        //                                    tmp_targetuidcheck = reader.GetInt64(0);
        //                                    _resp.MoveToNickName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
        //                                }
        //                            }
        //                            if (tmp_targetuidcheck <= 0)
        //                            {
        //                                err = ErrorCode.ErrorBindCodeAccCant;
        //                                AppSrv.g_Log.DebugCmd($"用户不存在，{_resp.MoveStarCount}个");
        //                            }
        //                        }
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        err = ErrorCode.ErrorBindCodeException;
        //                        AppSrv.g_Log.DebugCmd($"迁移异常，{e.ToString()}");
        //                    }
        //                }

        //                //开始处理
        //                if (err == ErrorCode.ErrorOk)
        //                {
        //                    try
        //                    {
        //                        #region 迁移收藏表 （要求是 rom_stars表中 src的往target的合并，都是target的uid，但是要求数据不重复，保留最新的logdata）
        //                        query = @"
        //START TRANSACTION;

        //UPDATE rom_stars src
        //JOIN rom_stars tgt
        //  ON tgt.uid = @target AND tgt.romid = src.romid
        //SET src.uid = @target
        //WHERE src.uid = @src
        //  AND src.logdate > tgt.logdate;

        //DELETE src
        //FROM rom_stars src
        //JOIN rom_stars tgt
        //  ON tgt.uid = @target AND tgt.romid = src.romid
        //WHERE src.uid = @src
        //  AND src.logdate <= tgt.logdate;

        //UPDATE rom_stars
        //SET uid = @target
        //WHERE uid = @src
        //  AND romid NOT IN (
        //      SELECT romid FROM rom_stars WHERE uid = @target
        //  );

        //DELETE FROM rom_stars WHERE uid = @src;

        //COMMIT;
        //";

        //                        using (var cmd = new MySqlCommand(query, conn))
        //                        {
        //                            cmd.Parameters.AddWithValue("@target", code_targetuid);
        //                            cmd.Parameters.AddWithValue("@src", _c.UID);
        //                            _resp.MoveStarCount = cmd.ExecuteNonQuery();
        //                            AppSrv.g_Log.DebugCmd($"按logdate合并收藏，影响{_resp.MoveStarCount}条，src已清空");
        //                        }
        //                        #endregion

        //                        #region 迁移游戏存档（要求是 user_gamesavedata表中 src的往target的合并，都是target的uid，每一个游戏romid都有多个存档位savidx，但是要求每个存档位数据不重复，每个（romid+savidx）组合都保留最新的保留最新的savDate作为依据，请删除已删除记录的文件）
        //                        var toDeleteFiles = new List<(string Url, string ImgUrl)>();

        //                        // 1️⃣ 查要删除的旧存档
        //                        const string selectSql = @"
        //SELECT src.savUrl, src.savImgUrl
        //FROM user_gamesavedata src
        //JOIN user_gamesavedata tgt
        //  ON tgt.uid = @target
        // AND tgt.romid = src.romid
        // AND tgt.savidx = src.savidx
        //WHERE src.uid = @src
        //  AND src.savDate <= tgt.savDate;";

        //                        using (var cmd = new MySqlCommand(selectSql, conn))
        //                        {
        //                            cmd.Parameters.AddWithValue("@src", _c.UID);
        //                            cmd.Parameters.AddWithValue("@target", code_targetuid);

        //                            using var reader = cmd.ExecuteReader();
        //                            while (reader.Read())
        //                            {
        //                                toDeleteFiles.Add((
        //                                    reader.GetString("savUrl"),
        //                                    reader.GetString("savImgUrl")
        //                                ));
        //                            }
        //                        }

        //                        // 2️⃣ 事务执行合并
        //                        const string mergeSql = @"
        //START TRANSACTION;

        //-- 删除 src 旧存档
        //DELETE src
        //FROM user_gamesavedata src
        //JOIN user_gamesavedata tgt
        //  ON tgt.uid = @target
        // AND tgt.romid = src.romid
        // AND tgt.savidx = src.savidx
        //WHERE src.uid = @src
        //  AND src.savDate <= tgt.savDate;

        //-- 覆盖更新的存档
        //UPDATE user_gamesavedata src
        //JOIN user_gamesavedata tgt
        //  ON tgt.uid = @target
        // AND tgt.romid = src.romid
        // AND tgt.savidx = src.savidx
        //SET
        //    tgt.savName = src.savName,
        //    tgt.savNote = src.savNote,
        //    tgt.savUrl = src.savUrl,
        //    tgt.savImgUrl = src.savImgUrl,
        //    tgt.savDate = src.savDate,
        //    tgt.savSequence = src.savSequence
        //WHERE src.uid = @src
        //  AND src.savDate > tgt.savDate;

        //-- 合并独有存档
        //UPDATE user_gamesavedata
        //SET uid = @target
        //WHERE uid = @src
        //  AND NOT EXISTS (
        //      SELECT 1
        //      FROM user_gamesavedata tgt
        //      WHERE tgt.uid = @target
        //        AND tgt.romid = user_gamesavedata.romid
        //        AND tgt.savidx = user_gamesavedata.savidx
        //  );

        //-- 清空 src
        //DELETE FROM user_gamesavedata WHERE uid = @src;

        //COMMIT;";

        //                        using (var cmd = new MySqlCommand(mergeSql, conn))
        //                        {
        //                            cmd.Parameters.AddWithValue("@src", _c.UID);
        //                            cmd.Parameters.AddWithValue("@target", code_targetuid);
        //                            cmd.ExecuteNonQuery();
        //                        }

        //                        // 3️⃣ 删物理文件
        //                        foreach (var (url, imgUrl) in toDeleteFiles)
        //                        {
        //                            Helper.FileDelete(Path.Combine(Config.cfg.wwwRootPath, url));
        //                            Helper.FileDelete(Path.Combine(Config.cfg.wwwRootPath, imgUrl));
        //                        }

        //                        AppSrv.g_Log.DebugCmd($"合并存档完成，删除旧存档{toDeleteFiles.Count}条");
        //                        #endregion
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        err = ErrorCode.ErrorBindCodeException;
        //                        AppSrv.g_Log.DebugCmd($"迁移异常，{e.ToString()}");
        //                    }
        //                }

        //                //修改账户
        //                if (err == ErrorCode.ErrorOk)
        //                {
        //                    try
        //                    {
        //                        query = "update users set parentuid = ?target where uid = ?src";
        //                        using (var command = new MySqlCommand(query, conn))
        //                        {
        //                            command.Parameters.AddWithValue("?target", code_targetuid);
        //                            command.Parameters.AddWithValue("?src", _c.UID);
        //                            AppSrv.g_Log.DebugCmd($"{_c.UID}修改parentui->{code_targetuid}");
        //                        }

        //                        query = "UPDATE user_bindcode set isused = TRUE,usedtime = now(),useduid = ?src where id = ?id";
        //                        using (var command = new MySqlCommand(query, conn))
        //                        {
        //                            command.Parameters.AddWithValue("?src", _c.UID);
        //                            command.Parameters.AddWithValue("?id", code_id);
        //                            AppSrv.g_Log.DebugCmd($"标记code为不可用");
        //                        }
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        err = ErrorCode.ErrorBindCodeException;
        //                        AppSrv.g_Log.DebugCmd($"迁移异常，{e.ToString()}");
        //                    }
        //                }

        //                //内存中父ID也发生变化 （虽然客户端请求结束后要重新登录，但这里修改，避免存档等并发上传）
        //                if (err == ErrorCode.ErrorOk)
        //                {
        //                    _c.ParentUID = code_targetuid;
        //                    _c.NickName = _resp.MoveToNickName;
        //                }
        //            }

        //            AppSrv.g_ClientMgr.ClientSend(_c, (int)CommandID.CmdGamesavGetGameSavList, (int)err, ProtoBufHelper.Serizlize(_resp));
        //        }
    }
}