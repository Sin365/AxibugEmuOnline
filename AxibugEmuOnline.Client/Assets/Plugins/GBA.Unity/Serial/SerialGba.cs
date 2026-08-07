using System;

namespace OptimeGBA
{
    /// <summary>
    /// GBA Serial I/O。行为对齐 mGBA src/gba/sio.c（Multi 优先）。
    /// </summary>
    public sealed class SerialGba
    {
        public const int MaxPlayers = 4;

        // mGBA GBASIOCyclesPerTransfer[baud][connectedIndex]
        static readonly int[,] CyclesPerTransfer =
        {
            { 31976, 63427, 94884, 125829 },
            { 8378,  16241, 24104, 31457  },
            { 5750,  10998, 16241, 20972  },
            { 3140,  5755,  8376,  10486  },
        };

        public enum SioMode
        {
            Normal8 = 0,
            Normal32 = 1,
            Multi = 2,
            Uart = 3,
            Gpio = 8,
            Joybus = 12,
        }

        readonly Gba _gba;
        readonly SchedulerCallback _onComplete;

        public GbaLink Link; // 本机联机线，可 null

        public SioMode Mode = (SioMode)(-1);
        public ushort Rcnt = unchecked((ushort)0x8000); // mGBA RCNT_INITIAL
        public ushort Siocnt;

        /// <summary>SIOMULTI0..3，各 16bit，按字节存低在前。</summary>
        public byte[] MultiData = new byte[8];

        /// <summary>SIOMLT_SEND / SIODATA8 低 16bit。</summary>
        public ushort MultiSend = 0xFFFF;

        public int PlayerId;   // 由 GbaLink 设置，0=Parent
        public bool IsChild;   // PlayerId != 0

        bool _completeScheduled;

        public SerialGba(Gba gba)
        {
            _gba = gba;
            _onComplete = OnTransferComplete;
            for (int i = 0; i < 8; i++) MultiData[i] = 0xFF;
        }

        public void Reset()
        {
            CancelComplete();
            Rcnt = unchecked((ushort)0x8000);
            Siocnt = 0;
            Mode = (SioMode)(-1);
            MultiSend = 0xFFFF;
            for (int i = 0; i < 8; i++) MultiData[i] = 0xFF;
            RefreshMode();
        }

        int ConnectedIndex
        {
            get
            {
                if (Link == null) return 0;
                int n = Link.AttachedCount;
                if (n <= 1) return 0;
                if (n > 4) n = 4;
                return n - 1; // mGBA: connected 作表下标 0..3
            }
        }

        public void RefreshMode()
        {
            // mGBA: ((rcnt & 0xC000) | (siocnt & 0x3000)) >> 12
            uint bits = (uint)(((Rcnt & 0xC000) | (Siocnt & 0x3000)) >> 12);
            SioMode neu;
            if (bits < 8)
                neu = (SioMode)(bits & 0x3);
            else
                neu = (SioMode)(bits & 0xC);

            if (neu != Mode)
            {
                Mode = neu;
                if (Mode == SioMode.Multi)
                {
                    // SI = child
                    if (IsChild)
                        Rcnt = (ushort)(Rcnt | (1 << 2));
                    else
                        Rcnt = (ushort)(Rcnt & ~(1 << 2));
                }
            }
        }

        public static int TransferCycles(SioMode mode, ushort siocnt, int connectedIndex)
        {
            if (connectedIndex < 0) connectedIndex = 0;
            if (connectedIndex > 3) connectedIndex = 3;
            switch (mode)
            {
                case SioMode.Multi:
                    return CyclesPerTransfer[siocnt & 3, connectedIndex];
                case SioMode.Normal8:
                    {
                        bool internalSc = (siocnt & 2) != 0;
                        int div = (internalSc ? 2048 : 256) * 1024;
                        return 8 * (16777216 / div);
                    }
                case SioMode.Normal32:
                    {
                        bool internalSc = (siocnt & 2) != 0;
                        int div = (internalSc ? 2048 : 256) * 1024;
                        return 32 * (16777216 / div);
                    }
                default:
                    return 0;
            }
        }

        void CancelComplete()
        {
            if (!_completeScheduled) return;
            _gba.Scheduler.CancelEventsById(SchedulerId.Sio);
            _completeScheduled = false;
        }

        void StartTransfer()
        {
            int cycles = TransferCycles(Mode, Siocnt, ConnectedIndex);
            if (cycles <= 0) cycles = 1;
            CancelComplete();
            _completeScheduled = true;
            _gba.Scheduler.AddEventRelative(SchedulerId.Sio, cycles, _onComplete);
        }

        void OnTransferComplete(long cyclesLate)
        {
            _completeScheduled = false;
            if (Mode != SioMode.Multi)
                return;

            var words = new ushort[4];
            for (int i = 0; i < 4; i++) words[i] = 0xFFFF;

            if (Link != null)
                Link.CollectMultiplayerWords(words);
            else
                words[0] = MultiSend; // 未接线：仅本机

            // 本机 + 链上其它核一并完成（本机同步）
            if (Link != null)
                Link.FinishAllMultiplayer(words);
            else
                FinishMultiplayerTransfer(words);
        }

