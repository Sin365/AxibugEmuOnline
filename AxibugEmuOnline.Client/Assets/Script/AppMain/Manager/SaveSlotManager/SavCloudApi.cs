using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Network;
using AxibugProtobuf;
using System.Collections.Generic;
using System.Linq;

namespace AxibugEmuOnline.Client
{
    /// <summary>
    /// 负责存档的云端保存和获取功能
    /// </summary>
    public class SavCloudApi
    {
        public delegate void OnFetchGameSavListHandle(int romID, Protobuf_Mine_GameSavInfo[] savSlotData);
        public event OnFetchGameSavListHandle OnFetchGameSavList;
        public delegate void OnUploadedSavDataHandle(int romID, int slotIndex, Protobuf_Mine_GameSavInfo savInfo);
        public event OnUploadedSavDataHandle OnUploadedSavData;

        public SavCloudApi()
        {
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Mine_GetGameSavList_RESP>((int)CommandID.CmdGamesavGetGameSavList, RecvGetGameSavList);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Mine_DelGameSav_RESP>((int)CommandID.CmdGamesavDelGameSav, RecvDelGameSavList);
            NetMsg.Instance.RegNetMsgEvent<Protobuf_Mine_UpLoadGameSav_RESP>((int)CommandID.CmdGamesavUploadGameSav, RecvUpLoadGameSav);
        }

        private HashSet<int> m_fetchingRomIDs = new HashSet<int>();
        /// <summary>
        /// 发送请求即时存档列表
        /// </summary>
        /// <param name="RomID"></param>
        public void SendGetGameSavList(int RomID)
        {
            if (m_fetchingRomIDs.Contains(RomID)) return;

            Protobuf_Mine_GetGameSavList req = new Protobuf_Mine_GetGameSavList()
            {
                RomID = RomID,
            };
            App.log.Info($"SendGetGameSavList");
            App.network.SendToServer((int)CommandID.CmdGamesavGetGameSavList, ProtoBufHelper.Serizlize(req));
        }

        void RecvGetGameSavList(Protobuf_Mine_GetGameSavList_RESP msg)
        {
            if (m_fetchingRomIDs.Remove(msg.RomID)) return;

            Protobuf_Mine_GameSavInfo[] savArr = new Protobuf_Mine_GameSavInfo[4];
            for (int i = 0; i < savArr.Length; i++)
            {
                Protobuf_Mine_GameSavInfo info = msg.SavDataList.FirstOrDefault(w => w.SavDataIdx == i);
                savArr[i] = info;
            }
            OnFetchGameSavList?.Invoke(msg.RomID, savArr);
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
            App.network.SendToServer((int)CommandID.CmdGamesavDelGameSav, ProtoBufHelper.Serizlize(req));
        }

        void RecvDelGameSavList(Protobuf_Mine_DelGameSav_RESP msg)
        {
            Eventer.Instance.PostEvent(EEvent.OnNetGameSavDeleted, msg.RomID, msg.SavDataIdx);
        }

        /// <summary>
        /// 上传即时存档
        /// </summary>
        /// <param name="RomID"></param>
        /// <param name="SavDataIdx"></param>
        public void SendUpLoadGameSav(int RomID, int SavDataIdx, uint sequence, byte[] RawData, byte[] SavImgData)
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
                Sequence = (int)sequence
            };

            App.log.Info($"SendDelGameSavList");
            App.log.Info($"上传即时存档数据 原数据大小:{RawData.Length},压缩后;{compressRawData.Length}");
            App.log.Info($"上传截图 原数据大小:{SavImgData.Length},压缩后;{compressImgData.Length}");

            App.network.SendToServer((int)CommandID.CmdGamesavUploadGameSav, ProtoBufHelper.Serizlize(req));
        }

        void RecvUpLoadGameSav(Protobuf_Mine_UpLoadGameSav_RESP msg)
        {
            OnUploadedSavData?.Invoke(msg.RomID, msg.SavDataIdx, msg.UploadSevInfo);
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