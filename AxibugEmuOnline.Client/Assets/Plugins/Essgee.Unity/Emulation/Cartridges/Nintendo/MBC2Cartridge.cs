using System;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    public class MBC2Cartridge : IGameBoyCartridge
    {
        byte[] romData, ramData;
        bool hasBattery;

        byte romBank;
        bool ramEnable;

        public MBC2Cartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];
            ramData = new byte[ramSize];

            romBank = 1;

            ramEnable = false;

            hasBattery = false;
        }
        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            return data;
        }
        #endregion

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
            else if (address >= 0xA000 && address <= 0xA1FF)
            {
                if (ramEnable)
                {
                    var ramOffset = (address >> 1) & 0x00FF;
                    var valueShift = (address & 0x01) << 2;
                    return (byte)((ramData[ramOffset] >> valueShift) & 0x0F);
                }
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
                if ((address & 0x0100) == 0)
                    ramEnable = (value & 0x0F) == 0x0A;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                if ((address & 0x0100) != 0)
                {
                    romBank = (byte)((romBank & 0xF0) | (value & 0x0F));
                    romBank &= (byte)((romData.Length >> 14) - 1);
                    if ((romBank & 0x0F) == 0x00) romBank |= 0x01;
                }
            }
            else if (address >= 0xA000 && address <= 0xA1FF)
            {
                if (ramEnable)
                {
                    var ramOffset = (address >> 1) & 0x00FF;
                    var valueShift = (address & 0x01) << 2;

                    ramData[ramOffset] = (byte)((ramData[ramOffset] & (0x0F << (valueShift ^ 0x04))) | (value << valueShift));
                }
            }
        }
    }
}
