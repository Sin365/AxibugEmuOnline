using Essgee.Emulation;
using Essgee.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Essgee.Metadata
{
    /// <summary>
    /// 单独定义meta加载接口
    /// </summary>
    public interface IGameMetaReources
    {
        public bool GetCartMetadataDatabase(out string loadedData);
        public bool GetDatBytes(string DatName, out byte[] loadedData);
    }


    public interface IEssgeeIOSupport
    {
        bool File_Exists(string path);
        byte[] File_ReadAllBytes(string path);
        void File_WriteAllBytes(string savePath, byte[] data);
        void File_WriteAllBytesFromStre(string path, System.IO.MemoryStream ms);
    }

    public class GameMetadataHandler
    {
        public static GameMetadataHandler instance;
        //static string datDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "No-Intro");
        //static string metadataDatabaseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "MetadataDatabase.json");

        public IGameMetaReources gameMetaReources;
        public IEssgeeIOSupport uegIO;

        //readonly Dictionary<string, DatFile> datFiles;
        readonly List<CartridgeJSON> cartMetadataDatabase;

        //public int NumKnownSystems { get { return datFiles.Count; } }
        //public int NumKnownGames { get { return datFiles.Sum(x => x.Value.Game.Count()); } }

        public GameMetadataHandler(IGameMetaReources metaresources)
        {
            instance = this;
            gameMetaReources = metaresources;

            //if (!gameMetaReources.GetCartMetadataDatabase(out string loadedData))
            //    throw new HandlerException("CartMetadataDatabase file not found");

            //cartMetadataDatabase = JsonConvert.DeserializeObject<List<CartridgeJSON>>(loadedData);
            cartMetadataDatabase = new List<CartridgeJSON>()
            {
                new CartridgeJSON(){
    Name = "The Castle (SG-1000)",
    Notes = "8k volatile RAM",
    Crc32 = 0x092F29D6,
    RomSize = 32768,
    RamSize = 8192
  },
  new CartridgeJSON(){
    Name = "Othello (SG-1000)",
    Notes = "2k volatile RAM",
    Crc32 = 0xAF4F14BC,
    RomSize = 32768,
    RamSize = 2048
  },
  new CartridgeJSON(){
    Name = "Sega Basic Level II (SC-3000)",
    Notes = "2k volatile RAM (for Level IIb)",
    Crc32 = 0xF691F9C7,
    RomSize = 32768,
    RamSize = 2048
  },
  new CartridgeJSON(){
    Name = "Sega Basic Level III (SC-3000)",
    Notes = "32k volatile RAM (for Level IIIb)",
    Crc32 = 0x5D9F11CA,
    RomSize = 32768,
    RamSize = 32768
  },
  new CartridgeJSON(){
    Name = "Back to the Future 2 (SMS)",
    Notes = "PAL only",
    Crc32 = 0xE5FF50D8,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Back to the Future 3 (SMS)",
    Notes = "PAL only",
    Crc32 = 0x2D48C1D3,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "California Games 2 (SMS, Europe)",
    Notes = "PAL only",
    Crc32 = 0xC0E25D62,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Chase HQ (SMS)",
    Notes = "PAL only",
    Crc32 = 0x85CFC9C9,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Cosmic Spacehead (SMS)",
    Notes = "Codemasters mapper & PAL only",
    Crc32 = 0x29822980,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Dinobasher (SMS)",
    Notes = "Codemasters mapper",
    Crc32 = 0xEA5C3A6F,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Excellent Dizzy Collection (SMS)",
    Notes = "Codemasters mapper & PAL only",
    Crc32 = 0x8813514B,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Fantastic Dizzy (SMS)",
    Notes = "Codemasters mapper & PAL only",
    Crc32 = 0xB9664AE1,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Home Alone (SMS)",
    Notes = "PAL only",
    Crc32 = 0xC9DBF936,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Micro Machines (SMS)",
    Notes = "Codemasters mapper & PAL only",
    Crc32 = 0xA577CE46,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "NewZealand Story (SMS)",
    Notes = "PAL only",
    Crc32 = 0xC660FF34,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Power Strike 2 (SMS)",
    Notes = "PAL only",
    Crc32 = 0xA109A6FE,
    RomSize = 524288,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Predator 2 (SMS, Europe)",
    Notes = "PAL only",
    Crc32 = 0x0047B615,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Sonic the Hedgehog 2 (SMS)",
    Notes = "PAL only",
    Crc32 = 0x5B3B922C,
    RomSize = 524288,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Sonic the Hedgehog 2 (SMS, Revision 1)",
    Notes = "PAL only",
    Crc32 = 0xD6F2BFCA,
    RomSize = 524288,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Space Harrier (SMS, Europe)",
    Notes = "PAL only",
    Crc32 = 0xCA1D3752,
    RomSize = 262144,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "CJ Elephant Fugitive (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0x72981057,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Cosmic Spacehead (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0x6CAA625B,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Dropzone (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0x152F0DCC,
    RomSize = 131072,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Ernie Els Golf (GG)",
    Notes = "Codemasters mapper & 8k volatile RAM",
    Crc32 = 0x5E53C7F7,
    RomSize = 262144,
    RamSize = 8192,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Micro Machines (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0xF7C524F6,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Micro Machines 2: Turbo Tournament (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0xDBE8895C,
    RomSize = 524288,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Pete Sampras Tennis (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0xC1756BEE,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Pop Breaker (GG, Japan)",
    Notes = "Domestic/Japan only",
    Crc32 = 0x71DEBA5A,
    RomSize = 131072,
    PreferredRegion = Region.Domestic
  },
  new CartridgeJSON(){
    Name = "S.S. Lucifer: Man Overboard (GG)",
    Notes = "Codemasters mapper",
    Crc32 = 0xD9A7F170,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.CodemastersCartridge),
  },
  new CartridgeJSON(){
    Name = "Sonic Chaos (SMS, Jun 30 1993 Prototype)",
    Notes = "Disallow memory control",
    Crc32 = 0xD3AD67FA,
    RomSize = 524288,
    AllowMemoryControl = false
  },
  new CartridgeJSON(){
    Name = "94 Super World Cup Soccer (SMS)",
    Notes = "Korean mapper",
    Crc32 = 0x060D6A7C,
    RomSize = 262144,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "Jang Pung II (SMS)",
    Notes = "Korean mapper",
    Crc32 = 0x929222C4,
    RomSize = 524288,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "Jang Pung 3 (SMS)",
    Notes = "Korean mapper",
    Crc32 = 0x18FB98A3,
    RomSize = 1048576,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "Janggun-ui Adeul (SMS)",
    Notes = "Korean sprite-flip mapper",
    Crc32 = 0x192949D5,
    RomSize = 524288,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanSpriteMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "Sangokushi 3 (SMS)",
    Notes = "Korean mapper",
    Crc32 = 0x97D03541,
    RomSize = 1048576,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "4 Pak All Action (SMS)",
    Notes = "4 Pak mapper",
    Crc32 = 0xA67F2A5C,
    RomSize = 1048576,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.Multicart4PakAllActionCartridge),
  },
  new CartridgeJSON(){
    Name = "Cyborg Z (SMS)",
    Notes = "Korean MSX 8k mapper",
    Crc32 = 0x77EFE84A,
    RomSize = 131072,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMSX8kMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "Street Master (SMS)",
    Notes = "Korean MSX 8k mapper",
    Crc32 = 0x83F0EEDE,
    RomSize = 131072,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMSX8kMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "Wonsiin (SMS)",
    Notes = "Korean MSX 8k mapper",
    Crc32 = 0xA05258F5,
    RomSize = 131072,
    Mapper = typeof(Essgee.Emulation.Cartridges.Sega.KoreanMSX8kMapperCartridge),
  },
  new CartridgeJSON(){
    Name = "SMS Bad Apple 1.00 (SMS)",
    Notes = "PAL only",
    Crc32 = 0x38434560,
    RomSize = 4194304,
    PreferredTVStandard = TVStandard.PAL
  },
  new CartridgeJSON(){
    Name = "Be No Sqr 1.01 (SMS)",
    Notes = "PAL only",
    Crc32 = 0xEE701BE6,
    RomSize = 524288,
    PreferredTVStandard = TVStandard.PAL
  }
            };

            //改为接口直接读取
            //XmlRootAttribute root;
            //XmlSerializer serializer;

            ///* Read No-Intro .dat files */

            //datFiles = new Dictionary<string, DatFile>();
            //foreach (var file in Directory.EnumerateFiles(EmuStandInfo.datDirectoryPath, "*.dat"))
            //{
            //    root = new XmlRootAttribute("datafile") { IsNullable = true };
            //    serializer = new XmlSerializer(typeof(DatFile), root);
            //    using (FileStream stream = new FileStream(Path.Combine(EmuStandInfo.datDirectoryPath, file), FileMode.Open))
            //    {
            //        datFiles.Add(Path.GetFileName(file), (DatFile)serializer.Deserialize(stream));
            //    }
            //}

            ///* Read cartridge metadata database */
            //cartMetadataDatabase = EmuStandInfo.metadataDatabaseFilePath.DeserializeFromFile<List<CartridgeJSON>>();

            ////EssgeeLogger.EnqueueMessageSuccess($"Metadata initialized; {NumKnownGames} game(s) known across {NumKnownSystems} system(s).");
        }

        ~GameMetadataHandler()
        {
            if(instance == this)
                instance = null;
        }

        public GameMetadata GetGameMetadata(string datFilename, string romFilename, uint romCrc32, int romSize)
        {
            /* Sanity checks */
            //if (!datFiles.ContainsKey(datFilename)) throw new HandlerException("Requested .dat file not found");

            //接口直接读取
            if (!gameMetaReources.GetDatBytes(datFilename, out byte[] loadedData))
                throw new HandlerException("Requested .dat file not found");

            DatFile datFile;

            XmlRootAttribute root;
            XmlSerializer serializer;
            root = new XmlRootAttribute("datafile") { IsNullable = true };
            serializer = new XmlSerializer(typeof(DatFile), root);
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(loadedData))
            {
                datFile = (DatFile)serializer.Deserialize(stream);
            }

            /* Get information from No-Intro .dat */
            //var datFile = datFiles[datFilename];
            var crcString = string.Format("{0:X8}", romCrc32);
            var sizeString = string.Format("{0:D}", romSize);
            var gameInfo = datFile.Game.FirstOrDefault(x => x.Rom.Any(y => y.Crc == crcString && y.Size == sizeString));

            /* Get information from cartridge metadata database */
            var cartridgeInfo = cartMetadataDatabase.FirstOrDefault(x => x.Crc32 == romCrc32 && x.RomSize == romSize);

            /* Create game metadata */
            var gameMetadata = new GameMetadata()
            {
                FileName = System.IO.Path.GetFileName(romFilename),
                KnownName = gameInfo?.Name,
                RomCrc32 = romCrc32,
                RomSize = romSize
            };

            if (cartridgeInfo != null)
            {
                if (gameMetadata.KnownName == null)
                    gameMetadata.KnownName = cartridgeInfo.Name;

                gameMetadata.Notes = cartridgeInfo.Notes;
                gameMetadata.RamSize = cartridgeInfo.RamSize;
                gameMetadata.MapperType = cartridgeInfo.Mapper;
                gameMetadata.HasNonVolatileRam = cartridgeInfo.HasNonVolatileRam;
                gameMetadata.PreferredTVStandard = cartridgeInfo.PreferredTVStandard;
                gameMetadata.PreferredRegion = cartridgeInfo.PreferredRegion;
                gameMetadata.AllowMemoryControl = cartridgeInfo.AllowMemoryControl;
            }

            if (gameMetadata.KnownName == null)
                gameMetadata.KnownName = "unrecognized game";

            return gameMetadata;
        }

        public class CartridgeJSON
        {
            //[JsonProperty(Required = Required.Always)]
            public string Name { get; set; } = string.Empty;

            //[JsonProperty(Required = Required.Always)]
            public string Notes { get; set; } = string.Empty;

            //[JsonProperty(Required = Required.Always), JsonConverter(typeof(HexadecimalJsonConverter))]
            public uint Crc32 { get; set; } = 0xFFFFFFFF;

            //[JsonProperty(Required = Required.Always)]
            public int RomSize { get; set; } = 0;

            //[JsonProperty(Required = Required.Default), DefaultValue(0)]
            public int RamSize { get; set; } = 0;

            //[JsonProperty(Required = Required.Default), JsonConverter(typeof(TypeNameJsonConverter), "Essgee.Emulation.Cartridges"), DefaultValue(null)]
            public Type Mapper { get; set; } = null;

            //[JsonProperty(Required = Required.Default), DefaultValue(false)]
            public bool HasNonVolatileRam { get; set; } = false;

            //[JsonProperty(Required = Required.Default), JsonConverter(typeof(StringEnumConverter)), DefaultValue(TVStandard.Auto)]
            public TVStandard PreferredTVStandard { get; set; } = TVStandard.Auto;

            //[JsonProperty(Required = Required.Default), JsonConverter(typeof(StringEnumConverter)), DefaultValue(Region.Auto)]
            public Region PreferredRegion { get; set; } = Region.Auto;

            //[JsonProperty(Required = Required.Default), DefaultValue(true)]
            public bool AllowMemoryControl { get; set; } = true;
        }

        public class DatHeader
        {
            [XmlElement("name")]
            public string Name { get; set; }
            [XmlElement("description")]
            public string Description { get; set; }
            [XmlElement("category")]
            public string Category { get; set; }
            [XmlElement("version")]
            public string Version { get; set; }
            [XmlElement("date")]
            public string Date { get; set; }
            [XmlElement("author")]
            public string Author { get; set; }
            [XmlElement("email")]
            public string Email { get; set; }
            [XmlElement("homepage")]
            public string Homepage { get; set; }
            [XmlElement("url")]
            public string Url { get; set; }
            [XmlElement("comment")]
            public string Comment { get; set; }
        }

        public class DatRelease
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("region")]
            public string Region { get; set; }
            [XmlAttribute("language")]
            public string Language { get; set; }
            [XmlAttribute("date")]
            public string Date { get; set; }
            [XmlAttribute("default")]
            public string Default { get; set; }
        }

        public class DatBiosSet
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("description")]
            public string Description { get; set; }
            [XmlAttribute("default")]
            public string Default { get; set; }
        }

        public class DatRom
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("size")]
            public string Size { get; set; }
            [XmlAttribute("crc")]
            public string Crc { get; set; }
            [XmlAttribute("sha1")]
            public string Sha1 { get; set; }
            [XmlAttribute("md5")]
            public string Md5 { get; set; }
            [XmlAttribute("merge")]
            public string Merge { get; set; }
            [XmlAttribute("status")]
            public string Status { get; set; }
            [XmlAttribute("date")]
            public string Date { get; set; }
        }

        public class DatDisk
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("sha1")]
            public string Sha1 { get; set; }
            [XmlAttribute("md5")]
            public string Md5 { get; set; }
            [XmlAttribute("merge")]
            public string Merge { get; set; }
            [XmlAttribute("status")]
            public string Status { get; set; }
        }

        public class DatSample
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class DatArchive
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class DatGame
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("sourcefile")]
            public string SourceFile { get; set; }
            [XmlAttribute("isbios")]
            public string IsBios { get; set; }
            [XmlAttribute("cloneof")]
            public string CloneOf { get; set; }
            [XmlAttribute("romof")]
            public string RomOf { get; set; }
            [XmlAttribute("sampleof")]
            public string SampleOf { get; set; }
            [XmlAttribute("board")]
            public string Board { get; set; }
            [XmlAttribute("rebuildto")]
            public string RebuildTo { get; set; }

            [XmlElement("year")]
            public string Year { get; set; }
            [XmlElement("manufacturer")]
            public string Manufacturer { get; set; }

            [XmlElement("release")]
            public DatRelease[] Release { get; set; }

            [XmlElement("biosset")]
            public DatBiosSet[] BiosSet { get; set; }

            [XmlElement("rom")]
            public DatRom[] Rom { get; set; }

            [XmlElement("disk")]
            public DatDisk[] Disk { get; set; }

            [XmlElement("sample")]
            public DatSample[] Sample { get; set; }

            [XmlElement("archive")]
            public DatArchive[] Archive { get; set; }
        }

        [Serializable()]
        public class DatFile
        {
            [XmlElement("header")]
            public DatHeader Header { get; set; }

            [XmlElement("game")]
            public DatGame[] Game { get; set; }
        }
    }
}
