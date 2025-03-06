using AxibugEmuOnline.Client.Manager;
using System;

namespace Assets.Script.AppMain.AxiInput
{
    public abstract class AxiInputUGUIHandleBase : IDisposable
    {
        public int Handle { get; private set; }
        public AxiInputUGuiBtnType UguiBtnType { get; private set; }

        public AxiInputUGUIHandleBase(AxiInputUGuiBtnType uguiBtnType)
        {

            Handle = AxiInputUGUICenter.GetNextSeed();
            this.UguiBtnType = uguiBtnType;
            AxiInputUGUICenter.RegHandle(this);
        }
        public abstract bool GetKeyDown();
        public abstract bool GetKey();
        public abstract bool GetKeyUp();

        public void Dispose()
        {
            AxiInputUGUICenter.UnregHandle(this);
        }
    }
}
