using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Linq;

namespace Essgee.Emulation.Cartridges.Sega
{
    public class KoreanMapperCartridge : ICartridge
    {
        byte[] romData;

        [StateRequired]
        byte bankMask, pagingRegister;



        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            bankMask = data.MemberData[nameof(bankMask)].First();
            pagingRegister = data.MemberData[nameof(pagingRegister)].First();
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();

            data.MemberData[nameof(bankMask)] = BitConverter.GetBytes(bankMask);
            data.MemberData[nameof(pagingRegister)] = BitConverter.GetBytes(pagingRegister);

            return data;
        }
        #endregion

        public KoreanMapperCartridge(int romSize, int ramSize)
        {
            pagingRegister = 0x02;

            romData = new byte[romSize];
        }

        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));

            var romSizeRounded = 1;
            while (romSizeRounded < romData.Length) romSizeRounded <<= 1;

            bankMask = (byte)((romSizeRounded >> 14) - 1);
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
            switch (address & 0xC000)
            {
                case 0x0000:
                    return romData[address & 0x3FFF];

                case 0x4000:
                    return romData[(0x01 << 14) | (address & 0x3FFF)];

                case 0x8000:
                    return romData[((pagingRegister << 14) | (address & 0x3FFF))];

                default:
                    throw new EmulationException(string.Format("Korean mapper: Cannot read from cartridge address 0x{0:X4}", address));
            }
        }

        public void Write(ushort address, byte value)
        {
            switch (address)
            {
                case 0xA000:
                    pagingRegister = (byte)(value & bankMask);
                    break;
            }
        }
    }
}
