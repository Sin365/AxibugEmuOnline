using System;
using static OptimeGBA.Bits;

namespace OptimeGBA
{
    public enum EepromState
    {
        Ready,
        StartRequest,
        ReceiveAddrForRead,
        ReceiveAddrForWrite,
        ReceiveDataForWrite,
        ReceiveTerminatingZeroWrite,
        ReceiveTerminatingZeroRead,
    }

    public enum EepromSize
    {
        Eeprom4k,   // 512 bytes, 6-bit address
        Eeprom64k   // 8192 bytes, 14-bit address (有效 10-bit)
    }

    public sealed class Eeprom : SaveProvider
    {
        EepromState State = EepromState.Ready;
        public EepromSize Size;
        public bool SizeLocked; // 一旦根据协议确定尺寸就锁定

        public byte[] EEPROM;

        public uint Addr;          // 写：页地址
        public uint ReadAddr;      // 读：页地址
        public uint BitAddr;      // 写用的 bit 地址
        public uint ReadBitAddr;  // 读用的 bit 地址

        public uint BitsRemaining;
        public uint ReadBitsRemaining;
        public uint DataBitIndex;

        public Gba Gba;

        //public Eeprom(Gba gba, EepromSize size)
        //{
        //    Gba = gba;
        //    Size = size;
        //    SizeLocked = false;
        //    // 先按最大分配，后面可裁剪；默认全 0（你说 0xFF 会搞坏耀西岛，就保持 0）
        //    EEPROM = new byte[8192];
        //}
        public Eeprom(Gba gba, EepromSize size)
        {
            Gba = gba;
            Size = size;
            SizeLocked = false;
            EEPROM = new byte[size == EepromSize.Eeprom64k ? 8192 : 512];
            for (int i = 0; i < EEPROM.Length; i++)
                EEPROM[i] = 0xFF;   // 擦除态
        }

        // 有效页数：4k → 64 页(0..0x3F)，64k → 1024 页(0..0x3FF)
        //private uint PageMask => Size == EepromSize.Eeprom64k ? 0x3FFu : 0x3Fu;
        private uint AddressMask => Size == EepromSize.Eeprom64k ? 0x3FFu : 0x3Fu;
        public override byte Read8(uint addr)
        {
            if (!Gba.Dma.DmaLock)
                return 1;   // 非 DMA：不推进状态
            // 空闲：必须返回 1，禁止在这里设 ReadBitsRemaining = 68
            if (ReadBitsRemaining == 0)
                return 1;

            ReadBitsRemaining--;

            // 前 4 个 dummy（剩余 67..64）：必须返回 0
            if (ReadBitsRemaining >= 64)
                return 0;

            // 后 64 个数据 bit：MSB first
            int step = 63 - (int)ReadBitsRemaining;
            uint byteIndex = (ReadAddr * 8) + ((uint)step >> 3);
            int bitInByte = 7 - (step & 7);

            if (byteIndex >= (uint)EEPROM.Length)
                return 1;

            return (byte)((EEPROM[byteIndex] >> bitInByte) & 1);
        }
        public override void Write8(uint addr, byte val)
        {
            if (!Gba.Dma.DmaLock)
                return;     // 非 DMA：忽略写
            bool bit = (val & 1) != 0;

            switch (State)
            {
                case EepromState.Ready:
                    if (bit)
                        State = EepromState.StartRequest;
                    break;

                case EepromState.StartRequest:
                    // Size 已由 DMA 定好
                    BitsRemaining = Size == EepromSize.Eeprom64k ? 14u : 6u;
                    if (bit)
                    {
                        // 读命令 11
                        State = EepromState.ReceiveAddrForRead;
                        ReadAddr = 0;
                    }
                    else
                    {
                        // 写命令 10
                        State = EepromState.ReceiveAddrForWrite;
                        Addr = 0;
                    }
#if DEBUG
                    UnityEngine.Debug.Log(
    $"[EEPROM] cmd={(bit ? "READ" : "WRITE")} " +
    $"Size={(Size == EepromSize.Eeprom64k ? "64k" : "4k")} " +
    $"addrBits={BitsRemaining}"
);
#endif
                    break;

                case EepromState.ReceiveAddrForRead:
                    ReadAddr = ((ReadAddr << 1) | (bit ? 1u : 0u)) & AddressMask;
                    BitsRemaining--;
                    if (BitsRemaining == 0)
                        State = EepromState.ReceiveTerminatingZeroRead;
                    break;

                case EepromState.ReceiveAddrForWrite:
                    Addr = ((Addr << 1) | (bit ? 1u : 0u)) & AddressMask;
                    BitsRemaining--;
                    if (BitsRemaining == 0)
                    {
                        BitsRemaining = 64;
                        DataBitIndex = 0;
                        BitAddr = Addr << 3;
                        State = EepromState.ReceiveDataForWrite;
                    }
                    break;

                case EepromState.ReceiveDataForWrite:
                    {
                        uint byteIndex = (Addr * 8) + (DataBitIndex >> 3);
                        int bitInByte = 7 - (int)(DataBitIndex & 7);

                        if (byteIndex < (uint)EEPROM.Length)
                        {
                            if (bit)
                                EEPROM[byteIndex] = BitSet(EEPROM[byteIndex], (byte)bitInByte);
                            else
                                EEPROM[byteIndex] = BitClear(EEPROM[byteIndex], (byte)bitInByte);
                        }

                        DataBitIndex++;
                        BitsRemaining--;
                        if (BitsRemaining == 0)
                        {
                            Dirty = true;
                            State = EepromState.ReceiveTerminatingZeroWrite;
                        }
                        break;
                    }

                // 写：可删掉 BitAddr = Addr << 3，不再需要
                // 读：
                case EepromState.ReceiveTerminatingZeroRead:
                    // ReadBitAddr 可以不用了
                    ReadBitsRemaining = 68;
                    State = EepromState.Ready;
#if DEBUG
                    UnityEngine.Debug.Log($"[EEPROM] READ page=0x{ReadAddr:X} -> 68 bits");
#endif
                    break;

                case EepromState.ReceiveTerminatingZeroWrite:
                    {
#if DEBUG
                        uint off = Addr << 3;
                        UnityEngine.Debug.Log(
                            $"[EEPROM] WRITE done page=0x{Addr:X} " +
                            $"{EEPROM[off]:X2}{EEPROM[off + 1]:X2}{EEPROM[off + 2]:X2}{EEPROM[off + 3]:X2}");
#endif
                        State = EepromState.Ready;
                    }
                    break;
            }
        }

