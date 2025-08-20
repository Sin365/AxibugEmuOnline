namespace Essgee.Emulation.Audio
{
    public interface IDMGAudioChannel
    {
        int OutputVolume { get; }
        bool IsActive { get; }

        void Reset();
        void LengthCounterClock();
        void SweepClock();
        void VolumeEnvelopeClock();
        void Step();

        void WritePort(byte port, byte value);
        byte ReadPort(byte port);
        void WriteWaveRam(byte offset, byte value);
        byte ReadWaveRam(byte offset);
    }
}
