using Essgee.Exceptions;
using Essgee.Utilities;
using System;
using System.Linq;
using static Essgee.Emulation.Utilities;

namespace Essgee.Emulation.Cartridges.Sega
{
    /* Mostly standard Sega mapper, but with bit-reversing functionality to flip sprites
	 * 
	 * Mapper writes: https://github.com/ocornut/meka/blob/0f1bf8f876a99cb23c440043d2aadfd683c5c812/meka/srcs/mappers.cpp#L571
	 * Bit-reversing logic: https://stackoverflow.com/a/3590938 */

    public class KoreanSpriteMapperCartridge : ICartridge
    {
        byte[] romData;

        [StateRequired]
        byte[] ramData;


        [StateRequired]//TODO 感觉不用保存 保留readonly
        byte[] pagingRegisters;
        //readonly byte[] pagingRegisters;

        [StateRequired]
        byte romBankMask;
        [StateRequired]
        bool hasCartRam;

        bool isRamEnabled { get { return IsBitSet(pagingRegisters[0], 3); } }
        bool isRomWriteEnable { get { return IsBitSet(pagingRegisters[0], 7); } }
        int ramBank { get { return ((pagingRegisters[0] >> 2) & 0x01); } }
        int romBank0 { get { return pagingRegisters[1]; } }
        int romBank1 { get { return pagingRegisters[2]; } }
        int romBank2 { get { return pagingRegisters[3]; } }

        [StateRequired]
        bool isBitReverseBank1, isBitReverseBank2;

        public KoreanSpriteMapperCartridge(int romSize, int ramSize)
        {
            pagingRegisters = new byte[0x04];
            pagingRegisters[0] = 0x00;  /* Mapper control */
            pagingRegisters[1] = 0x00;  /* Page 0 ROM bank */
            pagingRegisters[2] = 0x01;  /* Page 1 ROM bank */
            pagingRegisters[3] = 0x02;  /* Page 2 ROM bank */

            romData = new byte[romSize];
            ramData = new byte[ramSize];

            romBankMask = 0xFF;
            hasCartRam = false;

            isBitReverseBank1 = isBitReverseBank2 = false;
        }


        #region AxiState

        public void LoadAxiStatus(AxiEssgssStatusData data)
        {
            ramData = data.MemberData[nameof(ramData)];
            pagingRegisters = data.MemberData[nameof(pagingRegisters)];
            romBankMask = data.MemberData[nameof(romBankMask)].First();
            hasCartRam = BitConverter.ToBoolean(data.MemberData[nameof(hasCartRam)]);
            isBitReverseBank1 = BitConverter.ToBoolean(data.MemberData[nameof(isBitReverseBank1)]);
            isBitReverseBank2 = BitConverter.ToBoolean(data.MemberData[nameof(isBitReverseBank2)]);
        }

        public AxiEssgssStatusData SaveAxiStatus()
        {
            AxiEssgssStatusData data = new AxiEssgssStatusData();
            data.MemberData[nameof(ramData)] = ramData;
            data.MemberData[nameof(pagingRegisters)] = pagingRegisters;
            data.MemberData[nameof(romBankMask)] = BitConverter.GetBytes(romBankMask);
            data.MemberData[nameof(hasCartRam)] = BitConverter.GetBytes(hasCartRam);
            data.MemberData[nameof(isBitReverseBank1)] = BitConverter.GetBytes(isBitReverseBank1);
            data.MemberData[nameof(isBitReverseBank2)] = BitConverter.GetBytes(isBitReverseBank2);
            return data;
        }
        #endregion
        public void LoadRom(byte[] data)
        {
            Buffer.BlockCopy(data, 0, romData, 0, Math.Min(data.Length, romData.Length));

            var romSizeRounded = 1;
            while (romSizeRounded < romData.Length) romSizeRounded <<= 1;

            romBankMask = (byte)((romSizeRounded >> 14) - 1);

            /* Ensure startup banks are within ROM size */
            pagingRegisters[1] &= romBankMask;
            pagingRegisters[2] &= romBankMask;
            pagingRegisters[3] &= romBankMask;
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
            return hasCartRam;
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
                    if (address < 0x400)
                        /* First 1kb is constant to preserve interrupt vectors */
                        return romData[address];
                    else
                        return romData[((romBank0 << 14) | (address & 0x3FFF))];

                case 0x4000:
                    {
                        /* If requested, reverse bits before return */
                        var romAddress = ((romBank1 << 14) | (address & 0x3FFF));
                        if (!isBitReverseBank1)
                            return romData[romAddress];
                        else
                            return (byte)(((romData[romAddress] * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
                    }

                case 0x8000:
                    if (isRamEnabled)
                        return ramData[((ramBank << 14) | (address & 0x3FFF))];
                    else
                    {
                        /* If requested, reverse bits before return */
                        var romAddress = ((romBank2 << 14) | (address & 0x3FFF));
                        if (!isBitReverseBank2)
                            return romData[romAddress];
                        else
                            return (byte)(((romData[romAddress] * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
                    }

                default:
                    throw new EmulationException(string.Format("Korean sprite-flip mapper: Cannot read from cartridge address 0x{0:X4}", address));
            }
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0xFFFC && address <= 0xFFFF)
            {
                /* Check for bit-reverse flags */
                if ((address & 0x0003) == 0x02)
                    isBitReverseBank1 = ((value & 0x40) == 0x40);
                else if ((address & 0x0003) == 0x03)
                    isBitReverseBank2 = ((value & 0x40) == 0x40);

                /* Write to paging register */
                if ((address & 0x0003) != 0x00) value &= romBankMask;
                pagingRegisters[address & 0x0003] = value;

                /* Check if RAM ever gets enabled; if it is, indicate that we'll need to save the RAM */
                if (!hasCartRam && isRamEnabled && (address & 0x0003) == 0x0000)
                    hasCartRam = true;
            }
            if (isRamEnabled && (address & 0xC000) == 0x8000)
            {
                /* Cartridge RAM */
                ramData[((ramBank << 14) | (address & 0x3FFF))] = value;
            }
            else if (isRomWriteEnable)
            {
                /* ROM write enabled...? */
            }

            /* Otherwise ignore writes to ROM, as some games seem to be doing that? (ex. Gunstar Heroes GG to 0000) */
        }
    }
}
