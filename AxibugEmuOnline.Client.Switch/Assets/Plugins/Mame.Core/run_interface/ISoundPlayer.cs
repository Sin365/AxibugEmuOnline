namespace MAME.Core
{
    public interface ISoundPlayer
    {
        void BufferWirte(int Off, byte[] Data);
        void SubmitSamples(byte[] buffer, int samples_a);
        void SetVolume(int Vol);
        void GetCurrentPosition(out int play_position, out int write_position);
    }
}
