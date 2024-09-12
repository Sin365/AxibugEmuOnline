using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using Google.Protobuf;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppNetGame
    {
        int CurrRoomID;
        int[] _palette;
        public int[] _renderbuffer { private set; get; }
        public AppNetGame()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnScreen);
        }

        Protobuf_Screnn_Frame _Protobuf_Screnn_Frame = new Protobuf_Screnn_Frame();

        public void SendScreen(byte[] RenderBuffer)
        {
            byte[] comData = Helper.CompressByteArray(RenderBuffer);
            _Protobuf_Screnn_Frame.FrameID = 0;
            _Protobuf_Screnn_Frame.RawBitmap = ByteString.CopyFrom(comData);
            App.network.SendToServer((int)CommandID.CmdScreen, ProtoBufHelper.Serizlize(_Protobuf_Screnn_Frame));
        }

        public void OnScreen(byte[] reqData)
        {
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
            lock (_renderbuffer)
            {
                byte[] data = Helper.DecompressByteArray(msg.RawBitmap.ToArray());
                for (int i = 0; i < data.Length; i++)
                {
                    _renderbuffer[i] = _palette[data[i]];
                }
            }
        }

        
    }
}
