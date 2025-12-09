namespace Essgee.Emulation.Audio
{
    public partial class CGBAudio
    {
        //本身wave就继承了IDMGAudioChannel
        public class CGBWave : Wave//, IDMGAudioChannel
        {
            public unsafe override void Reset()
            {
                base.Reset();

                //for (var i = 0; i < sampleBuffer.Length; i += 2)
                //{
                //    sampleBuffer[i + 0] = 0x00;
                //    sampleBuffer[i + 1] = 0xFF;
                //}
                for (var i = 0; i < sampleBufferLength; i += 2)
                {
                    *(sampleBuffer + i) = 0x00;
                    *(sampleBuffer + i + 1) = 0xFF;
                }
            }
        }
    }
}
