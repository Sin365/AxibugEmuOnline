
namespace MAME.Core
{
    public unsafe class Namco
    {
        public struct sound_channel
        {
            public int frequency;
            public int counter;
            public int[] volume;
            public int noise_sw;
            public int noise_state;
            public int noise_seed;
            public int noise_counter;
            public int noise_hold;
            public int waveform_select;
        };
        public struct namco_sound
        {
            public sound_channel[] channel_list;
            public int wave_size;
            public int num_voices;
            public int sound_enable;
            public sound_stream stream;
            public int namco_clock;
            public int sample_rate;
            public int f_fracbits;
            public int stereo;
            public short[][] waveform;
        };
        public static byte[] namco_wavedata;
        public static namco_sound nam1;
        public static void update_namco_waveform(int offset, byte data)
        {
            if (nam1.wave_size == 1)
            {
                short wdata;
                int v;
                for (v = 0; v < 16; v++)
                {
                    wdata = (short)(((data >> 4) & 0x0f) - 8);
                    nam1.waveform[v][offset * 2] = (short)((wdata * v) * 0x100 / nam1.num_voices);
                    wdata = (short)((data & 0x0f) - 8);
                    nam1.waveform[v][offset * 2 + 1] = (short)((wdata * v) * 0x100 / nam1.num_voices);
                }
            }
            else
            {
                int v;
                for (v = 0; v < 16; v++)
                    nam1.waveform[v][offset] = (short)((((data & 0x0f) - 8) * v) * 0x100 / nam1.num_voices);
            }
        }
        public static void build_decoded_waveform()
        {
            int offset;
            int v;
            nam1.wave_size = 1;
            nam1.waveform = new short[16][];
            for (v = 0; v < 16; v++)
            {
                nam1.waveform[v] = new short[32 * 16];
            }
            for (offset = 0; offset < 256; offset++)
                update_namco_waveform(offset, namco_wavedata[offset]);
        }
        public static uint namco_update_one(int[] buffer, int length, short[] wave, uint counter, uint freq)
        {
            int i;
            for (i = 0; i < length; i++)
            {
                buffer[i] += wave[((counter) >> nam1.f_fracbits) & 0x1f];
                counter += freq;
            }
            return counter;
        }
        public static void namco_update_stereo(int offset, int length)
        {
            int voice;
            int i;
            int counter;
            for (i = 0; i < length; i++)
            {
                Sound.namcostream.streamoutput_Ptrs[0][offset + i] = 0;
                Sound.namcostream.streamoutput_Ptrs[1][offset + i] = 0;
            }
            for (voice = 0; voice < 8; voice++)
            {
                int lv = nam1.channel_list[voice].volume[0];
                int rv = nam1.channel_list[voice].volume[1];
                if (nam1.channel_list[voice].noise_sw != 0)
                {
                    int f = nam1.channel_list[voice].frequency & 0xff;
                    if ((lv != 0 || rv != 0) && f != 0)
                    {
                        int hold_time = 1 << (nam1.f_fracbits - 16);
                        int hold = nam1.channel_list[voice].noise_hold;
                        int delta = f << 4;
                        int c = nam1.channel_list[voice].noise_counter;
                        short l_noise_data = (short)((0x07 * (lv >> 1)) * 32);
                        short r_noise_data = (short)((0x07 * (rv >> 1)) * 32);
                        for (i = 0; i < length; i++)
                        {
                            int cnt;
                            if (nam1.channel_list[voice].noise_state != 0)
                            {
                                Sound.namcostream.streamoutput_Ptrs[0][offset + i] += l_noise_data;
                                Sound.namcostream.streamoutput_Ptrs[1][offset + i] += r_noise_data;
                            }
                            else
                            {
                                Sound.namcostream.streamoutput_Ptrs[0][offset + i] += l_noise_data;
                                Sound.namcostream.streamoutput_Ptrs[1][offset + i] += r_noise_data;
                            }
                            if (hold != 0)
                            {
                                hold--;
                                continue;
                            }
                            hold = hold_time;
                            c += delta;
                            cnt = (c >> 12);
                            c &= (1 << 12) - 1;
                            for (; cnt > 0; cnt--)
                            {
                                if (((nam1.channel_list[voice].noise_seed + 1) & 2) != 0)
                                    nam1.channel_list[voice].noise_state ^= 1;
                                if ((nam1.channel_list[voice].noise_seed & 1) != 0)
                                    nam1.channel_list[voice].noise_seed ^= 0x28000;
                                nam1.channel_list[voice].noise_seed >>= 1;
                            }
                        }
                        nam1.channel_list[voice].noise_counter = c;
                        nam1.channel_list[voice].noise_hold = hold;
                    }
                }
                else
                {
                    if (nam1.channel_list[voice].frequency != 0)
                    {
                        int c = nam1.channel_list[voice].counter;
                        if (lv != 0)
                        {
                            counter = nam1.channel_list[voice].counter;
                            for (i = 0; i < length; i++)
                            {
                                Sound.namcostream.streamoutput_Ptrs[0][offset + i] += nam1.waveform[lv][nam1.channel_list[voice].waveform_select * 32 + (counter >> nam1.f_fracbits) & 0x1f];
                                counter += nam1.channel_list[voice].frequency;
                            }
                            c = counter;
                        }
                        if (rv != 0)
                        {
                            counter = nam1.channel_list[voice].counter;
                            for (i = 0; i < length; i++)
                            {
                                Sound.namcostream.streamoutput_Ptrs[1][offset + i] += nam1.waveform[rv][nam1.channel_list[voice].waveform_select * 32 + (counter >> nam1.f_fracbits) & 0x1f];
                                counter += nam1.channel_list[voice].frequency;
                            }
                            c = counter;
                        }
                        nam1.channel_list[voice].counter = c;
                    }
                }
            }
        }
        public static void namco_start()
        {
            int voice;
            nam1.num_voices = 8;
            nam1.namco_clock = 192000;
            nam1.f_fracbits = 4 + 15;
            nam1.sample_rate = nam1.namco_clock;
            build_decoded_waveform();
            nam1.channel_list = new sound_channel[8];
            for (voice = 0; voice < 8; voice++)
            {
                int state_index = voice;
                nam1.channel_list[voice].frequency = 0;
                nam1.channel_list[voice].volume = new int[2];
                nam1.channel_list[voice].volume[0] = nam1.channel_list[voice].volume[1] = 0;
                nam1.channel_list[voice].waveform_select = 0;
                nam1.channel_list[voice].counter = 0;
                nam1.channel_list[voice].noise_sw = 0;
                nam1.channel_list[voice].noise_state = 0;
                nam1.channel_list[voice].noise_seed = 1;
                nam1.channel_list[voice].noise_counter = 0;
                nam1.channel_list[voice].noise_hold = 0;
            }
        }
        public static void namcos1_sound_w(int offset, byte data)
        {
            int ch, ch1;
            int nssw;
            if (offset > 63)
            {
                return;
            }
            if (namco_wavedata[0x100 + offset] == data)
                return;
            Sound.namcostream.stream_update();
            namco_wavedata[0x100 + offset] = data;
            ch = offset / 8;
            if (ch >= nam1.num_voices)
                return;
            ch1 = ch;
            switch (offset - ch * 8)
            {
                case 0x00:
                    nam1.channel_list[ch1].volume[0] = data & 0x0f;
                    break;

                case 0x01:
                    nam1.channel_list[ch1].waveform_select = (data >> 4) & 15;
                    nam1.channel_list[ch1].frequency = (namco_wavedata[0x100 + ch * 8 + 0x01] & 15) << 16;
                    nam1.channel_list[ch1].frequency += namco_wavedata[0x100 + ch * 8 + 0x02] << 8;
                    nam1.channel_list[ch1].frequency += namco_wavedata[0x100 + ch * 8 + 0x03];
                    break;
                case 0x02:
                case 0x03:
                    nam1.channel_list[ch1].frequency = (namco_wavedata[0x100 + ch * 8 + 0x01] & 15) << 16;
                    nam1.channel_list[ch1].frequency += namco_wavedata[0x100 + ch * 8 + 0x02] << 8;
                    nam1.channel_list[ch1].frequency += namco_wavedata[0x100 + ch * 8 + 0x03];
                    break;

                case 0x04:
                    nam1.channel_list[ch1].volume[1] = data & 0x0f;
                    nssw = ((data & 0x80) >> 7);
                    if (ch1 == 7)
                    {
                        ch1 = 0;
                    }
                    nam1.channel_list[ch1].noise_sw = nssw;
                    break;
            }
        }
        public static void namcos1_cus30_w(int offset, byte data)
        {
            if (offset < 0x100)
            {
                if (namco_wavedata[offset] != data)
                {
                    Sound.namcostream.stream_update();
                    namco_wavedata[offset] = data;
                    update_namco_waveform(offset, data);
                }
            }
            else if (offset < 0x140)
                namcos1_sound_w(offset - 0x100, data);
            else
                namco_wavedata[offset] = data;
        }
        public static byte namcos1_cus30_r(int offset)
        {
            return namco_wavedata[offset];
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i, j;
            writer.Write(nam1.num_voices);
            writer.Write(nam1.sound_enable);
            for (i = 0; i < 16; i++)
            {
                for (j = 0; j < 32 * 16; j++)
                {
                    writer.Write(nam1.waveform[i][j]);
                }
            }
            for (i = 0; i < 8; i++)
            {
                writer.Write(nam1.channel_list[i].frequency);
                writer.Write(nam1.channel_list[i].counter);
                writer.Write(nam1.channel_list[i].volume[0]);
                writer.Write(nam1.channel_list[i].volume[1]);
                writer.Write(nam1.channel_list[i].noise_sw);
                writer.Write(nam1.channel_list[i].noise_state);
                writer.Write(nam1.channel_list[i].noise_seed);
                writer.Write(nam1.channel_list[i].noise_hold);
                writer.Write(nam1.channel_list[i].noise_counter);
                writer.Write(nam1.channel_list[i].waveform_select);
            }
            writer.Write(namco_wavedata, 0, 0x400);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i, j;
            nam1.num_voices = reader.ReadInt32();
            nam1.sound_enable = reader.ReadInt32();
            for (i = 0; i < 16; i++)
            {
                for (j = 0; j < 32 * 16; j++)
                {
                    nam1.waveform[i][j] = reader.ReadInt16();
                }
            }
            for (i = 0; i < 8; i++)
            {
                nam1.channel_list[i].frequency = reader.ReadInt32();
                nam1.channel_list[i].counter = reader.ReadInt32();
                nam1.channel_list[i].volume[0] = reader.ReadInt32();
                nam1.channel_list[i].volume[1] = reader.ReadInt32();
                nam1.channel_list[i].noise_sw = reader.ReadInt32();
                nam1.channel_list[i].noise_state = reader.ReadInt32();
                nam1.channel_list[i].noise_seed = reader.ReadInt32();
                nam1.channel_list[i].noise_hold = reader.ReadInt32();
                nam1.channel_list[i].noise_counter = reader.ReadInt32();
                nam1.channel_list[i].waveform_select = reader.ReadInt32();
            }
            namco_wavedata = reader.ReadBytes(0x400);
        }
    }
}