        /// <summary>对齐 mGBA GBASIOMultiplayerFinishTransfer。</summary>
        public void FinishMultiplayerTransfer(ushort[] data)
        {
            if (data == null || data.Length < 4) return;

            WriteMultiWord(0, data[0]);
            WriteMultiWord(1, data[1]);
            WriteMultiWord(2, data[2]);
            WriteMultiWord(3, data[3]);

            // Clear Busy bit7
            Siocnt = (ushort)(Siocnt & ~(1 << 7));
            // Id bits 4-5
            Siocnt = (ushort)((Siocnt & ~0x30) | ((PlayerId & 3) << 4));
            // SC high
            Rcnt = (ushort)(Rcnt | 1);

            if ((Siocnt & (1 << 14)) != 0)
                _gba.HwControl.FlagInterrupt((uint)InterruptGba.Serial);
        }

        void WriteMultiWord(int index, ushort w)
        {
            int o = index * 2;
            MultiData[o] = (byte)(w & 0xFF);
            MultiData[o + 1] = (byte)(w >> 8);
        }

        // ---------- HWIO ----------

        public byte ReadHwio8(uint addr)
        {
            switch (addr)
            {
                case 0x4000120:
                case 0x4000121:
                case 0x4000122:
                case 0x4000123:
                case 0x4000124:
                case 0x4000125:
                case 0x4000126:
                case 0x4000127:
                    return MultiData[addr - 0x4000120];

                case 0x4000128:
                    return (byte)(ReadSiocnt() & 0xFF);
                case 0x4000129:
                    return (byte)(ReadSiocnt() >> 8);

                case 0x400012A:
                    return (byte)(MultiSend & 0xFF);
                case 0x400012B:
                    return (byte)(MultiSend >> 8);

                case 0x4000134:
                    return (byte)(Rcnt & 0xFF);
                case 0x4000135:
                    return (byte)(Rcnt >> 8);

                default:
                    return 0;
            }
        }

        public void WriteHwio8(uint addr, byte val)
        {
            switch (addr)
            {
                case 0x4000120:
                case 0x4000121:
                case 0x4000122:
                case 0x4000123:
                case 0x4000124:
                case 0x4000125:
                case 0x4000126:
                case 0x4000127:
                    MultiData[addr - 0x4000120] = val;
                    break;

                case 0x4000128:
                    ApplySiocntWrite((ushort)((Siocnt & 0xFF00) | val));
                    break;
                case 0x4000129:
                    ApplySiocntWrite((ushort)((Siocnt & 0x00FF) | (val << 8)));
                    break;

                case 0x400012A:
                    MultiSend = (ushort)((MultiSend & 0xFF00) | val);
                    break;
                case 0x400012B:
                    MultiSend = (ushort)((MultiSend & 0x00FF) | (val << 8));
                    break;

                case 0x4000134:
                    ApplyRcntWrite((ushort)((Rcnt & 0xFF00) | val));
                    break;
                case 0x4000135:
                    ApplyRcntWrite((ushort)((Rcnt & 0x00FF) | (val << 8)));
                    break;
            }
        }

        ushort ReadSiocnt()
        {
            ushort v = Siocnt;
            if (Mode == SioMode.Multi)
            {
                int connected = Link != null ? Link.AttachedCount : 0;
                // Slave bit2：非 Parent 或未连接
                if (IsChild || connected == 0)
                    v = (ushort)(v | (1 << 2));
                else
                    v = (ushort)(v & ~(1 << 2));

                v = (ushort)((v & ~0x30) | ((PlayerId & 3) << 4));

                // Ready bit3：有线或单机 dummy 都置 1，避免菜单死等
                v = (ushort)(v | (1 << 3));
            }
            return v;
        }

        void ApplySiocntWrite(ushort value)
        {
            ushort old = Siocnt;

            if (((value ^ old) & 0x3000) != 0)
            {
                Siocnt = (ushort)((Siocnt & ~0x3000) | (value & 0x3000));
                RefreshMode();
            }

            int connected = Link != null ? Link.AttachedCount : 0;

            switch (Mode)
            {
                case SioMode.Multi:
                    {
                        // mGBA: value &= 0xFF83; 再并上旧 0x00FC 等
                        value = (ushort)(value & 0xFF83);
                        if (IsChild || connected == 0)
                            value = (ushort)(value | (1 << 2));
                        else
                            value = (ushort)(value & ~(1 << 2));
                        value = (ushort)((value & ~0x30) | ((PlayerId & 3) << 4));
                        value = (ushort)(value | (old & 0x00FC));

                        Rcnt = (ushort)(Rcnt | 1);

                        bool newBusy = (value & (1 << 7)) != 0;
                        bool oldBusy = (old & (1 << 7)) != 0;
                        Siocnt = value;

                        if (newBusy && !oldBusy && !IsChild)
                        {
                            // Parent 启动
                            for (int i = 0; i < 8; i++) MultiData[i] = 0xFF;
                            Rcnt = (ushort)(Rcnt & ~1);
                            StartTransfer();
                        }
                        break;
                    }
                case SioMode.Normal8:
                case SioMode.Normal32:
                    {
                        Siocnt = value;
                        bool newStart = (value & (1 << 7)) != 0;
                        bool oldStart = (old & (1 << 7)) != 0;
                        if (newStart && !oldStart)
                            StartTransfer(); // Normal 完成第二阶段再补
                        break;
                    }
                default:
                    Siocnt = value;
                    break;
            }
        }

        void ApplyRcntWrite(ushort value)
        {
            // mGBA WriteRCNT 简化
            Rcnt = (ushort)((Rcnt & 0x1FF) | (value & 0xC000));
            RefreshMode();
            if (Mode == SioMode.Gpio)
                Rcnt = (ushort)((Rcnt & 0xC000) | (value & 0x1FF));
            else
                Rcnt = (ushort)((Rcnt & 0xC00F) | (value & 0x1F0));
        }
    }
}