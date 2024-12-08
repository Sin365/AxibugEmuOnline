using System;

namespace VirtualNes.Core
{
    public class APU_FDS : APU_INTERFACE
    {
        FDSSOUND fds = new FDSSOUND();
        FDSSOUND fds_sync = new FDSSOUND();
        int[] output_buf = new int[8];
        int sampling_rate;

        public APU_FDS()
        {
            fds.ZeroMemory();
            fds_sync.ZeroMemory();

            Array.Clear(output_buf, 0, output_buf.Length);

            sampling_rate = 22050;
        }

        public override void Reset(float fClock, int nRate)
        {
            fds.ZeroMemory();
            fds_sync.ZeroMemory();

            sampling_rate = 22050;
        }

        public override void Setup(float fClock, int nRate)
        {
            sampling_rate = nRate;
        }

        int[] tbl_writesub = { 30, 20, 15, 12 };

        private void WriteSub(ushort addr, byte data, FDSSOUND ch, double rate)
        {
            if (addr < 0x4040 || addr > 0x40BF)
                return;

            ch.reg[addr - 0x4040] = data;
            if (addr >= 0x4040 && addr <= 0x407F)
            {
                if (ch.wave_setup != 0)
                {
                    ch.main_wavetable[addr - 0x4040] = 0x20 - (data & 0x3F);
                }
            }
            else
            {
                switch (addr)
                {
                    case 0x4080:    // Volume Envelope
                        ch.volenv_mode = (byte)(data >> 6);
                        if ((data & 0x80) != 0)
                        {
                            ch.volenv_gain = (byte)(data & 0x3F);

                            // 即時反映
                            if (ch.main_addr == 0)
                            {
                                ch.now_volume = (ch.volenv_gain < 0x21) ? ch.volenv_gain : 0x20;
                            }
                        }
                        // エンベロープ1段階の演算
                        ch.volenv_decay = (byte)(data & 0x3F);
                        ch.volenv_phaseacc = (double)ch.envelope_speed * (double)(ch.volenv_decay + 1) * rate / (232.0 * 960.0);
                        break;

                    case 0x4082:    // Main Frequency(Low)
                        ch.main_frequency = (ch.main_frequency & ~0x00FF) | data;
                        break;
                    case 0x4083:    // Main Frequency(High)
                        ch.main_enable = (byte)((~data) & (1 << 7));
                        ch.envelope_enable = (byte)((~data) & (1 << 6));
                        if (ch.main_enable == 0)
                        {
                            ch.main_addr = 0;
                            ch.now_volume = (ch.volenv_gain < 0x21) ? ch.volenv_gain : 0x20;
                        }
                        //				ch.main_frequency  = (ch.main_frequency&0x00FF)|(((INT)data&0x3F)<<8);
                        ch.main_frequency = (ch.main_frequency & 0x00FF) | ((data & 0x0F) << 8);
                        break;

                    case 0x4084:    // Sweep Envelope
                        ch.swpenv_mode = (byte)(data >> 6);
                        if ((data & 0x80) != 0)
                        {
                            ch.swpenv_gain = (byte)(data & 0x3F);
                        }
                        // エンベロープ1段階の演算
                        ch.swpenv_decay = (byte)(data & 0x3F);
                        ch.swpenv_phaseacc = (double)ch.envelope_speed * (double)(ch.swpenv_decay + 1) * rate / (232.0 * 960.0);
                        break;

                    case 0x4085:    // Sweep Bias
                        if ((data & 0x40) != 0) ch.sweep_bias = (data & 0x3f) - 0x40;
                        else ch.sweep_bias = data & 0x3f;
                        ch.lfo_addr = 0;
                        break;

                    case 0x4086:    // Effector(LFO) Frequency(Low)
                        ch.lfo_frequency = (ch.lfo_frequency & (~0x00FF)) | data;
                        break;
                    case 0x4087:    // Effector(LFO) Frequency(High)
                        ch.lfo_enable = (byte)((~data & 0x80));
                        ch.lfo_frequency = (ch.lfo_frequency & 0x00FF) | ((data & 0x0F) << 8);
                        break;

                    case 0x4088:    // Effector(LFO) wavetable
                        if (ch.lfo_enable == 0)
                        {
                            // FIFO?
                            for (byte i = 0; i < 31; i++)
                            {
                                ch.lfo_wavetable[i * 2 + 0] = ch.lfo_wavetable[(i + 1) * 2 + 0];
                                ch.lfo_wavetable[i * 2 + 1] = ch.lfo_wavetable[(i + 1) * 2 + 1];
                            }
                            ch.lfo_wavetable[31 * 2 + 0] = (byte)(data & 0x07);
                            ch.lfo_wavetable[31 * 2 + 1] = (byte)(data & 0x07);
                        }
                        break;

                    case 0x4089:    // Sound control
                        {
                            ch.master_volume = tbl_writesub[data & 3];
                            ch.wave_setup = (byte)(data & 0x80);
                        }
                        break;

                    case 0x408A:    // Sound control 2
                        ch.envelope_speed = data;
                        break;

                    default:
                        break;
                }
            }
        }

