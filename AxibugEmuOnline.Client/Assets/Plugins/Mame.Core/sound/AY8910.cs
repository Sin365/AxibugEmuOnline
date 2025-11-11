
namespace MAME.Core
{
    public unsafe class AY8910
    {
        public struct _ay_ym_param
        {
            public double r_up;
            public double r_down;
            public int res_count;
            public double[] res;
        }
        public struct ay8910_context
        {
            public int streams;
            public int ready;
            public int register_latch;
            public byte[] regs;
            public int last_enable;
            public int[] count;
            public byte[] output;
            public byte output_noise;
            public int count_noise;
            public int count_env;
            public sbyte env_step;
            public int env_volume;
            public byte hold, alternate, attack, holding;
            public int rng;
            public byte env_step_mask;
            public int step;
            public int zero_is_off;
            public byte[] vol_enabled;
            public int[][] vol_table;
            public int[][] env_table;
            public int[] vol3d_table;
        }
        public static _ay_ym_param ay8910_param;
        public static _ay_ym_param ym2149_param, ym2149_param_env;
        public struct ay8910_interface
        {
            public int flags;
            public int[] res_load;
            public read8handler portAread;
            public read8handler portBread;
            public write8handler portAwrite;
            public write8handler portBwrite;
        }
        public delegate byte read8handler(int offset);
        public delegate void write8handler(int offset, byte value);
        public static _ay_ym_param ay_ym_param, ay_ym_param_env;
        public ay8910_context ay8910info;
        public static AY8910[] AA8910 = new AY8910[3];
        public static ay8910_interface ay8910_intf;

        public sound_stream stream;
        private int NOISE_ENABLEQ(int chan)
        {
            return (ay8910info.regs[7] >> (3 + chan)) & 1;
        }
        private int TONE_ENABLEQ(int chan)
        {
            return (ay8910info.regs[7] >> chan) & 1;
        }
        private int TONE_PERIOD(int chan)
        {
            return ay8910info.regs[chan << 1] | ((ay8910info.regs[(chan << 1) | 1] & 0x0f) << 8);
        }
        private int NOISE_PERIOD()
        {
            return ay8910info.regs[6] & 0x1f;
        }

        //private int TONE_VOLUME(int chan)
        //{
        //    return ay8910info.regs[8 + chan] & 0x0f;
        //}

        //private int TONE_ENVELOPE(int chan)
        //{
        //    return (ay8910info.regs[8 + chan] >> 4) & 1;
        //}

        //用常量优化海量访问

        private const int TONE_VOLUME_REG_OFFSET = 8;
        private const int TONE_VOLUME_VOLUME_MASK = 0x0f;
        private int TONE_VOLUME(int chan)
        {
            return ay8910info.regs[TONE_VOLUME_REG_OFFSET + chan] & TONE_VOLUME_VOLUME_MASK;
        }

        private const int TONE_ENVELOPE_REG_OFFSET = 8;
        private const int TONE_ENVELOPE_MOVE = 4;
        private const int TONE_ENVELOPE_VOLUME_MASK = 0x01;
        private int TONE_ENVELOPE(int chan)
        {
            return (ay8910info.regs[TONE_ENVELOPE_REG_OFFSET + chan] >> TONE_ENVELOPE_MOVE) & TONE_ENVELOPE_VOLUME_MASK;
        }

