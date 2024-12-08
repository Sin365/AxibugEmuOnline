//using Codice.CM.Client.Differences;
using System;

namespace VirtualNes.Core
{
    public class APU_MMC5 : APU_INTERFACE
    {
        public const int RECTANGLE_VOL_SHIFT = 8;
        public const int DAOUT_VOL_SHIFT = 6;

        SYNCRECTANGLE sch0 = new SYNCRECTANGLE();
        SYNCRECTANGLE sch1 = new SYNCRECTANGLE();
        RECTANGLE ch0 = new RECTANGLE();
        RECTANGLE ch1 = new RECTANGLE();

        byte reg5010;
        byte reg5011;
        byte reg5015;
        byte sync_reg5015;
        int FrameCycle;
        float cpu_clock;
        int cycle_rate;

        // Tables
        static int[] vbl_length = new int[32];
        static int[] duty_lut = new int[4];

        static int[] decay_lut = new int[16];
        static int[] vbl_lut = new int[32];

        public APU_MMC5()
        {
            // 仮設定
            Reset(APU_INTERFACE.APU_CLOCK, 22050);
        }

        public override void Reset(float fClock, int nRate)
        {
            sch0.ZeroMemory();
            sch1.ZeroMemory();

            reg5010 = reg5011 = reg5015 = 0;

            sync_reg5015 = 0;
            FrameCycle = 0;

            Setup(fClock, nRate);

            for (ushort addr = 0x5000; addr <= 0x5015; addr++)
            {
                Write(addr, 0);
            }
        }

        public override void Setup(float fClock, int nRate)
        {
            cpu_clock = fClock;
            cycle_rate = (int)(fClock * 65536.0f / nRate);

            // Create Tables
            int i;
            int samples = (int)(nRate / 60.0f);
            for (i = 0; i < 16; i++)
                decay_lut[i] = (i + 1) * samples * 5;
            for (i = 0; i < 32; i++)
                vbl_lut[i] = vbl_length[i] * samples * 5;
        }

