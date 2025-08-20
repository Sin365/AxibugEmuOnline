using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Tools;

namespace AxibugEmuOnline.Client
{
    public partial class SaveFile
    {
        public class SyncFailedState : SimpleFSM<SaveFile>.State
        {
            public string Error { get; set; }

            public override void OnEnter(SimpleFSM<SaveFile>.State preState)
            {
                App.log.Error($"同步失败:{Error}");
            }
        }
    }
}