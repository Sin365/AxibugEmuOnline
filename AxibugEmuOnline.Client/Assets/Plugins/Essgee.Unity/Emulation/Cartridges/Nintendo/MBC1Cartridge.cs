using System;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    public class MBC1Cartridge : IGameBoyCartridge
    {
        byte[] romData, ramData;
        bool hasBattery;

        byte romBank, ramBank;
        bool ramEnable;

        byte bankingMode;

        public MBC1Cartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];
            ramData = new byte[ramSize];

            romBank = 1;
            ramBank = 0;

            ramEnable = false;

            bankingMode = 0;

            hasBattery = false;
        }

        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));
        }

        public void LoadRam(byte[] data)
        {
            Buffer.BlockCopy(data, 0, ramData, 0, Math.Min(data.Length, ramData.Length));
        }

        public byte[] GetRomData()
        {
            return romData;
        }

        public byte[] GetRamData()
        {
            return ramData;
        }

        public bool IsRamSaveNeeded()
        {
            return hasBattery;
        }

        public ushort GetLowerBound()
        {
            return 0x0000;
        }

        public ushort GetUpperBound()
        {
            return 0x7FFF;
        }

        public void SetCartridgeConfig(bool battery, bool rtc, bool rumble)
        {
            hasBattery = battery;
        }

        public void Step(int clockCyclesInStep)
        {
            /* Nothing to do */
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3FFF)
            {
                return romData[address & 0x3FFF];
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                return romData[(romBank << 14) | (address & 0x3FFF)];
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (ramEnable && ramData.Length != 0)
                    return ramData[(ramBank << 13) | (address & 0x1FFF)];
                else
                    return 0xFF;
            }
            else
                return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                ramEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                romBank = (byte)((romBank & 0xE0) | (value & 0x1F));
                romBank &= (byte)((romData.Length >> 14) - 1);
                if ((romBank & 0x1F) == 0x00) romBank |= 0x01;
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if (bankingMode == 0)
                {
                    romBank = (byte)((romBank & 0x9F) | ((value & 0x03) << 5));
                    romBank &= (byte)((romData.Length >> 14) - 1);
                    if ((romBank & 0x1F) == 0x00) romBank |= 0x01;
                }
                else
                {
                    ramBank = (byte)(value & 0x03);
                }
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                bankingMode = (byte)(value & 0b1);
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (ramEnable && ramData.Length != 0)
                    ramData[(ramBank << 13) | (address & 0x1FFF)] = value;
            }
        }
    }
}
