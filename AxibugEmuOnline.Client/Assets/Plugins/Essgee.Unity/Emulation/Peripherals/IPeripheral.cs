namespace Essgee.Emulation.Peripherals
{
    interface IPeripheral : IAxiStatus
    {
        void Startup();
        void Shutdown();
        void Reset();

        byte ReadPort(byte port);
        void WritePort(byte port, byte value);
    }
}
