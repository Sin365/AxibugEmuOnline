
namespace MAME.Core
{
    public unsafe class Iremga20
    {
        public struct IremGA20_channel_def
        {
            public int rate;
            public int size;
            public int start;
            public int pos;
            public int frac;
            public int end;
            public int volume;
            public int pan;
            public int effect;
            public int play;
        };
        public struct IremGA20_chip_def
        {
            public ushort[] regs;
            public IremGA20_channel_def[] channel;
        };
        public static IremGA20_chip_def chip;
        public static byte[] iremrom;


        static int[] update_rate = new int[4], update_pos = new int[4], update_frac = new int[4], update_end = new int[4], update_vol = new int[4], update_play = new int[4];

        public static void iremga20_update(int offset, int length)
        {
            //int[] rate = new int[4], pos = new int[4], frac = new int[4], end = new int[4], vol = new int[4], play = new int[4];
            int i, sampleout;
            for (i = 0; i < 4; i++)
            {
                update_rate[i] = chip.channel[i].rate;
                update_pos[i] = chip.channel[i].pos;
                update_frac[i] = chip.channel[i].frac;
                update_end[i] = chip.channel[i].end - 0x20;
                update_vol[i] = chip.channel[i].volume;
                update_play[i] = chip.channel[i].play;
            }
            i = length;
            for (i = 0; i < length; i++)
            {
                sampleout = 0;
                if (update_play[0] != 0)
                {
                    sampleout += (iremrom[update_pos[0]] - 0x80) * update_vol[0];
                    update_frac[0] += update_rate[0];
                    update_pos[0] += update_frac[0] >> 24;
                    update_frac[0] &= 0xffffff;
                    update_play[0] = update_pos[0] < update_end[0] ? 1 : 0;
                }
                if (update_play[1] != 0)
                {
                    sampleout += (iremrom[update_pos[1]] - 0x80) * update_vol[1];
                    update_frac[1] += update_rate[1];
                    update_pos[1] += update_frac[1] >> 24;
                    update_frac[1] &= 0xffffff;
                    update_play[1] = update_pos[1] < update_end[1] ? 1 : 0;
                }
                if (update_play[2] != 0)
                {
                    sampleout += (iremrom[update_pos[2]] - 0x80) * update_vol[2];
                    update_frac[2] += update_rate[2];
                    update_pos[2] += update_frac[2] >> 24;
                    update_frac[2] &= 0xffffff;
                    update_play[2] = update_pos[2] < update_end[2] ? 1 : 0;
                }
                if (update_play[3] != 0)
                {
                    sampleout += (iremrom[update_pos[3]] - 0x80) * update_vol[3];
                    update_frac[3] += update_rate[3];
                    update_pos[3] += update_frac[3] >> 24;
                    update_frac[3] &= 0xffffff;
                    update_play[3] = update_pos[3] < update_end[3] ? 1 : 0;
                }
                sampleout >>= 2;
                Sound.iremga20stream.streamoutput_Ptrs[0][offset + i] = sampleout;
                Sound.iremga20stream.streamoutput_Ptrs[1][offset + i] = sampleout;
            }
            for (i = 0; i < 4; i++)
            {
                chip.channel[i].pos = update_pos[i];
                chip.channel[i].frac = update_frac[i];
                chip.channel[i].play = update_play[i];
            }
        }
        public static void irem_ga20_w(int offset, ushort data)
        {
            int channel;
            Sound.iremga20stream.stream_update();
            channel = offset >> 3;
            chip.regs[offset] = data;
            switch (offset & 0x7)
            {
                case 0:
                    chip.channel[channel].start = ((chip.channel[channel].start) & 0xff000) | (data << 4);
                    break;
                case 1:
                    chip.channel[channel].start = ((chip.channel[channel].start) & 0x00ff0) | (data << 12);
                    break;
                case 2:
                    chip.channel[channel].end = ((chip.channel[channel].end) & 0xff000) | (data << 4);
                    break;
                case 3:
                    chip.channel[channel].end = ((chip.channel[channel].end) & 0x00ff0) | (data << 12);
                    break;
                case 4:
                    chip.channel[channel].rate = 0x1000000 / (256 - data);
                    break;
                case 5:
                    chip.channel[channel].volume = (data * 0x100) / (data + 10);
                    break;
                case 6:
                    chip.channel[channel].play = data;
                    chip.channel[channel].pos = chip.channel[channel].start;
                    chip.channel[channel].frac = 0;
                    break;
            }
        }
        public static ushort irem_ga20_r(int offset)
        {
            int channel;
            ushort result;
            Sound.iremga20stream.stream_update();
            channel = offset >> 3;
            switch (offset & 0x7)
            {
                case 7:
                    result = (ushort)(chip.channel[channel].play != 0 ? 1 : 0);
                    break;
                default:
                    result = 0;
                    break;
            }
            return result;
        }
        public static void iremga20_reset()
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                chip.channel[i].rate = 0;
                chip.channel[i].size = 0;
                chip.channel[i].start = 0;
                chip.channel[i].pos = 0;
                chip.channel[i].frac = 0;
                chip.channel[i].end = 0;
                chip.channel[i].volume = 0;
                chip.channel[i].pan = 0;
                chip.channel[i].effect = 0;
                chip.channel[i].play = 0;
            }
        }
        public static void iremga20_start()
        {
            int i;
            chip.regs = new ushort[0x40];
            chip.channel = new IremGA20_channel_def[4];
            iremga20_reset();
            for (i = 0; i < 0x40; i++)
            {
                chip.regs[i] = 0;
            }
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                writer.Write(chip.channel[i].rate);
                writer.Write(chip.channel[i].size);
                writer.Write(chip.channel[i].start);
                writer.Write(chip.channel[i].pos);
                writer.Write(chip.channel[i].frac);
                writer.Write(chip.channel[i].end);
                writer.Write(chip.channel[i].volume);
                writer.Write(chip.channel[i].pan);
                writer.Write(chip.channel[i].effect);
                writer.Write(chip.channel[i].play);
            }
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                chip.channel[i].rate = reader.ReadInt32();
                chip.channel[i].size = reader.ReadInt32();
                chip.channel[i].start = reader.ReadInt32();
                chip.channel[i].pos = reader.ReadInt32();
                chip.channel[i].frac = reader.ReadInt32();
                chip.channel[i].end = reader.ReadInt32();
                chip.channel[i].volume = reader.ReadInt32();
                chip.channel[i].pan = reader.ReadInt32();
                chip.channel[i].effect = reader.ReadInt32();
                chip.channel[i].play = reader.ReadInt32();
            }
        }
    }
}
