using System;

namespace Essgee.Emulation.Audio
{
    public partial class DMGAudio
    {
        public class Wave : IDMGAudioChannel
        {
            // NR30
            bool isDacEnabled;

            // NR31
            byte lengthLoad;

            // NR32
            byte volumeCode;

            // NR33
            byte frequencyLSB;

            // NR34
            bool trigger, lengthEnable;
            byte frequencyMSB;

            // Wave
            protected byte[] sampleBuffer;
            int frequencyCounter, positionCounter, volume;

            // Misc
            bool isChannelEnabled;
            int lengthCounter;

            public int OutputVolume { get; private set; }

            public bool IsActive { get { return isDacEnabled; } }   // TODO: correct? lengthCounter check makes Zelda Oracle games hang

            public Wave()
            {
                sampleBuffer = new byte[16];
            }

            public virtual void Reset()
            {
                for (var i = 0; i < sampleBuffer.Length; i++) sampleBuffer[i] = (byte)EmuStandInfo.Random.Next(255);
                frequencyCounter = positionCounter = 0;
                volume = 15;

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
                throw new Exception("Channel type does not support envelope");
            }

            public void Step()
            {
                if (!isChannelEnabled) return;

                frequencyCounter--;
                if (frequencyCounter == 0)
                {
                    frequencyCounter = (2048 - ((frequencyMSB << 8) | frequencyLSB)) * 2;
                    positionCounter++;
                    positionCounter %= 32;

                    var value = sampleBuffer[positionCounter / 2];
                    if ((positionCounter & 0b1) == 0) value >>= 4;
                    value &= 0b1111;

                    if (volumeCode != 0)
                        volume = value >> (volumeCode - 1);
                    else
                        volume = 0;
                }

                OutputVolume = isDacEnabled ? volume : 0;
            }

            private void Trigger()
            {
                isChannelEnabled = true;

                if (lengthCounter == 0) lengthCounter = 256;

                frequencyCounter = (2048 - ((frequencyMSB << 8) | frequencyLSB)) * 2;
                positionCounter = 0;
            }

            public void WritePort(byte port, byte value)
            {
                switch (port)
                {
                    case 0:
                        isDacEnabled = ((value >> 7) & 0b1) == 0b1;
                        break;

                    case 1:
                        lengthLoad = value;

                        lengthCounter = 256 - lengthLoad;
                        break;

                    case 2:
                        volumeCode = (byte)((value >> 5) & 0b11);
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
                        return (byte)(
                            0x7F |
                            (isDacEnabled ? (1 << 7) : 0));

                    case 1:
                        return 0xFF;

                    case 2:
                        return (byte)(
                            0x9F |
                            (volumeCode << 5));

                    case 4:
                        return (byte)(
                            0xBF |
                            (lengthEnable ? (1 << 6) : 0));

                    default:
                        return 0xFF;
                }
            }

            // TODO: more details on behavior on access w/ channel enabled

            public void WriteWaveRam(byte offset, byte value)
            {
                if (!isDacEnabled)
                    sampleBuffer[offset & (sampleBuffer.Length - 1)] = value;
                else
                    sampleBuffer[positionCounter & (sampleBuffer.Length - 1)] = value;
            }

            public byte ReadWaveRam(byte offset)
            {
                if (!isDacEnabled)
                    return sampleBuffer[offset & (sampleBuffer.Length - 1)];
                else
                    return 0xFF;
            }
        }
    }
}
