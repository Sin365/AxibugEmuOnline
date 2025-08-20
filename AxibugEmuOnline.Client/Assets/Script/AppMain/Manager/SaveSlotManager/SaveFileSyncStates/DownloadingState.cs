using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Tools;
using AxibugProtobuf;
using System;

namespace AxibugEmuOnline.Client
{
    public partial class SaveFile
    {
        public class DownloadingState : SimpleFSM<SaveFile>.State
        {
            uint m_sequece;
            private AxiHttpProxy.SendDownLoadProxy m_downloadTask;
            private AxiHttpProxy.SendDownLoadProxy m_downloadTaskImg;

            public override void OnEnter(SimpleFSM<SaveFile>.State preState)
            {
                Protobuf_Mine_GameSavInfo netData = null;
                if (preState is CheckingState checkState)
                {
                    netData = checkState.NetData;
                }
                else if (preState is ConflictState conflictState) //由冲突状态转换为下载状态，代表使用网络存档覆盖本地
                {
                    netData = conflictState.NetData;
                }

                m_sequece = (uint)netData.Sequence;
                m_downloadTask = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{netData.SavUrl}?v={m_sequece}");
                m_downloadTaskImg = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{netData.SavImgUrl}?v={m_sequece}");

                Host.SetSavingFlag();
            }

            public override void OnExit(SimpleFSM<SaveFile>.State nextState)
            {
                Host.ClearSavingFlag();
            }

            public override void OnUpdate()
            {
                if (!m_downloadTask.downloadHandler.isDone) return;

                if (m_downloadTask.downloadHandler.bHadErr) //下载失败
                {
                    FSM.GetState<SyncFailedState>().Error = m_downloadTask.downloadHandler.ErrInfo;
                    FSM.ChangeState<SyncFailedState>();
                    return;
                }

                if (!m_downloadTaskImg.downloadHandler.isDone) return;

                if (m_downloadTaskImg.downloadHandler.bHadErr) //下载失败
                {
                    FSM.GetState<SyncFailedState>().Error = m_downloadTaskImg.downloadHandler.ErrInfo;
                    FSM.ChangeState<SyncFailedState>();
                    return;
                }

                try
                {
                    var savData = Host.CloudAPI.UnGzipData(m_downloadTask.downloadHandler.data);
                    var imgData = m_downloadTaskImg.downloadHandler.data; //图片数据已被服务器解压

                    Host.Save(m_sequece, savData, imgData);
                    FSM.ChangeState<SyncedState>();
                }
                catch
                {
                    FSM.GetState<SyncFailedState>().Error = "云存档解压失败";
                    FSM.ChangeState<SyncFailedState>();
                    return;
                }
            }
        }

    }
}