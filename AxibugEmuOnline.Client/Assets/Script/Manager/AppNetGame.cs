using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using Google.Protobuf;
using MyNes.Core;
using System.IO;
using System.IO.Compression;
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
            _palette = NTSCPaletteGenerator.GeneratePalette();
        }

        Protobuf_Screnn_Frame _Protobuf_Screnn_Frame = new Protobuf_Screnn_Frame();

        public void SendScreen(byte[] RenderBuffer)
        {
            byte[] comData = CompressByteArray(RenderBuffer);
            _Protobuf_Screnn_Frame.FrameID = 0;
            _Protobuf_Screnn_Frame.RawBitmap = ByteString.CopyFrom(comData);
            AppAxibugEmuOnline.networkHelper.SendToServer((int)CommandID.CmdScreen, ProtoBufHelper.Serizlize(_Protobuf_Screnn_Frame));
        }

        public void OnScreen(byte[] reqData)
        {
            Protobuf_Screnn_Frame msg = ProtoBufHelper.DeSerizlize<Protobuf_Screnn_Frame>(reqData);
            lock (_renderbuffer)
            {
                byte[] data = DecompressByteArray(msg.RawBitmap.ToArray());
                for (int i = 0; i < data.Length; i++)
                {
                    _renderbuffer[i] = _palette[data[i]];
                }
            }
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
