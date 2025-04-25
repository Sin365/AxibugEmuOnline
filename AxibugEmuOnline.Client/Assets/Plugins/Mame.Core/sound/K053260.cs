
namespace MAME.Core
{
    public unsafe class K053260
    {
        public struct k053260_channel_def
        {
            public uint rate;
            public uint size;
            public uint start;
            public uint bank;
            public uint volume;
            public int play;
            public uint pan;
            public uint pos;
            public int loop;
            public int ppcm;
            public int ppcm_data;
        };
        public struct k053260_chip_def
        {
            public int mode;
            public int[] regs;
            public int rom_size;
            public uint[] delta_table;
            public k053260_channel_def[] channels;
        };
        public static byte[] k053260rom;
        public static k053260_chip_def ic1;
        public static void InitDeltaTable(int rate, int clock)
        {
            int i;
            double base1 = (double)rate;
            double max = (double)(clock);
            uint val;
            for (i = 0; i < 0x1000; i++)
            {
                double v = (double)(0x1000 - i);
                double target = (max) / v;
                double fixed1 = (double)(1 << 16);
                if ((target != 0) && (base1 != 0))
                {
                    target = fixed1 / (base1 / target);
                    val = (uint)target;
                    if (val == 0)
                    {
                        val = 1;
                    }
                }
                else
                {
                    val = 1;
                }
                ic1.delta_table[i] = val;
            }
        }
        public static void k053260_reset()
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                ic1.channels[i].rate = 0;
                ic1.channels[i].size = 0;
                ic1.channels[i].start = 0;
                ic1.channels[i].bank = 0;
                ic1.channels[i].volume = 0;
                ic1.channels[i].play = 0;
                ic1.channels[i].pan = 0;
                ic1.channels[i].pos = 0;
                ic1.channels[i].loop = 0;
                ic1.channels[i].ppcm = 0;
                ic1.channels[i].ppcm_data = 0;
            }
        }
        public static byte k053260_read(int chip, int offset)
        {
            byte result = 0;
            switch (offset)
            {
                case 0x29:
                    {
                        int i, status = 0;
                        for (i = 0; i < 4; i++)
                        {
                            status |= ic1.channels[i].play << i;
                        }
                        result = (byte)status;
                    }
                    break;

                case 0x2e: /* read rom */
                    if ((ic1.mode & 1) != 0)
                    {
                        int offs = (int)(ic1.channels[0].start + (ic1.channels[0].pos >> 16) + (ic1.channels[0].bank << 16));
                        ic1.channels[0].pos += (1 << 16);
                        if (offs > ic1.rom_size)
                        {
                            result = 0;
                        }
                        result = k053260rom[offs];
                    }
                    break;
                default:
                    result = (byte)ic1.regs[offset];
                    break;
            }
            return result;
        }
        public static byte k053260_0_r(int offset)
        {
            return k053260_read(0, offset);
        }
        public static void k053260_update(int offset, int length)
        {
            long[] dpcmcnv = new long[] { 0, 1, 2, 4, 8, 16, 32, 64, -128, -64, -32, -16, -8, -4, -2, -1 };
            int i, j;
            uint[] rom_offset;
            int[] lvol, rvol, play, loop, ppcm_data, ppcm;
            rom_offset = new uint[4];
            lvol = new int[4];
            rvol = new int[4];
            play = new int[4];
            loop = new int[4];
            ppcm_data = new int[4];
            ppcm = new int[4];
            byte[] rom;
            rom = new byte[4];
            uint[] delta, end, pos;
            delta = new uint[4];
            end = new uint[4];
            pos = new uint[4];
            int dataL, dataR;
            sbyte d;
            for (i = 0; i < 4; i++)
            {
                rom_offset[i] = ic1.channels[i].start + (ic1.channels[i].bank << 16);
                delta[i] = ic1.delta_table[ic1.channels[i].rate];
                lvol[i] = (int)(ic1.channels[i].volume * ic1.channels[i].pan);
                rvol[i] = (int)(ic1.channels[i].volume * (8 - ic1.channels[i].pan));
                end[i] = ic1.channels[i].size;
                pos[i] = ic1.channels[i].pos;
                play[i] = ic1.channels[i].play;
                loop[i] = ic1.channels[i].loop;
                ppcm[i] = ic1.channels[i].ppcm;
                ppcm_data[i] = ic1.channels[i].ppcm_data;
                if (ppcm[i] != 0)
                {
                    delta[i] /= 2;
                }
            }
            for (j = 0; j < length; j++)
            {
                dataL = dataR = 0;
                for (i = 0; i < 4; i++)
                {
                    if (play[i] != 0)
                    {
                        if ((pos[i] >> 16) >= end[i])
                        {
                            ppcm_data[i] = 0;
                            if (loop[i] != 0)
                            {
                                pos[i] = 0;
                            }
                            else
                            {
                                play[i] = 0;
                                continue;
                            }
                        }
                        if (ppcm[i] != 0)
                        {
                            if (pos[i] == 0 || ((pos[i] ^ (pos[i] - delta[i])) & 0x8000) == 0x8000)
                            {
                                int newdata;
                                if ((pos[i] & 0x8000) != 0)
                                {
                                    newdata = (k053260rom[rom_offset[i] + (pos[i] >> 16)] >> 4) & 0x0f;
                                }
                                else
                                {
                                    newdata = k053260rom[rom_offset[i] + (pos[i] >> 16)] & 0x0f;
                                }
                                ppcm_data[i] = (int)(((ppcm_data[i] * 62) >> 6) + dpcmcnv[newdata]);
                                if (ppcm_data[i] > 127)
                                {
                                    ppcm_data[i] = 127;
                                }
                                else
                                {
                                    if (ppcm_data[i] < -128)
                                    {
                                        ppcm_data[i] = -128;
                                    }
                                }
                            }
                            d = (sbyte)ppcm_data[i];
                            pos[i] += delta[i];
                        }
                        else
                        {
                            d = (sbyte)k053260rom[rom_offset[i] + (pos[i] >> 16)];
                            pos[i] += delta[i];
                        }
                        if ((ic1.mode & 2) != 0)
                        {
                            dataL += (d * lvol[i]) >> 2;
                            dataR += (d * rvol[i]) >> 2;
                        }
                    }
                }
                if (dataL < -32768)
                {
                    dataL = -32768;
                }
                else if (dataL > 32767)
                {
                    dataL = 32767;
                }
                if (dataR < -32768)
                {
                    dataR = -32768;
                }
                else if (dataR > 32767)
                {
                    dataR = 32767;
                }
                Sound.k053260stream.streamoutput_Ptrs[1][offset + j] = dataL;
                Sound.k053260stream.streamoutput_Ptrs[0][offset + j] = dataR;
            }
            for (i = 0; i < 4; i++)
            {
                ic1.channels[i].pos = pos[i];
                ic1.channels[i].play = play[i];
                ic1.channels[i].ppcm_data = ppcm_data[i];
            }
        }
        public static void k053260_start(int clock)
        {
            int rate = clock / 32;
            int i;
            ic1.regs = new int[0x30];
            ic1.channels = new k053260_channel_def[4];
            ic1.mode = 0;
            ic1.rom_size = k053260rom.Length - 1;
            k053260_reset();
            for (i = 0; i < 0x30; i++)
            {
                ic1.regs[i] = 0;
            }
            ic1.delta_table = new uint[0x1000];
            InitDeltaTable(rate, clock);
        }
        public static void check_bounds(int channel)
        {
            int channel_start = (int)((ic1.channels[channel].bank << 16) + ic1.channels[channel].start);
            int channel_end = (int)(channel_start + ic1.channels[channel].size - 1);
            if (channel_start > ic1.rom_size)
            {
                ic1.channels[channel].play = 0;
                return;
            }

            if (channel_end > ic1.rom_size)
            {
                ic1.channels[channel].size = (uint)(ic1.rom_size - channel_start);
            }
        }
        public static void k053260_write(int chip, int offset, byte data)
        {
            int i, t;
            int r = offset;
            int v = data;
            if (r > 0x2f)
            {
                return;
            }
            Sound.k053260stream.stream_update();
            if (r == 0x28)
            {
                t = ic1.regs[r] ^ v;
                for (i = 0; i < 4; i++)
                {
                    if ((t & (1 << i)) != 0)
                    {
                        if ((v & (1 << i)) != 0)
                        {
                            ic1.channels[i].play = 1;
                            ic1.channels[i].pos = 0;
                            ic1.channels[i].ppcm_data = 0;
                            check_bounds(i);
                        }
                        else
                        {
                            ic1.channels[i].play = 0;
                        }
                    }
                }
                ic1.regs[r] = v;
                return;
            }
            ic1.regs[r] = v;
            if (r < 8)
            {
                return;
            }
            if (r < 0x28)
            {
                int channel = (r - 8) / 8;
                switch ((r - 8) & 0x07)
                {
                    case 0:
                        ic1.channels[channel].rate &= 0x0f00;
                        ic1.channels[channel].rate |= (uint)v;
                        break;
                    case 1:
                        ic1.channels[channel].rate &= 0x00ff;
                        ic1.channels[channel].rate |= (uint)((v & 0x0f) << 8);
                        break;
                    case 2:
                        ic1.channels[channel].size &= 0xff00;
                        ic1.channels[channel].size |= (uint)v;
                        break;
                    case 3:
                        ic1.channels[channel].size &= 0x00ff;
                        ic1.channels[channel].size |= (uint)(v << 8);
                        break;
                    case 4:
                        ic1.channels[channel].start &= 0xff00;
                        ic1.channels[channel].start |= (uint)v;
                        break;
                    case 5:
                        ic1.channels[channel].start &= 0x00ff;
                        ic1.channels[channel].start |= (uint)(v << 8);
                        break;
                    case 6:
                        ic1.channels[channel].bank = (uint)(v & 0xff);
                        break;
                    case 7:
                        ic1.channels[channel].volume = (uint)(((v & 0x7f) << 1) | (v & 1));
                        break;
                }
                return;
            }
            switch (r)
            {
                case 0x2a:
                    for (i = 0; i < 4; i++)
                        ic1.channels[i].loop = ((v & (1 << i)) != 0 ? 1 : 0);
                    for (i = 4; i < 8; i++)
                        ic1.channels[i - 4].ppcm = ((v & (1 << i)) != 0 ? 1 : 0);
                    break;
                case 0x2c:
                    ic1.channels[0].pan = (uint)(v & 7);
                    ic1.channels[1].pan = (uint)((v >> 3) & 7);
                    break;
                case 0x2d:
                    ic1.channels[2].pan = (uint)(v & 7);
                    ic1.channels[3].pan = (uint)((v >> 3) & 7);
                    break;
                case 0x2f:
                    ic1.mode = v & 7;
                    break;
            }
        }
        public static void k053260_0_w(int offset, byte data)
        {
            k053260_write(0, offset, data);
        }
        public static void k053260_0_lsb_w(int offset, ushort data)
        {
            //if (ACCESSING_BITS_0_7)
            {
                k053260_0_w(offset, (byte)(data & 0xff));
            }
        }
        public static void k053260_0_lsb_w2(int offset, byte data)
        {
            k053260_0_w(offset, data);
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(ic1.mode);
            for (i = 0; i < 0x30; i++)
            {
                writer.Write(ic1.regs[i]);
            }
            for (i = 0; i < 4; i++)
            {
                writer.Write(ic1.channels[i].rate);
                writer.Write(ic1.channels[i].size);
                writer.Write(ic1.channels[i].start);
                writer.Write(ic1.channels[i].bank);
                writer.Write(ic1.channels[i].volume);
                writer.Write(ic1.channels[i].play);
                writer.Write(ic1.channels[i].pan);
                writer.Write(ic1.channels[i].pos);
                writer.Write(ic1.channels[i].loop);
                writer.Write(ic1.channels[i].ppcm);
                writer.Write(ic1.channels[i].ppcm_data);
            }
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            ic1.mode = reader.ReadInt32();
            for (i = 0; i < 0x30; i++)
            {
                ic1.regs[i] = reader.ReadInt32();
            }
            for (i = 0; i < 4; i++)
            {
                ic1.channels[i].rate = reader.ReadUInt32();
                ic1.channels[i].size = reader.ReadUInt32();
                ic1.channels[i].start = reader.ReadUInt32();
                ic1.channels[i].bank = reader.ReadUInt32();
                ic1.channels[i].volume = reader.ReadUInt32();
                ic1.channels[i].play = reader.ReadInt32();
                ic1.channels[i].pan = reader.ReadUInt32();
                ic1.channels[i].pos = reader.ReadUInt32();
                ic1.channels[i].loop = reader.ReadInt32();
                ic1.channels[i].ppcm = reader.ReadInt32();
                ic1.channels[i].ppcm_data = reader.ReadInt32();
            }
        }
    }
}
