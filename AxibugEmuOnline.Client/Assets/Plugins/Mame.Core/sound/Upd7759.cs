
namespace MAME.Core
{
    public unsafe class Upd7759
    {
        public struct upd7759_chip
        {
            public uint pos;
            public uint step;
            public Atime clock_period;
            public EmuTimer.emu_timer timer;

            public byte fifo_in;
            public byte reset;
            public byte start;
            public byte drq;
            public drqcallback drqcallback;

            public sbyte state;
            public int clocks_left;
            public ushort nibbles_left;
            public byte repeat_count;
            public sbyte post_drq_state;
            public int post_drq_clocks;
            public byte req_sample;
            public byte last_sample;
            public byte block_header;
            public byte sample_rate;
            public byte first_valid_header;
            public uint offset;
            public uint repeat_offset;

            public sbyte adpcm_state;
            public byte adpcm_data;
            public short sample;

            public int rombase;
            public uint romoffset;
        }
        public static int[,] upd7759_step = new int[16, 16]
        {
            { 0,  0,  1,  2,  3,   5,   7,  10,  0,   0,  -1,  -2,  -3,   -5,   -7,  -10 },
            { 0,  1,  2,  3,  4,   6,   8,  13,  0,  -1,  -2,  -3,  -4,   -6,   -8,  -13 },
            { 0,  1,  2,  4,  5,   7,  10,  15,  0,  -1,  -2,  -4,  -5,   -7,  -10,  -15 },
            { 0,  1,  3,  4,  6,   9,  13,  19,  0,  -1,  -3,  -4,  -6,   -9,  -13,  -19 },
            { 0,  2,  3,  5,  8,  11,  15,  23,  0,  -2,  -3,  -5,  -8,  -11,  -15,  -23 },
            { 0,  2,  4,  7, 10,  14,  19,  29,  0,  -2,  -4,  -7, -10,  -14,  -19,  -29 },
            { 0,  3,  5,  8, 12,  16,  22,  33,  0,  -3,  -5,  -8, -12,  -16,  -22,  -33 },
            { 1,  4,  7, 10, 15,  20,  29,  43, -1,  -4,  -7, -10, -15,  -20,  -29,  -43 },
            { 1,  4,  8, 13, 18,  25,  35,  53, -1,  -4,  -8, -13, -18,  -25,  -35,  -53 },
            { 1,  6, 10, 16, 22,  31,  43,  64, -1,  -6, -10, -16, -22,  -31,  -43,  -64 },
            { 2,  7, 12, 19, 27,  37,  51,  76, -2,  -7, -12, -19, -27,  -37,  -51,  -76 },
            { 2,  9, 16, 24, 34,  46,  64,  96, -2,  -9, -16, -24, -34,  -46,  -64,  -96 },
            { 3, 11, 19, 29, 41,  57,  79, 117, -3, -11, -19, -29, -41,  -57,  -79, -117 },
            { 4, 13, 24, 36, 50,  69,  96, 143, -4, -13, -24, -36, -50,  -69,  -96, -143 },
            { 4, 16, 29, 44, 62,  85, 118, 175, -4, -16, -29, -44, -62,  -85, -118, -175 },
            { 6, 20, 36, 54, 76, 104, 144, 214, -6, -20, -36, -54, -76, -104, -144, -214 },
        };
        public static int[] upd7759_state = new int[16] { -1, -1, 0, 0, 1, 2, 2, 3, -1, -1, 0, 0, 1, 2, 2, 3 };
        public static upd7759_chip chip;
        public static byte[] updrom;
        public delegate void drqcallback(int irq);


