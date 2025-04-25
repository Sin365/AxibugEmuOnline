using System;

namespace MAME.Core
{
    public class OKI6295
    {
        public struct adpcm_state
        {
            public int signal;
            public int step;
        };
        public struct ADPCMVoice
        {
            public bool playing;
            public uint base_offset;
            public uint sample;
            public uint count;
            public uint volume;
        };
        public struct okim6295Struct
        {
            public ADPCMVoice[] voice;
            public int command;
            public int bank_offset;
            public uint master_clock;
        };
        public static byte[] okirom;
        public static okim6295Struct OKI;
        public static adpcm_state[] adpcm;
        private static int[] index_shift = new int[8] { -1, -1, -1, -1, 2, 4, 6, 8 };
        private static int[] diff_lookup = new int[49 * 16];
        private static uint[] volume_table = new uint[16];
        private static int tables_computed = 0;
        private static void compute_tables()
        {
            int[,] nbl2bit = new int[16, 4]
            {
                { 1, 0, 0, 0}, { 1, 0, 0, 1}, { 1, 0, 1, 0}, { 1, 0, 1, 1},
                { 1, 1, 0, 0}, { 1, 1, 0, 1}, { 1, 1, 1, 0}, { 1, 1, 1, 1},
                {-1, 0, 0, 0}, {-1, 0, 0, 1}, {-1, 0, 1, 0}, {-1, 0, 1, 1},
                {-1, 1, 0, 0}, {-1, 1, 0, 1}, {-1, 1, 1, 0}, {-1, 1, 1, 1}
            };
            int step, nib;
            for (step = 0; step <= 48; step++)
            {
                int stepval = (int)Math.Floor(16.0 * Math.Pow(11.0 / 10.0, (double)step));
                for (nib = 0; nib < 16; nib++)
                {
                    diff_lookup[step * 16 + nib] = nbl2bit[nib, 0] *
                        (stepval * nbl2bit[nib, 1] +
                         stepval / 2 * nbl2bit[nib, 2] +
                         stepval / 4 * nbl2bit[nib, 3] +
                         stepval / 8);
                }
            }
            for (step = 0; step < 16; step++)
            {
                double dout = 256.0;
                int vol = step;
                while (vol-- > 0)
                    dout /= 1.412537545;
                volume_table[step] = (uint)dout;
            }
            tables_computed = 1;
        }
        private static void reset_adpcm(int i)
        {
            if (tables_computed == 0)
            {
                compute_tables();
            }
            adpcm[i].signal = -2;
            adpcm[i].step = 0;
        }
        private static short clock_adpcm(int i, byte nibble)
        {
            adpcm[i].signal += diff_lookup[adpcm[i].step * 16 + (nibble & 15)];
            if (adpcm[i].signal > 2047)
                adpcm[i].signal = 2047;
            else if (adpcm[i].signal < -2048)
                adpcm[i].signal = -2048;
            adpcm[i].step += index_shift[nibble & 7];
            if (adpcm[i].step > 48)
                adpcm[i].step = 48;
            else if (adpcm[i].step < 0)
                adpcm[i].step = 0;
            return (short)(adpcm[i].signal << 4);
        }
        private static void generate_adpcm(int i, short[] buffer, int samples)
        {
            int i1 = 0;
            if (OKI.voice[i].playing)
            {
                int bbase = (int)(OKI.bank_offset + OKI.voice[i].base_offset);
                int sample = (int)OKI.voice[i].sample;
                int count = (int)OKI.voice[i].count;
                while (samples != 0)
                {
                    int nibble = okirom[bbase + sample / 2] >> (((sample & 1) << 2) ^ 4);
                    buffer[i1] = (short)(clock_adpcm(i, (byte)nibble) * OKI.voice[i].volume / 256);
                    i1++;
                    samples--;
                    if (++sample >= count)
                    {
                        OKI.voice[i].playing = false;
                        break;
                    }
                }
                OKI.voice[i].sample = (uint)sample;
            }
            while ((samples--) != 0)
            {
                buffer[i1] = 0;
                i1++;
            }
        }
        //TODO 移动到这里，但是大小，还需要考虑
        static short[] sample_data = new short[10000];
        public unsafe static void okim6295_update(int offset, int length)
        {
            int i;
            for (i = 0; i < length; i++)
            {
                Sound.okistream.streamoutput_Ptrs[0][offset + i] = 0;
            }
            for (i = 0; i < 4; i++)
            {
                //不每次new，避免GC，排除问题。待验证影响
                //short[] sample_data = new short[10000];
                int remaining = length;
                while (remaining != 0)
                {
                    int samples1 = (remaining > 10000) ? 10000 : remaining;
                    int samp;
                    generate_adpcm(i, sample_data, samples1);
                    for (samp = 0; samp < length; samp++)
                    {
                        Sound.okistream.streamoutput_Ptrs[0][offset + samp] += sample_data[samp];
                    }
                    remaining -= samples1;
                }
            }
        }
        public static void okim6295_start()
        {
            int voice;
            compute_tables();
            OKI.command = -1;
            OKI.bank_offset = 0;
            OKI.master_clock = 1000000;
            OKI.voice = new ADPCMVoice[4];
            adpcm = new adpcm_state[4];
            for (voice = 0; voice < 4; voice++)
            {
                OKI.voice[voice].volume = 255;
                reset_adpcm(voice);
            }
        }
        public static void okim6295_reset()
        {
            int i;
            Sound.okistream.stream_update();
            for (i = 0; i < 4; i++)
            {
                OKI.voice[i].playing = false;
            }
        }
        public static void okim6295_set_bank_base(int base1)
        {
            Sound.okistream.stream_update();
            OKI.bank_offset = base1;
        }
        public static void okim6295_set_pin7(int pin7)
        {
            int divisor = pin7 != 0 ? 132 : 165;
            //stream_set_sample_rate(info->stream, info->master_clock / divisor);
        }
        public static int okim6295_status_r()
        {
            int i, result;
            result = 0xf0;
            Sound.okistream.stream_update();
            for (i = 0; i < 4; i++)
            {
                ADPCMVoice voice = OKI.voice[i];
                if (voice.playing)
                    result |= 1 << i;
            }
            return result;
        }
        private static void okim6295_data_w(int num, int data)
        {
            if (OKI.command != -1)
            {
                int temp = data >> 4, i, start, stop;
                int baseoffset;
                Sound.okistream.stream_update();
                for (i = 0; i < 4; i++, temp >>= 1)
                {
                    if ((temp & 1) != 0)
                    {
                        baseoffset = OKI.bank_offset + OKI.command * 8;
                        start = ((okirom[baseoffset + 0] << 16) + (okirom[baseoffset + 1] << 8) + okirom[baseoffset + 2]) & 0x3ffff;
                        stop = ((okirom[baseoffset + 3] << 16) + (okirom[baseoffset + 4] << 8) + okirom[baseoffset + 5]) & 0x3ffff;
                        if (start < stop)
                        {
                            if (!OKI.voice[i].playing)
                            {
                                OKI.voice[i].playing = true;
                                OKI.voice[i].base_offset = (uint)start;
                                OKI.voice[i].sample = 0;
                                OKI.voice[i].count = (uint)(2 * (stop - start + 1));
                                reset_adpcm(i);
                                OKI.voice[i].volume = volume_table[data & 0x0f];
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            OKI.voice[i].playing = false;
                        }
                    }
                }
                OKI.command = -1;
            }
            else if ((data & 0x80) != 0)
            {
                OKI.command = data & 0x7f;
            }
            else
            {
                int temp = data >> 3, i;
                Sound.okistream.stream_update();
                for (i = 0; i < 4; i++, temp >>= 1)
                {
                    if ((temp & 1) != 0)
                    {
                        OKI.voice[i].playing = false;
                    }
                }
            }
        }
        public static byte okim6295_status_0_r()
        {
            int i;
            byte result;
            result = 0xf0;
            Sound.okistream.stream_update();
            for (i = 0; i < 4; i++)
            {
                if (OKI.voice[i].playing)
                {
                    result |= (byte)(1 << i);
                }
            }
            return result;
        }
        public static int okim6295_status_0_lsb_r()
        {
            return okim6295_status_r();
        }
        public static void okim6295_data_0_w(byte data)
        {
            okim6295_data_w(0, data);
        }
        public static void okim6295_data_0_lsb_w(byte data)
        {
            //if (ACCESSING_BITS_0_7)
            okim6295_data_w(0, data & 0xff);
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(OKI.command);
            writer.Write(OKI.bank_offset);
            for (i = 0; i < 4; i++)
            {
                writer.Write(OKI.voice[i].playing);
                writer.Write(OKI.voice[i].sample);
                writer.Write(OKI.voice[i].count);
                writer.Write(OKI.voice[i].volume);
                writer.Write(OKI.voice[i].base_offset);
                writer.Write(adpcm[i].signal);
                writer.Write(adpcm[i].step);
            }
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            OKI.command = reader.ReadInt32();
            OKI.bank_offset = reader.ReadInt32();
            for (i = 0; i < 4; i++)
            {
                OKI.voice[i].playing = reader.ReadBoolean();
                OKI.voice[i].sample = reader.ReadUInt32();
                OKI.voice[i].count = reader.ReadUInt32();
                OKI.voice[i].volume = reader.ReadUInt32();
                OKI.voice[i].base_offset = reader.ReadUInt32();
                adpcm[i].signal = reader.ReadInt32();
                adpcm[i].step = reader.ReadInt32();
            }
        }
    }
}
