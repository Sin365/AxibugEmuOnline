using System;
using static VirtualNes.Core.APU_FME7;

namespace VirtualNes.Core
{
    public class APU_FME7 : APU_INTERFACE
    {
        // Envelope tables
        byte[] envelope_pulse0 = {
            0x1F, 0x1E, 0x1D, 0x1C, 0x1B, 0x1A, 0x19, 0x18,
            0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10,
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08,
            0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
        };
        byte[] envelope_pulse1 = {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x00
        };
        byte[] envelope_pulse2 = {
            0x1F, 0x1E, 0x1D, 0x1C, 0x1B, 0x1A, 0x19, 0x18,
            0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10,
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08,
            0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x1F
        };
        byte[] envelope_pulse3 = {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x1F
        };
        sbyte[] envstep_pulse = {
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 0
        };

        byte[] envelope_sawtooth0 = {
            0x1F, 0x1E, 0x1D, 0x1C, 0x1B, 0x1A, 0x19, 0x18,
            0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10,
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08,
            0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
        };
        byte[] envelope_sawtooth1 = {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
        };
        sbyte[] envstep_sawtooth = {
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, -15
        };

        byte[] envelope_triangle0 = {
            0x1F, 0x1E, 0x1D, 0x1C, 0x1B, 0x1A, 0x19, 0x18,
            0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10,
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08,
            0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
        };
        byte[] envelope_triangle1 = {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x1F, 0x1E, 0x1D, 0x1C, 0x1B, 0x1A, 0x19, 0x18,
            0x17, 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10,
            0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09, 0x08,
            0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
        };
        sbyte[] envstep_triangle = {
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, -31
        };

        byte[][] envelope_table;
        sbyte[][] envstep_table;

        ENVELOPE envelope;
        NOISE noise = new NOISE();
        CHANNEL[] op = new CHANNEL[3] { new CHANNEL(), new CHANNEL(), new CHANNEL() };
        byte address;
        int[] vol_table = new int[0x20];
        int cycle_rate;

        float cpu_clock;

        public APU_FME7()
        {
            envelope_table = new byte[16][]
            {
                envelope_pulse0,    envelope_pulse0, envelope_pulse0,    envelope_pulse0,
                envelope_pulse1,    envelope_pulse1, envelope_pulse1,    envelope_pulse1,
                envelope_sawtooth0, envelope_pulse0, envelope_triangle0, envelope_pulse2,
                envelope_sawtooth1, envelope_pulse3, envelope_triangle1, envelope_pulse1
            };

            envstep_table = new sbyte[16][]
            {
                envstep_pulse,    envstep_pulse, envstep_pulse,    envstep_pulse,
                envstep_pulse,    envstep_pulse, envstep_pulse,    envstep_pulse,
                envstep_sawtooth, envstep_pulse, envstep_triangle, envstep_pulse,
                envstep_sawtooth, envstep_pulse, envstep_triangle, envstep_pulse
            };
            envelope = new ENVELOPE(envelope_table, envstep_table);

            Reset(APU_CLOCK, 22050);
        }

        public override void Reset(float fClock, int nRate)
        {
            int i;

            envelope.ZeroMemory();
            noise.ZeroMemory();

            foreach (var item in op)
            {
                item.ZeroMemory();
            }

            envelope.envtbl_index = 0;
            envelope.envstep_index = 0;

            noise.noiserange = 1;
            noise.noiseout = 0xFF;

            address = 0;

            // Volume to voltage
            double @out = 0x1FFF;
            for (i = 31; i > 1; i--)
            {
                vol_table[i] = (int)(@out + 0.5);
                @out /= 1.188502227; /* = 10 ^ (1.5/20) = 1.5dB */
            }
            vol_table[1] = 0;
            vol_table[0] = 0;

            Setup(fClock, nRate);
        }

        public override void Setup(float fClock, int nRate)
        {
            cpu_clock = fClock;
            cycle_rate = (int)((fClock / 16.0f) * (1 << 16) / nRate);
        }

