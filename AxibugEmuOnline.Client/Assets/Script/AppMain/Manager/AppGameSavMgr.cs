using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System.Linq;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppGameSavMgr
    {
        public AppGameSavMgr()
        {
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamesavGetGameSavList, RecvGetGameSavList);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamesavDelGameSav, RecvDelGameSavList);
            NetMsg.Instance.RegNetMsgEvent((int)CommandID.CmdGamesavUploadGameSav, RecvUpLoadGameSav);
        }

        /// <summary>
        /// 发送请求即时存档列表
        /// </summary>
        /// <param name="RomID"></param>
        public void SendGetGameSavList(int RomID)
        {
            Protobuf_Mine_GetGameSavList req = new Protobuf_Mine_GetGameSavList()
            {
                RomID = RomID,
            };
            App.log.Info($"SendGetGameSavList");
            App.network.SendToServer((int)CommandID.CmdGamesavGetGameSavList, ProtoBufHelper.Serizlize(req));
        }

        void RecvGetGameSavList(byte[] reqData)
        {
            Protobuf_Mine_GetGameSavList_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Mine_GetGameSavList_RESP>(reqData);
            Protobuf_Mine_GameSavInfo[] savArr = new Protobuf_Mine_GameSavInfo[4];
            for (int i = 0; i < savArr.Length; i++)
            {
                Protobuf_Mine_GameSavInfo info = msg.SavDataList.FirstOrDefault(w => w.SavDataIdx == i);
                savArr[i] = info;
            }
            Eventer.Instance.PostEvent(EEvent.OnNetGameSavListGot, msg.RomID, savArr);
        }

        /// <summary>
        /// 发送删除即时存档
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="SavDataIdx"></param>
        public void SendDelGameSavList(int RomID, int SavDataIdx)
        {
            Protobuf_Mine_DelGameSav req = new Protobuf_Mine_DelGameSav()
            {
                RomID = RomID,
                SavDataIdx = SavDataIdx
            };
            App.log.Info($"SendDelGameSavList");
            App.network.SendToServer((int)CommandID.CmdGamesavGetGameSavList, ProtoBufHelper.Serizlize(req));
        }

        void RecvDelGameSavList(byte[] reqData)
        {
            Protobuf_Mine_DelGameSav_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Mine_DelGameSav_RESP>(reqData);
            Eventer.Instance.PostEvent(EEvent.OnNetGameSavDeleted, msg.RomID, msg.SavDataIdx);
        }

        /// <summary>
        /// 上传即时存档
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="SavDataIdx"></param>
        public void SendUpLoadGameSav(int RomID, int SavDataIdx, byte[] RawData, byte[] SavImgData)
        {
            //压缩
            byte[] compressRawData = Helper.CompressByteArray(RawData);

            //压缩
            byte[] compressImgData = Helper.CompressByteArray(SavImgData);

            Protobuf_Mine_UpLoadGameSav req = new Protobuf_Mine_UpLoadGameSav()
            {
                RomID = RomID,
                SavDataIdx = SavDataIdx,
                StateRaw = Google.Protobuf.ByteString.CopyFrom(compressRawData),
                SavImg = Google.Protobuf.ByteString.CopyFrom(compressImgData),
            };

            App.log.Info($"SendDelGameSavList");
            App.log.Info($"上传即时存档数据 原数据大小:{RawData.Length},压缩后;{compressRawData.Length}");
            App.log.Info($"上传截图 原数据大小:{SavImgData.Length},压缩后;{compressImgData.Length}");

            App.network.SendToServer((int)CommandID.CmdGamesavGetGameSavList, ProtoBufHelper.Serizlize(req));
        }

        void RecvUpLoadGameSav(byte[] reqData)
        {
            Protobuf_Mine_UpLoadGameSav_RESP msg = ProtoBufHelper.DeSerizlize<Protobuf_Mine_UpLoadGameSav_RESP>(reqData);
            Eventer.Instance.PostEvent(EEvent.OnNetUploaded, msg.RomID,msg.SavDataIdx, msg.UploadSevInfo);
        }

        /// <summary>
        /// 即时存档或网络截图下载完成之后，需要先解压再使用
        /// </summary>
        /// <returns></returns>
        public byte[] UnGzipData(byte[] data)
        {
            return Helper.DecompressByteArray(data);
        }
    }
}