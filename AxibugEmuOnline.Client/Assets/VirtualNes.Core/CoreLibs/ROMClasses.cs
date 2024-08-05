using System;

namespace VirtualNes.Core
{
    public enum EnumRomControlByte1 : byte
    {
        ROM_VMIRROR = 0x01,
        ROM_SAVERAM = 0x02,
        ROM_TRAINER = 0x04,
        ROM_4SCREEN = 0x08,
    }

    public enum EnumRomControlByte2 : byte
    {
        ROM_VSUNISYSTEM = 0x01
    }

    public enum EnumRomType
    {
        InValid,
        NES,
        /// <summary> Nintendo Disk System </summary>
        FDS,
        NSF
    }

    public class NSFHEADER
    {
        byte[] ID;
        byte Version;
        byte TotalSong;
        byte StartSong;
        ushort LoadAddress;
        ushort InitAddress;
        ushort PlayAddress;
        byte[] SongName;
        byte[] ArtistName;
        byte[] CopyrightName;
        ushort SpeedNTSC;
        byte[] BankSwitch;
        ushort SpeedPAL;
        byte NTSC_PALbits;
        public byte ExtraChipSelect;
        byte[] Expansion;		// must be 0


        public static int SizeOf()
        {
            return 128;
        }

        public static NSFHEADER GetDefault()
        {
            var res = new NSFHEADER();
            res.ID = new byte[5];
            res.SongName = new byte[32];
            res.ArtistName = new byte[32];
            res.CopyrightName = new byte[32];
            res.BankSwitch = new byte[8];
            res.Expansion = new byte[4];
            return res;
        }
    }

    public class NESHEADER
    {
        public byte[] ID;
        public byte PRG_PAGE_SIZE;
        public byte CHR_PAGE_SIZE;
        public byte control1;
        public byte control2;
        public byte[] reserved;

        public bool CheckValid()
        {
            return GetRomType() != EnumRomType.InValid;
        }

        public static int SizeOf()
        {
            return 16;
        }

        public EnumRomType GetRomType()
        {
            if (ID[0] == 'N' && ID[1] == 'E' && ID[2] == 'S' && ID[3] == 0x1A)
                return EnumRomType.NES;
            if (ID[0] == 'F' && ID[1] == 'D' && ID[2] == 'S' && ID[3] == 0x1A)
                return EnumRomType.FDS;
            if (ID[0] == 'N' && ID[1] == 'E' && ID[2] == 'S' && ID[3] == 'M')
                return EnumRomType.NSF;

            return EnumRomType.InValid;
        }

        public static NESHEADER GetDefault()
        {
            var res = new NESHEADER();
            res.ID = new byte[4];
            res.reserved = new byte[8];
            return res;
        }

        public static NESHEADER Read(Span<byte> data)
        {
            var res = new NESHEADER();
            res.ID = data.Slice(0, 4).ToArray();
            res.PRG_PAGE_SIZE = data[4];
            res.CHR_PAGE_SIZE = data[5];
            res.control1 = data[6];
            res.control2 = data[7];
            res.reserved = data.Slice(8, 8).ToArray();

            return res;
        }

        public byte[] DataToBytes()
        {
            byte[] res = new byte[16];
            res[0] = ID[0];
            res[1] = ID[1];
            res[2] = ID[2];
            res[3] = ID[3];
            res[4] = PRG_PAGE_SIZE;
            res[5] = CHR_PAGE_SIZE;
            res[6] = control1;
            res[7] = control2;
            res[8] = reserved[0];
            res[9] = reserved[1];
            res[10] = reserved[2];
            res[11] = reserved[3];
            res[12] = reserved[4];
            res[13] = reserved[5];
            res[14] = reserved[6];
            res[15] = reserved[7];

            return res;
        }
    }
}
