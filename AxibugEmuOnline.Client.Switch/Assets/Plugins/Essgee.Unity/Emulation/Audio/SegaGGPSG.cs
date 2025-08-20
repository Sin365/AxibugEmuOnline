using Essgee.Utilities;

namespace Essgee.Emulation.Audio
{
    public unsafe class SegaGGPSG : SegaSMSPSG
    {
        public const int PortStereoControl = 0x06;

        [StateRequired]
        readonly bool[] channel0Enable, channel1Enable, channel2Enable, channel3Enable;

        public SegaGGPSG() : base()
        {
            channel0Enable = new bool[2];
            channel1Enable = new bool[2];
            channel2Enable = new bool[2];
            channel3Enable = new bool[2];
        }

        public override void Reset()
        {
            base.Reset();

            WritePort(PortStereoControl, 0xFF);
        }

        protected override void GenerateSample()
        {
            for (int i = 0; i < numOutputChannels; i++)
            {
                /* Generate samples */
                var ch1 = (channel0Enable[i] ? (short)(volumeTable[volumeRegisters[0]] * ((toneRegisters[0] < 2 ? true : channelOutput[0]) ? 1.0 : 0.0)) : (short)0);
                var ch2 = (channel1Enable[i] ? (short)(volumeTable[volumeRegisters[1]] * ((toneRegisters[1] < 2 ? true : channelOutput[1]) ? 1.0 : 0.0)) : (short)0);
                var ch3 = (channel2Enable[i] ? (short)(volumeTable[volumeRegisters[2]] * ((toneRegisters[2] < 2 ? true : channelOutput[2]) ? 1.0 : 0.0)) : (short)0);
                var ch4 = (channel3Enable[i] ? (short)(volumeTable[volumeRegisters[3]] * (noiseLfsr & 0x1)) : (short)0);


                //废弃旧的数组方式
                //channelSampleBuffer[0].Add(ch1);
                //channelSampleBuffer[1].Add(ch2);
                //channelSampleBuffer[2].Add(ch3);
                //channelSampleBuffer[3].Add(ch4);

                //二维指针下标
                channelSampleBuffer_writePos++;
                channelSampleBuffer[0][channelSampleBuffer_writePos] = ch1;
                channelSampleBuffer[1][channelSampleBuffer_writePos] = ch2;
                channelSampleBuffer[2][channelSampleBuffer_writePos] = ch3;
                channelSampleBuffer[3][channelSampleBuffer_writePos] = ch4;

                /* Mix samples */
                var mixed = 0;
                if (channel1ForceEnable) mixed += ch1;
                if (channel2ForceEnable) mixed += ch2;
                if (channel3ForceEnable) mixed += ch3;
                if (channel4ForceEnable) mixed += ch4;
                mixed /= numChannels;

                //废弃旧的方式
                //mixedSampleBuffer.Add((short)mixed);
                //指针下标
                mixedSampleBuffer_writePos++;
                mixedSampleBuffer[mixedSampleBuffer_writePos] = (short)mixed;
            }
        }

        public override void WritePort(byte port, byte data)
        {
            if (port == PortStereoControl)
            {
                /* Stereo control */
                channel0Enable[0] = ((data & 0x10) != 0);   /* Ch1 Left */
                channel0Enable[1] = ((data & 0x01) != 0);   /* Ch1 Right */

                channel1Enable[0] = ((data & 0x20) != 0);   /* Ch2 Left */
                channel1Enable[1] = ((data & 0x02) != 0);   /* Ch2 Right */

                channel2Enable[0] = ((data & 0x40) != 0);   /* Ch3 Left */
                channel2Enable[1] = ((data & 0x04) != 0);   /* Ch3 Right */

                channel3Enable[0] = ((data & 0x80) != 0);   /* Ch4 Left */
                channel3Enable[1] = ((data & 0x08) != 0);   /* Ch4 Right */
            }
            else
                base.WritePort(port, data);
        }
    }
}