        public override void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                // MMC5 CH0 rectangle
                case 0x5000:
                    ch0.reg[0] = data;
                    ch0.volume = (byte)(data & 0x0F);
                    ch0.holdnote = (byte)(data & 0x20);
                    ch0.fixed_envelope = (byte)(data & 0x10);
                    ch0.env_decay = decay_lut[data & 0x0F];
                    ch0.duty_flip = duty_lut[data >> 6];
                    break;
                case 0x5001:
                    ch0.reg[1] = data;
                    break;
                case 0x5002:
                    ch0.reg[2] = data;
                    ch0.freq = INT2FIX(((ch0.reg[3] & 0x07) << 8) + data + 1);
                    break;
                case 0x5003:
                    ch0.reg[3] = data;
                    ch0.vbl_length = vbl_lut[data >> 3];
                    ch0.env_vol = 0;
                    ch0.freq = INT2FIX(((data & 0x07) << 8) + ch0.reg[2] + 1);
                    if ((reg5015 & 0x01) != 0)
                        ch0.enable = 0xFF;
                    break;
                // MMC5 CH1 rectangle
                case 0x5004:
                    ch1.reg[0] = data;
                    ch1.volume = (byte)(data & 0x0F);
                    ch1.holdnote = (byte)(data & 0x20);
                    ch1.fixed_envelope = (byte)(data & 0x10);
                    ch1.env_decay = decay_lut[data & 0x0F];
                    ch1.duty_flip = duty_lut[data >> 6];
                    break;
                case 0x5005:
                    ch1.reg[1] = data;
                    break;
                case 0x5006:
                    ch1.reg[2] = data;
                    ch1.freq = INT2FIX(((ch1.reg[3] & 0x07) << 8) + data + 1);
                    break;
                case 0x5007:
                    ch1.reg[3] = data;
                    ch1.vbl_length = vbl_lut[data >> 3];
                    ch1.env_vol = 0;
                    ch1.freq = INT2FIX(((data & 0x07) << 8) + ch1.reg[2] + 1);
                    if ((reg5015 & 0x02) != 0)
                        ch1.enable = 0xFF;
                    break;
                case 0x5010:
                    reg5010 = data;
                    break;
                case 0x5011:
                    reg5011 = data;
                    break;
                case 0x5012:
                case 0x5013:
                case 0x5014:
                    break;
                case 0x5015:
                    reg5015 = data;
                    if ((reg5015 & 0x01) != 0)
                    {
                        ch0.enable = 0xFF;
                    }
                    else
                    {
                        ch0.enable = 0;
                        ch0.vbl_length = 0;
                    }
                    if ((reg5015 & 0x02) != 0)
                    {
                        ch1.enable = 0xFF;
                    }
                    else
                    {
                        ch1.enable = 0;
                        ch1.vbl_length = 0;
                    }
                    break;
            }
        }

        internal void SyncWrite(ushort addr, byte data)
        {
            switch (addr)
            {
                // MMC5 CH0 rectangle
                case 0x5000:
                    sch0.reg[0] = data;
                    sch0.holdnote = (byte)(data & 0x20);
                    break;
                case 0x5001:
                case 0x5002:
                    sch0.reg[addr & 3] = data;
                    break;
                case 0x5003:
                    sch0.reg[3] = data;
                    sch0.vbl_length = vbl_length[data >> 3];
                    if ((sync_reg5015 & 0x01) != 0)
                        sch0.enable = 0xFF;
                    break;
                // MMC5 CH1 rectangle
                case 0x5004:
                    sch1.reg[0] = data;
                    sch1.holdnote = (byte)(data & 0x20);
                    break;
                case 0x5005:
                case 0x5006:
                    sch1.reg[addr & 3] = data;
                    break;
                case 0x5007:
                    sch1.reg[3] = data;
                    sch1.vbl_length = vbl_length[data >> 3];
                    if ((sync_reg5015 & 0x02) != 0)
                        sch1.enable = 0xFF;
                    break;
                case 0x5010:
                case 0x5011:
                case 0x5012:
                case 0x5013:
                case 0x5014:
                    break;
                case 0x5015:
                    sync_reg5015 = data;
                    if ((sync_reg5015 & 0x01) != 0)
                    {
                        sch0.enable = 0xFF;
                    }
                    else
                    {
                        sch0.enable = 0;
                        sch0.vbl_length = 0;
                    }
                    if ((sync_reg5015 & 0x02) != 0)
                    {
                        sch1.enable = 0xFF;
                    }
                    else
                    {
                        sch1.enable = 0;
                        sch1.vbl_length = 0;
                    }
                    break;
            }
        }

        internal byte SyncRead(ushort addr)
        {
            byte data = 0;

            if (addr == 0x5015)
            {
                if ((sch0.enable != 0) && sch0.vbl_length > 0) data |= (1 << 0);
                if ((sch1.enable != 0) && sch1.vbl_length > 0) data |= (1 << 1);
            }

            return data;
        }

        public override bool Sync(int cycles)
        {
            FrameCycle += cycles;
            if (FrameCycle >= 7457 * 5 / 2)
            {
                FrameCycle -= 7457 * 5 / 2;

                if (sch0.enable != 0 && sch0.holdnote == 0)
                {
                    if ((sch0.vbl_length) != 0)
                    {
                        sch0.vbl_length--;
                    }
                }
                if (sch1.enable != 0 && sch1.holdnote == 0)
                {
                    if ((sch1.vbl_length) != 0)
                    {
                        sch1.vbl_length--;
                    }
                }
            }

            return false;
        }

        public override int Process(int channel)
        {
            switch (channel)
            {
                case 0:
                    return RectangleRender(ch0);
                case 1:
                    return RectangleRender(ch1);
                case 2:
                    return reg5011 << DAOUT_VOL_SHIFT;
            }

            return 0;
        }

        public override int GetFreq(int channel)
        {
            if (channel == 0 || channel == 1)
            {
                RECTANGLE ch = null;
                if (channel == 0) ch = ch0;
                else ch = ch1;

                if (ch.enable == 0 || ch.vbl_length <= 0)
                    return 0;
                if (ch.freq < INT2FIX(8))
                    return 0;
                if (ch.fixed_envelope != 0)
                {
                    if (ch.volume == 0)
                        return 0;
                }
                else
                {
                    if ((0x0F - ch.env_vol) == 0)
                        return 0;
                }

                return (int)(256.0f * cpu_clock / (FIX2INT(ch.freq) * 16.0f));
            }

            return 0;
        }

        private int RectangleRender(RECTANGLE ch)
        {
            if (ch.enable == 0 || ch.vbl_length <= 0)
                return 0;

            // vbl length counter
            if (ch.holdnote == 0)
                ch.vbl_length -= 5;

            // envelope unit
            ch.env_phase -= 5 * 4;
            while (ch.env_phase < 0)
            {
                ch.env_phase += ch.env_decay;
                if ((ch.holdnote) != 0)
                    ch.env_vol = (byte)((ch.env_vol + 1) & 0x0F);
                else if (ch.env_vol < 0x0F)
                    ch.env_vol++;
            }

            if (ch.freq < INT2FIX(8))
                return 0;

            int volume;
            if ((ch.fixed_envelope) != 0)
                volume = ch.volume;
            else
                volume = (0x0F - ch.env_vol);

            int output = volume << RECTANGLE_VOL_SHIFT;

            ch.phaseacc -= cycle_rate;
            if (ch.phaseacc >= 0)
            {
                if (ch.adder < ch.duty_flip)
                    ch.output_vol = output;
                else
                    ch.output_vol = -output;
                return ch.output_vol;
            }

            if (ch.freq > cycle_rate)
            {
                ch.phaseacc += ch.freq;
                ch.adder = (ch.adder + 1) & 0x0F;
                if (ch.adder < ch.duty_flip)
                    ch.output_vol = output;
                else
                    ch.output_vol = -output;
            }
            else
            {
                // 加重平均
                int num_times, total;
                num_times = total = 0;
                while (ch.phaseacc < 0)
                {
                    ch.phaseacc += ch.freq;
                    ch.adder = (ch.adder + 1) & 0x0F;
                    if (ch.adder < ch.duty_flip)
                        total += output;
                    else
                        total -= output;
                    num_times++;
                }
                ch.output_vol = total / num_times;
            }

            return ch.output_vol;
        }

        public class SYNCRECTANGLE
        {
            // For sync
            public byte[] reg = new byte[4];
            public byte enable;
            public byte holdnote;
            public byte[] dummy = new byte[2];
            public int vbl_length;

            public void ZeroMemory()
            {
                Array.Clear(reg, 0, reg.Length);
                enable = 0;
                holdnote = 0;
                Array.Clear(dummy, 0, dummy.Length);
                vbl_length = 0;
            }
        }        

        public class RECTANGLE
        {
            public byte[] reg = new byte[4];
            public byte enable;

            public int vbl_length;

            public int phaseacc;
            public int freq;

            public int output_vol;
            public byte fixed_envelope;
            public byte holdnote;
            public byte volume;

            public byte env_vol;
            public int env_phase;
            public int env_decay;

            public int adder;
            public int duty_flip;
        }
    }
}
