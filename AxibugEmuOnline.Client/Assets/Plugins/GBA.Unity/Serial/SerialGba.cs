using System;

namespace OptimeGBA
{
    /// <summary>
    /// GBA Serial I/O。行为对齐 mGBA src/gba/sio.c（Multi 优先，Normal 次之）。
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

        public GbaLink Link;

        public SioMode Mode = (SioMode)(-1);
        public ushort Rcnt = unchecked((ushort)0x8000);
        public ushort Siocnt;

        /// <summary>SIOMULTI0..3 / SIODATA32 低 4 字节，小端。</summary>
        public byte[] MultiData = new byte[8];

        /// <summary>SIOMLT_SEND / SIODATA8。</summary>
        public ushort MultiSend = 0xFFFF;

        public int PlayerId;
        public bool IsChild;

        bool _completeScheduled;
        // SerialGba 内
        public long LastTransferActivityTick = long.MinValue / 2;

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
                return n - 1;
            }
        }

        public void RefreshMode()
        {
            uint bits = (uint)(((Rcnt & 0xC000) | (Siocnt & 0x3000)) >> 12);
            SioMode neu = bits < 8 ? (SioMode)(bits & 0x3) : (SioMode)(bits & 0xC);

            if (neu != Mode)
            {
                Mode = neu;
                if (Mode == SioMode.Multi)
                {
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

        /// <summary>由 GbaLink 在 Parent Start 时调用：全员进入 Busy。</summary>
        public void BeginMultiTransferFromLink()
        {
            for (int i = 0; i < 8; i++) MultiData[i] = 0xFF;
            Siocnt = (ushort)(Siocnt | (1 << 7));
            Rcnt = (ushort)(Rcnt & ~1);
            // Child 不 Schedule，等 Parent OnTransferComplete → FinishAll

            LastTransferActivityTick = _gba.Scheduler.CurrentTicks;
        }

        void OnTransferComplete(long cyclesLate)
        {
            LastTransferActivityTick = _gba.Scheduler.CurrentTicks;

            _completeScheduled = false;

            switch (Mode)
            {
                case SioMode.Multi:
                    {
                        var words = new ushort[4];
                        for (int i = 0; i < 4; i++) words[i] = 0xFFFF;

                        if (Link != null)
                        {
                            Link.CollectMultiplayerWords(words);
                            Link.FinishAllMultiplayer(words);
                        }
                        else
                        {
                            words[0] = MultiSend;
                            FinishMultiplayerTransfer(words);
                        }
                        break;
                    }
                case SioMode.Normal8:
                    {
                        byte send = (byte)(MultiSend & 0xFF);
                        byte recv = Link != null
                            ? Link.ExchangeNormal8(send, PlayerId)
                            : send;
                        FinishNormal8(recv);
                        break;
                    }
                case SioMode.Normal32:
                    {
                        uint send = GetNormal32();
                        uint recv = Link != null
                            ? Link.ExchangeNormal32(send, PlayerId)
                            : send;
                        FinishNormal32(recv);
                        break;
                    }
            }
        }

        public void FinishMultiplayerTransfer(ushort[] data)
        {
            if (data == null || data.Length < 4) return;

            WriteMultiWord(0, data[0]);
            WriteMultiWord(1, data[1]);
            WriteMultiWord(2, data[2]);
            WriteMultiWord(3, data[3]);

            Siocnt = (ushort)(Siocnt & ~(1 << 7));          // Busy clear
            Siocnt = (ushort)(Siocnt & ~(1 << 6));          // Error clear
            Siocnt = (ushort)((Siocnt & ~0x30) | ((PlayerId & 3) << 4));
            Rcnt = (ushort)(Rcnt | 1);

            if ((Siocnt & (1 << 14)) != 0)
                _gba.HwControl.FlagInterrupt((uint)InterruptGba.Serial);
        }

        public void FinishNormal8(byte data)
        {
            Siocnt = (ushort)(Siocnt & ~(1 << 7));
            MultiSend = (ushort)((MultiSend & 0xFF00) | data);
            if ((Siocnt & (1 << 14)) != 0)
                _gba.HwControl.FlagInterrupt((uint)InterruptGba.Serial);
        }

        public void FinishNormal32(uint data)
        {
            Siocnt = (ushort)(Siocnt & ~(1 << 7));
            MultiData[0] = (byte)(data);
            MultiData[1] = (byte)(data >> 8);
            MultiData[2] = (byte)(data >> 16);
            MultiData[3] = (byte)(data >> 24);
            if ((Siocnt & (1 << 14)) != 0)
                _gba.HwControl.FlagInterrupt((uint)InterruptGba.Serial);
        }

        public uint GetNormal32()
        {
            return (uint)(MultiData[0] | (MultiData[1] << 8) |
                          (MultiData[2] << 16) | (MultiData[3] << 24));
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
                // Slave(SI)：只由本机是否为 Child 决定，不要用 connected==0 强行当 Slave
                if (IsChild)
                    v = (ushort)(v | (1 << 2));
                else
                    v = (ushort)(v & ~(1 << 2));

                v = (ushort)((v & ~0x30) | ((PlayerId & 3) << 4));

                // Ready(SD)：有 GbaLink 就视为线已接好（双实例本机联机）
                // 无 Link 时也置 1，避免单机菜单死等（与改前行为一致）
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
                        value = (ushort)(value & 0xFF83);
                        if (IsChild)
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
                            for (int i = 0; i < 8; i++) MultiData[i] = 0xFF;
                            Rcnt = (ushort)(Rcnt & ~1);
                            Link?.NotifyParentStartedTransfer();
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
                        {
                            if (Link != null)
                                Link.NotifyNormalStart(this);
                            StartTransfer();
                        }
                        break;
                    }
                default:
                    Siocnt = value;
                    break;
            }
        }

        void ApplyRcntWrite(ushort value)
        {
            Rcnt = (ushort)((Rcnt & 0x1FF) | (value & 0xC000));
            RefreshMode();
            if (Mode == SioMode.Gpio)
                Rcnt = (ushort)((Rcnt & 0xC000) | (value & 0x1FF));
            else
                Rcnt = (ushort)((Rcnt & 0xC00F) | (value & 0x1F0));
        }
    }
}