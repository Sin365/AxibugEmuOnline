using AxibugEmuOnline.Client.Tools;

namespace AxibugEmuOnline.Client
{
    public partial class SaveFile
    {
        public class UploadingState : SimpleFSM<SaveFile>.State
        {
            public override void OnEnter(SimpleFSM<SaveFile>.State preState)
            {
                Host.CloudAPI.OnUploadedSavData += Api_OnUploadedSavData;

                Host.GetSavData(out byte[] savData, out byte[] screenData);
                Host.CloudAPI.SendUpLoadGameSav(Host.RomID, Host.SlotIndex, Host.Sequecen, savData, screenData);
            }

            public override void OnExit(SimpleFSM<SaveFile>.State nextState)
            {
                Host.CloudAPI.OnUploadedSavData -= Api_OnUploadedSavData;
            }

            private void Api_OnUploadedSavData(int romID, int slotIndex, AxibugProtobuf.Protobuf_Mine_GameSavInfo savInfo)
            {
                if (Host.RomID != romID) return;
                if (Host.SlotIndex != slotIndex) return;

                FSM.ChangeState<SyncedState>();
            }
        }
    }
}