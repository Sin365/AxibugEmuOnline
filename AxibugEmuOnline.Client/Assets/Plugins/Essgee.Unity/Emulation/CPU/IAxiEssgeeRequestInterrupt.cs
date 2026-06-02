using static Essgee.Emulation.CPU.SM83;

namespace Essgee.Emulation.CPU
{
    public interface IAxiEssgeeRequestInterrupt
    {
        public void RequestInterrupt(InterruptSource source);
    }
}
