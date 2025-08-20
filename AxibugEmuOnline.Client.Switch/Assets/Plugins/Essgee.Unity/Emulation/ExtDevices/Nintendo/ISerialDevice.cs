using Essgee.EventArguments;
using System;

namespace Essgee.Emulation.ExtDevices.Nintendo
{
    public interface ISerialDevice
    {
        event EventHandler<SaveExtraDataEventArgs> SaveExtraData;

        void Initialize();
        void Shutdown();

        byte ExchangeBit(int left, byte data);
    }
}
