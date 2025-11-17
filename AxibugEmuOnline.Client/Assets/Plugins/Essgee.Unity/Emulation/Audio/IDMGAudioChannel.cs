namespace Essgee.Emulation.Audio
{
    //public interface IDMGAudioChannel
    //    {
    //        int OutputVolume { get; }
    //    bool IsActive { get; }

    //    void Reset();
    //    void LengthCounterClock();
    //    void SweepClock();
    //    void VolumeEnvelopeClock();
    //    void Step();

    //    void WritePort(byte port, byte value);
    //    byte ReadPort(byte port);
    //    void WriteWaveRam(byte offset, byte value);
    //    byte ReadWaveRam(byte offset);
    //}
    public abstract class IDMGAudioChannel
    {
        public int OutputVolume;
        public abstract bool IsActive { get; }
        public abstract void Reset();
        public abstract void LengthCounterClock();
        public abstract void SweepClock();
        public abstract void VolumeEnvelopeClock();
        public abstract void Step();
        public abstract void WritePort(byte port, byte value);
        public abstract byte ReadPort(byte port);
        public abstract void WriteWaveRam(byte offset, byte value);
        public abstract byte ReadWaveRam(byte offset);
    }
}
