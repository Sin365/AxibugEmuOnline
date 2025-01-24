using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MAME.Core
{
    public unsafe class Sample
    {
        public struct sample_channel
        {
            //public short[] source;

            #region //指针化 source
            short[] source_src;
            GCHandle source_handle;
            public short* source;
            public int sourceLength;
            public bool source_IsNull => source == null;
            public short[] source_set
            {
                set
                {
                    source_handle.ReleaseGCHandle();
                    if (value == null)
                        return;
                    source_src = value;
                    sourceLength = value.Length;
                    source_src.GetObjectPtr(ref source_handle, ref source);
                }
            }
            #endregion

            public int source_length;
            public int source_num;
            public uint pos;
            public uint frac;
            public uint step;
            public uint basefreq;
            public byte loop;
            public byte paused;
        }
        public struct samples_info
        {
            public int numchannels;
            public sample_channel[] channel;
            public loaded_samples[] samples;
            public starthandler starthandler;
        }
        public struct loaded_sample
        {
            public int length;
            public int frequency;
        }
        public struct loaded_samples
        {
            public int total;
            public loaded_sample[] sample;
        }
        public static samples_info info = new samples_info();
        public delegate void starthandler();
        public static void sample_start_raw_n(int num, int channel, short* sampledata, int samples, int frequency, int loop)
        {
            Sound.samplestream.stream_update();
            info.channel[channel].source_length = samples;
            info.channel[channel].source_set = new short[samples];
            AxiArray.Copy(sampledata, 0, info.channel[channel].source, 0, samples);
            info.channel[channel].source_num = -1;
            info.channel[channel].pos = 0;
            info.channel[channel].frac = 0;
            info.channel[channel].basefreq = (uint)frequency;
            info.channel[channel].step = (uint)(((long)info.channel[channel].basefreq << 24) / 48000);
            info.channel[channel].loop = (byte)loop;
        }
        public static void sample_start_raw(int channel, short* sampledata, int samples, int frequency, int loop)
        {
            sample_start_raw_n(0, channel, sampledata, samples, frequency, loop);
        }
        public static void sample_stop_n(int num, int channel)
        {
            Sound.samplestream.stream_update();
            info.channel[channel].source = null;
            info.channel[channel].source_num = -1;
        }
        public static void sample_stop(int channel)
        {
            sample_stop_n(0, channel);
        }
        public static int sample_playing_n(int num, int channel)
        {
            Sound.samplestream.stream_update();
            return (info.channel[channel].source != null) ? 1 : 0;
        }
        public static int sample_playing(int channel)
        {
            return sample_playing_n(0, channel);
        }
        public static void sample_update_sound(int offset, int length)
        {
            int i, j;
            if (info.channel[0].source != null && info.channel[0].paused == 0)
            {
                uint pos = info.channel[0].pos;
                uint frac = info.channel[0].frac;
                uint step = info.channel[0].step;
                int sample_length = info.channel[0].source_length;
                for (i = 0; i < length; i++)
                {
                    int sample1 = info.channel[0].source[pos];
                    int sample2 = info.channel[0].source[(pos + 1) % sample_length];
                    int fracmult = (int)(frac >> (24 - 14));
                    Sound.samplestream.streamoutput_Ptrs[0][offset + i] = ((0x4000 - fracmult) * sample1 + fracmult * sample2) >> 14;
                    frac += step;
                    pos += frac >> 24;
                    frac = frac & ((1 << 24) - 1);
                    if (pos >= sample_length)
                    {
                        if (info.channel[0].loop != 0)
                        {
                            pos %= (uint)sample_length;
                        }
                        else
                        {
                            info.channel[0].source = null;
                            info.channel[0].source_num = -1;
                            if (i + 1 < length)
                            {
                                for (j = i + 1; j < length; j++)
                                {
                                    Sound.samplestream.streamoutput_Ptrs[0][offset + j] = 0;
                                }
                            }
                            break;
                        }
                    }
                }
                info.channel[0].pos = pos;
                info.channel[0].frac = frac;
            }
            else
            {
                for (i = 0; i < length; i++)
                {
                    Sound.samplestream.streamoutput_Ptrs[0][offset + i] = 0;
                }
            }
        }
        public static void samples_start()
        {
            int i;
            info.numchannels = 1;
            info.channel = new sample_channel[info.numchannels];
            for (i = 0; i < info.numchannels; i++)
            {
                info.channel[i].source = null;
                info.channel[i].source_num = -1;
                info.channel[i].step = 0;
                info.channel[i].loop = 0;
                info.channel[i].paused = 0;
            }
            switch (Machine.sName)
            {
                case "starfigh":
                    info.starthandler = SunA8.suna8_sh_start;
                    break;
                case "tmnt":
                case "tmntu":
                case "tmntua":
                case "tmntub":
                case "tmht":
                case "tmhta":
                case "tmhtb":
                case "tmntj":
                case "tmnta":
                case "tmht2p":
                case "tmht2pa":
                case "tmnt2pj":
                case "tmnt2po":
                    info.starthandler = Konami68000.tmnt_decode_sample;
                    break;
                default:
                    info.starthandler = null;
                    break;
            }
            if (info.starthandler != null)
            {
                info.starthandler();
            }
        }
        public static void SaveStateBinary(BinaryWriter writer)
        {
            int i;
            for (i = 0; i < info.numchannels; i++)
            {
                writer.Write(info.channel[i].source_length);
                writer.Write(info.channel[i].source_num);
                writer.Write(info.channel[i].pos);
                writer.Write(info.channel[i].frac);
                writer.Write(info.channel[i].step);
                writer.Write(info.channel[i].basefreq);
                writer.Write(info.channel[i].loop);
                writer.Write(info.channel[i].paused);
            }
        }
        public static void LoadStateBinary(BinaryReader reader)
        {
            int i;
            for (i = 0; i < info.numchannels; i++)
            {
                info.channel[i].source_length = reader.ReadInt32();
                info.channel[i].source_num = reader.ReadInt32();
                info.channel[i].pos = reader.ReadUInt32();
                info.channel[i].frac = reader.ReadUInt32();
                info.channel[i].step = reader.ReadUInt32();
                info.channel[i].basefreq = reader.ReadUInt32();
                info.channel[i].loop = reader.ReadByte();
                info.channel[i].paused = reader.ReadByte();
            }
        }
    }
}
