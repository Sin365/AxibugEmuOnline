using System;
using System.IO;
using System.Linq;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class ROM
    {
        protected NESHEADER header;
        protected string path;
        protected string name;
        protected string fullpath;
        protected bool bPAL;
        protected bool bNSF;
        protected int NSF_PAGE_SIZE;
        protected byte[] lpPRG;
        protected byte[] lpCHR;
        protected byte[] lpTrainer;
        protected byte[] lpDiskBios;
        protected byte[] lpDisk;
        protected ulong crc;
        protected ulong crcall;
        protected int mapper;
        protected int diskno;
        public ROM(string fname)
        {
            Stream fp = null;
            byte[] temp = null;
            byte[] bios = null;
            long FileSize = 0;

            header = NESHEADER.GetDefault();
            path = string.Empty;
            name = string.Empty;

            bPAL = false;
            bNSF = false;
            NSF_PAGE_SIZE = 0;

            lpPRG = lpCHR = lpTrainer = lpDiskBios = lpDisk = null;

            crc = crcall = 0;
            mapper = 0;
            diskno = 0;

            try
            {
                fp = Supporter.OpenFile(fname);
                if (fp == null)
                {
                    throw new System.Exception($"Open Rom Failed:[{fname}]");
                }

                FileSize = fp.Length;
                if (FileSize < 17)
                {
                    throw new System.Exception($"File too small:[{fname}]");
                }

                temp = new byte[FileSize];
                fp.Read(temp, 0, temp.Length);

                fp.Dispose();

                header = NESHEADER.Read(temp);

                if (!header.CheckValid())
                    throw new Exception($"rom file is not valid:[{fname}]");

                ulong PRGoffset, CHRoffset;
                long PRGsize, CHRsize;

                var romType = header.GetRomType();
                if (romType == EnumRomType.NES)
                {
                    PRGsize = (long)header.PRG_PAGE_SIZE * 0x4000;
                    CHRsize = (long)header.CHR_PAGE_SIZE * 0x2000;
                    PRGoffset = (ulong)NESHEADER.SizeOf();
                    CHRoffset = PRGoffset + (ulong)PRGsize;

                    if (IsTRAINER())
                    {
                        PRGoffset += 512;
                        CHRoffset += 512;
                    }

                    if (PRGsize <= 0 || (PRGsize + CHRsize) > FileSize)
                    {
                        // NES僿僢僟偑堎忢偱偡
                        throw new Exception($"Invalid NesHeader:[{fname}]");
                    }

                    //PRG BANK
                    lpPRG = new byte[PRGsize];
                    Array.Copy(temp, (int)PRGoffset, lpPRG, 0, PRGsize);

                    //CHR BANK
                    if (CHRsize > 0)
                    {
                        lpCHR = new byte[CHRsize];
                        if (FileSize >= (long)CHRoffset + CHRsize)
                        {
                            Array.Copy(temp, (int)CHRoffset, lpCHR, 0, CHRsize);
                        }
                        else
                        {
                            //CHR Bank太少...
                            CHRsize -= ((long)CHRoffset + CHRsize - FileSize);
                            Array.Copy(temp, (int)CHRoffset, lpCHR, 0, CHRsize);
                        }
                    }
                    else
                    {
                        lpCHR = null;
                    }

                    if (IsTRAINER())
                    {
                        lpTrainer = new byte[512];
                        Array.Copy(temp, NESHEADER.SizeOf(), lpTrainer, 0, 512);
                    }
                    else
                    {
                        lpTrainer = null;
                    }
                }
                else if (romType == EnumRomType.FDS)
                {
                    diskno = header.PRG_PAGE_SIZE;

                    if (FileSize < (16 + 65500 * diskno))
                    {
                        throw new Exception($"Illegal Disk Size:[{fname}]");
                    }
                    if (diskno > 8)
                    {
                        throw new Exception($"Unsupport disk:[{fname}]");
                    }

                    header = NESHEADER.GetDefault();
                    header.ID[0] = (byte)'N';
                    header.ID[1] = (byte)'E';
                    header.ID[2] = (byte)'S';
                    header.ID[3] = 0x1A;
                    header.PRG_PAGE_SIZE = (byte)(diskno * 4);
                    header.CHR_PAGE_SIZE = 0;
                    header.control1 = 0x40;
                    header.control2 = 0x10;

                    PRGsize = NESHEADER.SizeOf() + 65500 * diskno;
                    //PRG BANK
                    lpPRG = new byte[PRGsize];
                    lpDisk = new byte[PRGsize];
                    lpCHR = null;

                    var headerBuffer = header.DataToBytes();
                    Array.Copy(headerBuffer, lpPRG, headerBuffer.Length);
                    Array.Copy(temp, NESHEADER.SizeOf(), lpPRG, NESHEADER.SizeOf(), 65500 * diskno);

                    lpPRG[0] = (byte)'F';
                    lpPRG[1] = (byte)'D';
                    lpPRG[2] = (byte)'S';
                    lpPRG[3] = 0x1A;
                    lpPRG[4] = (byte)diskno;

                    fp = Supporter.OpenFile("DISKSYS.ROM");
                    if (fp == null)
                    {
                        throw new Exception($"Not found DISKSYS.ROM for [{fname}]");
                    }

                    FileSize = fp.Length;
                    if (FileSize < 17)
                    {
                        throw new Exception($"Small File Of DISKSYS.ROM");
                    }

                    bios = new byte[FileSize];
                    fp.Read(bios, 0, (int)FileSize);
                    fp.Dispose();

                    lpDiskBios = new byte[8 * 1024];
                    if (bios[0] == 'N' && bios[1] == 'E' && bios[2] == 'S' && bios[3] == 0x1A)
                    {

                    }
                }
            }
            catch
            {

            }
        }

        public bool IsTRAINER()
        {
            return (header.control1 & (byte)EnumRomControlByte1.ROM_TRAINER) > 0;
        }
    }

    public enum EnumRomControlByte1 : byte
    {
        ROM_VMIRROR = 0x01,
        ROM_SAVERAM = 0x02,
        ROM_TRAINER = 0x04,
        ROM_4SCREEN = 0x08,
    }

    public enum EnumRomType
    {
        InValid,
        NES,
        /// <summary> Nintendo Disk System </summary>
        FDS,
        NSF
    }

    public struct NESHEADER
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
