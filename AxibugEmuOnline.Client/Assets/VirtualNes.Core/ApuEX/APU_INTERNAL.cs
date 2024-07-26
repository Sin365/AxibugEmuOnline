namespace VirtualNes.Core
{
    public class APU_INTERNAL : APU_INTERFACE
    {
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

        // $4015 Reg
        private byte reg4015, sync_reg4015;

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
            throw new System.NotImplementedException();
        }

        public override void Setup(float fClock, int nRate)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(ushort addr, byte data)
        {
            throw new System.NotImplementedException();
        }

        public override int Process(int channel)
        {
            throw new System.NotImplementedException();
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

                // VirtuaNES固有ポート
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
    }
}