        public override void Write(ushort addr, byte data)
        {
            if (addr == 0xC000)
            {
                address = data;
            }
            else if (addr == 0xE000)
            {
                byte chaddr = address;
                switch (chaddr)
                {
                    case 0x00:
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                    case 0x05:
                        {
                            CHANNEL ch = op[chaddr >> 1];
                            ch.reg[chaddr & 0x01] = data;
                            ch.freq = INT2FIX(((ch.reg[1] & 0x0F) << 8) + ch.reg[0] + 1);
                        }
                        break;
                    case 0x06:
                        noise.freq = INT2FIX((data & 0x1F) + 1);
                        break;
                    case 0x07:
                        {
                            for (byte i = 0; i < 3; i++)
                            {
                                op[i].enable = (byte)(data & (1 << i));
                                op[i].noise_on = (byte)(data & (8 << i));
                            }
                        }
                        break;
                    case 0x08:
                    case 0x09:
                    case 0x0A:
                        {
                            CHANNEL ch = op[chaddr & 3];
                            ch.reg[2] = data;
                            ch.env_on = (byte)(data & 0x10);
                            ch.volume = (byte)((data & 0x0F) * 2);
                        }
                        break;
                    case 0x0B:
                    case 0x0C:
                        envelope.reg[chaddr - 0x0B] = data;
                        envelope.freq = INT2FIX(((envelope.reg[1] & 0x0F) << 8) + envelope.reg[0] + 1);
                        break;
                    case 0x0D:
                        envelope.envtbl_index = (byte)(data & 0x0F);
                        envelope.envstep_index = (byte)(data & 0x0F);
                        envelope.envadr = 0;
                        break;
                }
            }
        }

        public override int Process(int channel)
        {
            if (channel < 3)
            {
                return ChannelRender(op[channel]);
            }
            else if (channel == 3)
            {
                // 必ずch3を1回呼んでからch0-2を呼ぶ事
                EnvelopeRender();
                NoiseRender();
            }

            return 0;
        }

        public override int GetFreq(int channel)
        {
            if (channel < 3)
            {
                CHANNEL ch = op[channel];

                if (ch.enable != 0 || ch.freq == 0)
                    return 0;
                if (ch.env_on != 0)
                {
                    if (envelope.volume == 0)
                        return 0;
                }
                else
                {
                    if (ch.volume == 0)
                        return 0;
                }

                return (int)(256.0f * cpu_clock / (FIX2INT(ch.freq) * 16.0f));
            }

            return 0;
        }

        void EnvelopeRender()
        {
            if (envelope.freq == 0)
                return;
            envelope.phaseacc -= cycle_rate;
            if (envelope.phaseacc >= 0)
                return;
            while (envelope.phaseacc < 0)
            {
                envelope.phaseacc += envelope.freq;
                envelope.envadr += envelope.envstep[envelope.envadr];
            }
            envelope.volume = envelope.envtbl[envelope.envadr];
        }
        void NoiseRender()
        {
            if (noise.freq == 0)
                return;
            noise.phaseacc -= cycle_rate;
            if (noise.phaseacc >= 0)
                return;
            while (noise.phaseacc < 0)
            {
                noise.phaseacc += noise.freq;
                if (((noise.noiserange + 1) & 0x02) != 0)
                    noise.noiseout = (byte)(~noise.noiseout);
                if ((noise.noiserange & 0x01) != 0)
                    noise.noiserange ^= 0x28000;
                noise.noiserange >>= 1;
            }
        }

        int ChannelRender(CHANNEL ch)
        {
            int output, volume;

            if (ch.enable != 0)
                return 0;
            if (ch.freq == 0)
                return 0;

            ch.phaseacc -= cycle_rate;
            while (ch.phaseacc < 0)
            {
                ch.phaseacc += ch.freq;
                ch.adder++;
            }

            output = volume = 0;
            volume = ch.env_on != 0 ? vol_table[envelope.volume] : vol_table[ch.volume + 1];

            if ((ch.adder & 0x01) != 0)
            {
                output += volume;
            }
            else
            {
                output -= volume;
            }
            if (ch.noise_on == 0)
            {
                if (noise.noiseout != 0)
                    output += volume;
                else
                    output -= volume;
            }

            ch.output_vol = output;

            return ch.output_vol;
        }

