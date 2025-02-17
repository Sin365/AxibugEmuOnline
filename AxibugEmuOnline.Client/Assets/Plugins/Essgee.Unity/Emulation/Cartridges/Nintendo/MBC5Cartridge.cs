using System;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    // TODO: rumble?

    public class MBC5Cartridge : IGameBoyCartridge
    {
        public event EventHandler<EventArgs> EnableRumble;
        protected virtual void OnEnableRumble(EventArgs e) { EnableRumble?.Invoke(this, EventArgs.Empty); }

        byte[] romData, ramData;
        bool hasBattery, hasRumble;

        ushort romBank;
        byte ramBank;
        bool ramEnable;

        public MBC5Cartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];
            ramData = new byte[ramSize];

            romBank = 1;
            ramBank = 0;

            ramEnable = false;

            hasBattery = false;
            hasRumble = false;
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
            hasRumble = rumble;
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
            else if (address >= 0x2000 && address <= 0x2FFF)
            {
                romBank = (ushort)((romBank & 0x0100) | value);
                romBank &= (ushort)((romData.Length >> 14) - 1);
            }
            else if (address >= 0x3000 && address <= 0x3FFF)
            {
                romBank = (ushort)((romBank & 0x00FF) | ((value & 0x01) << 8));
                romBank &= (ushort)((romData.Length >> 14) - 1);
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                if (hasRumble)
                {
                    if ((value & 0x08) == 0x08) OnEnableRumble(EventArgs.Empty);
                    ramBank = (byte)(value & 0x07);
                    ramBank %= (byte)(ramData.Length >> 13);
                }
                else
                {
                    ramBank = (byte)(value & 0x0F);
                    ramBank %= (byte)(ramData.Length >> 13);
                }
            }
            else if (address >= 0xA000 && address <= 0xBFFF)
            {
                if (ramEnable && ramData.Length != 0)
                    ramData[(ramBank << 13) | (address & 0x1FFF)] = value;
            }
        }
    }
}
