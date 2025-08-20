using Essgee.EventArguments;
using System;
using System.ComponentModel;

namespace Essgee.Emulation.ExtDevices.Nintendo
{
    [Description("None")]
    //todo Unity [ElementPriority(0)]
    public class DummyDevice : ISerialDevice
    {
        public event EventHandler<SaveExtraDataEventArgs> SaveExtraData;
        protected virtual void OnSaveExtraData(SaveExtraDataEventArgs e) { SaveExtraData?.Invoke(this, e); }

        public void Initialize() { }
        public void Shutdown() { }
        public byte ExchangeBit(int left, byte data) { return 0b1; }
    }
}
