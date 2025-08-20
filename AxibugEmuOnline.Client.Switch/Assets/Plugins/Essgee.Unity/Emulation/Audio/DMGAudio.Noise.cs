using System;

namespace Essgee.Emulation.Audio
{
    public partial class DMGAudio
    {
        public class Noise : IDMGAudioChannel
        {
            static readonly int[] divisors = new int[]
            {
                8, 16, 32, 48, 64, 80, 96, 112
            };

            // NR41
            byte lengthLoad;

            // NR42
            byte envelopeStartingVolume, envelopePeriodReload;
            bool envelopeAddMode;

            // NR43
            byte clockShift, divisorCode;
            bool lfsrWidthMode;

            // NR44
            bool trigger, lengthEnable;

            //

            // Noise
            int noiseCounter;
            ushort lfsr;

            // Envelope
            int volume, envelopeCounter;
            bool isEnvelopeUpdateEnabled;

            // Misc
            bool isChannelEnabled, isDacEnabled;
            int lengthCounter;

            public int OutputVolume { get; private set; }

            public bool IsActive { get { return lengthCounter != 0; } }

            public Noise()
            {
                //
            }

            public void Reset()
            {
                noiseCounter = 0;
                lfsr = 0;

                volume = 15;
                envelopeCounter = 0;
                isEnvelopeUpdateEnabled = false;

                isChannelEnabled = isDacEnabled = false;
                lengthCounter = 0;

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
                throw new Exception("Channel type does not support sweep");
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

                noiseCounter--;
                if (noiseCounter == 0)
                {
                    noiseCounter = divisors[divisorCode] << clockShift;

                    var result = (lfsr & 0b1) ^ ((lfsr >> 1) & 0b1);
                    lfsr = (ushort)((lfsr >> 1) | (result << 14));

                    if (lfsrWidthMode)
                        lfsr = (ushort)((lfsr & 0b10111111) | (result << 6));
                }

                OutputVolume = isDacEnabled && ((lfsr & 0b1) == 0) ? volume : 0;
            }

            private void Trigger()
            {
                isChannelEnabled = true;

                if (lengthCounter == 0) lengthCounter = 64;

                noiseCounter = divisors[divisorCode] << clockShift;
                volume = envelopeStartingVolume;
                envelopeCounter = envelopePeriodReload;
                isEnvelopeUpdateEnabled = true;

                lfsr = 0x7FFF;
            }

            public void WritePort(byte port, byte value)
            {
                switch (port)
                {
                    case 0:
                        break;

                    case 1:
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
                        clockShift = (byte)((value >> 4) & 0b1111);
                        lfsrWidthMode = ((value >> 3) & 0b1) == 0b1;
                        divisorCode = (byte)((value >> 0) & 0b111);
                        break;

                    case 4:
                        trigger = ((value >> 7) & 0b1) == 0b1;
                        lengthEnable = ((value >> 6) & 0b1) == 0b1;

                        if (trigger) Trigger();
                        break;
                }
            }

            public byte ReadPort(byte port)
            {
                switch (port)
                {
                    case 0:
                        return 0xFF;

                    case 1:
                        return 0xFF;

                    case 2:
                        return (byte)(
                            (envelopeStartingVolume << 4) |
                            (envelopeAddMode ? (1 << 3) : 0) |
                            (envelopePeriodReload << 0));

                    case 3:
                        return (byte)(
                            (clockShift << 4) |
                            (lfsrWidthMode ? (1 << 3) : 0) |
                            (divisorCode << 0));

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
