using System;
using static OptimeGBA.Bits;
namespace OptimeGBA
{
    public enum EepromState
    {
        Ready,
        StartRequest,
        ReceiveRequestType,
        ReceiveAddrForRead,
        ReceiveAddrForWrite,
        ReceiveDataForWrite,
        ReceiveTerminatingZero
    }

    public enum EepromSize
    {
        Eeprom4k,
        Eeprom64k
    }

    public sealed class Eeprom : SaveProvider
    {
        private const uint PageSizeBytes = 8;

        EepromState State = EepromState.Ready;
        EepromSize Size;

        public byte[] EEPROM;
        public uint Addr = 0;
        public uint ReadAddr = 0;

        public uint BitsRemaining = 0;
        public uint ReadBitsRemaining = 0;
        public uint DataBitIndex = 0;

        public Gba Gba;

        public Eeprom(Gba gba, EepromSize size)
        {
            Gba = gba;
            Size = size;
            EEPROM = new byte[Size == EepromSize.Eeprom64k ? 8192 : 512];
        }

        private uint AddressMask => Size == EepromSize.Eeprom64k ? 0x3FFFu : 0x3Fu;

        public byte ReadBitEEPROM()
        {
            if (ReadBitsRemaining > 64)
            {
                return 1;
            }

            uint byteIndex = (ReadAddr << 3) + (DataBitIndex >> 3);
            byte bitIndex = (byte)(DataBitIndex & 7);
            byte result = (byte)(BitTest(EEPROM[byteIndex], bitIndex) ? 1 : 0);
            DataBitIndex++;
            return result;
        }
        public void WriteBitEEPROM(bool bit)
        {
            uint byteIndex = (Addr << 3) + (DataBitIndex >> 3);
            byte bitIndex = (byte)(DataBitIndex & 7);
            if (bit)
            {
                EEPROM[byteIndex] = BitSet(EEPROM[byteIndex], bitIndex);
            }
            else
            {
                EEPROM[byteIndex] = BitClear(EEPROM[byteIndex], bitIndex);
            }
            DataBitIndex++;
        }

        public override byte Read8(uint addr)
        {
            if (Gba.Dma.DmaLock)
            {
                // Debug.Log("[EEPROM] Read from DMA");
            }

            byte val = 0;
            if (ReadBitsRemaining > 0)
            {
                if (ReadBitsRemaining <= 64)
                {
                    val = ReadBitEEPROM();
                }
                else
                {
                    val = 1;
                }

                ReadBitsRemaining--;
            }
            else
            {
                ReadBitsRemaining = 68;
            }

            return val;
        }

        public override void Write8(uint addr, byte val)
        {
            if (Gba.Dma.DmaLock)
            {
                // Debug.Log("[EEPROM] Write from DMA");
            }

            bool bit = BitTest(val, 0);
            switch (State)
            {
                case EepromState.Ready:
                    if (bit)
                    {
                        State = EepromState.StartRequest;
                    }
                    break;
                case EepromState.StartRequest:
                    BitsRemaining = Size == EepromSize.Eeprom64k ? 14U : 6U;
                    if (bit)
                    {
                        State = EepromState.ReceiveAddrForRead;
                        ReadAddr = 0;
                    }
                    else
                    {
                        State = EepromState.ReceiveAddrForWrite;
                        Addr = 0;
                    }
                    break;
                case EepromState.ReceiveAddrForRead:
                    if (BitsRemaining > 0)
                    {
                        ReadAddr = ((ReadAddr << 1) | (bit ? 1u : 0u)) & AddressMask;
                        BitsRemaining--;

                        if (BitsRemaining == 0)
                        {
                            State = EepromState.ReceiveTerminatingZero;
                            ReadBitsRemaining = 68;
                            DataBitIndex = 0;
                        }
                    }
                    break;
                case EepromState.ReceiveAddrForWrite:
                    if (BitsRemaining > 0)
                    {
                        Addr = ((Addr << 1) | (bit ? 1u : 0u)) & AddressMask;
                        BitsRemaining--;

                        if (BitsRemaining == 0)
                        {
                            BitsRemaining = 64;
                            State = EepromState.ReceiveDataForWrite;
                            DataBitIndex = 0;
                        }
                    }
                    break;
                case EepromState.ReceiveDataForWrite:
                    if (BitsRemaining > 0)
                    {
                        WriteBitEEPROM(bit);
                        BitsRemaining--;

                        if (BitsRemaining == 0)
                        {
                            Dirty = true;
                            State = EepromState.Ready;
                        }
                    }
                    break;
                case EepromState.ReceiveTerminatingZero:
                    State = EepromState.Ready;
                    break;
            }
        }

        public override byte[] GetSave()
        {
            return EEPROM;
        }

        public override void LoadSave(byte[] save)
        {
            for (uint i = 0; i < save.Length && i < EEPROM.Length; i++)
            {
                EEPROM[i] = save[i];
            }
        }
    }
}