        public override byte[] GetSave()
        {
            if (Size == EepromSize.Eeprom4k)
            {
                byte[] small = new byte[512];
                int n = Math.Min(512, EEPROM.Length);
                Array.Copy(EEPROM, 0, small, 0, n);
                return small;
            }
            return EEPROM;
        }

        public override void LoadSave(byte[] save)
        {
            if (save == null || save.Length == 0)
                return;

            if (save.Length >= 8192)
            {
                Size = EepromSize.Eeprom64k;
                if (EEPROM.Length < 8192)
                    EEPROM = new byte[8192];
            }
            else
            {
                Size = EepromSize.Eeprom4k;
                if (EEPROM.Length < 512)
                    EEPROM = new byte[512];
            }

            for (int i = 0; i < save.Length && i < EEPROM.Length; i++)
                EEPROM[i] = save[i];

            SizeLocked = true;
        }

        public void DetectSizeFromDmaLength(uint dmaLength)
        {
            if (SizeLocked)
                return;

            EepromSize detected;

            // 只认真正的 EEPROM 命令长度，其它全部忽略（含 7936）
            if (dmaLength == 81 || dmaLength == 17 ||
                (dmaLength >= 79 && dmaLength <= 83) ||
                (dmaLength >= 15 && dmaLength <= 18))
            {
                detected = EepromSize.Eeprom64k;
            }
            else if (dmaLength == 73 || dmaLength == 9 ||
                     (dmaLength >= 71 && dmaLength <= 75) ||
                     (dmaLength >= 8 && dmaLength <= 10))
            {
                detected = EepromSize.Eeprom4k;
            }
            else
            {
                // 7936、1024、512、28... 全部 return，不锁
                return;
            }

            if (detected == EepromSize.Eeprom64k && EEPROM.Length < 8192)
            {
                byte[] n = new byte[8192];
                for (int i = 0; i < n.Length; i++)
                    n[i] = 0xFF;
                Array.Copy(EEPROM, n, EEPROM.Length);
                EEPROM = n;
            }
            else if (detected == EepromSize.Eeprom4k && EEPROM.Length != 512)
            {
                // 若从默认变 4k，可保持 512；若已是 8192 也可只改 Size 不缩
                // 这里只改 Size 即可
            }

            Size = detected;
            SizeLocked = true;
#if DEBUG
            UnityEngine.Debug.Log(
                $"[EEPROM] size -> {(Size == EepromSize.Eeprom64k ? "64k" : "4k")} dmaLen={dmaLength}"
            );
#endif
        }
    }
}