        private int ENVELOPE_PERIOD()
        {
            return ay8910info.regs[11] | (ay8910info.regs[12] << 8);
        }
        public static void ay8910_start_ym(int chip_type, int sndindex, int clock, ay8910_interface intf)
        {
            int i;
            AA8910[sndindex] = new AY8910();
            ym2149_param.r_up = 630;
            ym2149_param.r_down = 801;
            ym2149_param.res_count = 16;
            ym2149_param.res = new double[]
            { 73770, 37586, 27458, 21451, 15864, 12371, 8922,  6796,
               4763,  3521,  2403,  1737,  1123,   762,  438,   251,
                  0,     0,     0,     0,     0,     0,    0,     0,
                  0,     0,     0,     0,     0,     0,    0,     0,};
            ym2149_param_env.r_up = 630;
            ym2149_param_env.r_down = 801;
            ym2149_param_env.res_count = 32;
            ym2149_param_env.res = new double[]
            { 103350, 73770, 52657, 37586, 32125, 27458, 24269, 21451,
               18447, 15864, 14009, 12371, 10506,  8922,  7787,  6796,
                5689,  4763,  4095,  3521,  2909,  2403,  2043,  1737,
                1397,  1123,   925,   762,   578,   438,   332,   251 };
            ay8910_param.r_up = 5806;
            ay8910_param.r_down = 300;
            ay8910_param.res_count = 16;
            ay8910_param.res = new double[]
            { 118996, 42698, 33105, 24770, 17925, 12678,  9331,  5807,
                4936,  3038,  2129,  1658,  1271,   969,   781,   623,
                0,     0,     0,     0,     0,     0,    0,     0,
                0,     0,     0,     0,     0,     0,    0,     0,};
            AA8910[sndindex].ay8910info = new ay8910_context();
            AA8910[sndindex].ay8910info.regs = new byte[16];
            AA8910[sndindex].ay8910info.count = new int[3];
            AA8910[sndindex].ay8910info.output = new byte[3];
            AA8910[sndindex].ay8910info.vol_enabled = new byte[3];
            AA8910[sndindex].ay8910info.vol3d_table = new int[8 * 32 * 32 * 32];
            AA8910[sndindex].ay8910info.vol_table = new int[3][];
            for (i = 0; i < 3; i++)
            {
                AA8910[sndindex].ay8910info.vol_table[i] = new int[16];
            }
            AA8910[sndindex].ay8910info.env_table = new int[3][];
            for (i = 0; i < 3; i++)
            {
                AA8910[sndindex].ay8910info.env_table[i] = new int[32];
            }
            ay8910_intf = intf;
            if ((ay8910_intf.flags & 2) != 0)
            {
                AA8910[sndindex].ay8910info.streams = 1;
            }
            else
            {
                AA8910[sndindex].ay8910info.streams = 3;
            }
            switch (chip_type)
            {
                case 6:
                case 9:
                    AA8910[sndindex].ay8910info.step = 2;
                    ay_ym_param = ay8910_param;
                    ay_ym_param_env = ay8910_param;
                    AA8910[sndindex].ay8910info.zero_is_off = 1;
                    AA8910[sndindex].ay8910info.env_step_mask = 0x0f;
                    break;
                case 10:
                case 14:
                case 17:
                case 18:
                case 16:
                case 12:
                case 13:
                case 11:
                default:
                    AA8910[sndindex].ay8910info.step = 1;
                    ay_ym_param = ym2149_param;
                    ay_ym_param_env = ym2149_param_env;
                    AA8910[sndindex].ay8910info.zero_is_off = 0;
                    AA8910[sndindex].ay8910info.env_step_mask = 0x1f;
                    break;
            }
            AA8910[sndindex].build_mixer_table();
            AA8910[sndindex].stream = new sound_stream(clock / 8, 0, AA8910[sndindex].ay8910info.streams, AA8910[sndindex].ay8910_update);
            AA8910[sndindex].ay8910_set_clock_ym(clock);
        }
        private void build_3D_table(double rl, int normalize, double factor, int zero_is_off)
        {
            int j, j1, j2, j3, e, indx;
            double rt, rw, n;
            double min = 10.0, max = 0.0;
            double[] temp = new double[8 * 32 * 32 * 32];
            for (e = 0; e < 8; e++)
            {
                for (j1 = 0; j1 < 32; j1++)
                {
                    for (j2 = 0; j2 < 32; j2++)
                    {
                        for (j3 = 0; j3 < 32; j3++)
                        {
                            if (zero_is_off != 0)
                            {
                                n = (j1 != 0 || (e & 0x01) != 0) ? 1 : 0;
                                n += (j2 != 0 || (e & 0x02) != 0) ? 1 : 0;
                                n += (j3 != 0 || (e & 0x04) != 0) ? 1 : 0;
                            }
                            else
                            {
                                n = 3.0;
                            }
                            rt = n / ay_ym_param.r_up + 3.0 / ay_ym_param.r_down + 1.0 / rl;
                            rw = n / ay_ym_param.r_up;
                            rw += 1.0 / (((e & 0x01) != 0) ? ay_ym_param_env.res[j1] : ay_ym_param.res[j1]);
                            rt += 1.0 / (((e & 0x01) != 0) ? ay_ym_param_env.res[j1] : ay_ym_param.res[j1]);
                            rw += 1.0 / (((e & 0x02) != 0) ? ay_ym_param_env.res[j2] : ay_ym_param.res[j2]);
                            rt += 1.0 / (((e & 0x02) != 0) ? ay_ym_param_env.res[j2] : ay_ym_param.res[j2]);
                            rw += 1.0 / (((e & 0x04) != 0) ? ay_ym_param_env.res[j3] : ay_ym_param.res[j3]);
                            rt += 1.0 / (((e & 0x04) != 0) ? ay_ym_param_env.res[j3] : ay_ym_param.res[j3]);
                            indx = (e << 15) | (j3 << 10) | (j2 << 5) | j1;
                            temp[indx] = rw / rt;
                            if (temp[indx] < min)
                            {
                                min = temp[indx];
                            }
                            if (temp[indx] > max)
                            {
                                max = temp[indx];
                            }
                        }
                    }
                }
            }
            if (normalize != 0)
            {
                for (j = 0; j < 32 * 32 * 32 * 8; j++)
                {
                    ay8910info.vol3d_table[j] = (int)(0x7fff * (((temp[j] - min) / (max - min))) * factor);
                }
            }
            else
            {
                for (j = 0; j < 32 * 32 * 32 * 8; j++)
                {
                    ay8910info.vol3d_table[j] = (int)(0x7fff * temp[j]);
                }
            }
        }
        public static void build_single_table(double rl, _ay_ym_param par, int normalize, int[] ii, int zero_is_off)
        {
            int j;
            double rt, rw = 0;
            double[] temp = new double[32];
            double min = 10.0, max = 0.0;
            for (j = 0; j < par.res_count; j++)
            {
                rt = 1.0 / par.r_down + 1.0 / rl;
                rw = 1.0 / par.res[j];
                rt += 1.0 / par.res[j];
                if (!((zero_is_off != 0) && (j == 0)))
                {
                    rw += 1.0 / par.r_up;
                    rt += 1.0 / par.r_up;
                }
                temp[j] = rw / rt;
                if (temp[j] < min)
                {
                    min = temp[j];
                }
                if (temp[j] > max)
                {
                    max = temp[j];
                }
            }
            if (normalize != 0)
            {
                for (j = 0; j < par.res_count; j++)
                {
                    ii[j] = (int)(0x7fff * (((temp[j] - min) / (max - min)) - 0.25) * 0.5);
                }
            }
            else
            {
                for (j = 0; j < par.res_count; j++)
                {
                    ii[j] = (int)(0x7fff * temp[j]);
                }
            }
        }
        //private int mix_3D()
        //{
        //    int indx = 0, chan;
        //    for (chan = 0; chan < 3; chan++)
        //    {
        //        if (TONE_ENVELOPE(chan) != 0)
        //        {
        //            indx |= ((1 << (chan + 15)) | ((ay8910info.vol_enabled[chan] != 0) ? ay8910info.env_volume << (chan * 5) : 0));
        //        }
        //        else
        //        {
        //            indx |= ((ay8910info.vol_enabled[chan] != 0) ? TONE_VOLUME(chan) << (chan * 5) : 0);
        //        }
        //    }
        //    return ay8910info.vol3d_table[indx];
        //}

