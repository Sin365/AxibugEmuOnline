using System;

namespace Essgee.Emulation.Cartridges.Nintendo
{
    public class NoMapperCartridge : IGameBoyCartridge
    {
        byte[] romData, ramData;
        bool hasBattery;

        public NoMapperCartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];
            ramData = new byte[ramSize];
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

        public void Step(int clockCyclesInStep)
        {
            /* Nothing to do */
        }

        public void SetCartridgeConfig(bool battery, bool rtc, bool rumble)
        {
            hasBattery = battery;
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x7FFF)
                return romData[address & 0x7FFF];
            else if (address >= 0xA000 && address <= 0xBFFF)
                return ramData[address & 0x1FFF];
            else
                return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0xA000 && address <= 0xBFFF)
                ramData[address & 0x1FFF] = value;
        }
    }
}
