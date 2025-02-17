namespace Essgee.Emulation.CPU
{
    interface ICPU : IAxiStatus
    {
        void Startup();
        void Shutdown();
        void Reset();
        int Step();

        void SetStackPointer(ushort value);
        void SetProgramCounter(ushort value);
    }
}
