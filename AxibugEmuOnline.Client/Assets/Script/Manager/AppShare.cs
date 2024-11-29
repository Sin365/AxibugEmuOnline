using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppShare
    {
        public AppShare()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGameMark, RecvGameStar);
        }

        /// <summary>
        /// 发送收藏
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="Motion">[0]收藏[1]取消收藏</param>
        public void SendGameStar(int RomID, PlatformType Platform, int Motion)
        {
            Protobuf_Game_Mark req = new Protobuf_Game_Mark()
            {
                State = Motion,
                RomID = RomID,
                PlatformType = Platform
            };

            App.log.Info($"LeavnRoom");
            App.network.SendToServer((int)CommandID.CmdGameMark, ProtoBufHelper.Serizlize(req));
        }

        /// <summary>
        /// 离开房间成功
        /// </summary>
        /// <param name="reqData"></param>
        void RecvGameStar(byte[] reqData)
        {
            Protobuf_Game_Mark_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Game_Mark_RESP>(reqData);

            Eventer.Instance.PostEvent(EEvent.OnDoStars, msg.PlatformType, msg.RomID);
        }

    }
}