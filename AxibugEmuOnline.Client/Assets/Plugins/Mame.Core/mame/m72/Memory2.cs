using System;

namespace MAME.Core
{
    public unsafe partial class M72
    {
        public static byte NReadOpByte_airduel(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0xb0000 && address <= 0xb0fff)
            {
                int offset = address - 0xb0000;
                result = protection_ram[offset];
            }
            else
            {
                result = Memory.mainrom[address];
            }
            return result;
        }
        public static byte NReadByte_m72_airduel(int address)
        {
            address &= 0xfffff;
            byte result = 0;
            if (address >= 0xb0ffa && address <= 0xb0ffb)
            {
                int offset = address - 0xb0ffa;
                Array.Copy(airduelm72_code, protection_ram, 96);
                result = protection_ram[0xffa + offset];
            }
            else if (address >= 0xb0000 && address <= 0xb0fff)
            {
                int offset = address - 0xb0000;
                result = protection_ram[offset];
            }
            else
            {
                result = NReadByte_m72(address);
            }
            return result;
        }
        public static ushort NReadWord_m72_airduel(int address)
        {
            address &= 0xfffff;
            ushort result = 0;
            if (address >= 0xb0ffa && address + 1 <= 0xb0ffb)
            {
                int offset = address - 0xb0ffa;
                Array.Copy(airduelm72_code, protection_ram, 96);
                result = (ushort)(protection_ram[0xffa + offset] + protection_ram[0xffa + offset + 1] * 0x100);
            }
            else if (address >= 0xb0000 && address + 1 <= 0xb0fff)
            {
                int offset = address - 0xb0000;
                result = (ushort)(protection_ram[offset] + protection_ram[offset + 1] * 0x100);
            }
            else
            {
                result = NReadWord_m72(address);
            }
            return result;
        }
        public static void NWriteByte_m72_airduel(int address, byte value)
        {
            address &= 0xfffff;
            if (address >= 0xb0000 && address <= 0xb0fff)
            {
                int offset = address - 0xb0000;
                protection_w(airduelm72_crc, offset, value);
            }
            else
            {
                NWriteByte_m72(address, value);
            }
        }
        public static void NWriteWord_m72_airduel(int address, ushort value)
        {
            address &= 0xfffff;
            if (address >= 0xb0000 && address + 1 <= 0xb0fff)
            {
                int offset = (address - 0xb0000) / 2;
                protection_w(airduelm72_crc, offset, value);
            }
            else
            {
                NWriteWord_m72(address, value);
            }
        }
        public static void NWriteIOByte_m72_airduel(int address, byte data)
        {
            if (address >= 0xc0 && address <= 0xc1)
            {
                airduelm72_sample_trigger_w(data);
            }
            else
            {
                NWriteIOByte_m72(address, data);
            }
        }
        public static void NWriteIOWord_m72_airduel(int address, ushort data)
        {
            if (address >= 0xc0 && address + 1 <= 0xc1)
            {
                airduelm72_sample_trigger_w((byte)data);
            }
            else
            {
                NWriteIOWord_m72(address, data);
            }
        }
    }
}
