using Essgee.Emulation;
using System;

namespace Essgee.Metadata
{
    public class GameMetadata
    {
        public string FileName { get; set; } = string.Empty;
        public string KnownName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public uint RomCrc32 { get; set; } = 0xFFFFFFFF;
        public int RomSize { get; set; } = 0;
        public int RamSize { get; set; } = 0;
        public Type MapperType { get; set; } = null;
        public bool HasNonVolatileRam { get; set; } = false;
        public TVStandard PreferredTVStandard { get; set; } = TVStandard.Auto;
        public Region PreferredRegion { get; set; } = Region.Auto;
        public bool AllowMemoryControl { get; set; } = true;
    }
}
