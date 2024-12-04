
using System;

namespace VirtualNes.Core
{
    public class APU_INTERNAL : APU_INTERFACE
    {
        // Volume shift
        public const int RECTANGLE_VOL_SHIFT = 8;
        public const int TRIANGLE_VOL_SHIFT = 9;
        public const int NOISE_VOL_SHIFT = 8;
        public const int DPCM_VOL_SHIFT = 8;

        // Tables
        static public int[] freq_limit = new int[8]
        {
            0x03FF, 0x0555, 0x0666, 0x071C, 0x0787, 0x07C1, 0x07E0, 0x07F0
        };
        static public int[] duty_lut = new int[4]
        {
            2,  4,  8, 12
        };
        static public int[] noise_freq = new int[16]{
                4,    8,   16,   32,   64,   96,  128,  160,
              202,  254,  380,  508,  762, 1016, 2034, 4068
        };

        private static int[] vbl_length = new int[32]
        {
            5,  127,   10,   1,   19,   2,   40,   3,
            80,   4,   30,   5,    7,   6,   13,   7,
            6,    8,   12,   9,   24,  10,   48,  11,
            96,  12,   36,  13,    8,  14,   16,  15,
        };

        private static int[] dpcm_cycles_pal = new int[16]
        {
            397, 353, 315, 297, 265, 235, 209, 198,
            176, 148, 131, 118,  98,  78,  66,  50,
        };

        private static int[] dpcm_cycles = new int[16]
        {
            428, 380, 340, 320, 286, 254, 226, 214,
            190, 160, 142, 128, 106,  85,  72,  54,
        };

        private NES nes;
        // Frame Counter
        private int FrameCycle;
        private int FrameCount;
        private int FrameType;
        private byte FrameIRQ;
        private byte FrameIRQoccur;

        // Channels
        private RECTANGLE ch0 = new RECTANGLE();
        private RECTANGLE ch1 = new RECTANGLE();
        private TRIANGLE ch2 = new TRIANGLE();
        private NOISE ch3 = new NOISE();
        private DPCM ch4 = new DPCM();

        // Sound
        private float cpu_clock;
        private int sampling_rate;
        private int cycle_rate;

        // $4015 Reg
        private byte reg4015, sync_reg4015;

        private const int TONEDATA_MAX = 16;
        private const int TONEDATA_LEN = 32;
        private const int CHANNEL_MAX = 3;
        private const int TONE_MAX = 4;

        bool[] bToneTableEnable = new bool[TONEDATA_MAX];
        int[,] ToneTable = new int[TONEDATA_MAX, TONEDATA_LEN];
        int[,] ChannelTone = new int[CHANNEL_MAX, TONE_MAX];

        public void SetParent(NES parent)
        {
            nes = parent;
        }

        public override bool Sync(int cycles)
        {
            FrameCycle -= cycles * 2;
            if (FrameCycle <= 0)
            {
                FrameCycle += 14915;

                UpdateFrame();
            }

            var result = FrameIRQoccur | (SyncUpdateDPCM(cycles) ? 1 : 0);
            return result != 0;
        }

        private bool SyncUpdateDPCM(int cycles)
        {
            bool bIRQ = false;

            if (ch4.sync_enable != 0)
            {
                ch4.sync_cycles -= cycles;
                while (ch4.sync_cycles < 0)
                {
                    ch4.sync_cycles += ch4.sync_cache_cycles;
                    if (ch4.sync_dmalength != 0)
                    {
                        //				if( !(--ch4.sync_dmalength) ) {
                        if (--ch4.sync_dmalength < 2)
                        {
                            if (ch4.sync_looping != 0)
                            {
                                ch4.sync_dmalength = ch4.sync_cache_dmalength;
                            }
                            else
                            {
                                ch4.sync_dmalength = 0;

                                if (ch4.sync_irq_gen != 0)
                                {
                                    ch4.sync_irq_enable = 0xFF;
                                    nes.cpu.SetIRQ(CPU.IRQ_DPCM);
                                }
                            }
                        }
                    }
                }
            }
            if (ch4.sync_irq_enable != 0)
            {
                bIRQ = true;
            }

            return bIRQ;
        }

        private void UpdateFrame()
        {
            if (FrameCount == 0)
            {
                if ((FrameIRQ & 0xC0) == 0 && nes.GetFrameIRQmode())
                {
                    FrameIRQoccur = 0xFF;
                    nes.cpu.SetIRQ(CPU.IRQ_FRAMEIRQ);
                }
            }

            if (FrameCount == 3)
            {
                if ((FrameIRQ & 0x80) != 0)
                {
                    FrameCycle += 14915;
                }
            }

            // Counters Update
            nes.Write(0x4018, (byte)FrameCount);

            FrameCount = (FrameCount + 1) & 3;
        }

        public override void Reset(float fClock, int nRate)
        {
            ch0.ZeroMemory();
            ch1.ZeroMemory();
            ch2.ZeroMemory();
            ch3.ZeroMemory();

            Array.Clear(bToneTableEnable, 0, bToneTableEnable.Length);
            Array.Clear(ToneTable, 0, ToneTable.Length);
            Array.Clear(ChannelTone, 0, ChannelTone.Length);

            reg4015 = sync_reg4015 = 0;

            // Sweep complement
            ch0.complement = 0x00;
            ch1.complement = 0xFF;

            // Noise shift register
            ch3.shift_reg = 0x4000;

            Setup(fClock, nRate);

            // $4011は初期化しない
            ushort addr;
            for (addr = 0x4000; addr <= 0x4010; addr++)
            {
                Write(addr, 0x00);
                SyncWrite(addr, 0x00);
            }
            //	Write( 0x4001, 0x08 );	// Reset時はincモードになる?
            //	Write( 0x4005, 0x08 );	// Reset時はincモードになる?
            Write(0x4012, 0x00);
            Write(0x4013, 0x00);
            Write(0x4015, 0x00);
            SyncWrite(0x4012, 0x00);
            SyncWrite(0x4013, 0x00);
            SyncWrite(0x4015, 0x00);

            // $4017は書き込みで初期化しない(初期モードが0であるのを期待したソフトがある為)
            FrameIRQ = 0xC0;
            FrameCycle = 0;
            FrameIRQoccur = 0;
            FrameCount = 0;
            FrameType = 0;
        }

