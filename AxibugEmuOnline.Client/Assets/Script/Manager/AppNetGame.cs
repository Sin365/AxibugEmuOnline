using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using Google.Protobuf;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppNetGame
    {
        public AppNetGame()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnScreen);
        }
        Protobuf_Screnn_Frame _Protobuf_Screnn_Frame = new Protobuf_Screnn_Frame();

        public void SendScreen(byte[] ScreenData)
        {
            byte[] comData = CompressByteArray(ScreenData);
            _Protobuf_Screnn_Frame.FrameID = 0;
            _Protobuf_Screnn_Frame.RawBitmap = ByteString.CopyFrom(comData);
            AppAxibugEmuOnline.networkHelper.SendToServer((int)CommandID.CmdScreen, ProtoBufHelper.Serizlize(_Protobuf_Screnn_Frame));
        }

        public void OnScreen(byte[] reqData)
        {
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
            //lock (RawBitmap)
            //{
            //    byte[] data = DecompressByteArray(msg.RawBitmap.ToArray());
            //    for (int i = 0; i < data.Length; i++) 
            //    {
            //        RawBitmap[i] = _palette[data[i]];
            //    }
            //}
        }

        public static byte[] CompressByteArray(byte[] bytesToCompress)
        {
            using (var compressedMemoryStream = new MemoryStream())
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(bytesToCompress, 0, bytesToCompress.Length);
                gzipStream.Close();
                return compressedMemoryStream.ToArray();
            }
        }

        public static byte[] DecompressByteArray(byte[] compressedBytes)
        {
            using (var compressedMemoryStream = new MemoryStream(compressedBytes))
            using (var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
            using (var resultMemoryStream = new MemoryStream())
            {
                gzipStream.CopyTo(resultMemoryStream);
                return resultMemoryStream.ToArray();
            }
        }
    }
}
