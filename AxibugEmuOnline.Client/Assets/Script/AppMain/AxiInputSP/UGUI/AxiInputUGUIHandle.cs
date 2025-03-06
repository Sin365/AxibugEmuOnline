using System;

namespace AxiInputSP.UGUI
{
    public class AxiInputUGUIHandle
    {
        public int Handle { get; private set; }
        public AxiInputUGuiBtnType UguiBtnType { get; private set; }

        public AxiInputUGUIHandle(AxiInputUGuiBtnType uguiBtnType)
        {
            Handle = AxiInputUGUICenter.GetNextSeed();
            this.UguiBtnType = uguiBtnType;
            AxiInputUGUICenter.RegHandle(this);
        }
        public bool GetKey()
        {
            return GetKeyHandle != null ? GetKeyHandle.Invoke() : false;
        }
        public bool GetKeyUp()
        {
            return GetKeyUpHandle != null ? GetKeyUpHandle.Invoke() : false;
        }
        public bool GetKeyDown()
        {
            return GetKeyDownHandle != null ? GetKeyDownHandle.Invoke() : false;
        }
        public Func<bool> GetKeyHandle;
        public Func<bool> GetKeyUpHandle;
        public Func<bool> GetKeyDownHandle;

        public void Dispose()
        {
            GetKeyHandle = null;
            GetKeyUpHandle = null;
            GetKeyDownHandle = null;
            AxiInputUGUICenter.UnregHandle(this);
        }
    }
}
