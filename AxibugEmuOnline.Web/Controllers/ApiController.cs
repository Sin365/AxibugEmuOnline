using AxibugEmuOnline.Web.Common;
using AxibugProtobuf;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AxibugEmuOnline.Web.Controllers
{
    public class ApiController : Controller
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }


        [HttpGet]
        public JsonResult CheckStandInfo(int platform, string version)
        {
            Resp_CheckStandInfo resp = new Resp_CheckStandInfo()
            {
                needUpdateClient = 0,
                clientVersion = Config.cfg.ClientVersion,
                serverIp = Config.cfg.ServerIp,
                serverPort = Config.cfg.ServerPort,
                downLoadUrl = Config.cfg.downLoadUrl,
                downLoadSavUrl = Config.cfg.downLoadSavUrl,
            };
            return new JsonResult(resp);
        }

        [HttpGet]
        public JsonResult RomList(string SearchKey, int Ptype, int GType, int Page, int PageSize, string Token)
        {
            long UID = 0;
            if (Helper.TryDecrypToken(Token, out Protobuf_Token_Struct tokenData))
            {
                UID = tokenData.UID;
            }

            bool bHadSearchKey = !string.IsNullOrEmpty(SearchKey);

            Resp_GameList resp = new Resp_GameList();
            resp.gameList = new List<Resp_RomInfo>();
            using (MySqlConnection conn = SQLRUN.GetConn("RomList"))
            {
                List<string> condition = new List<string>();

                if (bHadSearchKey)
                    condition.Add("r.`Name` like ?searchPattern");

                if (Ptype > (int)RomPlatformType.Invalid && Ptype < (int)RomPlatformType.All)
                    condition.Add($"r.PlatformType = '{Ptype}'");

                GameType SearchGType = (GameType)GType;
                switch (SearchGType)
                {
                    case GameType.NONE:
                        break;
                    case GameType.ALLINONE:
                        condition.Add($"r.GameType = '�Ͽ�' ");
                        break;
                    default:
                        condition.Add($"r.GameType = '{SearchGType.ToString()}' ");
                        break;
                }

                string query = "SELECT count(id) FROM romlist r " + GetWhereCmd(condition);
                using (var command = new MySqlCommand(query, conn))
                {
                    // ���ò���ֵ  
                    if (bHadSearchKey)
                        command.Parameters.AddWithValue("?searchPattern", $"%{SearchKey}%");

                    // ִ�в�ѯ��������  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.resultAllCount = reader.GetInt32(0);
                            resp.page = Page;
                            if (PageSize > 0)
                                resp.maxPage = (int)Math.Ceiling((float)resp.resultAllCount / PageSize);
                            else
                                resp.maxPage = 0;
                        }
                    }
                }

                string HotOrderBy = "ORDER BY r.playcount DESC, id ASC";

                if (UID > 0)
                {
                    query = $"SELECT r.id,r.`Name`,r.GameType,r.Note,r.RomUrl,r.ImgUrl,r.`Hash`,r.`playcount`,r.`stars`,r.`PlatformType`,r.`parentids`,IF(s.uid = {UID}, TRUE, FALSE) AS is_collected FROM romlist r left join rom_stars s on r.id = s.romid {GetWhereCmd(condition)} {HotOrderBy} LIMIT ?offset, ?pageSize;";
                }
                else
                {
                    query = $"SELECT r.id,r.`Name`,r.GameType,r.Note,r.RomUrl,r.ImgUrl,r.`Hash`,r.`playcount`,r.`stars`,r.`PlatformType`,r.`parentids` FROM romlist r {GetWhereCmd(condition)} {HotOrderBy} LIMIT ?offset, ?pageSize;";
                }

                using (var command = new MySqlCommand(query, conn))
                {
                    // ���ò���ֵ  
                    if (bHadSearchKey)
                        command.Parameters.AddWithValue("?searchPattern", $"%{SearchKey}%");

                    command.Parameters.AddWithValue("?offset", Page * PageSize);
                    command.Parameters.AddWithValue("?pageSize", PageSize);
                    // ִ�в�ѯ��������  
                    using (var reader = command.ExecuteReader())
                    {
                        int orderIndex = Page * PageSize;
                        while (reader.Read())
                        {
                            Resp_RomInfo data = new Resp_RomInfo()
                            {
                                orderid = orderIndex++,
                                id = reader.GetInt32(0),
                                romName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                                gType = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                                desc = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                                url = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                imgUrl = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty,
                                hash = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                playcount = reader.GetInt32(7),
                                stars = reader.GetInt32(8),
                                ptype = reader.GetInt32(9),
                            };
                            string parentsStr = !reader.IsDBNull(10) ? reader.GetString(10) : string.Empty;
                            if (!string.IsNullOrEmpty(parentsStr))
                            {
                                int[] arr = Array.ConvertAll(parentsStr.Split(',', StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    data.parentRomIdsList.Add(arr[i]);
                                }
                            }

                            if (UID > 0)
                            {
                                data.isStar = reader.GetInt32(11) > 0 ? 1 : 0;
                            }

                            resp.gameList.Add(data);
                        }
                    }
                }
            }
            return new JsonResult(resp);
        }

        public static string GetWhereCmd(List<string> conditionCmd)
        {
            string wherecmd = string.Empty;
            for (int i = 0; i < conditionCmd.Count; i++)
            {
                if (i == 0)
                    wherecmd += " where ";
                else
                    wherecmd += " and ";

                wherecmd += conditionCmd[i];
            }
            return wherecmd;
        }

        [HttpGet]
        public JsonResult MarkList(string SearchKey, int Ptype, int GType, int Page, int PageSize, string Token)
        {
            long UID = 0;
            if (Helper.TryDecrypToken(Token, out Protobuf_Token_Struct tokenData))
            {
                UID = tokenData.UID;
            }

            string searchPattern = $"%{SearchKey}%";
            Resp_GameList resp = new Resp_GameList();
            resp.gameList = new List<Resp_RomInfo>();
            using (MySqlConnection conn = SQLRUN.GetConn("MarkList"))
            {
                string platformCond = "";
                if (Ptype > (int)RomPlatformType.Invalid && Ptype < (int)RomPlatformType.All)
                {
                    platformCond = $" and romlist.PlatformType = '{Ptype}' ";
                }

                GameType SearchGType = (GameType)GType;
                string GameTypeCond = "";
                switch (SearchGType)
                {
                    case GameType.NONE:
                        GameTypeCond = string.Empty;
                        break;
                    case GameType.ALLINONE:
                        GameTypeCond = $" and romlist.GameType = '�Ͽ�' ";
                        break;
                    default:
                        GameTypeCond = $" and romlist.GameType = '{SearchGType.ToString()}' ";
                        break;
                }

                string query = "SELECT count(romlist.id) from rom_stars LEFT JOIN romlist on romlist.Id = rom_stars.id where  rom_stars.uid = ?uid and romlist.`Name` like ?searchPattern " + platformCond + GameTypeCond;
                using (var command = new MySqlCommand(query, conn))
                {
                    // ���ò���ֵ  
                    command.Parameters.AddWithValue("?uid", UID);
                    command.Parameters.AddWithValue("?searchPattern", searchPattern);
                    // ִ�в�ѯ��������  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.resultAllCount = reader.GetInt32(0);
                            resp.page = Page;
                            if (PageSize > 0)
                                resp.maxPage = (int)Math.Ceiling((float)resp.resultAllCount / PageSize);
                            else
                                resp.maxPage = 0;
                        }
                    }
                }


                string HotOrderBy = "ORDER BY playcount DESC, id ASC";

                query = @$"SELECT romlist.id,romlist.`Name`,romlist.GameType,romlist.Note,romlist.RomUrl,romlist.ImgUrl,romlist.`Hash`,romlist.`playcount`,romlist.`stars`,romlist.`PlatformType` ,romlist.`parentids` 
