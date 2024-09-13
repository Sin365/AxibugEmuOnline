using System;
using System.IO;
using VirtualNes.Core.Debug;

namespace VirtualNes.Core
{
    public class ROM
    {
        protected NESHEADER header;
        protected NSFHEADER nsfheader;
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
        protected uint crc;
        protected uint crcall;
        protected uint crcvrom;
        protected int mapper;
        protected int diskno;
        protected uint fdsmakerID;
        protected uint fdsgameID;

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
                fp = Supporter.OpenRom(fname);
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
                long PRGsize = 0, CHRsize = 0;

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

                    fp = Supporter.OpenFile_DISKSYS();
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
                        Array.Copy(bios, 0x6010, lpDiskBios, 0, lpDiskBios.Length);
                    }
                    else
                    {
                        Array.Copy(bios, lpDiskBios, lpDiskBios.Length);
                    }
                    bios = null;
                }
                else if (romType == EnumRomType.NSF)
                {
                    bNSF = true;
                    header = NESHEADER.GetDefault();

                    nsfheader = NSFHEADER.GetDefault();

                    PRGsize = FileSize - NSFHEADER.SizeOf();
                    Debuger.Log($"PRGSIZE:{PRGsize}");
                    PRGsize = (PRGsize + 0x0FFF) & ~0x0FFF;
                    Debuger.Log($"PRGSIZE:{PRGsize}");

                    lpPRG = new byte[PRGsize];
                    Array.Copy(temp, NSFHEADER.SizeOf(), lpPRG, 0, FileSize - NSFHEADER.SizeOf());

                    NSF_PAGE_SIZE = (int)(PRGsize >> 12);
                    Debuger.Log($"PAGESIZE:{NSF_PAGE_SIZE}");
                }
                else
                {
                    throw new Exception($"Unsupport format:[{fname}]");
                }

                Supporter.GetFilePathInfo(fname, out fullpath, out path);
                name = Path.GetFileNameWithoutExtension(fullpath);
                if (!bNSF)
                {
                    mapper = (header.control1 >> 4) | (header.control2 & 0xF0);
                    crc = crcall = crcvrom = 0;

                    if (mapper != 20)
                    {
                        Span<byte> sTemp = temp;
                        if (IsTRAINER())
                        {
                            crcall = CRC.CrcRev((int)(512 + PRGsize + CHRsize), sTemp.Slice(NESHEADER.SizeOf()));
                            crc = CRC.CrcRev((int)(512 + PRGsize), sTemp);
                            if (CHRsize > 0)
                                crcvrom = CRC.CrcRev((int)CHRsize, sTemp.Slice((int)(PRGsize + 512 + NESHEADER.SizeOf())));
                        }
                        else
                        {
                            crcall = CRC.CrcRev((int)(PRGsize + CHRsize), sTemp.Slice(NESHEADER.SizeOf()));
                            crc = CRC.CrcRev((int)(PRGsize), sTemp.Slice(NESHEADER.SizeOf()));
                            if (CHRsize > 0)
                                crcvrom = CRC.CrcRev((int)CHRsize, sTemp.Slice((int)(PRGsize + NESHEADER.SizeOf())));
                        }

                        FileNameCheck(fname);

                        if (Supporter.TryGetMapperNo(this, out int mapperNo))
                        {
                            Debuger.Log($"ROMDB Set Mapper #{mapper:000} to #{mapperNo:000}");
                            mapper = mapperNo;
                        }

                        RomPatch.DoPatch(ref crc, ref lpPRG, ref lpCHR, ref mapper, ref header);

                        fdsmakerID = fdsgameID = 0;
                    }
                    else //mapper==20
                    {
                        crc = crcall = crcvrom = 0;

                        fdsmakerID = lpPRG[0x1F];
                        fdsgameID = (uint)((lpPRG[0x20] << 24) | (lpPRG[0x21] << 16) | (lpPRG[0x22] << 8) | (lpPRG[0x23] << 0));
                    }
                }
                else //NSF
                {
                    mapper = 0x0100;    // Private mapper
                    crc = crcall = crcvrom = 0;
                    fdsmakerID = fdsgameID = 0;
                }

                temp = null;
            }
            catch (Exception ex)
            {
                fp?.Dispose();
                temp = null;
                bios = null;
                lpPRG = null;
                lpCHR = null;
                lpTrainer = null;
                lpDiskBios = null;
                lpDisk = null;

                throw ex;
            }
        }

        public void Dispose()
        {
            lpPRG = null;
            lpCHR = null;
            lpTrainer = null;
            lpDiskBios = null;
            lpDisk = null;
        }

        public bool IsTRAINER()
        {
            return (header.control1 & (byte)EnumRomControlByte1.ROM_TRAINER) > 0;
        }

        public bool IsNSF()
        {
            return bNSF;
        }
        public bool IsPAL()
        {
            return bPAL;
        }

        public bool IsSAVERAM()
        {
            return (header.control1 & (byte)EnumRomControlByte1.ROM_SAVERAM) > 0;
        }

        protected void FileNameCheck(string fname)
        {
            if (fname.Contains("(E)"))
            {
                bPAL = true;
                return;
            }
        }

        internal string GetRomName()
        {
            return name;
        }

        internal int GetMapperNo()
        {
            return mapper;
        }

        internal byte[] GetPROM()
        {
            return lpPRG;
        }

        internal byte[] GetVROM()
        {
            return lpCHR;
        }

        internal byte[] GetDISK()
        {
            return lpDisk;
        }

        internal int GetDiskNo()
        {
            return diskno;
        }

        internal void SetDiskNo(int v)
        {
            diskno = v;
        }

        internal uint GetGameID()
        {
            return fdsgameID;
        }

        internal void SetGameID(uint id)
        {
            fdsgameID = id;
        }

        internal uint GetMakerID()
        {
            return fdsmakerID;
        }

        internal void SetMakerID(uint id)
        {
            fdsmakerID = id;
        }

        internal bool IsVSUNISYSTEM()
        {
            return (header.control2 & (byte)EnumRomControlByte2.ROM_VSUNISYSTEM) != 0;
        }

        public uint GetPROM_CRC()
        {
            return crc;
        }

        public void SetPROM_CRC(uint v)
        {
            crc = v;
        }

        internal byte GetPROM_SIZE()
        {
            return header.PRG_PAGE_SIZE;
        }

        internal byte GetVROM_SIZE()
        {
            return header.CHR_PAGE_SIZE;
        }

        internal bool Is4SCREEN()
        {
            return (header.control1 & (byte)EnumRomControlByte1.ROM_4SCREEN) != 0;
        }

        internal bool IsVMIRROR()
        {
            return (header.control1 & (byte)EnumRomControlByte1.ROM_VMIRROR) != 0;
        }

        internal byte[] GetTRAINER()
        {
            return lpTrainer;
        }

        internal NSFHEADER GetNsfHeader()
        {
            return nsfheader;
        }

        internal string GetRomPath()
        {
            return path;
        }

        internal uint GetVROM_CRC()
        {
            return crcvrom;
        }
    }


}
