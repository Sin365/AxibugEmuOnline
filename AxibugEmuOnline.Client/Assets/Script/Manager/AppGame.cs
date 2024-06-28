using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using AxibugEmuOnline.Client.UNES;
using Google.Protobuf;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppGame
    {
        public AppGame()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdScreen, OnScreen);
        }
        Protobuf_Screnn_Frame _Protobuf_Screnn_Frame = new Protobuf_Screnn_Frame();

        public uint[] RawBitmap { get; set; } = new uint[UNESBehaviour.GameWidth * UNESBehaviour.GameHeight];
        // TODO: use real chroma/luma decoding
        private readonly uint[] _palette = {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400,
            0x503000, 0x007800, 0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000,
            0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
            0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000, 0x000000, 0x000000,
            0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
            0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8,
            0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8, 0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };
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
            lock (RawBitmap)
            {
                byte[] data = DecompressByteArray(msg.RawBitmap.ToArray());
                for (int i = 0; i < data.Length; i++) 
                {
                    RawBitmap[i] = _palette[data[i]];
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
