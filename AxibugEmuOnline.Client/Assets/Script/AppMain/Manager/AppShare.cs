using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppShare
    {
        public AppShare()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGameMark, RecvGameStar);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamescreenImgUpload, RecvGamescreenImgUpload);
        }

        /// <summary>
        /// 发送收藏
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="Motion">[0]取消收藏[1]收藏</param>
        public void SendGameStar(int RomID, int Motion)
        {
            Protobuf_Game_Mark req = new Protobuf_Game_Mark()
            {
                Motion = Motion,
                RomID = RomID,
            };
            App.log.Info($"SendGameStar");
            App.network.SendToServer((int)CommandID.CmdGameMark, ProtoBufHelper.Serizlize(req));
        }

        /// <summary>
        /// 收藏
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGameStar(byte[] reqData)
        {
            Protobuf_Game_Mark_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Game_Mark_RESP>(reqData);
            Eventer.Instance.PostEvent(EEvent.OnRomStarStateChanged, msg.RomID, msg.IsStar == 1);
        }

        /// <summary>
        /// 上传封面图
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="SavImgData"></param>
        public void SendUpLoadGameScreenCover(int RomID, byte[] SavImgData)
        {
            //压缩
            byte[] compressImgData = Helper.CompressByteArray(SavImgData);

            Protobuf_GameScreen_Img_Upload req = new Protobuf_GameScreen_Img_Upload()
            {
                RomID = RomID,
                SavImg = Google.Protobuf.ByteString.CopyFrom(compressImgData),
            };

            App.log.Info($"SendUpLoadGameScreenCover");
            App.log.Info($"上传截图 原数据大小:{SavImgData.Length},压缩后;{compressImgData.Length}");

            App.network.SendToServer((int)CommandID.CmdGamescreenImgUpload, ProtoBufHelper.Serizlize(req));
        }

        private void RecvGamescreenImgUpload(byte[] obj)
        {
            OverlayManager.PopTip("登录成功");
        }
    }
}