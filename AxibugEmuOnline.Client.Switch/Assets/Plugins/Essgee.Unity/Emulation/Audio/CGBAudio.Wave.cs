namespace Essgee.Emulation.Audio
{
    public partial class CGBAudio
    {
        public class CGBWave : Wave, IDMGAudioChannel
        {
            public override void Reset()
            {
                base.Reset();

                for (var i = 0; i < sampleBuffer.Length; i += 2)
                {
                    sampleBuffer[i + 0] = 0x00;
                    sampleBuffer[i + 1] = 0xFF;
                }
            }
        }
    }
}
