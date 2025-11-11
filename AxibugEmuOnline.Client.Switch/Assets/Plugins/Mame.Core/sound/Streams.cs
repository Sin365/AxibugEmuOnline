using System;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe class sound_stream
    {
        public int sample_rate;
        public int new_sample_rate;
        public int gain;
        public long attoseconds_per_sample;
        public int max_samples_per_update;
        public int inputs;
        public int outputs;
        public int output_sampindex;
        public int output_base_sampindex;
        //改成指针读取
        private int[][] streaminput;
        GCHandle[] streaminput_handles;
        public int*[] streaminput_Ptrs;
        //改成指针读取
        private int[][] streamoutput;
        GCHandle[] streamoutput_handles;
        public int*[] streamoutput_Ptrs;

        private updatedelegate updatecallback;
        public delegate void updatedelegate(int offset, int length);
        public sound_stream(int _sample_rate, int _inputs, int _outputs, updatedelegate callback)
        {
            int i;
            sample_rate = _sample_rate;
            inputs = _inputs;
            outputs = _outputs;
            attoseconds_per_sample = Attotime.ATTOSECONDS_PER_SECOND / sample_rate;
            max_samples_per_update = (int)((Sound.update_attoseconds + attoseconds_per_sample - 1) / attoseconds_per_sample);
            output_base_sampindex = -max_samples_per_update;
            streaminput = new int[inputs][];
            //
            streaminput_handles = new GCHandle[inputs];
            streaminput_Ptrs = new int*[inputs];
            for (i = 0; i < inputs; i++)
            {
                streaminput[i] = new int[max_samples_per_update];
                //
                streaminput_handles[i] = GCHandle.Alloc(streaminput[i], GCHandleType.Pinned);
                streaminput_Ptrs[i] = (int*)streaminput_handles[i].AddrOfPinnedObject();

            }
            streamoutput = new int[outputs][];
            //
            streamoutput_handles = new GCHandle[outputs];
            streamoutput_Ptrs = new int*[outputs];
            for (i = 0; i < outputs; i++)
            {
                streamoutput[i] = new int[5 * max_samples_per_update];
                //
                streamoutput_handles[i] = GCHandle.Alloc(streamoutput[i], GCHandleType.Pinned);
                streamoutput_Ptrs[i] = (int*)streamoutput_handles[i].AddrOfPinnedObject();
            }
            updatecallback = callback;
        }

        ~sound_stream()
        {
            // 释放句柄
            if (streaminput_handles != null)
            {
                for (int i = 0; i < streaminput_handles.Length; i++)
                {
                    if (streaminput_handles[i].IsAllocated)
                        streaminput_handles[i].Free();
                }
                streaminput_handles = null;
                streaminput_Ptrs = null;
            }


            if (streamoutput_handles != null)
            {
                for (int i = 0; i < streamoutput_handles.Length; i++)
                {
                    if (streamoutput_handles[i].IsAllocated)
                        streamoutput_handles[i].Free();
                }
                streamoutput_handles = null;
                streamoutput_Ptrs = null;
            }
        }
        public void stream_update()
        {
            int update_sampindex = time_to_sampindex(EmuTimer.get_current_time());
            int offset, samples;
            samples = update_sampindex - output_sampindex;
            if (samples > 0)
            {
                offset = output_sampindex - output_base_sampindex;
                updatecallback(offset, samples);
            }
            output_sampindex = update_sampindex;
        }
        public void adjuststream(bool second_tick)
        {
            int i, j;
            int output_bufindex = output_sampindex - output_base_sampindex;
            if (second_tick)
            {
                output_sampindex -= sample_rate;
                output_base_sampindex -= sample_rate;
            }
            if (output_bufindex > 3 * max_samples_per_update)
            {
                int samples_to_lose = output_bufindex - max_samples_per_update;
                for (i = 0; i < streamoutput.Length; i++)
                {
                    //src;
                    //for (j = 0; j < max_samples_per_update; j++)
                    //{
                    //    streamoutput[i][j] = streamoutput[i][samples_to_lose + j];
                    //}

                    //这是一个Monkey Patch， 这里一维数组是0~2，二位数组可能不正常
                    max_samples_per_update = Math.Min(max_samples_per_update, streamoutput[i].Length - samples_to_lose);
                    for (j = 0; j < max_samples_per_update; j++)
                    {
                        streamoutput[i][j] = streamoutput[i][samples_to_lose + j];
                    }
                }
                output_base_sampindex += samples_to_lose;
            }
        }
        private int time_to_sampindex(Atime time)
        {
            int sample = (int)(time.attoseconds / attoseconds_per_sample);
            if (time.seconds > Sound.last_update_second)
            {
                sample += sample_rate;
            }
            if (time.seconds < Sound.last_update_second)
            {
                sample -= sample_rate;
            }
            return sample;
        }
        public void updatesamplerate()
        {
            int i;
            if (new_sample_rate != 0)
            {
                int old_rate = sample_rate;
                sample_rate = new_sample_rate;
                new_sample_rate = 0;
                attoseconds_per_sample = (long)1e18 / sample_rate;
                max_samples_per_update = (int)((Sound.update_attoseconds + attoseconds_per_sample - 1) / attoseconds_per_sample);
                output_sampindex = (int)((long)output_sampindex * (long)sample_rate / old_rate);
                output_base_sampindex = output_sampindex - max_samples_per_update;
                for (i = 0; i < outputs; i++)
                {
                    Array.Clear(streamoutput[i], 0, max_samples_per_update);
                }
            }
        }
    };
    public unsafe partial class Sound
    {
        public static int last_update_second;
        public static sound_stream ym2151stream, okistream, mixerstream;
        public static sound_stream qsoundstream;
        public static sound_stream ym2610stream;
        public static sound_stream namcostream, dacstream;
        public static sound_stream ics2115stream;
        public static sound_stream ym3812stream, ym3526stream, ym2413stream;
        public static sound_stream iremga20stream;
        public static sound_stream k053260stream;
        public static sound_stream upd7759stream;
        public static sound_stream k007232stream;
        public static sound_stream samplestream;
        public static sound_stream k054539stream;
        public static long update_attoseconds = Attotime.ATTOSECONDS_PER_SECOND / 50;
        private unsafe static void generate_resampled_dataY5(int gain)
        {
            int offset;
            int sample0, sample1;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / ym2151stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / ym2151stream.attoseconds_per_sample) - 1);
            offset = basesample - ym2151stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * ym2151stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)ym2151stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample0 = ym2151stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = ym2151stream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += ym2151stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += ym2151stream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += ym2151stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += ym2151stream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    mixerstream.streaminput_Ptrs[0][sampindex] = (sample0 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[1][sampindex] = (sample1 * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private unsafe static void generate_resampled_dataO(int gain, int minput)
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - okistream.attoseconds_per_sample * 2;
            if (basetime >= 0)
                basesample = (int)(basetime / okistream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / okistream.attoseconds_per_sample) - 1);
            offset = basesample - okistream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * okistream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)okistream.sample_rate << 22) / 48000);
            if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    sample = (okistream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + okistream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataQ()
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - qsoundstream.attoseconds_per_sample * 2;
            if (basetime >= 0)
                basesample = (int)(basetime / qsoundstream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / qsoundstream.attoseconds_per_sample) - 1);
            offset = basesample - qsoundstream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * qsoundstream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)qsoundstream.sample_rate << 22) / 48000);
            if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    mixerstream.streaminput_Ptrs[0][sampindex] = (qsoundstream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + qsoundstream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12;
                    mixerstream.streaminput_Ptrs[1][sampindex] = (qsoundstream.streamoutput_Ptrs[1][offset] * (0x1000 - interp_frac) + qsoundstream.streamoutput_Ptrs[1][offset + 1] * interp_frac) >> 12;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataA_neogeo()
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / AY8910.AA8910[0].stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / AY8910.AA8910[0].stream.attoseconds_per_sample) - 1);
            offset = basesample - AY8910.AA8910[0].stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * AY8910.AA8910[0].stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)AY8910.AA8910[0].stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample = AY8910.AA8910[0].stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += AY8910.AA8910[0].stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += AY8910.AA8910[0].stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[0][sampindex] = (sample * 0x99) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataA3(int chip, int gain, int start)
        {
            int offset;
            int sample0, sample1, sample2;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / AY8910.AA8910[chip].stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / AY8910.AA8910[chip].stream.attoseconds_per_sample) - 1);
            offset = basesample - AY8910.AA8910[chip].stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * AY8910.AA8910[chip].stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)AY8910.AA8910[chip].stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample0 = AY8910.AA8910[chip].stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = AY8910.AA8910[chip].stream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    sample2 = AY8910.AA8910[chip].stream.streamoutput_Ptrs[2][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += AY8910.AA8910[chip].stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += AY8910.AA8910[chip].stream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        sample2 += AY8910.AA8910[chip].stream.streamoutput_Ptrs[2][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += AY8910.AA8910[chip].stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += AY8910.AA8910[chip].stream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample2 += AY8910.AA8910[chip].stream.streamoutput_Ptrs[2][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    sample2 /= smallstep;
                    mixerstream.streaminput_Ptrs[start][sampindex] = (sample0 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[start + 1][sampindex] = (sample1 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[start + 2][sampindex] = (sample2 * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataA_taitob()
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int gain;
            int sampindex;
            gain = (0x40 * AY8910.AA8910[0].stream.gain) >> 8;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / AY8910.AA8910[0].stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / AY8910.AA8910[0].stream.attoseconds_per_sample) - 1);
            offset = basesample - AY8910.AA8910[0].stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * AY8910.AA8910[0].stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)AY8910.AA8910[0].stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample = AY8910.AA8910[0].stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += AY8910.AA8910[0].stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += AY8910.AA8910[0].stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[0][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataYM2203(int c, int gain, int minput)
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - YM2203.FF2203[c].stream.attoseconds_per_sample * 2;
            if (basetime >= 0)
                basesample = (int)(basetime / YM2203.FF2203[c].stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / YM2203.FF2203[c].stream.attoseconds_per_sample) - 1);
            offset = basesample - YM2203.FF2203[c].stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * YM2203.FF2203[c].stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)YM2203.FF2203[c].stream.sample_rate << 22) / 48000);
            if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    int i2 = YM2203.FF2203[c].stream.streamoutput_Ptrs[0][offset];
                    int i3 = YM2203.FF2203[c].stream.streamoutput_Ptrs[0][offset + 1];
                    int i4 = (((YM2203.FF2203[c].stream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + YM2203.FF2203[c].stream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (((YM2203.FF2203[c].stream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + YM2203.FF2203[c].stream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataYM3526(int gain, int minput)
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / ym3526stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / ym3526stream.attoseconds_per_sample) - 1);
            offset = basesample - ym3526stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * ym3526stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)ym3526stream.sample_rate << 22) / 48000);
            if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (((ym3526stream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + ym3526stream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataY6()
        {
            int offset;
            int sample0, sample1;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / ym2610stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / ym2610stream.attoseconds_per_sample) - 1);
            offset = basesample - ym2610stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * ym2610stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)ym2610stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample0 = ym2610stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = ym2610stream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += ym2610stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += ym2610stream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += ym2610stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += ym2610stream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    mixerstream.streaminput_Ptrs[1][sampindex] = sample0;
                    mixerstream.streaminput_Ptrs[2][sampindex] = sample1;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataNa()
        {
            int offset;
            int sample0, sample1;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int gain;
            int sampindex;
            gain = 0x80;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / namcostream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / namcostream.attoseconds_per_sample) - 1);
            offset = basesample - namcostream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * namcostream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)namcostream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> (14));
                    sample0 = namcostream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = namcostream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += namcostream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += namcostream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += namcostream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += namcostream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    mixerstream.streaminput_Ptrs[2][sampindex] = (sample0 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[3][sampindex] = (sample1 * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataDac(int gain, int minput)
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / dacstream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / dacstream.attoseconds_per_sample) - 1);
            offset = basesample - dacstream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * dacstream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)dacstream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> (14));
                    sample = dacstream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += dacstream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += dacstream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataYM2413(int gain, int minput)
        {
            int offset;
            int sample0, sample1;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / ym2413stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / ym2413stream.attoseconds_per_sample) - 1);
            offset = basesample - ym2413stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * ym2413stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)ym2413stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample0 = ym2413stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = ym2413stream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += ym2413stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += ym2413stream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += ym2413stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += ym2413stream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (sample0 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[minput + 1][sampindex] = (sample1 * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataYM3812(int gain, int minput)
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / ym3812stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / ym3812stream.attoseconds_per_sample) - 1);
            offset = basesample - ym3812stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * ym3812stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)ym3812stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample = ym3812stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += ym3812stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += ym3812stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
            else if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (((ym3812stream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + ym3812stream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        private static void generate_resampled_dataIcs2115(int gain)
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - ics2115stream.attoseconds_per_sample * 2;
            if (basetime >= 0)
                basesample = (int)(basetime / ics2115stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / ics2115stream.attoseconds_per_sample) - 1);
            offset = basesample - ics2115stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * ics2115stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)ics2115stream.sample_rate << 22) / 48000);
            if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    mixerstream.streaminput_Ptrs[0][sampindex] = (((ics2115stream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + ics2115stream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    mixerstream.streaminput_Ptrs[1][sampindex] = (((ics2115stream.streamoutput_Ptrs[1][offset] * (0x1000 - interp_frac) + ics2115stream.streamoutput_Ptrs[1][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void generate_resampled_dataIremga20(int gain)
        {
            int offset;
            int sample0, sample1;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / iremga20stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / iremga20stream.attoseconds_per_sample) - 1);
            offset = basesample - iremga20stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * iremga20stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)iremga20stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample0 = iremga20stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = iremga20stream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += iremga20stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += iremga20stream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += iremga20stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += iremga20stream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    mixerstream.streaminput_Ptrs[2][sampindex] = (sample0 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[3][sampindex] = (sample1 * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void generate_resampled_dataK053260(int gain, int minput1, int minput2)
        {
            int offset;
            int sample0, sample1;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / k053260stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / k053260stream.attoseconds_per_sample) - 1);
            offset = basesample - k053260stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * k053260stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)k053260stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample0 = k053260stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    sample1 = k053260stream.streamoutput_Ptrs[1][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample0 += k053260stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        sample1 += k053260stream.streamoutput_Ptrs[1][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample0 += k053260stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample1 += k053260stream.streamoutput_Ptrs[1][offset + tpos] * remainder;
                    sample0 /= smallstep;
                    sample1 /= smallstep;
                    mixerstream.streaminput_Ptrs[minput1][sampindex] = (sample0 * gain) >> 8;
                    mixerstream.streaminput_Ptrs[minput2][sampindex] = (sample1 * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void generate_resampled_dataK007232(int gain)
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - k007232stream.attoseconds_per_sample * 2;
            if (basetime >= 0)
                basesample = (int)(basetime / k007232stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / k007232stream.attoseconds_per_sample) - 1);
            offset = basesample - k007232stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * k007232stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)k007232stream.sample_rate << 22) / 48000);
            if (step < 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int interp_frac = (int)(basefrac >> 10);
                    mixerstream.streaminput_Ptrs[2][sampindex] = (((k007232stream.streamoutput_Ptrs[0][offset] * (0x1000 - interp_frac) + k007232stream.streamoutput_Ptrs[0][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    mixerstream.streaminput_Ptrs[3][sampindex] = (((k007232stream.streamoutput_Ptrs[1][offset] * (0x1000 - interp_frac) + k007232stream.streamoutput_Ptrs[1][offset + 1] * interp_frac) >> 12) * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void generate_resampled_dataUpd7759(int gain)
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / upd7759stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / upd7759stream.attoseconds_per_sample) - 1);
            offset = basesample - upd7759stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * upd7759stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)upd7759stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample = upd7759stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += upd7759stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += upd7759stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[4][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void generate_resampled_dataSample(int gain, int minput)
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / samplestream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / samplestream.attoseconds_per_sample) - 1);
            offset = basesample - samplestream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * samplestream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)samplestream.sample_rate << 22) / 48000);
            if (step == 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (samplestream.streamoutput_Ptrs[0][offset + sampindex] * gain) >> 8;
                }
            }
        }
        public static void generate_resampled_dataK054539(int gain)
        {
            int offset;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / k054539stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / k054539stream.attoseconds_per_sample) - 1);
            offset = basesample - k054539stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * k054539stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)k054539stream.sample_rate << 22) / 48000);
            if (step == 0x400000)
            {
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    mixerstream.streaminput_Ptrs[0][sampindex] = (k054539stream.streamoutput_Ptrs[0][offset + sampindex] * gain) >> 8;
                    mixerstream.streaminput_Ptrs[1][sampindex] = (k054539stream.streamoutput_Ptrs[1][offset + sampindex] * gain) >> 8;
                }
            }
        }
        public static void generate_resampled_dataMSM5205_0(int gain, int minput)
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / MSM5205.mm1[0].voice.stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / MSM5205.mm1[0].voice.stream.attoseconds_per_sample) - 1);
            offset = basesample - MSM5205.mm1[0].voice.stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * MSM5205.mm1[0].voice.stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)MSM5205.mm1[0].voice.stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample = MSM5205.mm1[0].voice.stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += MSM5205.mm1[0].voice.stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += MSM5205.mm1[0].voice.stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void generate_resampled_dataMSM5205_1(int gain, int minput)
        {
            int offset;
            int sample;
            long basetime;
            int basesample;
            uint basefrac;
            uint step;
            int sampindex;
            basetime = mixerstream.output_sampindex * mixerstream.attoseconds_per_sample - mixerstream.attoseconds_per_sample;
            if (basetime >= 0)
                basesample = (int)(basetime / MSM5205.mm1[1].voice.stream.attoseconds_per_sample);
            else
                basesample = (int)(-(-basetime / MSM5205.mm1[1].voice.stream.attoseconds_per_sample) - 1);
            offset = basesample - MSM5205.mm1[1].voice.stream.output_base_sampindex;
            basefrac = (uint)((basetime - basesample * MSM5205.mm1[1].voice.stream.attoseconds_per_sample) / (Attotime.ATTOSECONDS_PER_SECOND >> 22));
            step = (uint)(((ulong)MSM5205.mm1[1].voice.stream.sample_rate << 22) / 48000);
            if (step > 0x400000)
            {
                int smallstep = (int)(step >> 14);
                for (sampindex = 0; sampindex < 0x3c0; sampindex++)
                {
                    int remainder = smallstep;
                    int tpos = 0;
                    int scale;
                    scale = (int)((0x400000 - basefrac) >> 14);
                    sample = MSM5205.mm1[1].voice.stream.streamoutput_Ptrs[0][offset + tpos] * scale;
                    tpos++;
                    remainder -= scale;
                    while (remainder > 0x100)
                    {
                        sample += MSM5205.mm1[1].voice.stream.streamoutput_Ptrs[0][offset + tpos] * 0x100;
                        tpos++;
                        remainder -= 0x100;
                    }
                    sample += MSM5205.mm1[1].voice.stream.streamoutput_Ptrs[0][offset + tpos] * remainder;
                    sample /= smallstep;
                    mixerstream.streaminput_Ptrs[minput][sampindex] = (sample * gain) >> 8;
                    basefrac += step;
                    offset += (int)(basefrac >> 22);
                    basefrac &= 0x3fffff;
                }
            }
        }
        public static void streams_updateC()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            okistream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateQ()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            qsoundstream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateDataeast_pcktgal()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            YM2203.FF2203[0].stream.adjuststream(second_tick);
            ym3812stream.adjuststream(second_tick);
            MSM5205.mm1[0].voice.stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
            AY8910.AA8910[0].stream.updatesamplerate();
        }
        private static void streams_updateTehkan()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            AY8910.AA8910[1].stream.adjuststream(second_tick);
            AY8910.AA8910[2].stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
            AY8910.AA8910[0].stream.updatesamplerate();
            AY8910.AA8910[1].stream.updatesamplerate();
            AY8910.AA8910[2].stream.updatesamplerate();
        }
        private static void streams_updateN()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            ym2610stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
            AY8910.AA8910[0].stream.updatesamplerate();
        }
        private static void streams_updateSunA8()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym3812stream.adjuststream(second_tick);
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            samplestream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
            //AY8910.AA8910[0].stream.updatesamplerate();
        }
        private static void streams_updateNa()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            namcostream.adjuststream(second_tick);
            dacstream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateIGS011_drgnwrld()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            okistream.adjuststream(second_tick);
            ym3812stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateIGS011_lhb()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            okistream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateIGS011_lhb2()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            okistream.adjuststream(second_tick);
            ym2413stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateIGS011_vbowl()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ics2115stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updatePGM()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ics2115stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateM72()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            dacstream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateM92()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            iremga20stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateTaito_tokio()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            YM2203.FF2203[0].stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateTaito_bublbobl()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            YM2203.FF2203[0].stream.adjuststream(second_tick);
            ym3526stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
            AY8910.AA8910[0].stream.updatesamplerate();
        }
        private static void streams_updateKonami68000_cuebrick()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateKonami68000_mia()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            k007232stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateKonami68000_tmnt()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            k007232stream.adjuststream(second_tick);
            upd7759stream.adjuststream(second_tick);
            samplestream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateKonami68000_glfgreat()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            k053260stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateKonami68000_ssriders()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            k053260stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateKonami68000_prmrsocr()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            k054539stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
        private static void streams_updateCapcom_gng()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            AY8910.AA8910[0].stream.adjuststream(second_tick);
            AY8910.AA8910[1].stream.adjuststream(second_tick);
            YM2203.FF2203[0].stream.adjuststream(second_tick);
            YM2203.FF2203[1].stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
            AY8910.AA8910[0].stream.updatesamplerate();
            AY8910.AA8910[1].stream.updatesamplerate();
        }
        private static void streams_updateCapcom_sf()
        {
            Atime curtime = EmuTimer.global_basetime;
            bool second_tick = false;
            if (curtime.seconds != last_update_second)
            {
                second_tick = true;
            }
            ym2151stream.adjuststream(second_tick);
            MSM5205.mm1[0].voice.stream.adjuststream(second_tick);
            MSM5205.mm1[1].voice.stream.adjuststream(second_tick);
            mixerstream.adjuststream(second_tick);
            last_update_second = curtime.seconds;
        }
    }
}
