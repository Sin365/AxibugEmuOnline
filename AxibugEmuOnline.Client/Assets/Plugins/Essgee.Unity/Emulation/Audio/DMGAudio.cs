using Essgee.EventArguments;
using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Essgee.Emulation.Audio
{
    public unsafe partial class DMGAudio : IAudio
    {
        // https://gbdev.gg8.se/wiki/articles/Gameboy_sound_hardware
        // http://emudev.de/gameboy-emulator/bleeding-ears-time-to-add-audio/
        // https://github.com/GhostSonic21/GhostBoy/blob/master/GhostBoy/APU.cpp

        protected const int numChannels = 4;

        protected const string channel1OptionName = "AudioEnableCh1Square";
        protected const string channel2OptionName = "AudioEnableCh2Square";
        protected const string channel3OptionName = "AudioEnableCh3Wave";
        protected const string channel4OptionName = "AudioEnableCh4Noise";

        protected IDMGAudioChannel channel1, channel2, channel3, channel4;

        // FF24 - NR50
        byte[] volumeRightLeft;
        bool[] vinEnableRightLeft;

        // FF25 - NR51
        bool[] channel1Enable, channel2Enable, channel3Enable, channel4Enable;

        // FF26 - NR52
        bool isSoundHwEnabled;

        protected int frameSequencerReload, frameSequencerCounter, frameSequencer;

        //protected List<short>[] channelSampleBuffer;
        #region //指针化 channelSampleBuffer
        static short[][] channelSampleBuffer_src;
        static GCHandle[] channelSampleBuffer_handle;
        public static short*[] channelSampleBuffer;
        public static int[] channelSampleBufferLength;
        public static int channelSampleBuffer_writePos;
        public static bool channelSampleBuffer_IsNull => channelSampleBuffer == null;
        public static void channelSampleBuffer_Init(int length1, int Lenght2)
        {
            if (channelSampleBuffer_src != null)
            {
                for (int i = 0; i < channelSampleBuffer_src.Length; i++)
                    channelSampleBuffer_handle[i].ReleaseGCHandle();
            }

            channelSampleBuffer_src = new short[length1][];
            channelSampleBuffer_handle = new GCHandle[length1];
            channelSampleBuffer = new short*[length1];
            channelSampleBuffer_writePos = 0;
            for (int i = 0; i < channelSampleBuffer_src.Length; i++)
            {
                channelSampleBuffer_src[i] = new short[Lenght2];
                channelSampleBuffer_src[i].GetObjectPtr(ref channelSampleBuffer_handle[i], ref channelSampleBuffer[i]);
            }
        }
        #endregion
        //protected List<short> mixedSampleBuffer;

        #region //指针化 mixedSampleBuffer
        short[] mixedSampleBuffer_src;
        GCHandle mixedSampleBuffer_handle;
        public short* mixedSampleBuffer;
        public int mixedSampleBufferLength;
        public int mixedSampleBuffer_writePos;
        public bool mixedSampleBuffer_IsNull => mixedSampleBuffer == null;
        public short[] mixedSampleBuffer_set
        {
            set
            {
                mixedSampleBuffer_handle.ReleaseGCHandle();
                mixedSampleBuffer_src = value;
                mixedSampleBufferLength = value.Length;
                mixedSampleBuffer_writePos = 0;
                mixedSampleBuffer_src.GetObjectPtr(ref mixedSampleBuffer_handle, ref mixedSampleBuffer);
            }
        }
        #endregion

        public virtual event EventHandler<EnqueueSamplesEventArgs> EnqueueSamples;
        public virtual void OnEnqueueSamples(EnqueueSamplesEventArgs e) { EnqueueSamples?.Invoke(this, e); }

        protected int sampleRate, numOutputChannels;

        //

        double clockRate, refreshRate;
        protected int samplesPerFrame, cyclesPerFrame, cyclesPerSample;
        [StateRequired]
        int sampleCycleCount, frameCycleCount;

        protected bool channel1ForceEnable, channel2ForceEnable, channel3ForceEnable, channel4ForceEnable;

        public (string Name, string Description)[] RuntimeOptions => new (string name, string description)[]
        {
            (channel1OptionName, "Channel 1 (Square)"),
            (channel2OptionName, "Channel 2 (Square)"),
            (channel3OptionName, "Channel 3 (Wave)"),
            (channel4OptionName, "Channel 4 (Noise)")
        };

        public DMGAudio()
        {
            //channelSampleBuffer = new List<short>[numChannels];
            //for (int i = 0; i < numChannels; i++) channelSampleBuffer[i] = new List<short>();
            //mixedSampleBuffer = new List<short>();

            //改为二维数组
            channelSampleBuffer_Init(numChannels, 1470);
            mixedSampleBuffer_set = new short[1470];


            channel1 = new Square(true);
            channel2 = new Square(false);
            channel3 = new Wave();
            channel4 = new Noise();

            samplesPerFrame = cyclesPerFrame = cyclesPerSample = -1;

            channel1ForceEnable = true;
            channel2ForceEnable = true;
            channel3ForceEnable = true;
            channel4ForceEnable = true;
        }


        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            volumeRightLeft = data.MemberData[nameof(volumeRightLeft)];
            vinEnableRightLeft = data.MemberData[nameof(vinEnableRightLeft)].ToBoolArray();

            clockRate = BitConverter.ToDouble(data.MemberData[nameof(clockRate)]);
            refreshRate = BitConverter.ToDouble(data.MemberData[nameof(refreshRate)]);

            channel1Enable = data.MemberData[nameof(channel1Enable)].ToBoolArray();
            channel2Enable = data.MemberData[nameof(channel2Enable)].ToBoolArray();
            channel3Enable = data.MemberData[nameof(channel3Enable)].ToBoolArray();
            channel4Enable = data.MemberData[nameof(channel4Enable)].ToBoolArray();

            isSoundHwEnabled = BitConverter.ToBoolean(data.MemberData[nameof(isSoundHwEnabled)]);

            samplesPerFrame = BitConverter.ToInt32(data.MemberData[nameof(samplesPerFrame)]);
            cyclesPerFrame = BitConverter.ToInt32(data.MemberData[nameof(cyclesPerFrame)]);
            cyclesPerSample = BitConverter.ToInt32(data.MemberData[nameof(cyclesPerSample)]);

            sampleCycleCount = BitConverter.ToInt32(data.MemberData[nameof(sampleCycleCount)]);
            frameCycleCount = BitConverter.ToInt32(data.MemberData[nameof(frameCycleCount)]);

            channel1ForceEnable = BitConverter.ToBoolean(data.MemberData[nameof(channel1ForceEnable)]);
            channel2ForceEnable = BitConverter.ToBoolean(data.MemberData[nameof(channel2ForceEnable)]);
            channel3ForceEnable = BitConverter.ToBoolean(data.MemberData[nameof(channel3ForceEnable)]);
            channel4ForceEnable = BitConverter.ToBoolean(data.MemberData[nameof(channel4ForceEnable)]);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();

            data.MemberData[nameof(volumeRightLeft)] = volumeRightLeft;
            data.MemberData[nameof(vinEnableRightLeft)] = vinEnableRightLeft.ToByteArray();

            data.MemberData[nameof(clockRate)] = BitConverter.GetBytes(clockRate);
            data.MemberData[nameof(refreshRate)] = BitConverter.GetBytes(refreshRate);

            data.MemberData[nameof(channel1Enable)] = channel1Enable.ToByteArray();
            data.MemberData[nameof(channel2Enable)] = channel2Enable.ToByteArray();
            data.MemberData[nameof(channel3Enable)] = channel3Enable.ToByteArray();
            data.MemberData[nameof(channel4Enable)] = channel4Enable.ToByteArray();

            data.MemberData[nameof(isSoundHwEnabled)] = BitConverter.GetBytes(isSoundHwEnabled);

            data.MemberData[nameof(samplesPerFrame)] = BitConverter.GetBytes(samplesPerFrame);
            data.MemberData[nameof(cyclesPerFrame)] = BitConverter.GetBytes(cyclesPerFrame);
            data.MemberData[nameof(cyclesPerSample)] = BitConverter.GetBytes(cyclesPerSample);

            data.MemberData[nameof(sampleCycleCount)] = BitConverter.GetBytes(sampleCycleCount);
            data.MemberData[nameof(frameCycleCount)] = BitConverter.GetBytes(frameCycleCount);

            data.MemberData[nameof(channel1ForceEnable)] = BitConverter.GetBytes(channel1ForceEnable);
            data.MemberData[nameof(channel2ForceEnable)] = BitConverter.GetBytes(channel2ForceEnable);
            data.MemberData[nameof(channel3ForceEnable)] = BitConverter.GetBytes(channel3ForceEnable);
            data.MemberData[nameof(channel4ForceEnable)] = BitConverter.GetBytes(channel4ForceEnable);
            return data;
        }
        #endregion

        public object GetRuntimeOption(string name)
        {
            switch (name)
            {
                case channel1OptionName: return channel1ForceEnable;
                case channel2OptionName: return channel2ForceEnable;
                case channel3OptionName: return channel3ForceEnable;
                case channel4OptionName: return channel4ForceEnable;
                default: return null;
            }
        }

        public void SetRuntimeOption(string name, object value)
        {
            switch (name)
            {
                case channel1OptionName: channel1ForceEnable = (bool)value; break;
                case channel2OptionName: channel2ForceEnable = (bool)value; break;
                case channel3OptionName: channel3ForceEnable = (bool)value; break;
                case channel4OptionName: channel4ForceEnable = (bool)value; break;
            }
        }

        public void SetSampleRate(int rate)
        {
            sampleRate = rate;
            ConfigureTimings();
        }

        public void SetOutputChannels(int channels)
        {
            numOutputChannels = channels;
            ConfigureTimings();
        }

        public void SetClockRate(double clock)
        {
            clockRate = clock;
            ConfigureTimings();
        }

        public void SetRefreshRate(double refresh)
        {
            refreshRate = refresh;
            ConfigureTimings();
        }

        private void ConfigureTimings()
        {
            samplesPerFrame = (int)(sampleRate / refreshRate);
            cyclesPerFrame = (int)Math.Round(clockRate / refreshRate);
            cyclesPerSample = (cyclesPerFrame / samplesPerFrame);

            volumeRightLeft = new byte[numOutputChannels];
            vinEnableRightLeft = new bool[numOutputChannels];

            channel1Enable = new bool[numOutputChannels];
            channel2Enable = new bool[numOutputChannels];
            channel3Enable = new bool[numOutputChannels];
            channel4Enable = new bool[numOutputChannels];

            FlushSamples();
        }

        public virtual void Startup()
        {
            Reset();

            if (samplesPerFrame == -1) throw new EmulationException("GB PSG: Timings not configured, invalid samples per frame");
            if (cyclesPerFrame == -1) throw new EmulationException("GB PSG: Timings not configured, invalid cycles per frame");
            if (cyclesPerSample == -1) throw new EmulationException("GB PSG: Timings not configured, invalid cycles per sample");
        }

        public virtual void Shutdown()
        {
            //
        }

        public virtual void Reset()
        {
            FlushSamples();

            channel1.Reset();
            channel2.Reset();
            channel3.Reset();
            channel4.Reset();

            for (var i = 0; i < numOutputChannels; i++)
            {
                volumeRightLeft[i] = 0;
                vinEnableRightLeft[i] = false;

                channel1Enable[i] = false;
                channel2Enable[i] = false;
                channel3Enable[i] = false;
                channel4Enable[i] = false;
            }

            frameSequencerReload = (int)(clockRate / 512);
            frameSequencerCounter = frameSequencerReload;
            frameSequencer = 0;

            sampleCycleCount = frameCycleCount = 0;
        }

        //独立声明，不在函数内部
        private bool[] channelEnableFlags = new bool[4];
        public void Step(int clockCyclesInStep)
        {
            if (!isSoundHwEnabled) return;

            sampleCycleCount += clockCyclesInStep;
            frameCycleCount += clockCyclesInStep;

            for (int i = 0; i < clockCyclesInStep; i++)
            {
                frameSequencerCounter--;
                if (frameSequencerCounter == 0)
                {
                    frameSequencerCounter = frameSequencerReload;

                    switch (frameSequencer)
                    {
                        case 0:
                            channel1.LengthCounterClock();
                            channel2.LengthCounterClock();
                            channel3.LengthCounterClock();
                            channel4.LengthCounterClock();
                            break;

                        case 1:
                            break;

                        case 2:
                            channel1.SweepClock();
                            channel1.LengthCounterClock();
                            channel2.LengthCounterClock();
                            channel3.LengthCounterClock();
                            channel4.LengthCounterClock();
                            break;

                        case 3:
                            break;

                        case 4:
                            channel1.LengthCounterClock();
                            channel2.LengthCounterClock();
                            channel3.LengthCounterClock();
                            channel4.LengthCounterClock();
                            break;

                        case 5:
                            break;

                        case 6:
                            channel1.SweepClock();
                            channel1.LengthCounterClock();
                            channel2.LengthCounterClock();
                            channel3.LengthCounterClock();
                            channel4.LengthCounterClock();
                            break;

                        case 7:
                            channel1.VolumeEnvelopeClock();
                            channel2.VolumeEnvelopeClock();
                            channel4.VolumeEnvelopeClock();
                            break;
                    }

                    frameSequencer++;
                    if (frameSequencer >= 8)
                        frameSequencer = 0;
                }

                channel1.Step();
                channel2.Step();
                channel3.Step();
                channel4.Step();
            }

            if (sampleCycleCount >= cyclesPerSample)
            {
                GenerateSample();

                sampleCycleCount -= cyclesPerSample;
            }

            //if (mixedSampleBuffer.Count >= (samplesPerFrame * numOutputChannels))
            if (mixedSampleBuffer_writePos >= (samplesPerFrame * numOutputChannels))
            {
                //EnqueueSamplesEventArgs eventArgs = EnqueueSamplesEventArgs.Create(
                //    numChannels,
                //    channelSampleBuffer.Select(x => x.ToArray()).ToArray(),
                //    new bool[] { !channel1ForceEnable, !channel2ForceEnable, !channel3ForceEnable, !channel4ForceEnable },
                //    mixedSampleBuffer.ToArray());

                //有GC
                //EnqueueSamplesEventArgs eventArgs = EnqueueSamplesEventArgs.Create(
                //    numChannels,
                //    channelSampleBuffer,
                //    new bool[] { !channel1ForceEnable, !channel2ForceEnable, !channel3ForceEnable, !channel4ForceEnable },
                //    mixedSampleBuffer,
                //    mixedSampleBuffer_writePos);


                // 在函数中使用
                channelEnableFlags[0] = !channel1ForceEnable;
                channelEnableFlags[1] = !channel2ForceEnable;
                channelEnableFlags[2] = !channel3ForceEnable;
                channelEnableFlags[3] = !channel4ForceEnable;

                EnqueueSamplesEventArgs eventArgs = EnqueueSamplesEventArgs.Create(
                    numChannels,
                    channelSampleBuffer,
                    channelEnableFlags,
                    mixedSampleBuffer,
                    mixedSampleBuffer_writePos);

                OnEnqueueSamples(eventArgs);

                FlushSamples();

                eventArgs.Release();

            }

            if (frameCycleCount >= cyclesPerFrame)
            {
                frameCycleCount -= cyclesPerFrame;
                sampleCycleCount = frameCycleCount;
            }
        }

        protected virtual void GenerateSample()
        {
            for (int i = 0; i < numOutputChannels; i++)
            {
                /* Generate samples */
                var ch1 = (short)(((channel1Enable[i] ? channel1.OutputVolume : 0) * (volumeRightLeft[i] + 1)) << 8);
                var ch2 = (short)(((channel2Enable[i] ? channel2.OutputVolume : 0) * (volumeRightLeft[i] + 1)) << 8);
                var ch3 = (short)(((channel3Enable[i] ? channel3.OutputVolume : 0) * (volumeRightLeft[i] + 1)) << 8);
                var ch4 = (short)(((channel4Enable[i] ? channel4.OutputVolume : 0) * (volumeRightLeft[i] + 1)) << 8);

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

        public void FlushSamples()
        {
            //for (int i = 0; i < numChannels; i++)
            //    channelSampleBuffer[i].Clear();
            channelSampleBuffer_writePos = 0;

            //mixedSampleBuffer.Clear();
            mixedSampleBuffer_writePos = 0;
        }

        public virtual byte ReadPort(byte port)
        {
            // Channels
            if (port >= 0x10 && port <= 0x14)
                return channel1.ReadPort((byte)(port - 0x10));
            else if (port >= 0x15 && port <= 0x19)
                return channel2.ReadPort((byte)(port - 0x15));
            else if (port >= 0x1A && port <= 0x1E)
                return channel3.ReadPort((byte)(port - 0x1A));
            else if (port >= 0x1F && port <= 0x23)
                return channel4.ReadPort((byte)(port - 0x1F));

            // Channel 3 Wave RAM
            else if (port >= 0x30 && port <= 0x3F)
                return channel3.ReadWaveRam((byte)(port - 0x30));

            // Control ports
            else
                switch (port)
                {
                    case 0x24:
                        return (byte)(
                            (vinEnableRightLeft[1] ? (1 << 7) : 0) |
                            (volumeRightLeft[1] << 4) |
                            (vinEnableRightLeft[0] ? (1 << 3) : 0) |
                            (volumeRightLeft[0] << 0));

                    case 0x25:
                        return (byte)(
                            (channel4Enable[1] ? (1 << 7) : 0) |
                            (channel3Enable[1] ? (1 << 6) : 0) |
                            (channel2Enable[1] ? (1 << 5) : 0) |
                            (channel1Enable[1] ? (1 << 4) : 0) |
                            (channel4Enable[0] ? (1 << 3) : 0) |
                            (channel3Enable[0] ? (1 << 2) : 0) |
                            (channel2Enable[0] ? (1 << 1) : 0) |
                            (channel1Enable[0] ? (1 << 0) : 0));

                    case 0x26:
                        return (byte)(
                            0x70 |
                            (isSoundHwEnabled ? (1 << 7) : 0) |
                            (channel4.IsActive ? (1 << 3) : 0) |
                            (channel3.IsActive ? (1 << 2) : 0) |
                            (channel2.IsActive ? (1 << 1) : 0) |
                            (channel1.IsActive ? (1 << 0) : 0));

                    default:
                        return 0xFF;
                }
        }

        public virtual void WritePort(byte port, byte value)
        {
            // Channels
            if (port >= 0x10 && port <= 0x14)
                channel1.WritePort((byte)(port - 0x10), value);
            else if (port >= 0x15 && port <= 0x19)
                channel2.WritePort((byte)(port - 0x15), value);
            else if (port >= 0x1A && port <= 0x1E)
                channel3.WritePort((byte)(port - 0x1A), value);
            else if (port >= 0x1F && port <= 0x23)
                channel4.WritePort((byte)(port - 0x1F), value);

            // Channel 3 Wave RAM
            else if (port >= 0x30 && port <= 0x3F)
                channel3.WriteWaveRam((byte)(port - 0x30), value);

            // Control ports
            else
                switch (port)
                {
                    case 0x24:
                        vinEnableRightLeft[1] = ((value >> 7) & 0b1) == 0b1;
                        volumeRightLeft[1] = (byte)((value >> 4) & 0b111);
                        vinEnableRightLeft[0] = ((value >> 3) & 0b1) == 0b1;
                        volumeRightLeft[0] = (byte)((value >> 0) & 0b111);
                        break;

                    case 0x25:
                        channel4Enable[1] = ((value >> 7) & 0b1) == 0b1;
                        channel3Enable[1] = ((value >> 6) & 0b1) == 0b1;
                        channel2Enable[1] = ((value >> 5) & 0b1) == 0b1;
                        channel1Enable[1] = ((value >> 4) & 0b1) == 0b1;
                        channel4Enable[0] = ((value >> 3) & 0b1) == 0b1;
                        channel3Enable[0] = ((value >> 2) & 0b1) == 0b1;
                        channel2Enable[0] = ((value >> 1) & 0b1) == 0b1;
                        channel1Enable[0] = ((value >> 0) & 0b1) == 0b1;
                        break;

                    case 0x26:
                        isSoundHwEnabled = ((value >> 7) & 0b1) == 0b1;
                        break;
                }
        }
    }
}
