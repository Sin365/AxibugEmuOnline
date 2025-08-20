using Essgee.Exceptions;
using Essgee.Utilities;
using System;

namespace Essgee.Emulation.Cartridges.Sega
{
    /* http://www.smspower.org/forums/post69724#69724 */

    public class Multicart4PakAllActionCartridge : ICartridge
    {
        byte[] romData;

        [StateRequired]//TODO 感觉不用保存 保留readonly
        int romMask;
        //readonly int romMask;

        [StateRequired]
        int romBank0, romBank1, romBank2;

        public Multicart4PakAllActionCartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];

            romMask = 1;
            while (romMask < romSize) romMask <<= 1;
            romMask -= 1;

            romBank0 = romBank1 = romBank2 = 0;
        }

        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            romMask = BitConverter.ToInt32(data.MemberData[nameof(romMask)]);
            romBank0 = BitConverter.ToInt32(data.MemberData[nameof(romBank0)]);
            romBank1 = BitConverter.ToInt32(data.MemberData[nameof(romBank1)]);
            romBank2 = BitConverter.ToInt32(data.MemberData[nameof(romBank2)]);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            data.MemberData[nameof(romMask)] = BitConverter.GetBytes(romMask);
            data.MemberData[nameof(romBank0)] = BitConverter.GetBytes(romBank0);
            data.MemberData[nameof(romBank1)] = BitConverter.GetBytes(romBank1);
            data.MemberData[nameof(romBank2)] = BitConverter.GetBytes(romBank2);
            return data;
        }
        #endregion
        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));
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
                    return romData[((romBank0 << 14) | (address & 0x3FFF))];

                case 0x4000:
                    return romData[((romBank1 << 14) | (address & 0x3FFF))];

                case 0x8000:
                    return romData[((((romBank0 & 0x30) + romBank2) << 14) | (address & 0x3FFF))];

                default:
                    throw new EmulationException(string.Format("4 Pak mapper: Cannot read from cartridge address 0x{0:X4}", address));
            }
        }

        public void Write(ushort address, byte value)
        {
            // TODO: really just these addresses? no mirroring?
            if (address == 0x3FFE)
                romBank0 = value;
            else if (address == 0x7FFF)
                romBank1 = value;
            else if (address == 0xBFFF)
                romBank2 = value;
        }
    }
}