        //手动内联
        private int mix_3D()
        {
            int indx = 0, chan;
            for (chan = 0; chan < 3; chan++)
            {
                //if (TONE_ENVELOPE(chan) != 0)
                if(((ay8910info.regs[TONE_ENVELOPE_REG_OFFSET + chan] >> TONE_ENVELOPE_MOVE) & TONE_ENVELOPE_VOLUME_MASK)!=0)
                {
                    indx |= ((1 << (chan + 15)) | ((ay8910info.vol_enabled[chan] != 0) ? ay8910info.env_volume << (chan * 5) : 0));
                }
                else
                {
                    indx |= ((ay8910info.vol_enabled[chan] != 0) ?
                        //TONE_VOLUME(chan)
                        (ay8910info.regs[TONE_VOLUME_REG_OFFSET + chan] & TONE_VOLUME_VOLUME_MASK)
                        << (chan * 5)
                        : 0);
                }
            }
            return ay8910info.vol3d_table[indx];
        }
        private void ay8910_write_reg(int r, byte v)
        {
            ay8910info.regs[r] = v;
            switch (r)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    break;
                case 7:
                    if ((ay8910info.last_enable == -1) || ((ay8910info.last_enable & 0x40) != (ay8910info.regs[7] & 0x40)))
                    {
                        if (ay8910_intf.portAwrite != null)
                        {
                            ay8910_intf.portAwrite(0, (ay8910info.regs[7] & 0x40) != 0 ? ay8910info.regs[14] : (byte)0xff);
                        }
                    }
                    if ((ay8910info.last_enable == -1) || ((ay8910info.last_enable & 0x80) != (ay8910info.regs[7] & 0x80)))
                    {
                        if (ay8910_intf.portBwrite != null)
                        {
                            ay8910_intf.portBwrite(0, (ay8910info.regs[7] & 0x80) != 0 ? ay8910info.regs[15] : (byte)0xff);
                        }
                    }
                    ay8910info.last_enable = ay8910info.regs[7];
                    break;
                case 13:
                    ay8910info.attack = ((ay8910info.regs[13] & 0x04) != 0) ? ay8910info.env_step_mask : (byte)0x00;
                    if ((ay8910info.regs[13] & 0x08) == 0)
                    {
                        ay8910info.hold = 1;
                        ay8910info.alternate = ay8910info.attack;
                    }
                    else
                    {
                        ay8910info.hold = (byte)(ay8910info.regs[13] & 0x01);
                        ay8910info.alternate = (byte)(ay8910info.regs[13] & 0x02);
                    }
                    ay8910info.env_step = (sbyte)ay8910info.env_step_mask;
                    ay8910info.holding = 0;
                    ay8910info.env_volume = (ay8910info.env_step ^ ay8910info.attack);
                    break;
                case 14:
                    if ((ay8910info.regs[7] & 0x40) != 0)
                    {
                        if (ay8910_intf.portAwrite != null)
                        {
                            ay8910_intf.portAwrite(0, ay8910info.regs[14]);
                        }
                    }
                    break;
                case 15:
                    if ((ay8910info.regs[7] & 0x80) != 0)
                    {
                        if (ay8910_intf.portBwrite != null)
                        {
                            ay8910_intf.portBwrite(0, ay8910info.regs[15]);
                        }
                    }
                    break;
            }
        }
        //public void ay8910_update(int offset, int length)
        //{
        //    int chan, i, j;
        //    if (ay8910info.ready == 0)
        //    {
        //        for (chan = 0; chan < ay8910info.streams; chan++)
        //        {
        //            for (j = 0; j < length; j++)
        //            {
        //                stream.streamoutput_Ptrs[chan][offset + j] = 0;
        //            }
        //        }
        //    }
        //    for (i = 0; i < length; i++)
        //    {
        //        for (chan = 0; chan < 3; chan++)
        //        {
        //            ay8910info.count[chan]++;
        //            if (ay8910info.count[chan] >= TONE_PERIOD(chan))
        //            {
        //                ay8910info.output[chan] ^= 1;
        //                ay8910info.count[chan] = 0; ;
        //            }
        //        }
        //        ay8910info.count_noise++;
        //        if (ay8910info.count_noise >= NOISE_PERIOD())
        //        {
        //            if (((ay8910info.rng + 1) & 2) != 0)
        //            {
        //                ay8910info.output_noise ^= 1;
        //            }
        //            if ((ay8910info.rng & 1) != 0)
        //            {
        //                ay8910info.rng ^= 0x24000;
        //            }
        //            ay8910info.rng >>= 1;
        //            ay8910info.count_noise = 0;
        //        }
        //        for (chan = 0; chan < 3; chan++)
        //        {
        //            ay8910info.vol_enabled[chan] = (byte)((ay8910info.output[chan] | TONE_ENABLEQ(chan)) & (ay8910info.output_noise | NOISE_ENABLEQ(chan)));
        //        }
        //        if (ay8910info.holding == 0)
        //        {
        //            ay8910info.count_env++;
        //            if (ay8910info.count_env >= ENVELOPE_PERIOD() * ay8910info.step)
        //            {
        //                ay8910info.count_env = 0;
        //                ay8910info.env_step--;
        //                if (ay8910info.env_step < 0)
        //                {
        //                    if (ay8910info.hold != 0)
        //                    {
        //                        if (ay8910info.alternate != 0)
        //                        {
        //                            ay8910info.attack ^= ay8910info.env_step_mask;
        //                        }
        //                        ay8910info.holding = 1;
        //                        ay8910info.env_step = 0;
        //                    }
        //                    else
        //                    {
        //                        if (ay8910info.alternate != 0 && (ay8910info.env_step & (ay8910info.env_step_mask + 1)) != 0)
        //                        {
        //                            ay8910info.attack ^= ay8910info.env_step_mask;
        //                        }
        //                        ay8910info.env_step &= (sbyte)ay8910info.env_step_mask;
        //                    }
        //                }
        //            }
        //        }
        //        ay8910info.env_volume = (ay8910info.env_step ^ ay8910info.attack);
        //        if (ay8910info.streams == 3)
        //        {
        //            for (chan = 0; chan < 3; chan++)
        //            {
        //                if (TONE_ENVELOPE(chan) != 0)
        //                {
        //                    int i1 = ay8910info.env_table[chan][ay8910info.vol_enabled[chan] != 0 ? ay8910info.env_volume : 0];
        //                    stream.streamoutput_Ptrs[chan][offset] = ay8910info.env_table[chan][ay8910info.vol_enabled[chan] != 0 ? ay8910info.env_volume : 0];
        //                }
        //                else
        //                {
        //                    int i1 = ay8910info.vol_table[chan][ay8910info.vol_enabled[chan] != 0 ? TONE_VOLUME(chan) : 0];
        //                    stream.streamoutput_Ptrs[chan][offset] = ay8910info.vol_table[chan][ay8910info.vol_enabled[chan] != 0 ? TONE_VOLUME(chan) : 0];
        //                }
        //            }
        //        }
        //        else
        //        {
        //            stream.streamoutput_Ptrs[0][offset] = mix_3D();
        //        }
        //        offset++;
        //    }
        //}


