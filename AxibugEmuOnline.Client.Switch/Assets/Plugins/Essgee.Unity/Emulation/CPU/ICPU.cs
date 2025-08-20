namespace Essgee.Emulation.CPU
{
    interface ICPU : IAxiEssgssStatus
    {
        void Startup();
        void Shutdown();
        void Reset();
        int Step();

        void SetStackPointer(ushort value);
        void SetProgramCounter(ushort value);
    }
}