        public static void update_adpcm(int data)
        {
            chip.sample += (short)upd7759_step[chip.adpcm_state, data];
            chip.adpcm_state += (sbyte)upd7759_state[data];
            if (chip.adpcm_state < 0)
            {
                chip.adpcm_state = 0;
            }
            else if (chip.adpcm_state > 15)
            {
                chip.adpcm_state = 15;
            }
        }
        public static void advance_state()
        {
            switch (chip.state)
            {
                case 0:
                    chip.clocks_left = 4;
                    break;
                case 1:
                    chip.drq = 0;
                    chip.clocks_left = chip.post_drq_clocks;
                    chip.state = chip.post_drq_state;
                    break;
                case 2:
                    chip.req_sample = (byte)(updrom != null ? chip.fifo_in : 0x10);
                    chip.clocks_left = 70;
                    chip.state = 3;
                    break;
                case 3:
                    chip.drq = 1;
                    chip.clocks_left = 44;
                    chip.state = 4;
                    break;
                case 4:
                    chip.last_sample = updrom != null ? updrom[0] : chip.fifo_in;
                    chip.drq = 1;
                    chip.clocks_left = 28;
                    chip.state = (sbyte)((chip.req_sample > chip.last_sample) ? 0 : 5);
                    break;
                case 5:
                    chip.drq = 1;
                    chip.clocks_left = 32;
                    chip.state = 6;
                    break;
                case 6:
                    chip.offset = (uint)((updrom != null ? updrom[chip.req_sample * 2 + 5] : chip.fifo_in) << 9);
                    chip.drq = 1;
                    chip.clocks_left = 44;
                    chip.state = 7;
                    break;
                case 7:
                    chip.offset |= (uint)((updrom != null ? updrom[chip.req_sample * 2 + 6] : chip.fifo_in) << 1);
                    chip.drq = 1;
                    chip.clocks_left = 36;
                    chip.state = 8;
                    break;
                case 8:
                    chip.offset++;
                    chip.first_valid_header = 0;
                    chip.drq = 1;
                    chip.clocks_left = 36;
                    chip.state = 9;
                    break;
                case 9:
                    if (chip.repeat_count != 0)
                    {
                        chip.repeat_count--;
                        chip.offset = chip.repeat_offset;
                    }
                    chip.block_header = updrom != null ? updrom[chip.offset++ & 0x1ffff] : chip.fifo_in;
                    chip.drq = 1;
                    switch (chip.block_header & 0xc0)
                    {
                        case 0x00:
                            chip.clocks_left = 1024 * ((chip.block_header & 0x3f) + 1);
                            chip.state = (sbyte)((chip.block_header == 0 && chip.first_valid_header != 0) ? 0 : 9);
                            chip.sample = 0;
                            chip.adpcm_state = 0;
                            break;
                        case 0x40:
                            chip.sample_rate = (byte)((chip.block_header & 0x3f) + 1);
                            chip.nibbles_left = 256;
                            chip.clocks_left = 36;
                            chip.state = 11;
                            break;
                        case 0x80:
                            chip.sample_rate = (byte)((chip.block_header & 0x3f) + 1);
                            chip.clocks_left = 36;
                            chip.state = 10;
                            break;
                        case 0xc0:
                            chip.repeat_count = (byte)((chip.block_header & 7) + 1);
                            chip.repeat_offset = chip.offset;
                            chip.clocks_left = 36;
                            chip.state = 9;
                            break;
                    }
                    if (chip.block_header != 0)
                    {
                        chip.first_valid_header = 1;
                    }
                    break;
                case 10:
                    chip.nibbles_left = (ushort)((updrom != null ? updrom[chip.offset++ & 0x1ffff] : chip.fifo_in) + 1);
                    chip.drq = 1;
                    chip.clocks_left = 36;
                    chip.state = 11;
                    break;
                case 11:
                    chip.adpcm_data = updrom != null ? updrom[chip.offset++ & 0x1ffff] : chip.fifo_in;
                    update_adpcm(chip.adpcm_data >> 4);
                    chip.drq = 1;
                    chip.clocks_left = chip.sample_rate * 4;
                    if (--chip.nibbles_left == 0)
                    {
                        chip.state = 9;
                    }
                    else
                    {
                        chip.state = 12;
                    }
                    break;
                case 12:
                    update_adpcm(chip.adpcm_data & 15);
                    chip.clocks_left = chip.sample_rate * 4;
                    if (--chip.nibbles_left == 0)
                    {
                        chip.state = 9;
                    }
                    else
                    {
                        chip.state = 11;
                    }
                    break;
            }
            if (chip.drq != 0)
            {
                chip.post_drq_state = chip.state;
                chip.post_drq_clocks = chip.clocks_left - 21;
                chip.state = 1;
                chip.clocks_left = 21;
            }
        }
        public static void upd7759_update(int offset, int length)
        {
            int clocks_left = chip.clocks_left;
            short sample = chip.sample;
            uint step = chip.step;
            uint pos = chip.pos;
            int i = 0, j;
            if (chip.state != 0)
            {
                for (i = 0; i < length; i++)
                {
                    Sound.upd7759stream.streamoutput_Ptrs[0][offset + i] = sample << 7;
                    pos += step;
                    while (updrom != null && pos >= 0x100000)
                    {
                        int clocks_this_time = (int)(pos >> 20);
                        if (clocks_this_time > clocks_left)
                        {
                            clocks_this_time = clocks_left;
                        }
                        pos -= (uint)(clocks_this_time * 0x100000);
                        clocks_left -= clocks_this_time;
                        if (clocks_left == 0)
                        {
                            advance_state();
                            if (chip.state == 0)
                            {
                                break;
                            }
                            clocks_left = chip.clocks_left;
                            sample = chip.sample;
                        }
                    }
                }
            }
            if (i < length - 1)
            {
                for (j = i; j < length; j++)
                {
                    Sound.upd7759stream.streamoutput_Ptrs[0][offset + j] = 0;
                }
            }
            chip.clocks_left = clocks_left;
            chip.pos = pos;
        }
        public static void upd7759_slave_update()
        {
            byte olddrq = chip.drq;
            Sound.upd7759stream.stream_update();
            //stream_update(chip.channel);
            advance_state();
            if (olddrq != chip.drq && chip.drqcallback != null)
            {
                chip.drqcallback(chip.drq);
            }
            if (chip.state != 0)
            {
                EmuTimer.timer_adjust_periodic(chip.timer, Attotime.attotime_mul(chip.clock_period, (uint)chip.clocks_left), Attotime.ATTOTIME_NEVER);
            }
        }
        public static void upd7759_reset()
        {
            chip.pos = 0;
            chip.fifo_in = 0;
            chip.drq = 0;
            chip.state = 0;
            chip.clocks_left = 0;
            chip.nibbles_left = 0;
            chip.repeat_count = 0;
            chip.post_drq_state = 0;
            chip.post_drq_clocks = 0;
            chip.req_sample = 0;
            chip.last_sample = 0;
            chip.block_header = 0;
            chip.sample_rate = 0;
            chip.first_valid_header = 0;
            chip.offset = 0;
            chip.repeat_offset = 0;
            chip.adpcm_state = 0;
            chip.adpcm_data = 0;
            chip.sample = 0;
            if (chip.timer != null)
            {
                EmuTimer.timer_adjust_periodic(chip.timer, Attotime.ATTOTIME_NEVER, Attotime.ATTOTIME_NEVER);
            }
        }
        public static void upd7759_start(int clock)
        {
            chip = new upd7759_chip();
            chip.step = 4 * 0x100000;
            chip.clock_period = new Atime(0, (long)(1e18 / clock));
            chip.state = 0;
            chip.rombase = 0;
            if (updrom == null)
            {
                //chip.timer = EmuTimer.timer_alloc_common(EmuTimer.TIME_ACT.Upd7759_upd7759_slave_update, false);
                EmuTimer.timer_alloc_common(ref chip.timer, EmuTimer.TIME_ACT.Upd7759_upd7759_slave_update, false);
            }
            chip.reset = 1;
            chip.start = 1;
            upd7759_reset();
        }
        public static void upd7759_reset_w(int which, byte data)
        {
            byte oldreset = chip.reset;
            chip.reset = (byte)((data != 0) ? 1 : 0);
            Sound.upd7759stream.stream_update();
            if (oldreset != 0 && chip.reset == 0)
            {
                upd7759_reset();
            }
        }
        public static void upd7759_start_w(int which, byte data)
        {
            byte oldstart = chip.start;
            chip.start = (byte)((data != 0) ? 1 : 0);
            Sound.upd7759stream.stream_update();
            if (chip.state == 0 && oldstart == 0 && chip.start != 0 && chip.reset != 0)
            {
                chip.state = 2;
                if (chip.timer != null)
                {
                    EmuTimer.timer_adjust_periodic(chip.timer, Attotime.ATTOTIME_ZERO, Attotime.ATTOTIME_NEVER);
                }
            }
        }
        public static void upd7759_port_w(int which, byte data)
        {
            chip.fifo_in = data;
        }
        public static int upd7759_busy_r(int which)
        {
            return (chip.state == 0) ? 1 : 0;
        }
        public static void upd7759_set_bank_base(int which, uint base1)
        {
            //chip.rom = chip.rombase + base1;
            chip.romoffset = base1;
        }
        public static void upd7759_0_start_w(byte data)
        {
            upd7759_start_w(0, data);
        }
        public static void upd7759_0_reset_w(byte data)
        {
            upd7759_reset_w(0, data);
        }
        public static void upd7759_0_port_w(byte data)
        {
            upd7759_port_w(0, data);
        }
        public static byte upd7759_0_busy_r()
        {
            return (byte)upd7759_busy_r(0);
        }
        public static void SaveStateBinary(System.IO.BinaryWriter writer)
        {
            writer.Write(chip.pos);
            writer.Write(chip.step);
            writer.Write(chip.fifo_in);
            writer.Write(chip.reset);
            writer.Write(chip.start);
            writer.Write(chip.drq);
            writer.Write(chip.state);
            writer.Write(chip.clocks_left);
            writer.Write(chip.nibbles_left);
            writer.Write(chip.repeat_count);
            writer.Write(chip.post_drq_state);
            writer.Write(chip.post_drq_clocks);
            writer.Write(chip.req_sample);
            writer.Write(chip.last_sample);
            writer.Write(chip.block_header);
            writer.Write(chip.sample_rate);
            writer.Write(chip.first_valid_header);
            writer.Write(chip.offset);
            writer.Write(chip.repeat_offset);
            writer.Write(chip.adpcm_state);
            writer.Write(chip.adpcm_data);
            writer.Write(chip.sample);
            writer.Write(chip.romoffset);
            writer.Write(chip.rombase);
        }
        public static void LoadStateBinary(System.IO.BinaryReader reader)
        {
            chip.pos = reader.ReadUInt32();
            chip.step = reader.ReadUInt32();
            chip.fifo_in = reader.ReadByte();
            chip.reset = reader.ReadByte();
            chip.start = reader.ReadByte();
            chip.drq = reader.ReadByte();
            chip.state = reader.ReadSByte();
            chip.clocks_left = reader.ReadInt32();
            chip.nibbles_left = reader.ReadUInt16();
            chip.repeat_count = reader.ReadByte();
            chip.post_drq_state = reader.ReadSByte();
            chip.post_drq_clocks = reader.ReadInt32();
            chip.req_sample = reader.ReadByte();
            chip.last_sample = reader.ReadByte();
            chip.block_header = reader.ReadByte();
            chip.sample_rate = reader.ReadByte();
            chip.first_valid_header = reader.ReadByte();
            chip.offset = reader.ReadUInt32();
            chip.repeat_offset = reader.ReadUInt32();
            chip.adpcm_state = reader.ReadSByte();
            chip.adpcm_data = reader.ReadByte();
            chip.sample = reader.ReadInt16();
            chip.romoffset = reader.ReadUInt32();
            chip.rombase = reader.ReadInt32();
        }
    }
}