from rom_stars  
LEFT JOIN romlist on romlist.Id = rom_stars.romid  
where rom_stars.uid = ?uid 
and romlist.`Name` like ?searchPattern {platformCond} {GameTypeCond} {HotOrderBy}
LIMIT ?offset, ?pageSize;";
                using (var command = new MySqlCommand(query, conn))
                {
                    // ���ò���ֵ  
                    command.Parameters.AddWithValue("?uid", UID);
                    command.Parameters.AddWithValue("?searchPattern", searchPattern);
                    command.Parameters.AddWithValue("?offset", Page * PageSize);
                    command.Parameters.AddWithValue("?pageSize", PageSize);
                    // ִ�в�ѯ��������  
                    using (var reader = command.ExecuteReader())
                    {
                        int orderIndex = Page * PageSize;
                        while (reader.Read())
                        {
                            Resp_RomInfo data = new Resp_RomInfo()
                            {
                                orderid = orderIndex++,
                                id = reader.GetInt32(0),
                                romName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty,
                                gType = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                                desc = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                                url = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                imgUrl = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty,
                                hash = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty,
                                playcount = reader.GetInt32(7),
                                stars = reader.GetInt32(8),
                                ptype = reader.GetInt32(9),
                            };
                            string parentsStr = !reader.IsDBNull(10) ? reader.GetString(10) : string.Empty;
                            if (!string.IsNullOrEmpty(parentsStr))
                            {
                                int[] arr = Array.ConvertAll(parentsStr.Split(',', StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    data.parentRomIdsList.Add(arr[i]);
                                }
                            }

                            //�Ͼ������Ѿ��ղ��˵�
                            data.isStar = 1;

                            resp.gameList.Add(data);
                        }
                    }
                }
            }
            return new JsonResult(resp);
        }

        [HttpGet]
        public JsonResult RomInfo(int Ptype, int RomID, string Token)
        {
            long UID = 0;
            if (Helper.TryDecrypToken(Token, out Protobuf_Token_Struct tokenData))
            {
                UID = tokenData.UID;
            }

            string searchPattern = $"%{RomInfo}%";
            Resp_RomInfo resp = new Resp_RomInfo();
            using (MySqlConnection conn = SQLRUN.GetConn("NesRomList"))
            {
                string query = $"SELECT id,`Name`,GameType,Note,RomUrl,ImgUrl,`Hash`,`playcount`,`stars`,`PlatformType`,`parentids` FROM romlist where id = ?romid;";
                using (var command = new MySqlCommand(query, conn))
                {
                    // ���ò���ֵ  
                    command.Parameters.AddWithValue("?romid", RomID);
                    // ִ�в�ѯ��������  
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resp.id = reader.GetInt32(0);
                            resp.romName = !reader.IsDBNull(1) ? reader.GetString(1) : string.Empty;
                            resp.gType = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty;
                            resp.desc = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty;
                            resp.url = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty;
                            resp.imgUrl = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;
                            resp.hash = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty;
                            resp.playcount = reader.GetInt32(7);
                            resp.stars = reader.GetInt32(8);
                            resp.ptype = reader.GetInt32(9);
                            string parentsStr = !reader.IsDBNull(10) ? reader.GetString(10) : string.Empty;
                            if (!string.IsNullOrEmpty(parentsStr))
                            {
                                int[] arr = Array.ConvertAll(parentsStr.Split(',', StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    resp.parentRomIdsList.Add(arr[i]);
                                }
                            }
                        }
                    }
                }
            }

            if (UID > 0)
            {
                if (CheckIsRomStar(resp.id, UID))
                    resp.isStar = 1;
            }

            return new JsonResult(resp);
        }


        enum GameType : byte
        {
            NONE = 0,
            ACT,
            ARPG,
            AVG,
            ETC,
            FTG,
            PUZ,
            RAC,
            RPG,
            SLG,
            SPG,
            SRPG,
            STG,
            TAB,
            /// <summary>
            /// �Ͽ�
            /// </summary>
            ALLINONE,
        }

        public bool CheckIsRomStar(int RomId, long uid)
        {
            bool bhad = false;
            using (MySqlConnection conn = SQLRUN.GetConn("CheckIsRomStart"))
            {
                try
                {
                    string query = $"SELECT count(0) from rom_stars where uid = ?uid and romid = ?romid";
                    using (var command = new MySqlCommand(query, conn))
                    {
                        // ���ò���ֵ
                        command.Parameters.AddWithValue("?romid", RomId);
                        command.Parameters.AddWithValue("?uid", uid);
                        // ִ�в�ѯ����ȡ���
                        bhad = (int)command.ExecuteScalar() > 0;
                    }
                }
                catch (Exception e)
                {
                    //AppSrv.g_Log.Error(e);
                }
            }
            return bhad;
        }

        class Resp_CheckStandInfo
        {
            public int needUpdateClient { get; set; }
            public string serverIp { get; set; }
            public ushort serverPort { get; set; }
            public string clientVersion { get; set; }
            public string downLoadUrl { get; set; }
            public string downLoadSavUrl { get; set; }
        }


        class Resp_GameList
        {
            public int page { get; set; }
            public int maxPage { get; set; }
            public int resultAllCount { get; set; }
            public List<Resp_RomInfo> gameList { get; set; }
        }

        public class Resp_RomInfo
        {
            public int orderid { get; set; }
            public int id { get; set; }
            public int ptype { get; set; }
            public string romName { get; set; }
            public string gType { get; set; }
            public string desc { get; set; }
            public string url { get; set; }
            public string imgUrl { get; set; }
            public string hash { get; set; }
            public int stars { get; set; }
            public int playcount { get; set; }
            public int isStar { get; set; }
            public List<int> parentRomIdsList { get; set; } = new List<int>();
        }

    }
}
