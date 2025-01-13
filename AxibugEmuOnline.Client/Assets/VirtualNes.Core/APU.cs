
using System;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class APU
    {
        public const uint QUEUE_LENGTH = 8192;

        // Volume adjust
        // Internal sounds
        public const uint RECTANGLE_VOL = 0x0F0;
        public const uint TRIANGLE_VOL = 0x130;
        public const uint NOISE_VOL = 0x0C0;
        public const uint DPCM_VOL = 0x0F0;
        // Extra sounds
        public const uint VRC6_VOL = 0x0F0;
        public const uint VRC7_VOL = 0x130;
        public const uint FDS_VOL = 0x0F0;
        public const uint MMC5_VOL = 0x0F0;
        public const uint N106_VOL = 0x088;
        public const uint FME7_VOL = 0x130;

        private NES nes;
        private byte exsound_select;
        private APU_INTERNAL @internal = new APU_INTERNAL();
        private APU_VRC6 vrc6 = new APU_VRC6();
        private APU_VRC7 vrc7 = new APU_VRC7();
        private APU_MMC5 mmc5 = new APU_MMC5();
        private APU_FDS fds = new APU_FDS();
        private APU_N106 n106 = new APU_N106();
        private APU_FME7 fme7 = new APU_FME7();
        private int last_data;
        private int last_diff;
        protected short[] m_SoundBuffer = new short[256];
        protected int[] lowpass_filter = new int[4];
        protected QUEUE queue = new QUEUE();
        protected QUEUE exqueue = new QUEUE();
        protected bool[] m_bMute = new bool[16];
        protected double elapsed_time;

        public APU(NES parent)
        {
            exsound_select = 0;

            nes = parent;
            @internal.SetParent(parent);

            last_data = last_diff = 0;

            Array.Clear(m_SoundBuffer, 0, m_SoundBuffer.Length);
            Array.Clear(lowpass_filter, 0, lowpass_filter.Length);

            for (int i = 0; i < m_bMute.Length; i++)
                m_bMute[i] = true;
        }

        public void Dispose()
        {
            @internal.Dispose();
            vrc6.Dispose();
            vrc7.Dispose();
            mmc5.Dispose();
            fds.Dispose();
            n106.Dispose();
            fme7.Dispose();
        }

        private int[] vol = new int[24];
        static double cutofftemp = (2.0 * 3.141592653579 * 40.0);
        static double tmp = 0.0;

        public void Process(ISoundDataBuffer lpBuffer, uint dwSize)
        {
            int nBits = Supporter.S.Config.sound.nBits;
            uint dwLength = (uint)(dwSize / (nBits / 8));
            int output;
            QUEUEDATA q = new QUEUEDATA();
            uint writetime;

            var pSoundBuf = m_SoundBuffer;
            int nCcount = 0;

            int nFilterType = Supporter.S.Config.sound.nFilterType;

            if (!Supporter.S.Config.sound.bEnable)
            {
                byte empty = (byte)(Supporter.S.Config.sound.nRate == 8 ? 128 : 0);
                for (int i = 0; i < dwSize; i++)
                    lpBuffer.WriteByte(empty);
                return;
            }

            // Volume setup
            //  0:Master
            //  1:Rectangle 1
            //  2:Rectangle 2
            //  3:Triangle
            //  4:Noise
            //  5:DPCM
            //  6:VRC6
            //  7:VRC7
            //  8:FDS
            //  9:MMC5
            // 10:N106
            // 11:FME7
            MemoryUtility.ZEROMEMORY(vol, vol.Length);

            var bMute = m_bMute;
            var nVolume = Supporter.S.Config.sound.nVolume;

            int nMasterVolume = bMute[0] ? nVolume[0] : 0;

            // Internal
            vol[0] = (int)(bMute[1] ? (RECTANGLE_VOL * nVolume[1] * nMasterVolume) / (100 * 100) : 0);
            vol[1] = (int)(bMute[2] ? (RECTANGLE_VOL * nVolume[2] * nMasterVolume) / (100 * 100) : 0);
            vol[2] = (int)(bMute[3] ? (TRIANGLE_VOL * nVolume[3] * nMasterVolume) / (100 * 100) : 0);
            vol[3] = (int)(bMute[4] ? (NOISE_VOL * nVolume[4] * nMasterVolume) / (100 * 100) : 0);
            vol[4] = (int)(bMute[5] ? (DPCM_VOL * nVolume[5] * nMasterVolume) / (100 * 100) : 0);

            // VRC6
            vol[5] = (int)(bMute[6] ? (VRC6_VOL * nVolume[6] * nMasterVolume) / (100 * 100) : 0);
            vol[6] = (int)(bMute[7] ? (VRC6_VOL * nVolume[6] * nMasterVolume) / (100 * 100) : 0);
            vol[7] = (int)(bMute[8] ? (VRC6_VOL * nVolume[6] * nMasterVolume) / (100 * 100) : 0);

            // VRC7
            vol[8] = (int)(bMute[6] ? (VRC7_VOL * nVolume[7] * nMasterVolume) / (100 * 100) : 0);

            // FDS
            vol[9] = (int)(bMute[6] ? (FDS_VOL * nVolume[8] * nMasterVolume) / (100 * 100) : 0);

            // MMC5
            vol[10] = (int)(bMute[6] ? (MMC5_VOL * nVolume[9] * nMasterVolume) / (100 * 100) : 0);
            vol[11] = (int)(bMute[7] ? (MMC5_VOL * nVolume[9] * nMasterVolume) / (100 * 100) : 0);
            vol[12] = (int)(bMute[8] ? (MMC5_VOL * nVolume[9] * nMasterVolume) / (100 * 100) : 0);

            // N106
            vol[13] = (int)(bMute[6] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[14] = (int)(bMute[7] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[15] = (int)(bMute[8] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[16] = (int)(bMute[9] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[17] = (int)(bMute[10] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[18] = (int)(bMute[11] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[19] = (int)(bMute[12] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);
            vol[20] = (int)(bMute[13] ? (N106_VOL * nVolume[10] * nMasterVolume) / (100 * 100) : 0);

            // FME7
            vol[21] = (int)(bMute[6] ? (FME7_VOL * nVolume[11] * nMasterVolume) / (100 * 100) : 0);
            vol[22] = (int)(bMute[7] ? (FME7_VOL * nVolume[11] * nMasterVolume) / (100 * 100) : 0);
            vol[23] = (int)(bMute[8] ? (FME7_VOL * nVolume[11] * nMasterVolume) / (100 * 100) : 0);

            //	double	cycle_rate = ((double)FRAME_CYCLES*60.0/12.0)/(double)Config.sound.nRate;
            double cycle_rate = (nes.nescfg.FrameCycles * 60.0 / 12.0) / Supporter.S.Config.sound.nRate;

            // CPUサイクル数がループしてしまった時の対策処理
            if (elapsed_time > nes.cpu.GetTotalCycles())
            {
                QueueFlush();
            }

            while ((dwLength--) != 0)
            {
                writetime = (uint)elapsed_time;

                while (GetQueue((int)writetime, ref q))
                {
                    WriteProcess(q.addr, q.data);
                }

                while (GetExQueue((int)writetime, ref q))
                {
                    WriteExProcess(q.addr, q.data);
                }

                // 0-4:internal 5-7:VRC6 8:VRC7 9:FDS 10-12:MMC5 13-20:N106 21-23:FME7
                output = 0;
                output += @internal.Process(0) * vol[0];
                output += @internal.Process(1) * vol[1];
                output += @internal.Process(2) * vol[2];
                output += @internal.Process(3) * vol[3];
                output += @internal.Process(4) * vol[4];

                if ((exsound_select & 0x01) != 0)
                {
                    output += vrc6.Process(0) * vol[5];
                    output += vrc6.Process(1) * vol[6];
                    output += vrc6.Process(2) * vol[7];
                }
                if ((exsound_select & 0x02) != 0)
                {
                    output += vrc7.Process(0) * vol[8];
                }
                if ((exsound_select & 0x04) != 0)
                {
                    output += fds.Process(0) * vol[9];
                }
                if ((exsound_select & 0x08) != 0)
                {
                    output += mmc5.Process(0) * vol[10];
                    output += mmc5.Process(1) * vol[11];
                    output += mmc5.Process(2) * vol[12];
                }
                if ((exsound_select & 0x10) != 0)
                {
                    output += n106.Process(0) * vol[13];
                    output += n106.Process(1) * vol[14];
                    output += n106.Process(2) * vol[15];
                    output += n106.Process(3) * vol[16];
                    output += n106.Process(4) * vol[17];
                    output += n106.Process(5) * vol[18];
                    output += n106.Process(6) * vol[19];
                    output += n106.Process(7) * vol[20];
                }
                if ((exsound_select & 0x20) != 0)
                {
                    fme7.Process(3);    // Envelope & Noise
                    output += fme7.Process(0) * vol[21];
                    output += fme7.Process(1) * vol[22];
                    output += fme7.Process(2) * vol[23];
                }

                output >>= 8;

                if (nFilterType == 1)
                {
                    //ローパスフィルターTYPE 1(Simple)
                    output = (lowpass_filter[0] + output) / 2;
                    lowpass_filter[0] = output;
                }
                else if (nFilterType == 2)
                {
                    //ローパスフィルターTYPE 2(Weighted type 1)
                    output = (lowpass_filter[1] + lowpass_filter[0] + output) / 3;
                    lowpass_filter[1] = lowpass_filter[0];
                    lowpass_filter[0] = output;
                }
                else if (nFilterType == 3)
                {
                    //ローパスフィルターTYPE 3(Weighted type 2)
                    output = (lowpass_filter[2] + lowpass_filter[1] + lowpass_filter[0] + output) / 4;
                    lowpass_filter[2] = lowpass_filter[1];
                    lowpass_filter[1] = lowpass_filter[0];
                    lowpass_filter[0] = output;
                }
                else if (nFilterType == 4)
                {
                    //ローパスフィルターTYPE 4(Weighted type 3)
                    output = (lowpass_filter[1] + lowpass_filter[0] * 2 + output) / 4;
                    lowpass_filter[1] = lowpass_filter[0];
                    lowpass_filter[0] = output;
                }
                // DC成分のカット(HPF TEST)
                {
                    //		static	double	cutoff = (2.0*3.141592653579*40.0/44100.0);
                    double cutoff = cutofftemp / Supporter.S.Config.sound.nRate;
                    double @in, @out;

                    @in = output;
                    @out = (@in - tmp);
                    tmp = tmp + cutoff * @out;

                    output = (int)@out;
                }

                // Limit
                if (output > 0x7FFF)
                {
                    output = 0x7FFF;
                }
                else if (output < -0x8000)
                {
                    output = -0x8000;
                }

                if (nBits != 8)
                {
                    byte highByte = (byte)(output >> 8); // 获取高8位
                    byte lowByte = (byte)(output & 0xFF); // 获取低8位
                    lpBuffer.WriteByte(highByte);
                    lpBuffer.WriteByte(lowByte);
                }
                else
                {
                    lpBuffer.WriteByte((byte)((output >> 8) ^ 0x80));
                }

                if (nCcount < 0x0100)
                    pSoundBuf[nCcount++] = (short)output;

                //		elapsedtime += cycle_rate;
                elapsed_time += cycle_rate;
            }


            if (elapsed_time > ((nes.nescfg.FrameCycles / 24) + nes.cpu.GetTotalCycles()))
            {
                elapsed_time = nes.cpu.GetTotalCycles();
            }
            if ((elapsed_time + (nes.nescfg.FrameCycles / 6)) < nes.cpu.GetTotalCycles())
            {
                elapsed_time = nes.cpu.GetTotalCycles();
            }
        }

        private bool GetExQueue(int writetime, ref QUEUEDATA ret)
        {
            if (exqueue.wrptr == exqueue.rdptr)
            {
                return false;
            }
            if (exqueue.data[exqueue.rdptr].time <= writetime)
            {
                ret = exqueue.data[exqueue.rdptr];
                exqueue.rdptr++;
                exqueue.rdptr = (int)(exqueue.rdptr & (QUEUE_LENGTH - 1));
                return true;
            }
            return false;
        }

        internal void QueueClear()
        {
            queue.Clear();
            exqueue.Clear();
        }

        private void QueueFlush()
        {
            while (queue.wrptr != queue.rdptr)
            {
                WriteProcess(queue.data[queue.rdptr].addr, queue.data[queue.rdptr].data);
                queue.rdptr++;
                queue.rdptr = (int)(queue.rdptr & (QUEUE_LENGTH - 1));
            }

            while (exqueue.wrptr != exqueue.rdptr)
            {
                WriteExProcess(exqueue.data[exqueue.rdptr].addr, exqueue.data[exqueue.rdptr].data);
                exqueue.rdptr++;
                exqueue.rdptr = (int)(exqueue.rdptr & (QUEUE_LENGTH - 1));
            }
        }

        private void WriteExProcess(ushort addr, byte data)
        {
            if ((exsound_select & 0x01) != 0)
            {
                vrc6.Write(addr, data);
            }
            if ((exsound_select & 0x02) != 0)
            {
                vrc7.Write(addr, data);
            }
            if ((exsound_select & 0x04) != 0)
            {
                fds.Write(addr, data);
            }
            if ((exsound_select & 0x08) != 0)
            {
                mmc5.Write(addr, data);
            }
            if ((exsound_select & 0x10) != 0)
            {
                if (addr == 0x0000)
                {
                    byte dummy = n106.Read(addr);
                }
                else
                {
                    n106.Write(addr, data);
                }
            }
            if ((exsound_select & 0x20) != 0)
            {
                fme7.Write(addr, data);
            }
        }

        private void WriteProcess(ushort addr, byte data)
        {
            // $4018はVirtuaNES固有ポート
            if (addr >= 0x4000 && addr <= 0x401F)
            {
                @internal.Write(addr, data);
            }
        }

        internal void SyncDPCM(int cycles)
        {
            @internal.Sync(cycles);
        }

        internal byte Read(ushort addr)
        {
            return @internal.SyncRead(addr);
        }

        internal void Write(ushort addr, byte data)
        {
            // $4018偼VirtuaNES屌桳億乕僩
            if (addr >= 0x4000 && addr <= 0x401F)
            {
                @internal.SyncWrite(addr, data);
                SetQueue(nes.cpu.GetTotalCycles(), addr, data);
            }
        }

        private void SetQueue(int writetime, ushort addr, byte data)
        {
            queue.data[queue.wrptr].time = writetime;
            queue.data[queue.wrptr].addr = addr;
            queue.data[queue.wrptr].data = data;
            queue.wrptr++;

            var newwrptr = (int)(queue.wrptr & (QUEUE_LENGTH - 1));
            queue.wrptr = newwrptr;

            if (queue.wrptr == queue.rdptr)
            {
                Debuger.LogError("queue overflow.");
            }
        }

        private bool GetQueue(int writetime, ref QUEUEDATA ret)
        {
            if (queue.wrptr == queue.rdptr)
            {
                return false;
            }
            if (queue.data[queue.rdptr].time <= writetime)
            {
                ret = queue.data[queue.rdptr];
                queue.rdptr++;
                var newrdptr = (int)(queue.rdptr & (QUEUE_LENGTH - 1));
                queue.rdptr = newrdptr;
                return true;
            }
            return false;
        }

        public void SoundSetup()
        {
            float fClock = nes.nescfg.CpuClock;
            int nRate = Supporter.S.Config.sound.nRate;

            @internal.Setup(fClock, nRate);
            vrc6.Setup(fClock, nRate);
            vrc7.Setup(fClock, nRate);
            mmc5.Setup(fClock, nRate);
            fds.Setup(fClock, nRate);
            n106.Setup(fClock, nRate);
            fme7.Setup(fClock, nRate);
        }

        internal void SelectExSound(byte data)
        {
            exsound_select = data;
        }

        internal void Reset()
        {
            queue = new QUEUE();
            exqueue = new QUEUE();

            elapsed_time = 0;

            float fClock = nes.nescfg.CpuClock;
            int nRate = Supporter.S.Config.sound.nRate;

            @internal.Reset(fClock, nRate);
            vrc6.Reset(fClock, nRate);
            vrc7.Reset(fClock, nRate);
            mmc5.Reset(fClock, nRate);
            fds.Reset(fClock, nRate);
            n106.Reset(fClock, nRate);
            fme7.Reset(fClock, nRate);

            SoundSetup();
        }

        internal void ExWrite(ushort addr, byte data)
        {
            SetExQueue(nes.cpu.GetTotalCycles(), addr, data);

            if ((exsound_select & 0x04) != 0)
            {
                if (addr >= 0x4040 && addr < 0x4100)
                {
                    fds.SyncWrite(addr, data);
                }
            }

            if ((exsound_select & 0x08) != 0)
            {
                if (addr >= 0x5000 && addr <= 0x5015)
                {
                    mmc5.SyncWrite(addr, data);
                }
            }
        }

        private void SetExQueue(int writetime, ushort addr, byte data)
        {
            exqueue.data[exqueue.wrptr].time = writetime;
            exqueue.data[exqueue.wrptr].addr = addr;
            exqueue.data[exqueue.wrptr].data = data;
            exqueue.wrptr++;
            var temp = QUEUE_LENGTH - 1;
            exqueue.wrptr = (int)(exqueue.wrptr & temp);
            if (exqueue.wrptr == exqueue.rdptr)
            {
                Debuger.LogError("exqueue overflow.");
            }
        }

        internal byte ExRead(ushort addr)
        {
            byte data = 0;

            if ((exsound_select & 0x10) != 0)
            {
                if (addr == 0x4800)
                {
                    SetExQueue(nes.cpu.GetTotalCycles(), 0, 0);
                }
            }
            if ((exsound_select & 0x04) != 0)
            {
                if (addr >= 0x4040 && addr < 0x4100)
                {
                    data = fds.SyncRead(addr);
                }
            }
            if ((exsound_select & 0x08) != 0)
            {
                if (addr >= 0x5000 && addr <= 0x5015)
                {
                    data = mmc5.SyncRead(addr);
                }
            }

            return data;
        }

        internal void GetFrameIRQ(ref int Cycle, ref byte Count, ref byte Type, ref byte IRQ, ref byte Occur)
        {
            @internal.GetFrameIRQ(ref Cycle, ref Count, ref Type, ref IRQ, ref Occur);
        }

        internal void SetFrameIRQ(int Cycle, byte Count, byte Type, byte IRQ, byte Occur)
        {
            @internal.SetFrameIRQ(Cycle, Count, Type, IRQ, Occur);
        }

        internal void SaveState(StateBuffer buffer)
        {
            // 時間軸を同期させる為Flushする
            QueueFlush();

            @internal.SaveState(buffer);
            buffer.Position += (@internal.GetSize() + 15) & (~0x0F);

            // VRC6
            if ((exsound_select & 0x01) != 0)
            {
                vrc6.SaveState(buffer);
                buffer.Position += (vrc6.GetSize() + 15) & (~0x0F);  // Padding
            }
            // VRC7 (not support)
            if ((exsound_select & 0x02) != 0)
            {
                vrc7.SaveState(buffer);
                buffer.Position += (vrc7.GetSize() + 15) & (~0x0F);  // Padding
            }
            // FDS
            if ((exsound_select & 0x04) != 0)
            {
                fds.SaveState(buffer);
                buffer.Position += (fds.GetSize() + 15) & (~0x0F);   // Padding

            }
            // MMC5
            if ((exsound_select & 0x08) != 0)
            {
                mmc5.SaveState(buffer);
                buffer.Position += (mmc5.GetSize() + 15) & (~0x0F);  // Padding
            }
            // N106
            if ((exsound_select & 0x10) != 0)
            {
                n106.SaveState(buffer);
                buffer.Position += (n106.GetSize() + 15) & (~0x0F);  // Padding
            }
            // FME7
            if ((exsound_select & 0x20) != 0)
            {
                fme7.SaveState(buffer);
                buffer.Position += (fme7.GetSize() + 15) & (~0x0F);  // Padding
            }
        }

        internal void LoadState(StateReader buffer)
        {
            @internal.LoadState(buffer);
            buffer.Skip((@internal.GetSize() + 15) & (~0x0F));

            // VRC6
            if ((exsound_select & 0x01) != 0)
            {
                vrc6.LoadState(buffer);
                buffer.Skip((int)((vrc6.GetSize() + 15) & (~0x0F)));  // Padding
            }
            // VRC7 (not support)
            if ((exsound_select & 0x02) != 0)
            {
                vrc7.LoadState(buffer);
                buffer.Skip((vrc7.GetSize() + 15) & (~0x0F));  // Padding
            }
            // FDS
            if ((exsound_select & 0x04) != 0)
            {
                fds.LoadState(buffer);
                buffer.Skip((fds.GetSize() + 15) & (~0x0F));   // Padding
            }
            // MMC5
            if ((exsound_select & 0x08) != 0)
            {
                mmc5.LoadState(buffer);
                buffer.Skip((mmc5.GetSize() + 15) & (~0x0F));  // Padding
            }
            // N106
            if ((exsound_select & 0x10) != 0)
            {
                n106.LoadState(buffer);
                buffer.Skip((n106.GetSize() + 15) & (~0x0F));  // Padding
            }
            // FME7
            if ((exsound_select & 0x20) != 0)
            {
                fme7.LoadState(buffer);
                buffer.Skip((fme7.GetSize() + 15) & (~0x0F));  // Padding
            }
        }
    }

    public struct QUEUEDATA
    {
        public int time;
        public ushort addr;
        public byte data;
        public byte reserved;
    }

    public class QUEUE
    {
        public int rdptr;
        public int wrptr;
        public QUEUEDATA[] data = new QUEUEDATA[8192];

        public void Clear()
        {
            rdptr = 0;wrptr = 0;
            data = new QUEUEDATA[8192];
        }
    }
}