        //手动内联优化
        public void ay8910_update(int offset, int length)
        {
            int chan, i, j;
            if (ay8910info.ready == 0)
            {
                for (chan = 0; chan < ay8910info.streams; chan++)
                {
                    for (j = 0; j < length; j++)
                    {
                        stream.streamoutput_Ptrs[chan][offset + j] = 0;
                    }
                }
            }
            for (i = 0; i < length; i++)
            {
                for (chan = 0; chan < 3; chan++)
                {
                    ay8910info.count[chan]++;
                    //if (ay8910info.count[chan] >= TONE_PERIOD(chan))
                    if (ay8910info.count[chan] >= (ay8910info.regs[chan << 1] | ((ay8910info.regs[(chan << 1) | 1] & 0x0f) << 8)))
                    {
                        ay8910info.output[chan] ^= 1;
                        ay8910info.count[chan] = 0; ;
                    }
                }
                ay8910info.count_noise++;
                //if (ay8910info.count_noise >= NOISE_PERIOD())
                if (ay8910info.count_noise >= (ay8910info.regs[6] & 0x1f))
                {
                    if (((ay8910info.rng + 1) & 2) != 0)
                    {
                        ay8910info.output_noise ^= 1;
                    }
                    if ((ay8910info.rng & 1) != 0)
                    {
                        ay8910info.rng ^= 0x24000;
                    }
                    ay8910info.rng >>= 1;
                    ay8910info.count_noise = 0;
                }
                for (chan = 0; chan < 3; chan++)
                {
                    //ay8910info.vol_enabled[chan] = (byte)((ay8910info.output[chan] | TONE_ENABLEQ(chan)) & (ay8910info.output_noise | NOISE_ENABLEQ(chan)));
                    ay8910info.vol_enabled[chan] = (byte)((ay8910info.output[chan] | 
                        //TONE_ENABLEQ(chan)
                        ((ay8910info.regs[7] >> chan) & 1)
                        ) & (ay8910info.output_noise |
                        //NOISE_ENABLEQ(chan)
                        ((ay8910info.regs[7] >> (3 + chan)) & 1)
                        ));
                }
                if (ay8910info.holding == 0)
                {
                    ay8910info.count_env++;
                    //if (ay8910info.count_env >= ENVELOPE_PERIOD() * ay8910info.step)
                    if (ay8910info.count_env >= 
                        //ENVELOPE_PERIOD()
                        (ay8910info.regs[11] | (ay8910info.regs[12] << 8))
                        * ay8910info.step)
                    {
                        ay8910info.count_env = 0;
                        ay8910info.env_step--;
                        if (ay8910info.env_step < 0)
                        {
                            if (ay8910info.hold != 0)
                            {
                                if (ay8910info.alternate != 0)
                                {
                                    ay8910info.attack ^= ay8910info.env_step_mask;
                                }
                                ay8910info.holding = 1;
                                ay8910info.env_step = 0;
                            }
                            else
                            {
                                if (ay8910info.alternate != 0 && (ay8910info.env_step & (ay8910info.env_step_mask + 1)) != 0)
                                {
                                    ay8910info.attack ^= ay8910info.env_step_mask;
                                }
                                ay8910info.env_step &= (sbyte)ay8910info.env_step_mask;
                            }
                        }
                    }
                }
                ay8910info.env_volume = (ay8910info.env_step ^ ay8910info.attack);
                if (ay8910info.streams == 3)
                {
                    for (chan = 0; chan < 3; chan++)
                    {
                        //if (TONE_ENVELOPE(chan) != 0)
                        if(((ay8910info.regs[TONE_ENVELOPE_REG_OFFSET + chan] >> TONE_ENVELOPE_MOVE) & TONE_ENVELOPE_VOLUME_MASK) != 0)
                        {
                            int i1 = ay8910info.env_table[chan][ay8910info.vol_enabled[chan] != 0 ? ay8910info.env_volume : 0];
                            stream.streamoutput_Ptrs[chan][offset] = ay8910info.env_table[chan][ay8910info.vol_enabled[chan] != 0 ? ay8910info.env_volume : 0];
                        }
                        else
                        {
                            int i1 = ay8910info.vol_table[chan][ay8910info.vol_enabled[chan] != 0 ?
                                //TONE_VOLUME(chan) 
                                ay8910info.regs[TONE_VOLUME_REG_OFFSET + chan] & TONE_VOLUME_VOLUME_MASK
                                : 0];
                            stream.streamoutput_Ptrs[chan][offset] = ay8910info.vol_table[chan][ay8910info.vol_enabled[chan] != 0 ?
                                //TONE_VOLUME(chan)
                                ay8910info.regs[TONE_VOLUME_REG_OFFSET + chan] & TONE_VOLUME_VOLUME_MASK
                                : 0];
                        }
                    }
                }
                else
                {
                    //stream.streamoutput_Ptrs[0][offset] = mix_3D();
                    {
                        int indx = 0, tempchan;
                        for (tempchan = 0; tempchan < 3; tempchan++)
                        {
                            if (((ay8910info.regs[TONE_ENVELOPE_REG_OFFSET + tempchan] >> TONE_ENVELOPE_MOVE) & TONE_ENVELOPE_VOLUME_MASK) != 0)
                            {
                                indx |= ((1 << (tempchan + 15)) | ((ay8910info.vol_enabled[tempchan] != 0) ? ay8910info.env_volume << (tempchan * 5) : 0));
                            }
                            else
                            {
                                indx |= ((ay8910info.vol_enabled[tempchan] != 0) ?
                                    (ay8910info.regs[TONE_VOLUME_REG_OFFSET + tempchan] & TONE_VOLUME_VOLUME_MASK)
                                    << (tempchan * 5)
                                    : 0);
                            }
                        }
                        stream.streamoutput_Ptrs[0][offset] = ay8910info.vol3d_table[indx];
                    }
                }
                offset++;
            }
        }
        public void build_mixer_table()
        {
            int normalize = 0;
            int chan;
            if ((ay8910_intf.flags & 1) != 0)
            {
                normalize = 1;
            }
            for (chan = 0; chan < 3; chan++)
            {
                build_single_table(ay8910_intf.res_load[chan], ay_ym_param, normalize, ay8910info.vol_table[chan], ay8910info.zero_is_off);
                build_single_table(ay8910_intf.res_load[chan], ay_ym_param_env, normalize, ay8910info.env_table[chan], 0);
            }
            build_3D_table(ay8910_intf.res_load[0], normalize, 3, ay8910info.zero_is_off);
        }
        public void ay8910_reset_ym()
        {
            int i;
            ay8910info.register_latch = 0;
            ay8910info.rng = 1;
            ay8910info.output[0] = 0;
            ay8910info.output[1] = 0;
            ay8910info.output[2] = 0;
            ay8910info.count[0] = 0;
            ay8910info.count[1] = 0;
            ay8910info.count[2] = 0;
            ay8910info.count_noise = 0;
            ay8910info.count_env = 0;
            ay8910info.output_noise = 0x01;
            for (i = 0; i < 14; i++)
            {
                ay8910_write_reg(i, 0);
            }
            ay8910info.ready = 1;
        }
        public void ay8910_set_clock_ym(int clock)
        {
            int rate1, rate2;
            rate1 = (stream.new_sample_rate != 0) ? stream.new_sample_rate : stream.sample_rate;
            rate2 = clock / 8;
            if (rate2 != rate1)
            {
                stream.new_sample_rate = rate2;
            }
        }
        public void ay8910_write_ym(int addr, byte data)
        {
            if ((addr & 1) != 0)
            {
                int r = ay8910info.register_latch;
                if (r > 15)
                {
                    return;
                }
                if (r == 13 || ay8910info.regs[r] != data)
                {
                    stream.stream_update();
                }
                ay8910_write_reg(r, data);
            }
            else
            {
                ay8910info.register_latch = data & 0x0f;
            }
        }
        public byte ay8910_read_ym()
        {
            int r = ay8910info.register_latch;
            if (r > 15)
            {
                return 0;
            }
            switch (r)
            {
                case 14:
                    if (ay8910_intf.portAread != null)
                    {
                        ay8910info.regs[14] = ay8910_intf.portAread(0);
                    }
                    break;
                case 15:
                    if (ay8910_intf.portBread != null)
                    {
                        ay8910info.regs[15] = ay8910_intf.portBread(0);
                    }
                    break;
            }
            return ay8910info.regs[r];
        }
        public void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            writer.Write(ay8910info.register_latch);
            writer.Write(ay8910info.regs, 0, 16);
            for (i = 0; i < 3; i++)
            {
                writer.Write(ay8910info.count[i]);
            }
            writer.Write(ay8910info.output, 0, 3);
            writer.Write(ay8910info.output_noise);
            writer.Write(ay8910info.count_noise);
            writer.Write(ay8910info.count_env);
            writer.Write(ay8910info.env_step);
            writer.Write(ay8910info.env_volume);
            writer.Write(ay8910info.hold);
            writer.Write(ay8910info.alternate);
            writer.Write(ay8910info.attack);
            writer.Write(ay8910info.holding);
            writer.Write(ay8910info.rng);
            writer.Write(ay8910info.vol_enabled, 0, 3);
        }
        public void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            ay8910info.register_latch = reader.ReadInt32();
            ay8910info.regs = reader.ReadBytes(16);
            for (i = 0; i < 3; i++)
            {
                ay8910info.count[i] = reader.ReadInt32();
            }
            ay8910info.output = reader.ReadBytes(3);
            ay8910info.output_noise = reader.ReadByte();
            ay8910info.count_noise = reader.ReadInt32();
            ay8910info.count_env = reader.ReadInt32();
            ay8910info.env_step = reader.ReadSByte();
            ay8910info.env_volume = reader.ReadInt32();
            ay8910info.hold = reader.ReadByte();
            ay8910info.alternate = reader.ReadByte();
            ay8910info.attack = reader.ReadByte();
            ay8910info.holding = reader.ReadByte();
            ay8910info.rng = reader.ReadInt32();
            ay8910info.vol_enabled = reader.ReadBytes(3);
        }
    }
}