        public override uint GetSize()
        {
            return (uint)(1 + envelope.GetSize() + noise.GetSize() + op[0].GetSize() * op.Length);
        }

        public override void SaveState(StateBuffer buffer)
        {
            buffer.Write(address);

            envelope.SaveState(buffer);
            noise.SaveState(buffer);
            foreach (var oneOp in op)
                oneOp.SaveState(buffer);
        }

        public class ENVELOPE : IStateBufferObject
        {
            public byte[] reg = new byte[3];
            public byte volume;

            public int freq;
            public int phaseacc;
            public int envadr;

            public byte envtbl_index;
            public byte envstep_index;

            byte[][] ref_envtbl;
            sbyte[][] ref_envstep;
            public byte[] envtbl => ref_envtbl[envtbl_index];
            public sbyte[] envstep => ref_envstep[envstep_index];
            public ENVELOPE(byte[][] envtbl, sbyte[][] envstep)
            {
                ref_envtbl = envtbl;
                ref_envstep = envstep;
            }


            public void ZeroMemory()
            {
                Array.Clear(reg, 0, 3);
                volume = 0;
                freq = 0;
                phaseacc = 0;
                envadr = 0;
                envtbl_index = 0;
                envstep_index = 0;
            }

            public uint GetSize()
            {
                return 18;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(reg);
                buffer.Write(volume);
                buffer.Write(freq);
                buffer.Write(phaseacc);
                buffer.Write(envadr);
                buffer.Write(envtbl_index);
                buffer.Write(envstep_index);
            }

            public void LoadState(StateReader buffer)
            {
                reg = buffer.Read_bytes(3);
                volume = buffer.Read_byte();
                freq = buffer.Read_int();
                phaseacc = buffer.Read_int();
                envadr = buffer.Read_int();
                envtbl_index = buffer.Read_byte();
                envstep_index = buffer.Read_byte();
            }
        }

        public class NOISE : IStateBufferObject
        {
            public int freq;
            public int phaseacc;
            public int noiserange;
            public byte noiseout;

            public void ZeroMemory()
            {
                freq = 0; phaseacc = 0; noiserange = 0; noiseout = 0;
            }

            public uint GetSize()
            {
                return 13;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(freq);
                buffer.Write(phaseacc);
                buffer.Write(noiserange);
                buffer.Write(noiseout);
            }

            public void LoadState(StateReader buffer)
            {
                freq = buffer.Read_int();
                phaseacc = buffer.Read_int();
                noiserange = buffer.Read_int();
                noiseout = buffer.Read_byte();
            }
        }

        public class CHANNEL : IStateBufferObject
        {
            public byte[] reg = new byte[3];
            public byte enable;
            public byte env_on;
            public byte noise_on;
            public byte adder;
            public byte volume;

            public int freq;
            public int phaseacc;

            public int output_vol;

            public void ZeroMemory()
            {
                Array.Clear(reg, 0, reg.Length);
                enable = 0;
                env_on = 0;
                noise_on = 0;
                adder = 0;
                volume = 0; freq = 0;
                phaseacc = 0;
                output_vol = 0;
            }

            public uint GetSize()
            {
                return 20;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(reg);
                buffer.Write(enable);
                buffer.Write(env_on);
                buffer.Write(noise_on);
                buffer.Write(adder);
                buffer.Write(volume);
                buffer.Write(freq);
                buffer.Write(phaseacc);
                buffer.Write(output_vol);
            }

            public void LoadState(StateReader buffer)
            {
                reg = buffer.Read_bytes(3);
                enable = buffer.Read_byte();
                env_on = buffer.Read_byte();
                noise_on = buffer.Read_byte();
                adder = buffer.Read_byte();
                volume = buffer.Read_byte();
                freq = buffer.Read_int();
                phaseacc = buffer.Read_int();
                output_vol = buffer.Read_int();
            }
        }
    }
}
