using Essgee.Exceptions;
using Essgee.Utilities;
using System;

namespace Essgee.Emulation.Cartridges.Sega
{
    public class KoreanMSX8kMapperCartridge : ICartridge
    {
        byte[] romData;

        [StateRequired]
        readonly byte[] pagingRegisters;

        [StateRequired]
        byte bankMask;

        public KoreanMSX8kMapperCartridge(int romSize, int ramSize)
        {
            pagingRegisters = new byte[4];

            romData = new byte[romSize];
        }

        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));

            var romSizeRounded = 1;
            while (romSizeRounded < romData.Length) romSizeRounded <<= 1;

            bankMask = (byte)((romSizeRounded >> 13) - 1);
        }

        public void LoadRam(byte[] data)
        {
            //
        }

        public byte[] GetRomData()
        {
            return romData;
        }

        public byte[] GetRamData()
        {
            return null;
        }

        public bool IsRamSaveNeeded()
        {
            return false;
        }

        public ushort GetLowerBound()
        {
            return 0x0000;
        }

        public ushort GetUpperBound()
        {
            return 0xBFFF;
        }

        public void Step(int clockCyclesInStep)
        {
            /* Nothing to do */
        }

        public byte Read(ushort address)
        {
            switch (address & 0xE000)
            {
                case 0x0000: return romData[(0x00 << 13) | (address & 0x1FFF)];
                case 0x2000: return romData[(0x01 << 13) | (address & 0x1FFF)];
                case 0x4000: return romData[(pagingRegisters[2] << 13) | (address & 0x1FFF)];
                case 0x6000: return romData[(pagingRegisters[3] << 13) | (address & 0x1FFF)];
                case 0x8000: return romData[(pagingRegisters[0] << 13) | (address & 0x1FFF)];
                case 0xA000: return romData[(pagingRegisters[1] << 13) | (address & 0x1FFF)];
                default: throw new EmulationException(string.Format("Korean MSX 8k mapper: Cannot read from cartridge address 0x{0:X4}", address));
            }
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x0003)
            {
                pagingRegisters[address & 0x0003] = (byte)(value & bankMask);
            }
        }
    }
}
