using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class NesCartDatabaseCartridgeInfo
    {
    	public string System;

    	public string CRC;

    	public string SHA1;

    	public string Dump;

    	public string Dumper;

    	public string DateDumped;

    	public string Board_Type;

    	public string Board_Pcb;

    	public string Board_Mapper;

    	public List<string> VRAM_sizes;

    	public List<SRAMBankInfo> WRAMBanks;

    	public string PRG_name;

    	public string PRG_size;

    	public string PRG_crc;

    	public string PRG_sha1;

    	public string CHR_name;

    	public string CHR_size;

    	public string CHR_crc;

    	public string CHR_sha1;

    	public List<string> chip_type;

    	public string CIC_type;

    	public string PAD_h;

    	public string PAD_v;
    }
}
