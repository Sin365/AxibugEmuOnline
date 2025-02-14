using System;

namespace Essgee.Emulation.Cartridges.Coleco
{
    public class ColecoCartridge : ICartridge
    {
        // TODO: http://atariage.com/forums/topic/210168-colecovision-bank-switching/ ?

        byte[] romData;

        public ColecoCartridge(int romSize, int ramSize)
        {
            romData = new byte[romSize];
        }

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
            return (ushort)(romData.Length - 1);
        }

        public void Step(int clockCyclesInStep)
        {
            /* Nothing to do */
        }

        public byte Read(ushort address)
        {
            if (address <= 0x1FFF)
            {
                /* BIOS */
                return romData[address & 0x1FFF];
            }
            else
            {
                /* Cartridge */
                address -= 0x8000;
                if (address >= romData.Length) address -= (ushort)romData.Length;
                return romData[address];
            }
        }

        public void Write(ushort address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