        public override void Write(ushort addr, byte data)
        {
            WriteSub(addr, data, fds, sampling_rate);
        }

        public override byte Read(ushort addr)
        {
            byte data = (byte)(addr >> 8);

            if (addr >= 0x4040 && addr <= 0x407F)
            {
                data = (byte)(fds.main_wavetable[addr & 0x3F] | 0x40);
            }
            else
            if (addr == 0x4090)
            {
                data = (byte)((fds.volenv_gain & 0x3F) | 0x40);
            }
            else
            if (addr == 0x4092)
            {
                data = (byte)((fds.swpenv_gain & 0x3F) | 0x40);
            }

            return data;
        }

        int[] tbl_process = { 0, 1, 2, 4, 0, -4, -2, -1 };
        public override int Process(int channel)
        {
            // Envelope unit
            if (fds.envelope_enable != 0 && fds.envelope_speed != 0)
            {
                // Volume envelope
                if (fds.volenv_mode < 2)
                {
                    double decay = ((double)fds.envelope_speed * (double)(fds.volenv_decay + 1) * (double)sampling_rate) / (232.0 * 960.0);
                    fds.volenv_phaseacc -= 1.0;
                    while (fds.volenv_phaseacc < 0.0)
                    {
                        fds.volenv_phaseacc += decay;

                        if (fds.volenv_mode == 0)
                        {
                            // 減少モード
                            if (fds.volenv_gain != 0)
                                fds.volenv_gain--;
                        }
                        else
                        if (fds.volenv_mode == 1)
                        {
                            if (fds.volenv_gain < 0x20)
                                fds.volenv_gain++;
                        }
                    }
                }

                // Sweep envelope
                if (fds.swpenv_mode < 2)
                {
                    double decay = ((double)fds.envelope_speed * (double)(fds.swpenv_decay + 1) * (double)sampling_rate) / (232.0 * 960.0);
                    fds.swpenv_phaseacc -= 1.0;
                    while (fds.swpenv_phaseacc < 0.0)
                    {
                        fds.swpenv_phaseacc += decay;

                        if (fds.swpenv_mode == 0)
                        {
                            // 減少モード
                            if (fds.swpenv_gain != 0)
                                fds.swpenv_gain--;
                        }
                        else
                        if (fds.swpenv_mode == 1)
                        {
                            if (fds.swpenv_gain < 0x20)
                                fds.swpenv_gain++;
                        }
                    }
                }
            }

            // Effector(LFO) unit
            int sub_freq = 0;
            //	if( fds.lfo_enable && fds.envelope_speed && fds.lfo_frequency ) {
            if (fds.lfo_enable != 0)
            {
                if (fds.lfo_frequency != 0)
                {
                    fds.lfo_phaseacc -= (1789772.5 * (double)fds.lfo_frequency) / 65536.0;
                    while (fds.lfo_phaseacc < 0.0)
                    {
                        fds.lfo_phaseacc += (double)sampling_rate;

                        if (fds.lfo_wavetable[fds.lfo_addr] == 4)
                            fds.sweep_bias = 0;
                        else
                            fds.sweep_bias += tbl_process[fds.lfo_wavetable[fds.lfo_addr]];

                        fds.lfo_addr = (fds.lfo_addr + 1) & 63;
                    }
                }

                if (fds.sweep_bias > 63)
                    fds.sweep_bias -= 128;
                else if (fds.sweep_bias < -64)
                    fds.sweep_bias += 128;

                int sub_multi = fds.sweep_bias * fds.swpenv_gain;

                if ((sub_multi & 0x0F) != 0)
                {
                    // 16で割り切れない場合
                    sub_multi = (sub_multi / 16);
                    if (fds.sweep_bias >= 0)
                        sub_multi += 2;    // 正の場合
                    else
                        sub_multi -= 1;    // 負の場合
                }
                else
                {
                    // 16で割り切れる場合
                    sub_multi = (sub_multi / 16);
                }
                // 193を超えると-258する(-64へラップ)
                if (sub_multi > 193)
                    sub_multi -= 258;
                // -64を下回ると+256する(192へラップ)
                if (sub_multi < -64)
                    sub_multi += 256;

                sub_freq = (fds.main_frequency) * sub_multi / 64;
            }

            // Main unit
            int output = 0;
            if (fds.main_enable != 0 && fds.main_frequency != 0 && fds.wave_setup == 0)
            {
                int freq;
                int main_addr_old = fds.main_addr;

                freq = (int)((fds.main_frequency + sub_freq) * 1789772.5 / 65536.0);

                fds.main_addr = (fds.main_addr + freq + 64 * sampling_rate) % (64 * sampling_rate);

                // 1周期を超えたらボリューム更新
                if (main_addr_old > fds.main_addr)
                    fds.now_volume = (fds.volenv_gain < 0x21) ? fds.volenv_gain : 0x20;

                output = fds.main_wavetable[(fds.main_addr / sampling_rate) & 0x3f] * 8 * fds.now_volume * fds.master_volume / 30;

                if (fds.now_volume != 0)
                    fds.now_freq = freq * 4;
                else
                    fds.now_freq = 0;
            }
            else
            {
                fds.now_freq = 0;
                output = 0;
            }

            // LPF
            output = (output_buf[0] * 2 + output) / 3;
            output_buf[0] = output;

            fds.output = output;
            return fds.output;
        }