        public override void Setup(float fClock, int nRate)
        {
            cpu_clock = fClock;
            sampling_rate = nRate;

            cycle_rate = (int)(fClock * 65536.0f / nRate);
        }

        public override void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                // CH0,1 rectangle
                case 0x4000:
                case 0x4001:
                case 0x4002:
                case 0x4003:
                case 0x4004:
                case 0x4005:
                case 0x4006:
                case 0x4007:
                    WriteRectangle((addr < 0x4004) ? 0 : 1, addr, data);
                    break;

                // CH2 triangle
                case 0x4008:
                case 0x4009:
                case 0x400A:
                case 0x400B:
                    WriteTriangle(addr, data);
                    break;

                // CH3 noise
                case 0x400C:
                case 0x400D:
                case 0x400E:
                case 0x400F:
                    WriteNoise(addr, data);
                    break;

                // CH4 DPCM
                case 0x4010:
                case 0x4011:
                case 0x4012:
                case 0x4013:
                    WriteDPCM(addr, data);
                    break;

                case 0x4015:
                    reg4015 = data;

                    if ((data & (1 << 0)) == 0)
                    {
                        ch0.enable = 0;
                        ch0.len_count = 0;
                    }
                    if ((data & (1 << 1)) == 0)
                    {
                        ch1.enable = 0;
                        ch1.len_count = 0;
                    }
                    if ((data & (1 << 2)) == 0)
                    {
                        ch2.enable = 0;
                        ch2.len_count = 0;
                        ch2.lin_count = 0;
                        ch2.counter_start = 0;
                    }
                    if ((data & (1 << 3)) == 0)
                    {
                        ch3.enable = 0;
                        ch3.len_count = 0;
                    }
                    if ((data & (1 << 4)) == 0)
                    {
                        ch4.enable = 0;
                        ch4.dmalength = 0;
                    }
                    else
                    {
                        ch4.enable = 0xFF;
                        if (ch4.dmalength == 0)
                        {
                            ch4.address = ch4.cache_addr;
                            ch4.dmalength = ch4.cache_dmalength;
                            ch4.phaseacc = 0;
                        }
                    }
                    break;

                case 0x4017:
                    break;

                // VirtuaNES固有ポート
                case 0x4018:
                    UpdateRectangle(ch0, data);
                    UpdateRectangle(ch1, data);
                    UpdateTriangle(data);
                    UpdateNoise(data);
                    break;

