using static Essgee.Emulation.CPU.SM83;

namespace Essgee.Emulation.CPU
{
    public interface IAxiEssgeeMemIO
    {
        public byte ReadMemory(ushort address);
        public void WriteMemory(ushort address, byte value);
    }
}
