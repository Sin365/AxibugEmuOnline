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
            Eventer.Instance.PostEvent(EEvent.OnRomStarStateChanged, msg.RomID,msg.IsStar == 1);
        }

    }
}