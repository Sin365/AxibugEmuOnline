using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Tools;
using System;

namespace AxibugEmuOnline.Client
{
    public partial class SaveFile
    {

        public class CheckingNetworkState : SimpleFSM<SaveFile>.State
        {
            public override void OnUpdate()
            {
                if (App.network.isConnected)
                {
                    FSM.ChangeState<CheckingState>();
                }
            }
        }
    }
}