        internal void SyncWrite(ushort addr, byte data)
        {
            WriteSub(addr, data, fds_sync, 1789772.5d);
        }



        internal byte SyncRead(ushort addr)
        {
            byte data = (byte)(addr >> 8);

            if (addr >= 0x4040 && addr <= 0x407F)
            {
                data = (byte)(fds_sync.main_wavetable[addr & 0x3F] | 0x40);
            }
            else
            if (addr == 0x4090)
            {
                data = (byte)((fds_sync.volenv_gain & 0x3F) | 0x40);
            }
            else
            if (addr == 0x4092)
            {
                data = (byte)((fds_sync.swpenv_gain & 0x3F) | 0x40);
            }

            return data;
        }

        public override bool Sync(int cycles)
        {
            // Envelope unit
            if (fds_sync.envelope_enable != 0 && fds_sync.envelope_speed != 0)
            {
                // Volume envelope
                double decay;
                if (fds_sync.volenv_mode < 2)
                {
                    decay = ((double)fds_sync.envelope_speed * (double)(fds_sync.volenv_decay + 1) * 1789772.5) / (232.0 * 960.0);
                    fds_sync.volenv_phaseacc -= (double)cycles;
                    while (fds_sync.volenv_phaseacc < 0.0)
                    {
                        fds_sync.volenv_phaseacc += decay;

                        if (fds_sync.volenv_mode == 0)
                        {
                            // 減少モード
                            if (fds_sync.volenv_gain != 0)
                                fds_sync.volenv_gain--;
                        }
                        else
                        if (fds_sync.volenv_mode == 1)
                        {
                            // 増加モード
                            if (fds_sync.volenv_gain < 0x20)
                                fds_sync.volenv_gain++;
                        }
                    }
                }

                // Sweep envelope
                if (fds_sync.swpenv_mode < 2)
                {
                    decay = ((double)fds_sync.envelope_speed * (double)(fds_sync.swpenv_decay + 1) * 1789772.5) / (232.0 * 960.0);
                    fds_sync.swpenv_phaseacc -= (double)cycles;
                    while (fds_sync.swpenv_phaseacc < 0.0)
                    {
                        fds_sync.swpenv_phaseacc += decay;

                        if (fds_sync.swpenv_mode == 0)
                        {
                            // 減少モード
                            if (fds_sync.swpenv_gain != 0)
                                fds_sync.swpenv_gain--;
                        }
                        else
                        if (fds_sync.swpenv_mode == 1)
                        {
                            // 増加モード
                            if (fds_sync.swpenv_gain < 0x20)
                                fds_sync.swpenv_gain++;
                        }
                    }
                }
            }

            return false;
        }

        public override int GetFreq(int channel)
        {
            return fds.now_freq;
        }

        private class FDSSOUND
        {
            public byte[] reg = new byte[0x80];
            public byte volenv_mode;       // Volume Envelope
            public byte volenv_gain;
            public byte volenv_decay;
            public double volenv_phaseacc;
            public byte swpenv_mode;       // Sweep Envelope
            public byte swpenv_gain;
            public byte swpenv_decay;
            public double swpenv_phaseacc;
            // For envelope unit
            public byte envelope_enable;   // $4083 bit6
            public byte envelope_speed;        // $408A
            // For $4089
            public byte wave_setup;        // bit7
            public int master_volume;      // bit1-0
            // For Main unit
            public int[] main_wavetable = new int[64];
            public byte main_enable;
            public int main_frequency;
            public int main_addr;
            // For Effector(LFO) unit
            public byte[] lfo_wavetable = new byte[64];
            public byte lfo_enable;        // 0:Enable 1:Wavetable setup
            public int lfo_frequency;
            public int lfo_addr;
            public double lfo_phaseacc;
            // For Sweep unit
            public int sweep_bias;
            // Misc
            public int now_volume;
            public int now_freq;
            public int output;

            public void ZeroMemory()
            {
                Array.Clear(reg, 0, reg.Length);
                volenv_mode = 0;
                volenv_gain = 0;
                volenv_decay = 0;
                volenv_phaseacc = 0.0;
                swpenv_mode = 0;
                swpenv_gain = 0;
                swpenv_decay = 0;
                swpenv_phaseacc = 0.0;
                envelope_enable = 0;
                envelope_speed = 0;
                wave_setup = 0;
                master_volume = 0;
                Array.Clear(main_wavetable, 0, main_wavetable.Length);
                main_enable = 0;
                main_frequency = 0;
                main_addr = 0;
                Array.Clear(lfo_wavetable, 0, lfo_wavetable.Length);
                lfo_enable = 0;
                lfo_frequency = 0;
                lfo_addr = 0;
                lfo_phaseacc = 0.0;
                sweep_bias = 0;
                now_volume = 0;
                now_freq = 0;
                output = 0;
            }
        }
    }
}
