using AxibugEmuOnline.Client.Tools;
using AxibugProtobuf;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CheckingState : SimpleFSM<SaveFile>.State
    {
        private float m_timeOut;

        public Protobuf_Mine_GameSavInfo NetData { get; private set; }

        public override void OnEnter(SimpleFSM<SaveFile>.State preState)
        {
            m_timeOut = 5f;
            Host.CloudAPI.OnFetchGameSavList += CloudAPI_OnFetchGameSavList;
            Host.CloudAPI.SendGetGameSavList(Host.RomID);
        }

        public override void OnExit(SimpleFSM<SaveFile>.State nextState)
        {
            Host.CloudAPI.OnFetchGameSavList -= CloudAPI_OnFetchGameSavList;
        }

        public override void OnUpdate()
        {
            m_timeOut -= Time.deltaTime;
            if (m_timeOut < 0) //已超时
            {
                FSM.ChangeState<UnkownState>();
            }
        }

        private void CloudAPI_OnFetchGameSavList(int romID, Protobuf_Mine_GameSavInfo[] savSlotData)
        {
            if (romID != Host.RomID) return;
            NetData = savSlotData[Host.SlotIndex];

            if (NetData == null) //云存档不存在,上传本地存档
            {
                FSM.ChangeState<UploadingState>();
            }
            else
            {
                FSM.ChangeState<DownloadingState>();
            }
        }
    }
}