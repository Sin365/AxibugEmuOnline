using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MyNes.Core
{
    public class NesCartDatabase
    {
        private static List<NesCartDatabaseGameInfo> _databaseRoms = new List<NesCartDatabaseGameInfo>();

        public static string DBVersion = "";

        public static string DBConformance = "";

        public static string DBAgent = "";

        public static string DBAuthor = "";

        public static string DBTimeStamp = "";

        public static bool Ready = false;

        public static List<NesCartDatabaseGameInfo> DatabaseRoms => _databaseRoms;

        public static void LoadDatabase(out bool success)
        {
            success = false;
            Ready = false;
            _databaseRoms.Clear();

            var stream = MyNesMain.Supporter.OpenDatabaseFile();
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.DtdProcessing = DtdProcessing.Ignore;
            xmlReaderSettings.IgnoreWhitespace = true;
            XmlReader xmlReader = XmlReader.Create(stream, xmlReaderSettings);
            NesCartDatabaseGameInfo nesCartDatabaseGameInfo = default(NesCartDatabaseGameInfo);
            nesCartDatabaseGameInfo.Cartridges = new List<NesCartDatabaseCartridgeInfo>();
            nesCartDatabaseGameInfo.Game_AltName = "";
            nesCartDatabaseGameInfo.Game_Catalog = "";
            nesCartDatabaseGameInfo.Game_Class = "";
            nesCartDatabaseGameInfo.Game_Developer = "";
            nesCartDatabaseGameInfo.Game_Name = "";
            nesCartDatabaseGameInfo.Game_Players = "";
            nesCartDatabaseGameInfo.Game_Publisher = "";
            nesCartDatabaseGameInfo.Game_Region = "";
            nesCartDatabaseGameInfo.Game_ReleaseDate = "";
            while (xmlReader.Read())
            {
                if ((xmlReader.Name == "xml") & xmlReader.IsStartElement())
                {
                    if (xmlReader.MoveToAttribute("version"))
                    {
                        DBVersion = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("conformance"))
                    {
                        DBConformance = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("agent"))
                    {
                        DBAgent = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("author"))
                    {
                        DBAuthor = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("timestamp"))
                    {
                        DBTimeStamp = xmlReader.Value;
                    }
                }
                else
                {
                    if (!((xmlReader.Name == "game") & xmlReader.IsStartElement()))
                    {
                        continue;
                    }
                    nesCartDatabaseGameInfo = default(NesCartDatabaseGameInfo);
                    if (xmlReader.MoveToAttribute("name"))
                    {
                        nesCartDatabaseGameInfo.Game_Name = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("altname"))
                    {
                        nesCartDatabaseGameInfo.Game_AltName = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("class"))
                    {
                        nesCartDatabaseGameInfo.Game_Class = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("catalog"))
                    {
                        nesCartDatabaseGameInfo.Game_Catalog = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("publisher"))
                    {
                        nesCartDatabaseGameInfo.Game_Publisher = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("developer"))
                    {
                        nesCartDatabaseGameInfo.Game_Developer = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("region"))
                    {
                        nesCartDatabaseGameInfo.Game_Region = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("players"))
                    {
                        nesCartDatabaseGameInfo.Game_Players = xmlReader.Value;
                    }
                    if (xmlReader.MoveToAttribute("date"))
                    {
                        nesCartDatabaseGameInfo.Game_ReleaseDate = xmlReader.Value;
                    }
                    NesCartDatabaseCartridgeInfo nesCartDatabaseCartridgeInfo = new NesCartDatabaseCartridgeInfo();
                    nesCartDatabaseCartridgeInfo.PAD_h = "";
                    nesCartDatabaseCartridgeInfo.PAD_v = "";
                    nesCartDatabaseCartridgeInfo.PRG_crc = "";
                    nesCartDatabaseCartridgeInfo.PRG_name = "";
                    nesCartDatabaseCartridgeInfo.PRG_sha1 = "";
                    nesCartDatabaseCartridgeInfo.PRG_size = "";
                    nesCartDatabaseCartridgeInfo.chip_type = new List<string>();
                    nesCartDatabaseCartridgeInfo.CHR_crc = "";
                    nesCartDatabaseCartridgeInfo.CHR_name = "";
                    nesCartDatabaseCartridgeInfo.CHR_sha1 = "";
                    nesCartDatabaseCartridgeInfo.CHR_size = "";
                    nesCartDatabaseCartridgeInfo.CIC_type = "";
                    nesCartDatabaseCartridgeInfo.Board_Mapper = "";
                    nesCartDatabaseCartridgeInfo.Board_Pcb = "";
                    nesCartDatabaseCartridgeInfo.Board_Type = "";
                    nesCartDatabaseCartridgeInfo.VRAM_sizes = new List<string>();
                    nesCartDatabaseCartridgeInfo.WRAMBanks = new List<SRAMBankInfo>();
                    while (xmlReader.Read())
                    {
                        if ((xmlReader.Name == "game") & !xmlReader.IsStartElement())
                        {
                            _databaseRoms.Add(nesCartDatabaseGameInfo);
                            break;
                        }
                        if ((xmlReader.Name == "cartridge") & xmlReader.IsStartElement())
                        {
                            if (nesCartDatabaseGameInfo.Cartridges == null)
                            {
                                nesCartDatabaseGameInfo.Cartridges = new List<NesCartDatabaseCartridgeInfo>();
                            }
                            nesCartDatabaseCartridgeInfo = new NesCartDatabaseCartridgeInfo();
                            nesCartDatabaseCartridgeInfo.PAD_h = "";
                            nesCartDatabaseCartridgeInfo.PAD_v = "";
                            nesCartDatabaseCartridgeInfo.PRG_crc = "";
                            nesCartDatabaseCartridgeInfo.PRG_name = "";
                            nesCartDatabaseCartridgeInfo.PRG_sha1 = "";
                            nesCartDatabaseCartridgeInfo.PRG_size = "";
                            nesCartDatabaseCartridgeInfo.chip_type = new List<string>();
                            nesCartDatabaseCartridgeInfo.CHR_crc = "";
                            nesCartDatabaseCartridgeInfo.CHR_name = "";
                            nesCartDatabaseCartridgeInfo.CHR_sha1 = "";
                            nesCartDatabaseCartridgeInfo.CHR_size = "";
                            nesCartDatabaseCartridgeInfo.CIC_type = "";
                            nesCartDatabaseCartridgeInfo.Board_Mapper = "";
                            nesCartDatabaseCartridgeInfo.Board_Pcb = "";
                            nesCartDatabaseCartridgeInfo.Board_Type = "";
                            nesCartDatabaseCartridgeInfo.VRAM_sizes = new List<string>();
                            nesCartDatabaseCartridgeInfo.WRAMBanks = new List<SRAMBankInfo>();
                            if (xmlReader.MoveToAttribute("system"))
                            {
                                nesCartDatabaseCartridgeInfo.System = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("crc"))
                            {
                                nesCartDatabaseCartridgeInfo.CRC = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("sha1"))
                            {
                                nesCartDatabaseCartridgeInfo.SHA1 = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("dump"))
                            {
                                nesCartDatabaseCartridgeInfo.Dump = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("dumper"))
                            {
                                nesCartDatabaseCartridgeInfo.Dumper = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("datedumped"))
                            {
                                nesCartDatabaseCartridgeInfo.DateDumped = xmlReader.Value;
                            }
                        }
                        else if ((xmlReader.Name == "cartridge") & !xmlReader.IsStartElement())
                        {
                            nesCartDatabaseGameInfo.Cartridges.Add(nesCartDatabaseCartridgeInfo);
                        }
                        else if ((xmlReader.Name == "board") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("type"))
                            {
                                nesCartDatabaseCartridgeInfo.Board_Type = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("pcb"))
                            {
                                nesCartDatabaseCartridgeInfo.Board_Pcb = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("mapper"))
                            {
                                nesCartDatabaseCartridgeInfo.Board_Mapper = xmlReader.Value;
                            }
                        }
                        else if ((xmlReader.Name == "prg") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("name"))
                            {
                                nesCartDatabaseCartridgeInfo.PRG_name = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("size"))
                            {
                                nesCartDatabaseCartridgeInfo.PRG_size = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("crc"))
                            {
                                nesCartDatabaseCartridgeInfo.PRG_crc = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("sha1"))
                            {
                                nesCartDatabaseCartridgeInfo.PRG_sha1 = xmlReader.Value;
                            }
                        }
                        else if ((xmlReader.Name == "chr") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("name"))
                            {
                                nesCartDatabaseCartridgeInfo.CHR_name = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("size"))
                            {
                                nesCartDatabaseCartridgeInfo.CHR_size = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("crc"))
                            {
                                nesCartDatabaseCartridgeInfo.CHR_crc = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("sha1"))
                            {
                                nesCartDatabaseCartridgeInfo.CHR_sha1 = xmlReader.Value;
                            }
                        }
                        else if ((xmlReader.Name == "vram") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("size"))
                            {
                                nesCartDatabaseCartridgeInfo.VRAM_sizes.Add(xmlReader.Value);
                            }
                        }
                        else if ((xmlReader.Name == "wram") & xmlReader.IsStartElement())
                        {
                            string sIZE = "";
                            bool bATTERY = false;
                            int result = 0;
                            if (xmlReader.MoveToAttribute("size"))
                            {
                                sIZE = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("battery"))
                            {
                                bATTERY = xmlReader.Value == "1";
                            }
                            if (xmlReader.MoveToAttribute("id"))
                            {
                                int.TryParse(xmlReader.Value, out result);
                            }
                            nesCartDatabaseCartridgeInfo.WRAMBanks.Add(new SRAMBankInfo(result, sIZE, bATTERY));
                        }
                        else if ((xmlReader.Name == "chip") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("type"))
                            {
                                if (nesCartDatabaseCartridgeInfo.chip_type == null)
                                {
                                    nesCartDatabaseCartridgeInfo.chip_type = new List<string>();
                                }
                                nesCartDatabaseCartridgeInfo.chip_type.Add(xmlReader.Value);
                            }
                        }
                        else if ((xmlReader.Name == "cic") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("type"))
                            {
                                nesCartDatabaseCartridgeInfo.CIC_type = xmlReader.Value;
                            }
                        }
                        else if ((xmlReader.Name == "pad") & xmlReader.IsStartElement())
                        {
                            if (xmlReader.MoveToAttribute("h"))
                            {
                                nesCartDatabaseCartridgeInfo.PAD_h = xmlReader.Value;
                            }
                            if (xmlReader.MoveToAttribute("v"))
                            {
                                nesCartDatabaseCartridgeInfo.PAD_v = xmlReader.Value;
                            }
                        }
                    }
                }
            }
            Ready = true;
            success = true;
            xmlReader.Close();
            stream.Close();
        }

        public static NesCartDatabaseGameInfo Find(string Cart_sha1, out bool found)
        {
            found = false;
            foreach (NesCartDatabaseGameInfo databaseRom in _databaseRoms)
            {
                foreach (NesCartDatabaseCartridgeInfo cartridge in databaseRom.Cartridges)
                {
                    if (cartridge.SHA1.ToLower() == Cart_sha1.ToLower())
                    {
                        found = true;
                        return databaseRom;
                    }
                }
            }
            return default(NesCartDatabaseGameInfo);
        }
    }
}
