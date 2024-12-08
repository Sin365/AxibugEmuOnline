using RECTANGLE = VirtualNes.Core.APU_VRC6.RECTANGLE;
using SAWTOOTH = VirtualNes.Core.APU_VRC6.SAWTOOTH;

namespace VirtualNes.Core
{
    public class APU_N106 : APU_INTERFACE
    {
        RECTANGLE ch0 = new RECTANGLE();
        RECTANGLE ch1 = new RECTANGLE();
        SAWTOOTH ch2 = new SAWTOOTH();
        float cpu_clock;
        int cycle_rate;

        public APU_N106()
        {
            Reset(APU_CLOCK, 22050);
        }

        public override void Reset(float fClock, int nRate)
        {
            ch0.ZeroMemory();
            ch1.ZeroMemory();
            ch2.ZeroMemory();

            Setup(fClock, nRate);
        }

        public override void Setup(float fClock, int nRate)
        {
            cpu_clock = fClock;
            cycle_rate = (int)(fClock * 65536.0f / nRate);
        }

        public override void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                // VRC6 CH0 rectangle
                case 0x9000:
                    ch0.reg[0] = data;
                    ch0.gate = (byte)(data & 0x80);
                    ch0.volume = (byte)(data & 0x0F);
                    ch0.duty_pos = (byte)((data >> 4) & 0x07);
                    break;
                case 0x9001:
                    ch0.reg[1] = data;
                    ch0.freq = INT2FIX((((ch0.reg[2] & 0x0F) << 8) | data) + 1);
                    break;
                case 0x9002:
                    ch0.reg[2] = data;
                    ch0.enable = (byte)(data & 0x80);
                    ch0.freq = INT2FIX((((data & 0x0F) << 8) | ch0.reg[1]) + 1);
                    break;
                // VRC6 CH1 rectangle
                case 0xA000:
                    ch1.reg[0] = data;
                    ch1.gate = (byte)(data & 0x80);
                    ch1.volume = (byte)(data & 0x0F);
                    ch1.duty_pos = (byte)((data >> 4) & 0x07);
                    break;
                case 0xA001:
                    ch1.reg[1] = data;
                    ch1.freq = INT2FIX((((ch1.reg[2] & 0x0F) << 8) | data) + 1);
                    break;
                case 0xA002:
                    ch1.reg[2] = data;
                    ch1.enable = (byte)(data & 0x80);
                    ch1.freq = INT2FIX((((data & 0x0F) << 8) | ch1.reg[1]) + 1);
                    break;
                // VRC6 CH2 sawtooth
                case 0xB000:
                    ch2.reg[1] = data;
                    ch2.phaseaccum = (byte)(data & 0x3F);
                    break;
                case 0xB001:
                    ch2.reg[1] = data;
                    ch2.freq = INT2FIX((((ch2.reg[2] & 0x0F) << 8) | data) + 1);
                    break;
                case 0xB002:
                    ch2.reg[2] = data;
                    ch2.enable = (byte)(data & 0x80);
                    ch2.freq = INT2FIX((((data & 0x0F) << 8) | ch2.reg[1]) + 1);
                    //			ch2.adder = 0;	// クリアするとノイズの原因になる
                    //			ch2.accum = 0;	// クリアするとノイズの原因になる
                    break;
            }
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
                    return SawtoothRender(ch2);
            }

            return 0;
        }

        public override int GetFreq(int channel)
        {
            if (channel == 0 || channel == 1)
            {
                RECTANGLE ch;
                if (channel == 0) ch = ch0;
                else ch = ch1;
                if (ch.enable == 0 || ch.gate != 0 || ch.volume == 0)
                    return 0;
                if (ch.freq < INT2FIX(8))
                    return 0;
                return (int)((256.0f * cpu_clock / (FIX2INT(ch.freq) * 16.0f)));
            }
            if (channel == 2)
            {
                SAWTOOTH ch = ch2;
                if (ch.enable == 0 || ch.phaseaccum == 0)
                    return 0;
                if (ch.freq < INT2FIX(8))
                    return 0;
                return (int)(256.0f * cpu_clock / (FIX2INT(ch.freq) * 14.0f));
            }

            return 0;
        }

        int RectangleRender(RECTANGLE ch)
        {
            // Enable?
            if (ch.enable == 0)
            {
                ch.output_vol = 0;
                ch.adder = 0;
                return ch.output_vol;
            }

            // Digitized output
            if (ch.gate != 0)
            {
                ch.output_vol = ch.volume << APU_VRC6.RECTANGLE_VOL_SHIFT;
                return ch.output_vol;
            }

            // 一定以上の周波数は処理しない(無駄)
            if (ch.freq < INT2FIX(8))
            {
                ch.output_vol = 0;
                return ch.output_vol;
            }

            ch.phaseacc -= cycle_rate;
            if (ch.phaseacc >= 0)
                return ch.output_vol;

            int output = ch.volume << APU_VRC6.RECTANGLE_VOL_SHIFT;

            if (ch.freq > cycle_rate)
            {
                // add 1 step
                ch.phaseacc += ch.freq;
                ch.adder = (byte)((ch.adder + 1) & 0x0F);
                if (ch.adder <= ch.duty_pos)
                    ch.output_vol = output;
                else
                    ch.output_vol = -output;
            }
            else
            {
                // average calculate
                int num_times, total;
                num_times = total = 0;
                while (ch.phaseacc < 0)
                {
                    ch.phaseacc += ch.freq;
                    ch.adder = (byte)((ch.adder + 1) & 0x0F);
                    if (ch.adder <= ch.duty_pos)
                        total += output;
                    else
                        total += -output;
                    num_times++;
                }
                ch.output_vol = total / num_times;
            }

            return ch.output_vol;
        }

        int SawtoothRender(SAWTOOTH ch)
        {
            // Digitized output
            if (ch.enable == 0)
            {
                ch.output_vol = 0;
                return ch.output_vol;
            }

            // 一定以上の周波数は処理しない(無駄)
            if (ch.freq < INT2FIX(9))
            {
                return ch.output_vol;
            }

            ch.phaseacc -= cycle_rate / 2;
            if (ch.phaseacc >= 0)
                return ch.output_vol;

            if (ch.freq > cycle_rate / 2)
            {
                // add 1 step
                ch.phaseacc += ch.freq;
                if (++ch.adder >= 7)
                {
                    ch.adder = 0;
                    ch.accum = 0;
                }
                ch.accum += ch.phaseaccum;
                ch.output_vol = ch.accum << APU_VRC6.SAWTOOTH_VOL_SHIFT;
            }
            else
            {
                // average calculate
                int num_times, total;
                num_times = total = 0;
                while (ch.phaseacc < 0)
                {
                    ch.phaseacc += ch.freq;
                    if (++ch.adder >= 7)
                    {
                        ch.adder = 0;
                        ch.accum = 0;
                    }
                    ch.accum += ch.phaseaccum;
                    total += ch.accum << APU_VRC6.SAWTOOTH_VOL_SHIFT;
                    num_times++;
                }
                ch.output_vol = (total / num_times);
            }

            return ch.output_vol;
        }
    }
}
