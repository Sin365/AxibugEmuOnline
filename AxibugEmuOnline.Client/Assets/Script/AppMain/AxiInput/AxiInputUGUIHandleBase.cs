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
        public abstract bool IsKeyDown();
        public abstract bool IsKey();

        public void Dispose()
        {
            AxiInputUGUICenter.UnregHandle(this);
        }
    }
}
