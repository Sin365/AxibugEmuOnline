using Codice.CM.Client.Differences;
using System;
using RECTANGLE = VirtualNes.Core.APU_VRC6.RECTANGLE;
using SAWTOOTH = VirtualNes.Core.APU_VRC6.SAWTOOTH;

namespace VirtualNes.Core
{
    public class APU_N106 : APU_INTERFACE
    {
        CHANNEL[] op = new CHANNEL[8];

        const int CHANNEL_VOL_SHIFT = 6;
        float cpu_clock;
        uint cycle_rate;

        byte addrinc;
        byte address;
        byte channel_use;

        byte[] tone = new byte[0x100];

        public APU_N106()
        {
            // 仮設定
            cpu_clock = APU_CLOCK;
            cycle_rate = (uint)(cpu_clock * 12.0f * (1 << 20) / (45.0f * 22050.0f));
        }

        public override void Reset(float fClock, int nRate)
        {
            for (int i = 0; i < 8; i++)
            {
                op[i].ZeroMemory();
                op[i].tonelen = 0x10 << 18;
            }

            address = 0;
            addrinc = 1;
            channel_use = 8;

            Setup(fClock, nRate);

            // TONEの初期化はしない...
        }

        public override void Setup(float fClock, int nRate)
        {
            cpu_clock = fClock;
            cycle_rate = (uint)(cpu_clock * 12.0f * (1 << 20) / (45.0f * nRate));
        }

        public override void Write(ushort addr, byte data)
        {
            if (addr == 0x4800)
            {
                //		tone[address*2+0] = (INT)(data&0x0F);
                //		tone[address*2+1] = (INT)(data  >>4);
                tone[address * 2 + 0] = (byte)(data & 0x0F);
                tone[address * 2 + 1] = (byte)(data >> 4);

                if (address >= 0x40)
                {
                    int no = (address - 0x40) >> 3;
                    uint tonelen = 0;
                    ref CHANNEL ch = ref op[no];

                    switch (address & 7)
                    {
                        case 0x00:
                            ch.freq = (uint)((ch.freq & ~0x000000FF) | data);
                            break;
                        case 0x02:
                            ch.freq = (uint)((ch.freq & ~0x0000FF00) | ((uint)data << 8));
                            break;
                        case 0x04:
                            ch.freq = (uint)((ch.freq & ~0x00030000) | (((uint)data & 0x03) << 16));
                            tonelen = (uint)((0x20 - (data & 0x1c)) << 18);
                            ch.databuf = (byte)((data & 0x1c) >> 2);
                            if (ch.tonelen != tonelen)
                            {
                                ch.tonelen = tonelen;
                                ch.phase = 0;
                            }
                            break;
                        case 0x06:
                            ch.toneadr = data;
                            break;
                        case 0x07:
                            ch.vol = (byte)(data & 0x0f);
                            ch.volupdate = 0xFF;
                            if (no == 7)
                                channel_use = (byte)(((data >> 4) & 0x07) + 1);
                            break;
                    }
                }

                if (addrinc != 0)
                {
                    address = (byte)((address + 1) & 0x7f);
                }
            }
            else if (addr == 0xF800)
            {
                address = (byte)(data & 0x7F);
                addrinc = (byte)(data & 0x80);
            }
        }

        public override byte Read(ushort addr)
        {
            // $4800 dummy read!!
            if (addr == 0x0000)
            {
                if (addrinc != 0)
                {
                    address = (byte)((address + 1) & 0x7F);
                }
            }

            return (byte)(addr >> 8);
        }

        public override int Process(int channel)
        {
            if (channel >= (8 - channel_use) && channel < 8)
            {
                return ChannelRender(ref op[channel]);
            }

            return 0;
        }

        public override int GetFreq(int channel)
        {
            if (channel < 8)
            {
                channel &= 7;
                if (channel < (8 - channel_use))
                    return 0;

                ref CHANNEL ch = ref op[channel & 0x07];
                if (ch.freq == 0 || ch.vol == 0)
                    return 0;
                int temp = channel_use * (8 - ch.databuf) * 4 * 45;
                if (temp == 0)
                    return 0;
                return (int)(256.0 * (double)cpu_clock * 12.0 * ch.freq / ((double)0x40000 * temp));
            }

            return 0;
        }

        private int ChannelRender(ref CHANNEL ch)
        {
            uint phasespd = (uint)(channel_use << 20);

            ch.phaseacc -= (int)cycle_rate;
            if (ch.phaseacc >= 0)
            {
                if (ch.volupdate != 0)
                {
                    ch.output = (tone[((ch.phase >> 18) + ch.toneadr) & 0xFF] * ch.vol) << CHANNEL_VOL_SHIFT;
                    ch.volupdate = 0;
                }
                return ch.output;
            }

            while (ch.phaseacc < 0)
            {
                ch.phaseacc += (int)phasespd;
                ch.phase += ch.freq;
            }
            while (ch.tonelen != 0 && (ch.phase >= ch.tonelen))
            {
                ch.phase -= ch.tonelen;
            }

            ch.output = (tone[((ch.phase >> 18) + ch.toneadr) & 0xFF] * ch.vol) << CHANNEL_VOL_SHIFT;

            return ch.output;
        }

        public override uint GetSize()
        {
            return (uint)(3 * sizeof(byte) + 8 * op[0].GetSize() + tone.Length);
        }

        public override void SaveState(StateBuffer buffer)
        {
            buffer.Write(addrinc);
            buffer.Write(address);
            buffer.Write(channel_use);

            foreach (var oneOp in op)
                oneOp.SaveState(buffer);

            buffer.Write(tone);
        }

        public struct CHANNEL : IStateBufferObject
        {
            public int phaseacc;

            public uint freq;
            public uint phase;
            public uint tonelen;

            public int output;

            public byte toneadr;
            public byte volupdate;

            public byte vol;
            public byte databuf;

            internal void ZeroMemory()
            {
                phaseacc = 0;
                freq = 0;
                phase = 0;
                tonelen = 0;
                output = 0;
                toneadr = 0;
                volupdate = 0;
                vol = 0;
                databuf = 0;
            }

            public uint GetSize()
            {
                return 4 * 5 + 4;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(phaseacc);
                buffer.Write(freq);
                buffer.Write(phase);
                buffer.Write(tonelen);
                buffer.Write(output);
                buffer.Write(toneadr);
                buffer.Write(volupdate);
                buffer.Write(vol);
                buffer.Write(databuf);
            }
        }
    }
}
