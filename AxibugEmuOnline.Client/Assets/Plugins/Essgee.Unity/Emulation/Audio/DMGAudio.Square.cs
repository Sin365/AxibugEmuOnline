using System;

namespace Essgee.Emulation.Audio
{
    public partial class DMGAudio
    {
        public class Square : IDMGAudioChannel
        {
            //static readonly bool[,] dutyCycleTable = new bool[,]
            //  {
            //  { false, false, false, false, false, false, false, true,  },	// 00000001    12.5%
            //	{ true,  false, false, false, false, false, false, true,  },	// 10000001    25%
            //	{ true,  false, false, false, false, true,  true,  true,  },	// 10000111    50%
            //	{ false, true,  true,  true,  true,  true,  true,  false, }		// 01111110    75%
            //};

            // 1. 初始化 - 假设原始数组是 4行 x 8列
            private const int Rows = 4;
            private const int Cols = 8;
            private readonly bool[] _dutyCycleTable1D = new bool[Rows * Cols]
            {
                // 第一行 (索引 0-7)
                false, false, false, false, false, false, false, true,
                // 第二行 (索引 8-15)
                true,  false, false, false, false, false, false, true,
                // 第三行 (索引 16-23)
                true,  false, false, false, false, true,  true,  true,
                // 第四行 (索引 24-31)
                false, true,  true,  true,  true,  true,  true,  false
            };

            // 2. 访问方法 - 替代原来的 dutyCycleTable[row, col]
            public bool GetValue(int row, int col)
            {
                // 重要的边界检查（在稳定后可通过条件编译移除以极致优化）
                // if (row < 0 || row >= Rows || col < 0 || col >= Cols) return false;
                return _dutyCycleTable1D[row * Cols + col];
            }

            // NR10/20
            byte sweepPeriodReload, sweepShift;
            bool sweepNegate;

            // NR11/21
            byte dutyCycle, lengthLoad;

            // NR12/22
            byte envelopeStartingVolume, envelopePeriodReload;
            bool envelopeAddMode;

            // NR13/23
            byte frequencyLSB;

            // NR14/24
            bool trigger, lengthEnable;
            byte frequencyMSB;

            //

            readonly bool channelSupportsSweep;

            // Sweep
            bool isSweepEnabled;
            int sweepCounter, sweepFreqShadow;

            // Frequency
            int frequencyCounter;

            // Envelope
            int volume, envelopeCounter;
            bool isEnvelopeUpdateEnabled;

            // Misc
            bool isChannelEnabled, isDacEnabled;
            int lengthCounter, dutyCounter;

            public int OutputVolume { get; private set; }

            public bool IsActive { get { return lengthCounter != 0; } }

            public Square(bool hasSweep)
            {
                channelSupportsSweep = hasSweep;
            }

            public void Reset()
            {
                isSweepEnabled = false;
                sweepCounter = sweepFreqShadow = 0;

                frequencyCounter = 0;

                volume = 15;
                envelopeCounter = 0;
                isEnvelopeUpdateEnabled = false;

                isChannelEnabled = isDacEnabled = false;
                lengthCounter = dutyCounter = 0;

                OutputVolume = volume;
            }

            public void LengthCounterClock()
            {
                if (lengthCounter > 0 && lengthEnable)
                {
                    lengthCounter--;
                    if (lengthCounter == 0)
                        isChannelEnabled = false;
                }
            }

            public void SweepClock()
            {
                if (!channelSupportsSweep) return;

                sweepCounter--;
                if (sweepCounter == 0)
                {
                    sweepCounter = sweepPeriodReload;

                    if (isSweepEnabled && sweepPeriodReload != 0)
                    {
                        var newFrequency = PerformSweepCalculations();
                        if (newFrequency <= 2047 && sweepShift != 0)
                        {
                            sweepFreqShadow = newFrequency;
                            frequencyMSB = (byte)((newFrequency >> 8) & 0b111);
                            frequencyLSB = (byte)(newFrequency & 0xFF);
                            PerformSweepCalculations();
                        }
                    }
                }
            }