                default:
                    break;
            }
        }

        private void UpdateNoise(int type)
        {
            if (ch3.enable == 0 || ch3.len_count <= 0)
                return;

            // Update Length
            if (ch3.holdnote == 0)
            {
                // Holdnote
                if ((type & 1) == 0 && ch3.len_count != 0)
                {
                    ch3.len_count--;
                }
            }

            // Update Envelope
            if (ch3.env_count != 0)
            {
                ch3.env_count--;
            }
            if (ch3.env_count == 0)
            {
                ch3.env_count = ch3.env_decay;

                // Holdnote
                if (ch3.holdnote != 0)
                {
                    ch3.env_vol = (ch3.env_vol - 1) & 0x0F;
                }
                else if (ch3.env_vol != 0)
                {
                    ch3.env_vol--;
                }
            }

            if (ch3.env_fixed == 0)
            {
                ch3.nowvolume = ch3.env_vol << RECTANGLE_VOL_SHIFT;
            }
        }

        private void UpdateTriangle(int type)
        {
            if (ch2.enable == 0)
                return;

            if ((type & 1) == 0 && ch2.holdnote == 0)
            {
                if (ch2.len_count != 0)
                {
                    ch2.len_count--;
                }
            }

            //	if( !ch2.len_count ) {
            //		ch2.lin_count = 0;
            //	}

            // Update Length/Linear
            if (ch2.counter_start != 0)
            {
                ch2.lin_count = ch2.reg[0] & 0x7F;
            }
            else if (ch2.lin_count != 0)
            {
                ch2.lin_count--;
            }
            if (ch2.holdnote == 0 && ch2.lin_count != 0)
            {
                ch2.counter_start = 0;
            }
        }

        private void UpdateRectangle(RECTANGLE ch, int type)
        {
            if (ch.enable == 0 || ch.len_count <= 0)
                return;

            // Update Length/Sweep
            if ((type & 1) == 0)
            {
                // Update Length
                if (ch.len_count != 0 && ch.holdnote == 0)
                {
                    // Holdnote
                    if (ch.len_count != 0)
                    {
                        ch.len_count--;
                    }
                }

                // Update Sweep
                if (ch.swp_on != 0 && ch.swp_shift != 0)
                {
                    if (ch.swp_count != 0)
                    {
                        ch.swp_count--;
                    }
                    if (ch.swp_count == 0)
                    {
                        ch.swp_count = ch.swp_decay;
                        if (ch.swp_inc != 0)
                        {
                            // Sweep increment(to higher frequency)
                            if (ch.complement == 0)
                                ch.freq += ~(ch.freq >> ch.swp_shift); // CH 0
                            else
                                ch.freq -= (ch.freq >> ch.swp_shift); // CH 1
                        }
                        else
                        {
                            // Sweep decrement(to lower frequency)
                            ch.freq += (ch.freq >> ch.swp_shift);
                        }
                    }
                }
            }

            // Update Envelope
            if (ch.env_count != 0)
            {
                ch.env_count--;
            }
            if (ch.env_count == 0)
            {
                ch.env_count = ch.env_decay;

                // Holdnote
                if (ch.holdnote != 0)
                {
                    ch.env_vol = (ch.env_vol - 1) & 0x0F;
                }
                else if (ch.env_vol != 0)
                {
                    ch.env_vol--;
                }
            }

            if (ch.env_fixed == 0)
            {
                ch.nowvolume = ch.env_vol << RECTANGLE_VOL_SHIFT;
            }
        }

        private void WriteDPCM(ushort addr, byte data)
        {
            ch4.reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch4.freq = INT2FIX(nes.GetVideoMode() ? dpcm_cycles_pal[data & 0x0F] : dpcm_cycles[data & 0x0F]);
                    //			ch4.freq    = INT2FIX( dpcm_cycles[data&0x0F] );
                    ////			ch4.freq    = INT2FIX( (dpcm_cycles[data&0x0F]-((data&0x0F)^0x0F)*2-2) );
                    ch4.looping = (byte)(data & 0x40);
                    break;
                case 1:
                    ch4.dpcm_value = (byte)((data & 0x7F) >> 1);
                    break;
                case 2:
                    ch4.cache_addr = (ushort)(0xC000 + (ushort)(data << 6));
                    break;
                case 3:
                    ch4.cache_dmalength = ((data << 4) + 1) << 3;
                    break;
            }
        }

        private void WriteNoise(ushort addr, byte data)
        {
            ch3.reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch3.holdnote = (byte)(data & 0x20);
                    ch3.volume = (byte)(data & 0x0F);
                    ch3.env_fixed = (byte)(data & 0x10);
                    ch3.env_decay = (byte)((data & 0x0F) + 1);
                    break;
                case 1: // Unused
                    break;
                case 2:
                    ch3.freq = INT2FIX(noise_freq[data & 0x0F]);
                    ch3.xor_tap = (byte)((data & 0x80) != 0 ? 0x40 : 0x02);
                    break;
                case 3: // Master
                    ch3.len_count = vbl_length[data >> 3] * 2;
                    ch3.env_vol = 0x0F;
                    ch3.env_count = (byte)(ch3.env_decay + 1);

                    if ((reg4015 & (1 << 3)) != 0)
                        ch3.enable = 0xFF;
                    break;
            }
        }

        private void WriteTriangle(ushort addr, byte data)
        {
            ch2.reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch2.holdnote = (byte)(data & 0x80);
                    break;
                case 1: // Unused
                    break;
                case 2:
                    ch2.freq = INT2FIX(((ch2.reg[3] & 0x07) << 8) + data + 1);
                    break;
                case 3: // Master
                    ch2.freq = INT2FIX((((data & 0x07) << 8) + ch2.reg[2] + 1));
                    ch2.len_count = vbl_length[data >> 3] * 2;
                    ch2.counter_start = 0x80;

                    if ((reg4015 & (1 << 2)) != 0)
                        ch2.enable = 0xFF;
                    break;
            }
        }

        private void WriteRectangle(int no, ushort addr, byte data)
        {
            RECTANGLE ch = (no == 0) ? ch0 : ch1;

            ch.reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch.holdnote = (byte)(data & 0x20);
                    ch.volume = (byte)(data & 0x0F);
                    ch.env_fixed = (byte)(data & 0x10);
                    ch.env_decay = (byte)((data & 0x0F) + 1);
                    ch.duty = duty_lut[data >> 6];
                    break;
                case 1:
                    ch.swp_on = (byte)(data & 0x80);
                    ch.swp_inc = (byte)(data & 0x08);
                    ch.swp_shift = (byte)(data & 0x07);
                    ch.swp_decay = (byte)(((data >> 4) & 0x07) + 1);
                    ch.freqlimit = freq_limit[data & 0x07];
                    break;
                case 2:
                    ch.freq = (ch.freq & (~0xFF)) + data;
                    break;
                case 3: // Master
                    ch.freq = ((data & 0x07) << 8) + (ch.freq & 0xFF);
                    ch.len_count = vbl_length[data >> 3] * 2;
                    ch.env_vol = 0x0F;
                    ch.env_count = (byte)(ch.env_decay + 1);
                    ch.adder = 0;

                    if ((reg4015 & (1 << no)) != 0)
                        ch.enable = 0xFF;
                    break;
            }
        }

        public override int Process(int channel)
        {
            switch (channel)
            {
                case 0:
                    return RenderRectangle(ch0);
                case 1:
                    return RenderRectangle(ch1);
                case 2:
                    return RenderTriangle();
                case 3:
                    return RenderNoise();
                case 4:
                    return RenderDPCM();
                default:
                    return 0;
            }
        }

        private int RenderDPCM()
        {
            if (ch4.dmalength != 0)
            {
                ch4.phaseacc -= cycle_rate;

                while (ch4.phaseacc < 0)
                {
                    ch4.phaseacc += ch4.freq;
                    if ((ch4.dmalength & 7) == 0)
                    {
                        ch4.cur_byte = nes.Read(ch4.address);
                        if (0xFFFF == ch4.address)
                            ch4.address = 0x8000;
                        else
                            ch4.address++;
                    }

                    if ((--ch4.dmalength) == 0)
                    {
                        if (ch4.looping != 0)
                        {
                            ch4.address = ch4.cache_addr;
                            ch4.dmalength = ch4.cache_dmalength;
                        }
                        else
                        {
                            ch4.enable = 0;
                            break;
                        }
                    }
                    // positive delta
                    if ((ch4.cur_byte & (1 << ((ch4.dmalength & 7) ^ 7))) != 0)
                    {
                        if (ch4.dpcm_value < 0x3F)
                            ch4.dpcm_value += 1;
                    }
                    else
                    {
                        // negative delta
                        if (ch4.dpcm_value > 1)
                            ch4.dpcm_value -= 1;
                    }
                }
            }

            // インチキ臭いプチノイズカット(TEST)
            ch4.dpcm_output_real = ((ch4.reg[1] & 0x01) + ch4.dpcm_value * 2) - 0x40;
            if (Math.Abs(ch4.dpcm_output_real - ch4.dpcm_output_fake) <= 8)
            {
                ch4.dpcm_output_fake = ch4.dpcm_output_real;
                ch4.output = ch4.dpcm_output_real << DPCM_VOL_SHIFT;
            }
            else
            {
                if (ch4.dpcm_output_real > ch4.dpcm_output_fake)
                    ch4.dpcm_output_fake += 8;
                else
                    ch4.dpcm_output_fake -= 8;
                ch4.output = ch4.dpcm_output_fake << DPCM_VOL_SHIFT;
            }
            return ch4.output;
        }

        private int RenderNoise()
        {
            if (ch3.enable == 0 || ch3.len_count <= 0)
                return 0;

            if (ch3.env_fixed != 0)
            {
                ch3.nowvolume = ch3.volume << RECTANGLE_VOL_SHIFT;
            }

            int vol = 256 - ((ch4.reg[1] & 0x01) + ch4.dpcm_value * 2);

            ch3.phaseacc -= cycle_rate;
            if (ch3.phaseacc >= 0)
                return ch3.output * vol / 256;

            if (ch3.freq > cycle_rate)
            {
                ch3.phaseacc += ch3.freq;
                if (NoiseShiftreg(ch3.xor_tap))
                    ch3.output = ch3.nowvolume;
                else
                    ch3.output = -ch3.nowvolume;

                return ch3.output * vol / 256;
            }

            int num_times, total;
            num_times = total = 0;
            while (ch3.phaseacc < 0)
            {
                ch3.phaseacc += ch3.freq;
                if (NoiseShiftreg(ch3.xor_tap))
                    ch3.output = ch3.nowvolume;
                else
                    ch3.output = -ch3.nowvolume;

                total += ch3.output;
                num_times++;
            }

            return (total / num_times) * vol / 256;
        }

        private bool NoiseShiftreg(byte xor_tap)
        {
            int bit0, bit14;

            bit0 = ch3.shift_reg & 1;
            if ((ch3.shift_reg & xor_tap) != 0) bit14 = bit0 ^ 1;
            else bit14 = bit0 ^ 0;
            ch3.shift_reg >>= 1;
            ch3.shift_reg |= (bit14 << 14);
            return (bit0 ^ 1) != 0;
        }

        private int RenderTriangle()
        {
            int vol;
            if (Supporter.Config.sound.bDisableVolumeEffect)
            {
                vol = 256;
            }
            else
            {
                vol = 256 - ((ch4.reg[1] & 0x01) + ch4.dpcm_value * 2);
            }

            if (ch2.enable == 0 || (ch2.len_count <= 0) || (ch2.lin_count <= 0))
            {
                return ch2.nowvolume * vol / 256;
            }

            if (ch2.freq < INT2FIX(8))
            {
                return ch2.nowvolume * vol / 256;
            }

            if (!(Supporter.Config.sound.bChangeTone && ChannelTone[2, 0] != 0))
            {
                ch2.phaseacc -= cycle_rate;
                if (ch2.phaseacc >= 0)
                {
                    return ch2.nowvolume * vol / 256;
                }

                if (ch2.freq > cycle_rate)
                {
                    ch2.phaseacc += ch2.freq;
                    ch2.adder = (ch2.adder + 1) & 0x1F;

                    if (ch2.adder < 0x10)
                    {
                        ch2.nowvolume = (ch2.adder & 0x0F) << TRIANGLE_VOL_SHIFT;
                    }
                    else
                    {
                        ch2.nowvolume = (0x0F - (ch2.adder & 0x0F)) << TRIANGLE_VOL_SHIFT;
                    }

                    return ch2.nowvolume * vol / 256;
                }

                // 加重平均
                int num_times, total;
                num_times = total = 0;
                while (ch2.phaseacc < 0)
                {
                    ch2.phaseacc += ch2.freq;
                    ch2.adder = (ch2.adder + 1) & 0x1F;

                    if (ch2.adder < 0x10)
                    {
                        ch2.nowvolume = (ch2.adder & 0x0F) << TRIANGLE_VOL_SHIFT;
                    }
                    else
                    {
                        ch2.nowvolume = (0x0F - (ch2.adder & 0x0F)) << TRIANGLE_VOL_SHIFT;
                    }

                    total += ch2.nowvolume;
                    num_times++;
                }

                return (total / num_times) * vol / 256;
            }
            else
            {
                int x = ChannelTone[2, 0] - 1;
                int pTone = 0;

                ch2.phaseacc -= cycle_rate;
                if (ch2.phaseacc >= 0)
                {
                    return ch2.nowvolume * vol / 256;
                }

                if (ch2.freq > cycle_rate)
                {
                    ch2.phaseacc += ch2.freq;
                    ch2.adder = (ch2.adder + 1) & 0x1F;
                    var temp = ToneTable[x, pTone + (ch2.adder & 0x1F)];
                    ch2.nowvolume = temp * 0x0F;
                    return ch2.nowvolume * vol / 256;
                }

                // 加重平均
                int num_times, total;
                num_times = total = 0;
                while (ch2.phaseacc < 0)
                {
                    ch2.phaseacc += ch2.freq;
                    ch2.adder = (ch2.adder + 1) & 0x1F;
                    var temp = ToneTable[x, pTone + (ch2.adder & 0x1F)];
                    total += temp * 0x0F;
                    num_times++;
                }

                return (total / num_times) * vol / 256;
            }
        }

        private int RenderRectangle(RECTANGLE ch)
        {
            if (ch.enable == 0 || ch.len_count <= 0)
                return 0;

            // Channel disable?
            if ((ch.freq < 8) || (ch.swp_inc == 0 && ch.freq > ch.freqlimit))
            {
                return 0;
            }

            if (ch.env_fixed != 0)
            {
                ch.nowvolume = ch.volume << RECTANGLE_VOL_SHIFT;
            }
            int volume = ch.nowvolume;

            if (!(Supporter.Config.sound.bChangeTone && (ChannelTone[(ch.complement == 0) ? 0 : 1, ch.reg[0] >> 6]) != 0))
            {
                // 補間処理
                double total;
                double sample_weight = ch.phaseacc;
                if (sample_weight > cycle_rate)
                {
                    sample_weight = cycle_rate;
                }
                total = (ch.adder < ch.duty) ? sample_weight : -sample_weight;

                int freq = INT2FIX(ch.freq + 1);
                ch.phaseacc -= cycle_rate;
                while (ch.phaseacc < 0)
                {
                    ch.phaseacc += freq;
                    ch.adder = (ch.adder + 1) & 0x0F;

                    sample_weight = freq;
                    if (ch.phaseacc > 0)
                    {
                        sample_weight -= ch.phaseacc;
                    }
                    total += (ch.adder < ch.duty) ? sample_weight : -sample_weight;
                }
                return (int)(volume * total / cycle_rate + 0.5);
            }
            else
            {
                int x = ChannelTone[(ch.complement == 0) ? 0 : 1, ch.reg[0] >> 6] - 1;
                int pTone = 0;

                // 更新無し
                ch.phaseacc -= cycle_rate * 2;
                if (ch.phaseacc >= 0)
                {
                    var temp = ToneTable[x, pTone + (ch.adder & 0x1F)];
                    return temp * volume / ((1 << RECTANGLE_VOL_SHIFT) / 2);
                }

                // 1ステップだけ更新
                int freq = INT2FIX(ch.freq + 1);
                if (freq > cycle_rate * 2)
                {
                    ch.phaseacc += freq;
                    ch.adder = (ch.adder + 1) & 0x1F;
                    var temp = ToneTable[x, pTone + (ch.adder & 0x1F)];
                    return temp * volume / ((1 << RECTANGLE_VOL_SHIFT) / 2);
                }

                // 加重平均
                int num_times, total;
                num_times = total = 0;
                while (ch.phaseacc < 0)
                {
                    ch.phaseacc += freq;
                    ch.adder = (ch.adder + 1) & 0x1F;
                    var temp = ToneTable[x, pTone + (ch.adder & 0x1F)];
                    total += temp * volume / ((1 << RECTANGLE_VOL_SHIFT) / 2);
                    num_times++;
                }
                return total / num_times;
            }
        }

        internal byte SyncRead(ushort addr)
        {
            byte data = (byte)(addr >> 8);

            if (addr == 0x4015)
            {
                data = 0;
                if ((ch0.sync_enable != 0) && ch0.sync_len_count > 0) data |= (1 << 0);
                if ((ch1.sync_enable != 0) && ch1.sync_len_count > 0) data |= (1 << 1);
                if ((ch2.sync_enable != 0) && ch2.sync_len_count > 0) data |= (1 << 2);
                if ((ch3.sync_enable != 0) && ch3.sync_len_count > 0) data |= (1 << 3);
                if ((ch4.sync_enable != 0) && (ch4.sync_dmalength != 0)) data |= (1 << 4);
                if (FrameIRQoccur != 0) data |= (1 << 6);
                if (ch4.sync_irq_enable != 0) data |= (1 << 7);
                FrameIRQoccur = 0;

                nes.cpu.ClrIRQ(CPU.IRQ_FRAMEIRQ);
            }
            if (addr == 0x4017)
            {
                if (FrameIRQoccur != 0)
                {
                    data = 0;
                }
                else
                {
                    data |= (1 << 6);
                }
            }
            return data;
        }

        internal void SyncWrite(ushort addr, byte data)
        {
            switch (addr)
            {
                // CH0,1 rectangle
                case 0x4000:
                case 0x4001:
                case 0x4002:
                case 0x4003:
                case 0x4004:
                case 0x4005:
                case 0x4006:
                case 0x4007:
                    SyncWriteRectangle((addr < 0x4004) ? 0 : 1, addr, data);
                    break;
                // CH2 triangle
                case 0x4008:
                case 0x4009:
                case 0x400A:
                case 0x400B:
                    SyncWriteTriangle(addr, data);
                    break;
                // CH3 noise
                case 0x400C:
                case 0x400D:
                case 0x400E:
                case 0x400F:
                    SyncWriteNoise(addr, data);
                    break;
                // CH4 DPCM
                case 0x4010:
                case 0x4011:
                case 0x4012:
                case 0x4013:
                    SyncWriteDPCM(addr, data);
                    break;

                case 0x4015:
                    sync_reg4015 = data;

                    if ((data & (1 << 0)) == 0)
                    {
                        ch0.sync_enable = 0;
                        ch0.sync_len_count = 0;
                    }
                    if ((data & (1 << 1)) == 0)
                    {
                        ch1.sync_enable = 0;
                        ch1.sync_len_count = 0;
                    }
                    if ((data & (1 << 2)) == 0)
                    {
                        ch2.sync_enable = 0;
                        ch2.sync_len_count = 0;
                        ch2.sync_lin_count = 0;
                        ch2.sync_counter_start = 0;
                    }
                    if ((data & (1 << 3)) == 0)
                    {
                        ch3.sync_enable = 0;
                        ch3.sync_len_count = 0;
                    }
                    if ((data & (1 << 4)) == 0)
                    {
                        ch4.sync_enable = 0;
                        ch4.sync_dmalength = 0;
                        ch4.sync_irq_enable = 0;

                        nes.cpu.ClrIRQ(CPU.IRQ_DPCM);
                    }
                    else
                    {
                        ch4.sync_enable = 0xFF;
                        if (ch4.sync_dmalength == 0)
                        {
                            //					ch4.sync_cycles    = ch4.sync_cache_cycles;
                            ch4.sync_dmalength = ch4.sync_cache_dmalength;
                            ch4.sync_cycles = 0;
                        }
                    }
                    break;

                case 0x4017:
                    SyncWrite4017(data);
                    break;

                // VirtuaNES屌桳億乕僩
                case 0x4018:
                    SyncUpdateRectangle(ch0, data);
                    SyncUpdateRectangle(ch1, data);
                    SyncUpdateTriangle(data);
                    SyncUpdateNoise(data);
                    break;
                default:
                    break;
            }
        }

        private void SyncUpdateNoise(int type)
        {
            if (ch3.sync_enable == 0 || ch3.sync_len_count <= 0)
                return;

            // Update Length
            if (ch3.sync_len_count != 0 && ch3.sync_holdnote == 0)
            {
                if ((type & 1) == 0 && ch3.sync_len_count != 0)
                {
                    ch3.sync_len_count--;
                }
            }
        }

        private void SyncUpdateTriangle(int type)
        {
            if (ch2.sync_enable == 0)
                return;

            if ((type & 1) == 0 && ch2.sync_holdnote == 0)
            {
                if (ch2.sync_len_count != 0)
                {
                    ch2.sync_len_count--;
                }
            }

            // Update Length/Linear
            if (ch2.sync_counter_start != 0)
            {
                ch2.sync_lin_count = ch2.sync_reg[0] & 0x7F;
            }
            else if (ch2.sync_lin_count != 0)
            {
                ch2.sync_lin_count--;
            }
            if (ch2.sync_holdnote == 0 && ch2.sync_lin_count != 0)
            {
                ch2.sync_counter_start = 0;
            }
        }

        private void SyncUpdateRectangle(RECTANGLE ch, int type)
        {
            if (ch.sync_enable == 0 || ch.sync_len_count <= 0)
                return;

            // Update Length
            if (ch.sync_len_count != 0 && ch.sync_holdnote == 0)
            {
                if ((type & 1) == 0 && ch.sync_len_count != 0)
                {
                    ch.sync_len_count--;
                }
            }
        }

        private void SyncWrite4017(byte data)
        {
            FrameCycle = 0;
            FrameIRQ = data;
            FrameIRQoccur = 0;

            nes.cpu.ClrIRQ(CPU.IRQ_FRAMEIRQ);

            FrameType = (data & 0x80) != 0 ? 1 : 0;
            FrameCount = 0;
            if ((data & 0x80) > 0)
            {
                UpdateFrame();
            }
            FrameCount = 1;
            FrameCycle = 14915;
        }

        private void SyncWriteDPCM(ushort addr, byte data)
        {
            ch4.reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch4.sync_cache_cycles = nes.GetVideoMode() ? dpcm_cycles_pal[data & 0x0F] * 8 : dpcm_cycles[data & 0x0F] * 8;
                    ch4.sync_looping = (byte)(data & 0x40);
                    ch4.sync_irq_gen = (byte)(data & 0x80);
                    if (ch4.sync_irq_gen == 0)
                    {
                        ch4.sync_irq_enable = 0;
                        nes.cpu.ClrIRQ(CPU.IRQ_DPCM);
                    }
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    ch4.sync_cache_dmalength = (data << 4) + 1;
                    break;
            }
        }

        private void SyncWriteNoise(ushort addr, byte data)
        {
            ch3.sync_reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch3.sync_holdnote = (byte)(data & 0x20);
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3: // Master
                    ch3.sync_len_count = vbl_length[data >> 3] * 2;
                    if ((sync_reg4015 & (1 << 3)) != 0)
                        ch3.sync_enable = 0xFF;
                    break;
            }
        }

        private void SyncWriteTriangle(ushort addr, byte data)
        {
            ch2.sync_reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch2.sync_holdnote = (byte)(data & 0x80);
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3: // Master
                    ch2.sync_len_count = vbl_length[ch2.sync_reg[3] >> 3] * 2;
                    ch2.sync_counter_start = 0x80;

                    if ((sync_reg4015 & (1 << 2)) != 0)
                        ch2.sync_enable = 0xFF;
                    break;
            }
        }

        private void SyncWriteRectangle(int no, ushort addr, byte data)
        {
            RECTANGLE ch = (no == 0) ? ch0 : ch1;

            ch.sync_reg[addr & 3] = data;
            switch (addr & 3)
            {
                case 0:
                    ch.sync_holdnote = (byte)(data & 0x20);
                    break;
                case 1:
                case 2:
                    break;
                case 3: // Master
                    ch.sync_len_count = vbl_length[data >> 3] * 2;
                    if ((sync_reg4015 & (1 << no)) != 0)
                        ch.sync_enable = 0xFF;
                    break;
            }
        }

        internal void GetFrameIRQ(ref int cycle, ref byte count, ref byte type, ref byte IRQ, ref byte occur)
        {
            cycle = FrameCycle;
            count = (byte)FrameCount;
            type = (byte)FrameType;
            IRQ = FrameIRQ;
            occur = FrameIRQoccur;
        }

        internal void SetFrameIRQ(int cycle, byte count, byte type, byte IRQ, byte occur)
        {
            FrameCycle = cycle;
            FrameCount = count;
            FrameType = type;
            FrameIRQ = IRQ;
            FrameIRQoccur = occur;
        }

        public override uint GetSize()
        {
            return sizeof(byte) +
            sizeof(byte) +
            sizeof(int) +
            sizeof(int) +
            sizeof(int) +
            sizeof(byte) +
            sizeof(byte) +
            ch0.GetSize() +
            ch1.GetSize() +
            ch2.GetSize() +
            ch3.GetSize() +
            ch4.GetSize();
        }

        public override void SaveState(StateBuffer p)
        {
            p.Write(reg4015);
            p.Write(sync_reg4015);
            p.Write(FrameCycle);
            p.Write(FrameCount);
            p.Write(FrameType);
            p.Write(FrameIRQ);
            p.Write(FrameIRQoccur);
            ch0.SaveState(p);
            ch1.SaveState(p);
            ch2.SaveState(p);
            ch3.SaveState(p);
            ch4.SaveState(p);
        }

        public class RECTANGLE : IStateBufferObject
        {
            public byte[] reg = new byte[4];        // register

            public byte enable;        // enable
            public byte holdnote;  // holdnote
            public byte volume;        // volume
            public byte complement;

            // For Render
            public int phaseacc;
            public int freq;
            public int freqlimit;
            public int adder;
            public int duty;
            public int len_count;

            public int nowvolume;

            // For Envelope
            public byte env_fixed;
            public byte env_decay;
            public byte env_count;
            public byte dummy0;
            public int env_vol;

            // For Sweep
            public byte swp_on;
            public byte swp_inc;
            public byte swp_shift;
            public byte swp_decay;
            public byte swp_count;
            public byte[] dummy1 = new byte[3];

            // For sync;
            public byte[] sync_reg = new byte[4];
            public byte sync_output_enable;
            public byte sync_enable;
            public byte sync_holdnote;
            public byte dummy2;
            public int sync_len_count;

            public void ZeroMemory()
            {
                Array.Clear(reg, 0, reg.Length);
                enable = 0;
                holdnote = 0;
                volume = 0;
                complement = 0;

                phaseacc = 0;
                freq = 0;
                freqlimit = 0;
                adder = 0;
                duty = 0;
                len_count = 0;

                nowvolume = 0;

                env_fixed = 0;
                env_decay = 0;
                env_count = 0;
                dummy0 = 0;
                env_vol = 0;

                swp_on = 0;
                swp_inc = 0;
                swp_shift = 0;
                swp_decay = 0;
                swp_count = 0;
                Array.Clear(dummy1, 0, dummy1.Length);

                Array.Clear(sync_reg, 0, sync_reg.Length);
                sync_output_enable = 0;
                sync_enable = 0;
                sync_holdnote = 0;
                dummy2 = 0;
                sync_len_count = 0;
            }

            public uint GetSize()
            {
                return 64;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(reg);
                buffer.Write(enable);
                buffer.Write(holdnote);
                buffer.Write(volume);
                buffer.Write(complement);
                buffer.Write(phaseacc);
                buffer.Write(freq);
                buffer.Write(freqlimit);
                buffer.Write(adder);
                buffer.Write(duty);
                buffer.Write(len_count);
                buffer.Write(nowvolume);
                buffer.Write(env_fixed);
                buffer.Write(env_decay);
                buffer.Write(env_count);
                buffer.Write(dummy0);
                buffer.Write(env_vol);
                buffer.Write(swp_on);
                buffer.Write(swp_inc);
                buffer.Write(swp_shift);
                buffer.Write(swp_decay);
                buffer.Write(swp_count);
                buffer.Write(dummy1);
                buffer.Write(sync_reg);
                buffer.Write(sync_output_enable);
                buffer.Write(sync_enable);
                buffer.Write(sync_holdnote);
                buffer.Write(dummy2);
                buffer.Write(sync_len_count);
            }

            public void LoadState(StateReader buffer)
            {
                reg = buffer.Read_bytes(4);
                enable = buffer.Read_byte();
                holdnote = buffer.Read_byte();
                volume = buffer.Read_byte();
                complement = buffer.Read_byte();
                phaseacc = buffer.Read_int();
                freq = buffer.Read_int();
                freqlimit = buffer.Read_int();
                adder = buffer.Read_int();
                duty = buffer.Read_int();
                len_count = buffer.Read_int();
                nowvolume = buffer.Read_int();
                env_fixed = buffer.Read_byte();
                env_decay = buffer.Read_byte();
                env_count = buffer.Read_byte();
                dummy0 = buffer.Read_byte();
                env_vol = buffer.Read_int();
                swp_on = buffer.Read_byte();
                swp_inc = buffer.Read_byte();
                swp_shift = buffer.Read_byte();
                swp_decay = buffer.Read_byte();
                swp_count = buffer.Read_byte();
                dummy1 = buffer.Read_bytes(3);
                sync_reg = buffer.Read_bytes(4);
                sync_output_enable = buffer.Read_byte();
                sync_enable = buffer.Read_byte();
                sync_holdnote = buffer.Read_byte();
                dummy2 = buffer.Read_byte();
                sync_len_count = buffer.Read_int();
            }
        }
        public class TRIANGLE : IStateBufferObject
        {
            public byte[] reg = new byte[4];

            public byte enable;
            public byte holdnote;
            public byte counter_start;
            public byte dummy0;

            public int phaseacc;
            public int freq;
            public int len_count;
            public int lin_count;
            public int adder;

            public int nowvolume;

            // For sync;
            public byte[] sync_reg = new byte[4];
            public byte sync_enable;
            public byte sync_holdnote;
            public byte sync_counter_start;
            //		public byte	dummy1;
            public int sync_len_count;
            public int sync_lin_count;


            internal void ZeroMemory()
            {
                Array.Clear(reg, 0, reg.Length);

                enable = 0;
                holdnote = 0;
                counter_start = 0;
                dummy0 = 0;
                phaseacc = 0;
                freq = 0;
                len_count = 0;
                lin_count = 0;
                adder = 0;
                nowvolume = 0;
                Array.Clear(sync_reg, 0, sync_reg.Length);
                sync_enable = 0;
                sync_holdnote = 0;
                sync_counter_start = 0;

                sync_len_count = 0;
                sync_lin_count = 0;
            }

            public uint GetSize()
            {
                return 47;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(reg);
                buffer.Write(enable);
                buffer.Write(holdnote);
                buffer.Write(counter_start);
                buffer.Write(dummy0);
                buffer.Write(phaseacc);
                buffer.Write(freq);
                buffer.Write(len_count);
                buffer.Write(lin_count);
                buffer.Write(adder);
                buffer.Write(nowvolume);
                buffer.Write(sync_reg);
                buffer.Write(sync_enable);
                buffer.Write(sync_holdnote);
                buffer.Write(sync_counter_start);
                buffer.Write(sync_len_count);
                buffer.Write(sync_lin_count);
            }

            public void LoadState(StateReader buffer)
            {
                reg = buffer.Read_bytes(4);
                enable = buffer.Read_byte();
                holdnote = buffer.Read_byte();
                counter_start = buffer.Read_byte();
                dummy0 = buffer.Read_byte();
                phaseacc = buffer.Read_int();
                freq = buffer.Read_int();
                len_count = buffer.Read_int();
                lin_count = buffer.Read_int();
                adder = buffer.Read_int();
                nowvolume = buffer.Read_int();
                sync_reg = buffer.Read_bytes(4);
                sync_enable = buffer.Read_byte();
                sync_holdnote = buffer.Read_byte();
                sync_counter_start = buffer.Read_byte();
                sync_len_count = buffer.Read_int();
                sync_lin_count = buffer.Read_int();
            }
        }
        public class DPCM : IStateBufferObject
        {
            public byte[] reg = new byte[4];
            public byte enable;
            public byte looping;
            public byte cur_byte;
            public byte dpcm_value;

            public int freq;
            public int phaseacc;
            public int output;

            public ushort address, cache_addr;
            public int dmalength, cache_dmalength;
            public int dpcm_output_real, dpcm_output_fake, dpcm_output_old, dpcm_output_offset;

            // For sync
            public byte[] sync_reg = new byte[4];
            public byte sync_enable;
            public byte sync_looping;
            public byte sync_irq_gen;
            public byte sync_irq_enable;
            public int sync_cycles, sync_cache_cycles;
            public int sync_dmalength, sync_cache_dmalength;

            public uint GetSize()
            {
                return 72;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(reg);
                buffer.Write(enable);
                buffer.Write(looping);
                buffer.Write(cur_byte);
                buffer.Write(dpcm_value);
                buffer.Write(freq);
                buffer.Write(phaseacc);
                buffer.Write(output);
                buffer.Write(address);
                buffer.Write(cache_addr);
                buffer.Write(dmalength);
                buffer.Write(cache_dmalength);
                buffer.Write(dpcm_output_real);
                buffer.Write(dpcm_output_fake);
                buffer.Write(dpcm_output_old);
                buffer.Write(dpcm_output_offset);
                buffer.Write(sync_reg);
                buffer.Write(sync_enable);
                buffer.Write(sync_looping);
                buffer.Write(sync_irq_gen);
                buffer.Write(sync_irq_enable);
                buffer.Write(sync_cycles);
                buffer.Write(sync_cache_cycles);
                buffer.Write(sync_dmalength);
                buffer.Write(sync_cache_dmalength);
            }

            public void LoadState(StateReader buffer)
            {
                reg = buffer.Read_bytes(4);
                enable = buffer.Read_byte();
                looping = buffer.Read_byte();
                cur_byte = buffer.Read_byte();
                dpcm_value = buffer.Read_byte();
                freq = buffer.Read_int();
                phaseacc = buffer.Read_int();
                output = buffer.Read_int();
                address = buffer.Read_ushort();
                cache_addr = buffer.Read_ushort();
                dmalength = buffer.Read_int();
                cache_dmalength = buffer.Read_int();
                dpcm_output_real = buffer.Read_int();
                dpcm_output_fake = buffer.Read_int();
                dpcm_output_old = buffer.Read_int();
                dpcm_output_offset = buffer.Read_int();
                sync_reg = buffer.Read_bytes(4);
                sync_enable = buffer.Read_byte();
                sync_looping = buffer.Read_byte();
                sync_irq_gen = buffer.Read_byte();
                sync_irq_enable = buffer.Read_byte();
                sync_cycles = buffer.Read_int();
                sync_cache_cycles = buffer.Read_int();
                sync_dmalength = buffer.Read_int();
                sync_cache_dmalength = buffer.Read_int();
            }
        }
        public class NOISE : IStateBufferObject
        {
            public byte[] reg = new byte[4];        // register

            public byte enable;        // enable
            public byte holdnote;  // holdnote
            public byte volume;        // volume
            public byte xor_tap;
            public int shift_reg;

            // For Render
            public int phaseacc;
            public int freq;
            public int len_count;

            public int nowvolume;
            public int output;

            // For Envelope
            public byte env_fixed;
            public byte env_decay;
            public byte env_count;
            public byte dummy0;
            public int env_vol;

            // For sync;
            public byte[] sync_reg = new byte[4];
            public byte sync_output_enable;
            public byte sync_enable;
            public byte sync_holdnote;
            public byte dummy1;
            public int sync_len_count;


            internal void ZeroMemory()
            {
                Array.Clear(reg, 0, reg.Length);

                enable = 0;
                holdnote = 0;
                volume = 0;
                xor_tap = 0;
                shift_reg = 0;

                phaseacc = 0;
                freq = 0;
                len_count = 0;
                nowvolume = 0;
                output = 0;

                env_fixed = 0;
                env_decay = 0;
                env_count = 0;
                dummy0 = 0;
                env_vol = 0;

                Array.Clear(sync_reg, 0, sync_reg.Length);
                sync_output_enable = 0;
                sync_enable = 0;
                sync_holdnote = 0;
                dummy1 = 0;
                sync_len_count = 0;
            }

            public uint GetSize()
            {
                return 52;
            }

            public void SaveState(StateBuffer buffer)
            {
                buffer.Write(reg);
                buffer.Write(enable);
                buffer.Write(holdnote);
                buffer.Write(volume);
                buffer.Write(xor_tap);
                buffer.Write(shift_reg);
                buffer.Write(phaseacc);
                buffer.Write(freq);
                buffer.Write(len_count);
                buffer.Write(nowvolume);
                buffer.Write(output);
                buffer.Write(env_fixed);
                buffer.Write(env_decay);
                buffer.Write(env_count);
                buffer.Write(dummy0);
                buffer.Write(env_vol);
                buffer.Write(sync_reg);
                buffer.Write(sync_output_enable);
                buffer.Write(sync_enable);
                buffer.Write(sync_holdnote);
                buffer.Write(dummy1);
                buffer.Write(sync_len_count);
            }

            public void LoadState(StateReader buffer)
            {
                reg = buffer.Read_bytes(4);
                enable = buffer.Read_byte();
                holdnote = buffer.Read_byte();
                volume = buffer.Read_byte();
                xor_tap = buffer.Read_byte();
                shift_reg = buffer.Read_int();
                phaseacc = buffer.Read_int();
                freq = buffer.Read_int();
                len_count = buffer.Read_int();
                nowvolume = buffer.Read_int();
                output = buffer.Read_int();
                env_fixed = buffer.Read_byte();
                env_decay = buffer.Read_byte();
                env_count = buffer.Read_byte();
                dummy0 = buffer.Read_byte();
                env_vol = buffer.Read_int();
                sync_reg = buffer.Read_bytes(4);
                sync_output_enable = buffer.Read_byte();
                sync_enable = buffer.Read_byte();
                sync_holdnote = buffer.Read_byte();
                dummy1 = buffer.Read_byte();
                sync_len_count = buffer.Read_int();
            }
        }
    }
}
