using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Tools;

namespace AxibugEmuOnline.Client
{
    public class DownloadingState : SimpleFSM<SaveFile>.State
    {
        uint m_sequece;
        private AxiHttpProxy.SendDownLoadProxy m_downloadTask;
        private AxiHttpProxy.SendDownLoadProxy m_downloadTaskImg;

        public override void OnEnter(SimpleFSM<SaveFile>.State preState)
        {
            var checkState = preState as CheckingState;

            var netData = checkState.NetData;

            if (Host.Sequecen >= (uint)netData.Sequence)
            {
                FSM.ChangeState<ConflictState>();
                return;
            }

            m_sequece = (uint)netData.Sequence;
            m_downloadTask = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{netData.SavUrl}");
            m_downloadTaskImg = AxiHttpProxy.GetDownLoad($"{App.httpAPI.WebHost}/{netData.SavImgUrl}");
        }

        public override void OnUpdate()
        {
            if (!m_downloadTask.downloadHandler.isDone) return;

            if (m_downloadTask.downloadHandler.bHadErr) //下载失败
            {
                FSM.ChangeState<UnkownState>();
                return;
            }

            if (!m_downloadTaskImg.downloadHandler.isDone) return;

            if (m_downloadTaskImg.downloadHandler.bHadErr) //下载失败
            {
                FSM.ChangeState<UnkownState>();
                return;
            }

            var savData = Host.CloudAPI.UnGzipData(m_downloadTask.downloadHandler.data);
            var imgData = Host.CloudAPI.UnGzipData(m_downloadTaskImg.downloadHandler.data);
            Host.Save(m_sequece, savData, imgData);
            FSM.ChangeState<SyncedState>();
        }
    }
}