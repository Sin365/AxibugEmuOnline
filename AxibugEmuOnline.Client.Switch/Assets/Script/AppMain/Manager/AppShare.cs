using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppShare
    {
        static Protobuf_Game_Mark _Protobuf_Game_Mark = new Protobuf_Game_Mark();
        static Protobuf_GameScreen_Img_Upload _Protobuf_GameScreen_Img_Upload = new Protobuf_GameScreen_Img_Upload();
        public AppShare()
        {
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Game_Mark_RESP>((int)CommandID.CmdGameMark, RecvGameStar);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_GameScreen_Img_Upload_RESP>((int)CommandID.CmdGamescreenImgUpload, RecvGamescreenImgUpload);
        }

        /// <summary>
        /// 发送收藏
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="Motion">[0]取消收藏[1]收藏</param>
        public void SendGameStar(int RomID, int Motion)
        {
            _Protobuf_Game_Mark.Motion = Motion;
            _Protobuf_Game_Mark.RomID = RomID;
            App.log.Info($"SendGameStar");
            App.network.SendToServer((int)CommandID.CmdGameMark, _Protobuf_Game_Mark);
        }

        /// <summary>
        /// 收藏
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGameStar(Protobuf_Game_Mark_RESP msg)
        {
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

            _Protobuf_GameScreen_Img_Upload.RomID = RomID;
            _Protobuf_GameScreen_Img_Upload.SavImg = Google.Protobuf.ByteString.CopyFrom(compressImgData);

            App.log.Info($"SendUpLoadGameScreenCover");
            App.log.Info($"上传截图 原数据大小:{SavImgData.Length},压缩后;{compressImgData.Length}");

            App.network.SendToServer((int)CommandID.CmdGamescreenImgUpload, _Protobuf_GameScreen_Img_Upload);
            _Protobuf_GameScreen_Img_Upload.Reset();
        }

        private void RecvGamescreenImgUpload(Protobuf_GameScreen_Img_Upload_RESP msg)
        {
            OverlayManager.PopTip("封面图上传成功");
            if (App.emu.Core == null || App.emu.RomID <= 0 || App.emu.Platform <= RomPlatformType.Invalid)
                return;

            RomLib lib = App.GetRomLib(App.emu.Platform);
            if (lib == null) return;
            RomFile file = lib.GetRomFile(App.emu.RomID);
            if (file == null) return;

            App.StartCoroutine(App.httpAPI.GetRomInfo(file.ID, (romInfo) =>
            {
                file.SetWebData(romInfo);
            }));
        }
    }
}