            public void VolumeEnvelopeClock()
            {
                envelopeCounter--;
                if (envelopeCounter == 0)
                {
                    envelopeCounter = envelopePeriodReload;

                    if (isEnvelopeUpdateEnabled)
                    {
                        var newVolume = volume;
                        if (envelopeAddMode) newVolume++;
                        else newVolume--;

                        if (newVolume >= 0 && newVolume <= 15)
                            volume = newVolume;
                        else
                            isEnvelopeUpdateEnabled = false;
                    }
                }
            }

            public void Step()
            {
                if (!isChannelEnabled) return;

                frequencyCounter--;
                if (frequencyCounter == 0)
                {
                    frequencyCounter = (2048 - ((frequencyMSB << 8) | frequencyLSB)) * 4;
                    dutyCounter++;
                    dutyCounter %= 8;
                }

                //OutputVolume = isDacEnabled && dutyCycleTable[dutyCycle, dutyCounter] ? volume : 0;

                //改为一维数组访问
                OutputVolume = isDacEnabled && _dutyCycleTable1D[dutyCycle * Cols + dutyCounter] ? volume : 0;
            }

            private void Trigger()
            {
                isChannelEnabled = true;

                if (lengthCounter == 0) lengthCounter = 64;

                frequencyCounter = (2048 - ((frequencyMSB << 8) | frequencyLSB)) * 4;
                volume = envelopeStartingVolume;
                envelopeCounter = envelopePeriodReload;
                isEnvelopeUpdateEnabled = true;

                if (channelSupportsSweep)
                {
                    sweepFreqShadow = (frequencyMSB << 8) | frequencyLSB;
                    sweepCounter = sweepPeriodReload;
                    isSweepEnabled = sweepPeriodReload != 0 || sweepShift != 0;
                    if (sweepShift != 0)
                        PerformSweepCalculations();
                }
            }

            private int PerformSweepCalculations()
            {
                var newFrequency = sweepFreqShadow >> sweepShift;
                if (sweepNegate) newFrequency = -newFrequency;
                newFrequency += sweepFreqShadow;
                if (newFrequency > 2047) isChannelEnabled = false;
                return newFrequency;
            }

            public void WritePort(byte port, byte value)
            {
                switch (port)
                {
                    case 0:
                        if (channelSupportsSweep)
                        {
                            sweepPeriodReload = (byte)((value >> 4) & 0b111);
                            sweepNegate = ((value >> 3) & 0b1) == 0b1;
                            sweepShift = (byte)((value >> 0) & 0b111);
                        }
                        break;

                    case 1:
                        dutyCycle = (byte)((value >> 6) & 0b11);
                        lengthLoad = (byte)((value >> 0) & 0b111111);

                        lengthCounter = 64 - lengthLoad;
                        break;

                    case 2:
                        envelopeStartingVolume = (byte)((value >> 4) & 0b1111);
                        envelopeAddMode = ((value >> 3) & 0b1) == 0b1;
                        envelopePeriodReload = (byte)((value >> 0) & 0b111);

                        isDacEnabled = ((value >> 3) & 0b11111) != 0;
                        break;

                    case 3:
                        frequencyLSB = value;
                        break;

                    case 4:
                        trigger = ((value >> 7) & 0b1) == 0b1;
                        lengthEnable = ((value >> 6) & 0b1) == 0b1;
                        frequencyMSB = (byte)((value >> 0) & 0b111);

                        if (trigger) Trigger();
                        break;
                }
            }

            public byte ReadPort(byte port)
            {
                switch (port)
                {
                    case 0:
                        if (channelSupportsSweep)
                        {
                            return (byte)(
                                0x80 |
                                (sweepPeriodReload << 4) |
                                (sweepNegate ? (1 << 3) : 0) |
                                (sweepShift << 0));
                        }
                        else
                            return 0xFF;

                    case 1:
                        return (byte)(
                            0x3F |
                            (dutyCycle << 6));

                    case 2:
                        return (byte)(
                            (envelopeStartingVolume << 4) |
                            (envelopeAddMode ? (1 << 3) : 0) |
                            (envelopePeriodReload << 0));

                    case 4:
                        return (byte)(
                            0xBF |
                            (lengthEnable ? (1 << 6) : 0));

                    default:
                        return 0xFF;
                }
            }

            public void WriteWaveRam(byte offset, byte value)
            {
                throw new Exception("Channel type does have Wave RAM");
            }

            public byte ReadWaveRam(byte offset)
            {
                throw new Exception("Channel type does have Wave RAM");
            }
        }
    }
}
