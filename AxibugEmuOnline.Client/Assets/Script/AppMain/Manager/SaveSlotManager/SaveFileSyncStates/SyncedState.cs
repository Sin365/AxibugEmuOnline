using AxibugEmuOnline.Client.Tools;

namespace AxibugEmuOnline.Client
{
    public partial class SaveFile
    {
        public class SyncedState : SimpleFSM<SaveFile>.State
        {
            public override void OnEnter(SimpleFSM<SaveFile>.State preState)
            {
                Host.ClearSavingFlag();
            }
        }
    }
}