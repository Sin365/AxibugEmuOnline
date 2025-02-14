using Essgee.EventArguments;
using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Audio
{
    public unsafe class SN76489 : IAudio
    {
        /* http://www.smspower.org/Development/SN76489 */
        /* Differences in various system's PSGs: http://forums.nesdev.com/viewtopic.php?p=190216#p190216 */

        protected const int numChannels = 4, numToneChannels = 3, noiseChannelIndex = 3;

        protected const string channel1OptionName = "AudioEnableCh1Square";
        protected const string channel2OptionName = "AudioEnableCh2Square";
        protected const string channel3OptionName = "AudioEnableCh3Square";
        protected const string channel4OptionName = "AudioEnableCh4Noise";

        /* Noise generation constants */
        protected virtual ushort noiseLfsrMask => 0x7FFF;
        protected virtual ushort noiseTappedBits => 0x0003;     /* Bits 0 and 1 */
        protected virtual int noiseBitShift => 14;

        /* Sample generation & event handling */
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

        /* Audio output variables */
        protected int sampleRate, numOutputChannels;

        /* Channel registers */
        [StateRequired]
        protected ushort[] volumeRegisters;     /* Channels 0-3: 4 bits */
        [StateRequired]
        protected ushort[] toneRegisters;       /* Channels 0-2 (tone): 10 bits; channel 3 (noise): 3 bits */

        /* Channel counters */
        [StateRequired]
        protected ushort[] channelCounters;     /* 10-bit counters */
        [StateRequired]
        protected bool[] channelOutput;

        /* Volume attenuation table */
        protected short[] volumeTable;          /* 2dB change per volume register step */

        /* Latched channel/type */
        [StateRequired]
        byte latchedChannel, latchedType;

        /* Linear-feedback shift register, for noise generation */
        [StateRequired]
        protected ushort noiseLfsr;             /* 15-bit */

        /* Timing variables */
        double clockRate, refreshRate;
        int samplesPerFrame, cyclesPerFrame, cyclesPerSample;
        [StateRequired]
        int sampleCycleCount, frameCycleCount, dividerCount;

        /* User-facing channel toggles */
        protected bool channel1ForceEnable, channel2ForceEnable, channel3ForceEnable, channel4ForceEnable;

        public (string Name, string Description)[] RuntimeOptions => new (string name, string description)[]
        {
            (channel1OptionName, "Channel 1 (Square)"),
            (channel2OptionName, "Channel 2 (Square)"),
            (channel3OptionName, "Channel 3 (Square)"),
            (channel4OptionName, "Channel 4 (Noise)")
        };

        public SN76489()
        {
            //channelSampleBuffer = new List<short>[numChannels];
            //for (int i = 0; i < numChannels; i++) channelSampleBuffer[i] = new List<short>();
            //mixedSampleBuffer = new List<short>();

            //改为二维数组
            channelSampleBuffer_Init(numChannels, 1470);
            mixedSampleBuffer_set = new short[1470];


            volumeRegisters = new ushort[numChannels];
            toneRegisters = new ushort[numChannels];

            channelCounters = new ushort[numChannels];
            channelOutput = new bool[numChannels];

            volumeTable = new short[16];
            for (int i = 0; i < volumeTable.Length; i++)
                volumeTable[i] = (short)(short.MaxValue * Math.Pow(2.0, i * -2.0 / 6.0));
            volumeTable[15] = 0;

            samplesPerFrame = cyclesPerFrame = cyclesPerSample = -1;

            channel1ForceEnable = true;
            channel2ForceEnable = true;
            channel3ForceEnable = true;
            channel4ForceEnable = true;
        }

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
            cyclesPerFrame = (int)(clockRate / refreshRate);
            cyclesPerSample = (cyclesPerFrame / samplesPerFrame);

            FlushSamples();
        }

        public virtual void Startup()
        {
            Reset();

            if (samplesPerFrame == -1) throw new EmulationException("SN76489: Timings not configured, invalid samples per frame");
            if (cyclesPerFrame == -1) throw new EmulationException("SN76489: Timings not configured, invalid cycles per frame");
            if (cyclesPerSample == -1) throw new EmulationException("SN76489: Timings not configured, invalid cycles per sample");
        }

        public virtual void Shutdown()
        {
            //
        }

        public virtual void Reset()
        {
            FlushSamples();

            latchedChannel = latchedType = 0x00;
            noiseLfsr = 0x4000;

            for (int i = 0; i < numChannels; i++)
            {
                volumeRegisters[i] = 0x000F;
                toneRegisters[i] = 0x0000;
            }

            sampleCycleCount = frameCycleCount = dividerCount = 0;
        }

        public void Step(int clockCyclesInStep)
        {
            sampleCycleCount += clockCyclesInStep;
            frameCycleCount += clockCyclesInStep;

            for (int i = 0; i < clockCyclesInStep; i++)
            {
                dividerCount++;
                if (dividerCount == 16)
                {
                    for (int ch = 0; ch < numToneChannels; ch++)
                        StepToneChannel(ch);
                    StepNoiseChannel();

                    dividerCount = 0;
                }
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

                EnqueueSamplesEventArgs eventArgs = EnqueueSamplesEventArgs.Create(
                    numChannels,
                    channelSampleBuffer,
                    new bool[] { !channel1ForceEnable, !channel2ForceEnable, !channel3ForceEnable, !channel4ForceEnable },
                    mixedSampleBuffer,
                    mixedSampleBufferLength);

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

        private void StepToneChannel(int ch)
        {
            /* Check for counter underflow */
            if ((channelCounters[ch] & 0x03FF) > 0)
                channelCounters[ch]--;

            /* Counter underflowed, reload and flip output bit, then generate sample */
            if ((channelCounters[ch] & 0x03FF) == 0)
            {
                channelCounters[ch] = (ushort)(toneRegisters[ch] & 0x03FF);
                channelOutput[ch] = !channelOutput[ch];
            }
        }

        private void StepNoiseChannel()
        {
            int chN = noiseChannelIndex;
            {
                /* Check for counter underflow */
                if ((channelCounters[chN] & 0x03FF) > 0)
                    channelCounters[chN]--;

                /* Counter underflowed, reload and flip output bit */
                if ((channelCounters[chN] & 0x03FF) == 0)
                {
                    switch (toneRegisters[chN] & 0x3)
                    {
                        case 0x0: channelCounters[chN] = 0x10; break;
                        case 0x1: channelCounters[chN] = 0x20; break;
                        case 0x2: channelCounters[chN] = 0x40; break;
                        case 0x3: channelCounters[chN] = (ushort)(toneRegisters[2] & 0x03FF); break;
                    }
                    channelOutput[chN] = !channelOutput[chN];

                    if (channelOutput[chN])
                    {
                        /* Check noise type, then generate sample */
                        bool isWhiteNoise = (((toneRegisters[chN] >> 2) & 0x1) == 0x1);

                        ushort newLfsrBit = (ushort)((isWhiteNoise ? CheckParity((ushort)(noiseLfsr & noiseTappedBits)) : (noiseLfsr & 0x01)) << noiseBitShift);

                        noiseLfsr = (ushort)((newLfsrBit | (noiseLfsr >> 1)) & noiseLfsrMask);
                    }
                }
            }
        }

        protected virtual void GenerateSample()
        {
            for (int i = 0; i < numOutputChannels; i++)
            {
                /* Generate samples */
                var ch1 = (short)(volumeTable[volumeRegisters[0]] * ((toneRegisters[0] < 2 ? true : channelOutput[0]) ? 1.0 : 0.0));
                var ch2 = (short)(volumeTable[volumeRegisters[1]] * ((toneRegisters[1] < 2 ? true : channelOutput[1]) ? 1.0 : 0.0));
                var ch3 = (short)(volumeTable[volumeRegisters[2]] * ((toneRegisters[2] < 2 ? true : channelOutput[2]) ? 1.0 : 0.0));
                var ch4 = (short)(volumeTable[volumeRegisters[3]] * (noiseLfsr & 0x1));

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

        private ushort CheckParity(ushort val)
        {
            val ^= (ushort)(val >> 8);
            val ^= (ushort)(val >> 4);
            val ^= (ushort)(val >> 2);
            val ^= (ushort)(val >> 1);
            return (ushort)(val & 0x1);
        }

        public virtual byte ReadPort(byte port)
        {
            throw new EmulationException("SN76489: Cannot read ports");
        }

        public virtual void WritePort(byte port, byte data)
        {
            if (IsBitSet(data, 7))
            {
                /* LATCH/DATA byte; get channel (0-3) and type (0 is tone/noise, 1 is volume) */
                latchedChannel = (byte)((data >> 5) & 0x03);
                latchedType = (byte)((data >> 4) & 0x01);

                /* Mask off non-data bits */
                data &= 0x0F;

                /* If target is channel 3 noise (3 bits), mask off highest bit */
                if (latchedChannel == 3 && latchedType == 0)
                    data &= 0x07;

                /* Write to register */
                if (latchedType == 0)
                {
                    /* Data is tone/noise */
                    toneRegisters[latchedChannel] = (ushort)((toneRegisters[latchedChannel] & 0x03F0) | data);
                }
                else
                {
                    /* Data is volume */
                    volumeRegisters[latchedChannel] = data;
                }
            }
            else
            {
                /* DATA byte; mask off non-data bits */
                data &= 0x3F;

                /* Write to register */
                if (latchedType == 0)
                {
                    /* Data is tone/noise */
                    if (latchedChannel == 3)
                    {
                        /* Target is channel 3 noise, mask off excess bits and write to low bits of register */
                        toneRegisters[latchedChannel] = (ushort)(data & 0x07);
                    }
                    else
                    {
                        /* Target is not channel 3 noise, write to high bits of register */
                        toneRegisters[latchedChannel] = (ushort)((toneRegisters[latchedChannel] & 0x000F) | (data << 4));
                    }
                }
                else
                {
                    /* Data is volume; mask off excess bits and write to low bits of register */
                    volumeRegisters[latchedChannel] = (ushort)(data & 0x0F);
                }
            }
        }
    }
}
