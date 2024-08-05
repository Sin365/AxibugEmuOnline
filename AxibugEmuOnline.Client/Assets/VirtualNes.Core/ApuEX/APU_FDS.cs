using System;

namespace VirtualNes.Core
{
    public class APU_FDS : APU_INTERFACE
    {
        private FDSSOUND fds = new FDSSOUND();
        private FDSSOUND fds_sync = new FDSSOUND();

        public override void Reset(float fClock, int nRate)
        {
            //todo : 实现
        }

        public override void Setup(float fClock, int nRate)
        {
            //todo : 实现
        }

        public override void Write(ushort addr, byte data)
        {
            //todo : 实现
        }

        public override int Process(int channel)
        {
            //todo : 实现
            return 0;
        }

        internal void SyncWrite(ushort addr, byte data)
        {
            WriteSub(addr, data, fds_sync, 1789772.5d);
        }

        private void WriteSub(ushort addr, byte data, FDSSOUND ch, double rate)
        {
            //todo : 实现
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
        }
    }
}
