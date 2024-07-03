using System;
using System.Collections.Generic;
using System.IO;

namespace MyNes.Core
{
    internal abstract class Board
    {
    	protected byte[][] PRG_RAM;

    	protected bool[] PRG_RAM_ENABLED;

    	protected bool[] PRG_RAM_WRITABLE;

    	protected bool[] PRG_RAM_BATTERY;

    	protected byte[][] PRG_ROM;

    	protected int PRG_RAM_08KB_DEFAULT_BLK_Count;

    	internal int PRG_ROM_04KB_Count;

    	protected int PRG_ROM_08KB_Count;

    	protected int PRG_ROM_16KB_Count;

    	protected int PRG_ROM_32KB_Count;

    	protected int PRG_ROM_04KB_Mask;

    	protected int PRG_ROM_08KB_Mask;

    	protected int PRG_ROM_16KB_Mask;

    	protected int PRG_ROM_32KB_Mask;

    	internal int PRG_RAM_04KB_Count;

    	protected int PRG_RAM_08KB_Count;

    	protected int PRG_RAM_16KB_Count;

    	protected int PRG_RAM_32KB_Count;

    	protected int PRG_RAM_04KB_Mask;

    	protected int PRG_RAM_08KB_Mask;

    	protected int PRG_RAM_16KB_Mask;

    	protected int PRG_RAM_32KB_Mask;

    	protected bool[] PRG_AREA_BLK_RAM;

    	protected int[] PRG_AREA_BLK_INDEX;

    	protected int PRG_TMP_INDX;

    	protected int PRG_TMP_AREA;

    	protected byte[][] CHR_RAM;

    	protected bool[] CHR_RAM_ENABLED;

    	protected bool[] CHR_RAM_WRITABLE;

    	protected bool[] CHR_RAM_BATTERY;

    	protected byte[][] CHR_ROM;

    	protected bool[] CHR_AREA_BLK_RAM;

    	protected int[] CHR_AREA_BLK_INDEX;

    	protected int CHR_TMP_INDX;

    	protected int CHR_TMP_AREA;

    	protected int CHR_ROM_01KB_DEFAULT_BLK_Count;

    	internal int CHR_ROM_01KB_Count;

    	protected int CHR_ROM_02KB_Count;

    	protected int CHR_ROM_04KB_Count;

    	protected int CHR_ROM_08KB_Count;

    	internal int CHR_ROM_01KB_Mask;

    	protected int CHR_ROM_02KB_Mask;

    	protected int CHR_ROM_04KB_Mask;

    	protected int CHR_ROM_08KB_Mask;

    	internal int CHR_RAM_01KB_Count;

    	protected int CHR_RAM_02KB_Count;

    	protected int CHR_RAM_04KB_Count;

    	protected int CHR_RAM_08KB_Count;

    	internal int CHR_RAM_01KB_Mask;

    	protected int CHR_RAM_02KB_Mask;

    	protected int CHR_RAM_04KB_Mask;

    	protected int CHR_RAM_08KB_Mask;

    	protected byte[][] NMT_RAM;

    	internal int[] NMT_AREA_BLK_INDEX;

    	protected int NMT_TMP_INDX;

    	protected int NMT_TMP_AREA;

    	internal Mirroring NMT_DEFAULT_MIRROR;

    	internal string SHA1 = "";

    	internal string CRC = "";

    	internal bool IsGameFoundOnDB;

    	internal NesCartDatabaseGameInfo GameInfo;

    	internal NesCartDatabaseCartridgeInfo GameCartInfo;

    	internal bool SRAMSaveRequired;

    	protected bool enabled_ppuA12ToggleTimer;

    	protected bool ppuA12TogglesOnRaisingEdge;

    	protected int old_vram_address;

    	protected int new_vram_address;

    	protected int ppu_cycles_timer;

    	internal bool enable_external_sound;

    	internal bool IsGameGenieActive;

    	internal GameGenieCode[] GameGenieCodes;

    	internal string BoardType { get; private set; }

    	internal string BoardPCB { get; private set; }

    	internal List<string> Chips { get; private set; }

    	internal string Name { get; set; }

    	internal int MapperNumber { get; set; }

    	internal bool HasIssues { get; set; }

    	internal virtual string Issues { get; set; }

    	public Board()
    	{
    		MapperNumber = -1;
    		PRG_RAM_08KB_DEFAULT_BLK_Count = 1;
    		CHR_ROM_01KB_DEFAULT_BLK_Count = 8;
    		LoadAttrs();
    	}

    	internal virtual void Initialize(IRom rom)
    	{
    		SHA1 = rom.SHA1;
    		SRAMSaveRequired = false;
    		IsGameGenieActive = false;
    		BoardType = "N/A";
    		BoardPCB = "N/A";
    		Chips = new List<string>();
    		if (NesCartDatabase.Ready)
    		{
    			Tracer.WriteLine("Looking for rom in the database ..");
    			GameInfo = NesCartDatabase.Find(SHA1, out IsGameFoundOnDB);
    			if (GameInfo.Cartridges != null)
    			{
    				foreach (NesCartDatabaseCartridgeInfo cartridge in GameInfo.Cartridges)
    				{
    					if (cartridge.SHA1.ToLower() == SHA1.ToLower())
    					{
    						GameCartInfo = cartridge;
    						break;
    					}
    				}
    			}
    			if (IsGameFoundOnDB)
    			{
    				Tracer.WriteInformation("Game found in Database !!");
    				Tracer.WriteLine("> Game name: " + GameInfo.Game_Name);
    				Tracer.WriteLine("> Game alt name: " + GameInfo.Game_AltName);
    				BoardType = GameCartInfo.Board_Type;
    				Tracer.WriteLine("> Board Type: " + BoardType);
    				BoardPCB = GameCartInfo.Board_Pcb;
    				Tracer.WriteLine("> Board Pcb: " + BoardPCB);
    				if (GameCartInfo.chip_type != null)
    				{
    					for (int i = 0; i < GameCartInfo.chip_type.Count; i++)
    					{
    						Console.WriteLine($"> CHIP {(i + 1).ToString()}: {GameCartInfo.chip_type[i]}");
    						Chips.Add(GameCartInfo.chip_type[i]);
    					}
    				}
    			}
    			else
    			{
    				Tracer.WriteWarning("Game is not found in database .");
    			}
    		}
    		Tracer.WriteLine("Initializing the board (Mapper # " + MapperNumber + ") ....");
    		Tracer.WriteLine("Loading PRG ROM ...");
    		PRG_AREA_BLK_RAM = new bool[16];
    		PRG_AREA_BLK_INDEX = new int[16];
    		PRG_ROM = new byte[0][];
    		int num = 0;
    		for (int j = 0; j < rom.PRG.Length; j += 4096)
    		{
    			Array.Resize(ref PRG_ROM, PRG_ROM.GetLength(0) + 1);
    			PRG_ROM[num] = new byte[4096];
    			for (int k = 0; k < 4096; k++)
    			{
    				PRG_ROM[num][k] = rom.PRG[j + k];
    			}
    			num++;
    		}
    		PRG_ROM_04KB_Count = PRG_ROM.GetLength(0);
    		PRG_ROM_04KB_Mask = PRG_ROM_04KB_Count - 1;
    		PRG_ROM_08KB_Count = PRG_ROM_04KB_Count / 2;
    		PRG_ROM_08KB_Mask = PRG_ROM_08KB_Count - 1;
    		PRG_ROM_16KB_Count = PRG_ROM_04KB_Count / 4;
    		PRG_ROM_16KB_Mask = PRG_ROM_16KB_Count - 1;
    		PRG_ROM_32KB_Count = PRG_ROM_04KB_Count / 8;
    		PRG_ROM_32KB_Mask = PRG_ROM_32KB_Count - 1;
    		Tracer.WriteLine("PRG ROM loaded successfully.");
    		Tracer.WriteLine("PRG ROM Size = " + PRG_ROM_04KB_Count * 4 + "KB");
    		Tracer.WriteLine("Loading PRG RAM ...");
    		SRAMBankInfo[] pRGRAM8KCountFromDB = GetPRGRAM8KCountFromDB();
    		PRG_RAM = new byte[0][];
    		PRG_RAM_BATTERY = new bool[0];
    		PRG_RAM_ENABLED = new bool[0];
    		PRG_RAM_WRITABLE = new bool[0];
    		SRAMBankInfo[] array = pRGRAM8KCountFromDB;
    		for (int l = 0; l < array.Length; l++)
    		{
    			SRAMBankInfo sRAMBankInfo = array[l];
    			if (sRAMBankInfo.BATTERY)
    			{
    				SRAMSaveRequired = true;
    			}
    			int result = 0;
    			int.TryParse(sRAMBankInfo.SIZE.Replace("k", ""), out result);
    			if (result > 0)
    			{
    				int num2 = result / 2;
    				for (int m = 0; m < num2; m++)
    				{
    					Array.Resize(ref PRG_RAM_BATTERY, PRG_RAM_BATTERY.Length + 1);
    					Array.Resize(ref PRG_RAM_ENABLED, PRG_RAM_ENABLED.Length + 1);
    					Array.Resize(ref PRG_RAM_WRITABLE, PRG_RAM_WRITABLE.Length + 1);
    					Array.Resize(ref PRG_RAM, PRG_RAM.GetLength(0) + 1);
    					PRG_RAM[PRG_RAM.GetLength(0) - 1] = new byte[4096];
    					PRG_RAM_BATTERY[PRG_RAM_BATTERY.Length - 1] = sRAMBankInfo.BATTERY;
    					PRG_RAM_ENABLED[PRG_RAM_ENABLED.Length - 1] = true;
    					PRG_RAM_WRITABLE[PRG_RAM_WRITABLE.Length - 1] = true;
    				}
    			}
    		}
    		PRG_RAM_04KB_Count = PRG_RAM.GetLength(0);
    		PRG_RAM_04KB_Mask = PRG_RAM_04KB_Count - 1;
    		PRG_RAM_08KB_Count = PRG_RAM_04KB_Count / 2;
    		PRG_RAM_08KB_Mask = PRG_RAM_08KB_Count - 1;
    		PRG_RAM_16KB_Count = PRG_RAM_04KB_Count / 4;
    		PRG_RAM_16KB_Mask = PRG_RAM_16KB_Count - 1;
    		PRG_RAM_32KB_Count = PRG_RAM_04KB_Count / 8;
    		PRG_RAM_32KB_Mask = PRG_RAM_32KB_Count - 1;
    		Tracer.WriteLine("PRG RAM loaded successfully.");
    		Tracer.WriteLine("PRG RAM Size = " + PRG_RAM_04KB_Count * 4 + "KB");
    		if (rom.HasTrainer)
    		{
    			rom.Trainer.CopyTo(PRG_RAM[3], 0);
    		}
    		Tracer.WriteLine("Loading CHR ROM ...");
    		CHR_ROM = new byte[0][];
    		CHR_AREA_BLK_RAM = new bool[8];
    		CHR_AREA_BLK_INDEX = new int[8];
    		num = 0;
    		for (int n = 0; n < rom.CHR.Length; n += 1024)
    		{
    			Array.Resize(ref CHR_ROM, CHR_ROM.GetLength(0) + 1);
    			CHR_ROM[num] = new byte[1024];
    			for (int num3 = 0; num3 < 1024; num3++)
    			{
    				CHR_ROM[num][num3] = rom.CHR[n + num3];
    			}
    			num++;
    		}
    		CHR_ROM_01KB_Count = CHR_ROM.GetLength(0);
    		CHR_ROM_01KB_Mask = CHR_ROM_01KB_Count - 1;
    		CHR_ROM_02KB_Count = CHR_ROM_01KB_Count / 2;
    		CHR_ROM_02KB_Mask = CHR_ROM_02KB_Count - 1;
    		CHR_ROM_04KB_Count = CHR_ROM_01KB_Count / 4;
    		CHR_ROM_04KB_Mask = CHR_ROM_04KB_Count - 1;
    		CHR_ROM_08KB_Count = CHR_ROM_01KB_Count / 8;
    		CHR_ROM_08KB_Mask = CHR_ROM_08KB_Count - 1;
    		Tracer.WriteLine("CHR ROM loaded successfully.");
    		Tracer.WriteLine("CHR ROM Size = " + CHR_ROM_01KB_Count + "KB");
    		Tracer.WriteLine("Loading CHR RAM ...");
    		int cHRRAM1KCountFromDB = GetCHRRAM1KCountFromDB();
    		CHR_RAM = new byte[0][];
    		CHR_RAM_BATTERY = new bool[cHRRAM1KCountFromDB];
    		CHR_RAM_ENABLED = new bool[cHRRAM1KCountFromDB];
    		CHR_RAM_WRITABLE = new bool[cHRRAM1KCountFromDB];
    		for (int num4 = 0; num4 < cHRRAM1KCountFromDB; num4++)
    		{
    			Array.Resize(ref CHR_RAM, CHR_RAM.GetLength(0) + 1);
    			CHR_RAM[num4] = new byte[1024];
    			CHR_RAM_BATTERY[num4] = false;
    			CHR_RAM_ENABLED[num4] = true;
    			CHR_RAM_WRITABLE[num4] = true;
    		}
    		CHR_RAM_01KB_Count = CHR_RAM.GetLength(0);
    		CHR_RAM_01KB_Mask = CHR_RAM_01KB_Count - 1;
    		CHR_RAM_02KB_Count = CHR_RAM_01KB_Count / 2;
    		CHR_RAM_02KB_Mask = CHR_RAM_02KB_Count - 1;
    		CHR_RAM_04KB_Count = CHR_RAM_01KB_Count / 4;
    		CHR_RAM_04KB_Mask = CHR_RAM_04KB_Count - 1;
    		CHR_RAM_08KB_Count = CHR_RAM_01KB_Count / 8;
    		CHR_RAM_08KB_Mask = CHR_RAM_08KB_Count - 1;
    		Tracer.WriteLine("CHR RAM loaded successfully.");
    		Tracer.WriteLine("CHR RAM Size = " + CHR_RAM_01KB_Count + "KB");
    		Tracer.WriteLine("Loading Nametables ...");
    		NMT_AREA_BLK_INDEX = new int[4];
    		NMT_RAM = new byte[0][];
    		for (int num5 = 0; num5 < 4; num5++)
    		{
    			Array.Resize(ref NMT_RAM, NMT_RAM.GetLength(0) + 1);
    			NMT_RAM[num5] = new byte[1024];
    		}
    		NMT_DEFAULT_MIRROR = rom.Mirroring;
    		Tracer.WriteLine("Mirroring set to " + NMT_DEFAULT_MIRROR);
    		Tracer.WriteLine("Board (Mapper # " + MapperNumber + ") initialized successfully.");
    	}

    	internal virtual void HardReset()
    	{
    		Tracer.WriteLine("Hard reset board (Mapper # " + MapperNumber + ") ....");
    		Tracer.WriteLine("Switching 16KB PRG RAM at 0x4000 - 0x7000");
    		Toggle16KPRG_RAM(ram: true, PRGArea.Area4000);
    		Switch16KPRG(0, PRGArea.Area4000);
    		Tracer.WriteLine("Switching 32KB PRG ROM at 0x8000 - 0xF000");
    		Toggle32KPRG_RAM(ram: false, PRGArea.Area8000);
    		Switch32KPRG(0, PRGArea.Area8000);
    		Tracer.WriteLine("Switching 8KB CHR " + ((CHR_ROM_01KB_Count == 0) ? "RAM" : "ROM") + " at 0x0000 - 0x1000");
    		Toggle08KCHR_RAM(CHR_ROM_01KB_Count == 0);
    		Switch08KCHR(0);
    		Tracer.WriteLine("Switching to mirroring: " + NMT_DEFAULT_MIRROR);
    		Switch01KNMTFromMirroring(NMT_DEFAULT_MIRROR);
    		Tracer.WriteLine("Hard reset board (Mapper # " + MapperNumber + ") is done successfully.");
    	}

    	internal virtual void SoftReset()
    	{
    	}

    	protected virtual void LoadAttrs()
    	{
    		enable_external_sound = false;
    		Attribute[] customAttributes = Attribute.GetCustomAttributes(GetType());
    		foreach (Attribute attribute in customAttributes)
    		{
    			if (attribute.GetType() == typeof(BoardInfoAttribute))
    			{
    				BoardInfoAttribute boardInfoAttribute = (BoardInfoAttribute)attribute;
    				Name = boardInfoAttribute.Name;
    				MapperNumber = boardInfoAttribute.Mapper;
    				PRG_RAM_08KB_DEFAULT_BLK_Count = boardInfoAttribute.DefaultPRG_RAM_8KB_BanksCount;
    				CHR_ROM_01KB_DEFAULT_BLK_Count = boardInfoAttribute.DefaultCHR_RAM_1KB_BanksCount;
    				enabled_ppuA12ToggleTimer = boardInfoAttribute.Enabled_ppuA12ToggleTimer;
    				ppuA12TogglesOnRaisingEdge = boardInfoAttribute.PPUA12TogglesOnRaisingEdge;
    			}
    			else if (attribute.GetType() == typeof(WithExternalSoundAttribute))
    			{
    				enable_external_sound = true;
    			}
    			else if (attribute.GetType() == typeof(HassIssuesAttribute))
    			{
    				HasIssues = true;
    			}
    		}
    	}

    	protected SRAMBankInfo[] GetPRGRAM8KCountFromDB()
    	{
    		Tracer.WriteLine("Retrieving PRG RAM information from database ....");
    		List<SRAMBankInfo> list = new List<SRAMBankInfo>();
    		if (IsGameFoundOnDB)
    		{
    			if (GameCartInfo.WRAMBanks.Count > 0)
    			{
    				foreach (SRAMBankInfo wRAMBank in GameCartInfo.WRAMBanks)
    				{
    					list.Add(wRAMBank);
    				}
    			}
    			else
    			{
    				Tracer.WriteLine("This game has no PRG RAM !");
    				Tracer.WriteWarning("> Adding 8K x " + PRG_RAM_08KB_DEFAULT_BLK_Count + " PRG RAM BANKS to avoid exceptions.");
    				SRAMBankInfo item = new SRAMBankInfo(0, PRG_RAM_08KB_DEFAULT_BLK_Count * 8 + "k", BATTERY: true);
    				list.Add(item);
    			}
    		}
    		else
    		{
    			Tracer.WriteWarning("Could't find this game in database .... Adding 8K x " + PRG_RAM_08KB_DEFAULT_BLK_Count + " PRG RAM BANKS to avoid exceptions.");
    			SRAMBankInfo item2 = new SRAMBankInfo(0, PRG_RAM_08KB_DEFAULT_BLK_Count * 8 + "k", BATTERY: true);
    			list.Add(item2);
    		}
    		return list.ToArray();
    	}

    	protected int GetCHRRAM1KCountFromDB()
    	{
    		int num = 0;
    		Tracer.WriteLine("Retrieving CHR RAM information from database ....");
    		if (IsGameFoundOnDB)
    		{
    			bool flag = false;
    			if (GameCartInfo.VRAM_sizes != null)
    			{
    				Tracer.WriteLine("Using database to initialize CHR RAM .....");
    				foreach (string vRAM_size in GameCartInfo.VRAM_sizes)
    				{
    					int result = 0;
    					if (int.TryParse(vRAM_size.Replace("k", ""), out result))
    					{
    						Tracer.WriteLine(">CHR RAM CHIP SIZE " + vRAM_size + " KB added");
    						num += result;
    						if (num > 0)
    						{
    							flag = true;
    						}
    					}
    				}
    			}
    			if (!flag)
    			{
    				Tracer.WriteLine("Game not found in database to initialize CHR RAM; CHR RAM size set to " + CHR_ROM_01KB_DEFAULT_BLK_Count + " KB");
    				num = CHR_ROM_01KB_DEFAULT_BLK_Count;
    			}
    		}
    		else
    		{
    			Tracer.WriteWarning("Game not found in database to initialize CHR RAM; CHR RAM size set to " + CHR_ROM_01KB_DEFAULT_BLK_Count + " KB");
    			num = CHR_ROM_01KB_DEFAULT_BLK_Count;
    		}
    		return num;
    	}

    	internal virtual void WriteEX(ref ushort addr, ref byte val)
    	{
    		PRG_TMP_AREA = (addr >> 12) & 0xF;
    		if (PRG_AREA_BLK_RAM[PRG_TMP_AREA])
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_RAM_04KB_Mask;
    			if (PRG_RAM_ENABLED[PRG_TMP_INDX] && PRG_RAM_WRITABLE[PRG_TMP_INDX])
    			{
    				PRG_RAM[PRG_TMP_INDX][addr & 0xFFF] = val;
    			}
    		}
    	}

    	internal virtual void WriteSRM(ref ushort addr, ref byte val)
    	{
    		PRG_TMP_AREA = (addr >> 12) & 0xF;
    		if (PRG_AREA_BLK_RAM[PRG_TMP_AREA])
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_RAM_04KB_Mask;
    			if (PRG_RAM_ENABLED[PRG_TMP_INDX] && PRG_RAM_WRITABLE[PRG_TMP_INDX])
    			{
    				PRG_RAM[PRG_TMP_INDX][addr & 0xFFF] = val;
    			}
    		}
    	}

    	internal virtual void WritePRG(ref ushort addr, ref byte val)
    	{
    		PRG_TMP_AREA = (addr >> 12) & 0xF;
    		if (PRG_AREA_BLK_RAM[PRG_TMP_AREA])
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_RAM_04KB_Mask;
    			if (PRG_RAM_ENABLED[PRG_TMP_INDX] && PRG_RAM_WRITABLE[PRG_TMP_INDX])
    			{
    				PRG_RAM[PRG_TMP_INDX][addr & 0xFFF] = val;
    			}
    		}
    	}

    	internal virtual void ReadEX(ref ushort addr, out byte val)
    	{
    		PRG_TMP_AREA = (addr >> 12) & 0xF;
    		if (PRG_AREA_BLK_RAM[PRG_TMP_AREA])
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_RAM_04KB_Mask;
    			if (PRG_RAM_ENABLED[PRG_TMP_INDX])
    			{
    				val = PRG_RAM[PRG_TMP_INDX][addr & 0xFFF];
    			}
    			else
    			{
    				val = 0;
    			}
    		}
    		else
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_ROM_04KB_Mask;
    			val = PRG_ROM[PRG_TMP_INDX][addr & 0xFFF];
    		}
    	}

    	internal virtual void ReadSRM(ref ushort addr, out byte val)
    	{
    		PRG_TMP_AREA = (addr >> 12) & 0xF;
    		if (PRG_AREA_BLK_RAM[PRG_TMP_AREA])
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_RAM_04KB_Mask;
    			if (PRG_RAM_ENABLED[PRG_TMP_INDX])
    			{
    				val = PRG_RAM[PRG_TMP_INDX][addr & 0xFFF];
    			}
    			else
    			{
    				val = 0;
    			}
    		}
    		else
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_ROM_04KB_Mask;
    			val = PRG_ROM[PRG_TMP_INDX][addr & 0xFFF];
    		}
    	}

    	internal virtual void ReadPRG(ref ushort addr, out byte val)
    	{
    		PRG_TMP_AREA = (addr >> 12) & 0xF;
    		if (PRG_AREA_BLK_RAM[PRG_TMP_AREA])
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_RAM_04KB_Mask;
    			if (PRG_RAM_ENABLED[PRG_TMP_INDX])
    			{
    				val = PRG_RAM[PRG_TMP_INDX][addr & 0xFFF];
    			}
    			else
    			{
    				val = 0;
    			}
    		}
    		else
    		{
    			PRG_TMP_INDX = PRG_AREA_BLK_INDEX[PRG_TMP_AREA] & PRG_ROM_04KB_Mask;
    			val = PRG_ROM[PRG_TMP_INDX][addr & 0xFFF];
    		}
    		if (!IsGameGenieActive)
    		{
    			return;
    		}
    		GameGenieCode[] gameGenieCodes = GameGenieCodes;
    		for (int i = 0; i < gameGenieCodes.Length; i++)
    		{
    			GameGenieCode gameGenieCode = gameGenieCodes[i];
    			if (!gameGenieCode.Enabled || gameGenieCode.Address != addr)
    			{
    				continue;
    			}
    			if (gameGenieCode.IsCompare)
    			{
    				if (gameGenieCode.Compare == val)
    				{
    					val = gameGenieCode.Value;
    				}
    			}
    			else
    			{
    				val = gameGenieCode.Value;
    			}
    			break;
    		}
    	}

    	internal virtual void WriteCHR(ref ushort addr, ref byte val)
    	{
    		CHR_TMP_AREA = (addr >> 10) & 7;
    		if (CHR_AREA_BLK_RAM[CHR_TMP_AREA])
    		{
    			CHR_TMP_INDX = CHR_AREA_BLK_INDEX[CHR_TMP_AREA] & CHR_RAM_01KB_Mask;
    			if (CHR_RAM_ENABLED[CHR_TMP_INDX] && CHR_RAM_WRITABLE[CHR_TMP_INDX])
    			{
    				CHR_RAM[CHR_TMP_INDX][addr & 0x3FF] = val;
    			}
    		}
    	}

    	internal virtual void ReadCHR(ref ushort addr, out byte val)
    	{
    		CHR_TMP_AREA = (addr >> 10) & 7;
    		CHR_TMP_INDX = CHR_AREA_BLK_INDEX[CHR_TMP_AREA];
    		if (CHR_AREA_BLK_RAM[CHR_TMP_AREA])
    		{
    			CHR_TMP_INDX &= CHR_RAM_01KB_Mask;
    			if (CHR_RAM_ENABLED[CHR_TMP_INDX])
    			{
    				val = CHR_RAM[CHR_TMP_INDX][addr & 0x3FF];
    			}
    			else
    			{
    				val = 0;
    			}
    		}
    		else
    		{
    			CHR_TMP_INDX &= CHR_ROM_01KB_Mask;
    			val = CHR_ROM[CHR_TMP_INDX][addr & 0x3FF];
    		}
    	}

    	internal virtual void WriteNMT(ref ushort addr, ref byte val)
    	{
    		NMT_TMP_AREA = (addr >> 10) & 3;
    		NMT_TMP_INDX = NMT_AREA_BLK_INDEX[NMT_TMP_AREA];
    		NMT_RAM[NMT_TMP_INDX][addr & 0x3FF] = val;
    	}

    	internal virtual void ReadNMT(ref ushort addr, out byte val)
    	{
    		NMT_TMP_AREA = (addr >> 10) & 3;
    		NMT_TMP_INDX = NMT_AREA_BLK_INDEX[NMT_TMP_AREA];
    		val = NMT_RAM[NMT_TMP_INDX][addr & 0x3FF];
    	}

    	protected void Switch04KPRG(int index, PRGArea area)
    	{
    		PRG_AREA_BLK_INDEX[(uint)area] = index;
    	}

    	protected void Switch08KPRG(int index, PRGArea area)
    	{
    		index *= 2;
    		PRG_AREA_BLK_INDEX[(uint)area] = index;
    		PRG_AREA_BLK_INDEX[(uint)(area + 1)] = index + 1;
    	}

    	protected void Switch16KPRG(int index, PRGArea area)
    	{
    		index *= 4;
    		PRG_AREA_BLK_INDEX[(uint)area] = index;
    		PRG_AREA_BLK_INDEX[(uint)(area + 1)] = index + 1;
    		PRG_AREA_BLK_INDEX[(uint)(area + 2)] = index + 2;
    		PRG_AREA_BLK_INDEX[(uint)(area + 3)] = index + 3;
    	}

    	protected void Switch32KPRG(int index, PRGArea area)
    	{
    		index *= 8;
    		PRG_AREA_BLK_INDEX[(uint)area] = index;
    		PRG_AREA_BLK_INDEX[(uint)(area + 1)] = index + 1;
    		PRG_AREA_BLK_INDEX[(uint)(area + 2)] = index + 2;
    		PRG_AREA_BLK_INDEX[(uint)(area + 3)] = index + 3;
    		PRG_AREA_BLK_INDEX[(uint)(area + 4)] = index + 4;
    		PRG_AREA_BLK_INDEX[(uint)(area + 5)] = index + 5;
    		PRG_AREA_BLK_INDEX[(uint)(area + 6)] = index + 6;
    		PRG_AREA_BLK_INDEX[(uint)(area + 7)] = index + 7;
    	}

    	protected void Toggle04KPRG_RAM(bool ram, PRGArea area)
    	{
    		PRG_AREA_BLK_RAM[(uint)area] = ram;
    	}

    	protected void Toggle08KPRG_RAM(bool ram, PRGArea area)
    	{
    		PRG_AREA_BLK_RAM[(uint)area] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 1)] = ram;
    	}

    	protected void Toggle16KPRG_RAM(bool ram, PRGArea area)
    	{
    		PRG_AREA_BLK_RAM[(uint)area] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 1)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 2)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 3)] = ram;
    	}

    	protected void Toggle32KPRG_RAM(bool ram, PRGArea area)
    	{
    		PRG_AREA_BLK_RAM[(uint)area] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 1)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 2)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 3)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 4)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 5)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 6)] = ram;
    		PRG_AREA_BLK_RAM[(uint)(area + 7)] = ram;
    	}

    	protected void TogglePRGRAMEnable(bool enable)
    	{
    		for (int i = 0; i < PRG_RAM_ENABLED.Length; i++)
    		{
    			PRG_RAM_ENABLED[i] = enable;
    		}
    	}

    	protected void TogglePRGRAMWritableEnable(bool enable)
    	{
    		for (int i = 0; i < PRG_RAM_WRITABLE.Length; i++)
    		{
    			PRG_RAM_WRITABLE[i] = enable;
    		}
    	}

    	protected void Toggle04KPRG_RAM_Enabled(bool enable, int index)
    	{
    		PRG_RAM_ENABLED[index] = enable;
    	}

    	protected void Toggle04KPRG_RAM_Writable(bool enable, int index)
    	{
    		PRG_RAM_WRITABLE[index] = enable;
    	}

    	protected void Toggle04KPRG_RAM_Battery(bool enable, int index)
    	{
    		PRG_RAM_BATTERY[index] = enable;
    	}

    	protected void Switch01KCHR(int index, CHRArea area)
    	{
    		CHR_AREA_BLK_INDEX[(uint)area] = index;
    	}

    	protected void Switch02KCHR(int index, CHRArea area)
    	{
    		index *= 2;
    		CHR_AREA_BLK_INDEX[(uint)area] = index;
    		CHR_AREA_BLK_INDEX[(uint)(area + 1)] = index + 1;
    	}

    	protected void Switch04KCHR(int index, CHRArea area)
    	{
    		index *= 4;
    		CHR_AREA_BLK_INDEX[(uint)area] = index;
    		CHR_AREA_BLK_INDEX[(uint)(area + 1)] = index + 1;
    		CHR_AREA_BLK_INDEX[(uint)(area + 2)] = index + 2;
    		CHR_AREA_BLK_INDEX[(uint)(area + 3)] = index + 3;
    	}

    	protected void Switch08KCHR(int index)
    	{
    		index *= 8;
    		CHR_AREA_BLK_INDEX[0] = index;
    		CHR_AREA_BLK_INDEX[1] = index + 1;
    		CHR_AREA_BLK_INDEX[2] = index + 2;
    		CHR_AREA_BLK_INDEX[3] = index + 3;
    		CHR_AREA_BLK_INDEX[4] = index + 4;
    		CHR_AREA_BLK_INDEX[5] = index + 5;
    		CHR_AREA_BLK_INDEX[6] = index + 6;
    		CHR_AREA_BLK_INDEX[7] = index + 7;
    	}

    	protected void Toggle01KCHR_RAM(bool ram, CHRArea area)
    	{
    		CHR_AREA_BLK_RAM[(uint)area] = ram;
    	}

    	protected void Toggle02KCHR_RAM(bool ram, CHRArea area)
    	{
    		CHR_AREA_BLK_RAM[(uint)area] = ram;
    		CHR_AREA_BLK_RAM[(uint)(area + 1)] = ram;
    	}

    	protected void Toggle04KCHR_RAM(bool ram, CHRArea area)
    	{
    		CHR_AREA_BLK_RAM[(uint)area] = ram;
    		CHR_AREA_BLK_RAM[(uint)(area + 1)] = ram;
    		CHR_AREA_BLK_RAM[(uint)(area + 2)] = ram;
    		CHR_AREA_BLK_RAM[(uint)(area + 3)] = ram;
    	}

    	protected void Toggle08KCHR_RAM(bool ram)
    	{
    		CHR_AREA_BLK_RAM[0] = ram;
    		CHR_AREA_BLK_RAM[1] = ram;
    		CHR_AREA_BLK_RAM[2] = ram;
    		CHR_AREA_BLK_RAM[3] = ram;
    		CHR_AREA_BLK_RAM[4] = ram;
    		CHR_AREA_BLK_RAM[5] = ram;
    		CHR_AREA_BLK_RAM[6] = ram;
    		CHR_AREA_BLK_RAM[7] = ram;
    	}

    	protected void Toggle01KCHR_RAM_Enabled(bool enable, int index)
    	{
    		CHR_RAM_ENABLED[index] = enable;
    	}

    	protected void Toggle01KCHR_RAM_Writable(bool enable, int index)
    	{
    		CHR_RAM_WRITABLE[index] = enable;
    	}

    	protected void ToggleCHRRAMWritableEnable(bool enable)
    	{
    		for (int i = 0; i < CHR_RAM_WRITABLE.Length; i++)
    		{
    			CHR_RAM_WRITABLE[i] = enable;
    		}
    	}

    	protected void Toggle01KCHR_RAM_Battery(bool enable, int index)
    	{
    		CHR_RAM_BATTERY[index] = enable;
    	}

    	protected void Switch01KNMT(int index, byte area)
    	{
    		NMT_AREA_BLK_INDEX[area] = index;
    	}

    	protected void Switch01KNMT(byte mirroring)
    	{
    		NMT_AREA_BLK_INDEX[0] = mirroring & 3;
    		NMT_AREA_BLK_INDEX[1] = (mirroring >> 2) & 3;
    		NMT_AREA_BLK_INDEX[2] = (mirroring >> 4) & 3;
    		NMT_AREA_BLK_INDEX[3] = (mirroring >> 6) & 3;
    	}

    	protected void Switch01KNMTFromMirroring(Mirroring mirroring)
    	{
    		NMT_AREA_BLK_INDEX[0] = (int)(mirroring & (Mirroring)3);
    		NMT_AREA_BLK_INDEX[1] = ((int)mirroring >> 2) & 3;
    		NMT_AREA_BLK_INDEX[2] = ((int)mirroring >> 4) & 3;
    		NMT_AREA_BLK_INDEX[3] = ((int)mirroring >> 6) & 3;
    	}

    	internal virtual void OnPPUAddressUpdate(ref ushort address)
    	{
    		if (!enabled_ppuA12ToggleTimer)
    		{
    			return;
    		}
    		old_vram_address = new_vram_address;
    		new_vram_address = address & 0x1000;
    		if (ppuA12TogglesOnRaisingEdge)
    		{
    			if (old_vram_address < new_vram_address)
    			{
    				if (ppu_cycles_timer > 8)
    				{
    					OnPPUA12RaisingEdge();
    				}
    				ppu_cycles_timer = 0;
    			}
    		}
    		else if (old_vram_address > new_vram_address)
    		{
    			if (ppu_cycles_timer > 8)
    			{
    				OnPPUA12RaisingEdge();
    			}
    			ppu_cycles_timer = 0;
    		}
    	}

    	internal virtual void OnCPUClock()
    	{
    	}

    	internal virtual void OnPPUClock()
    	{
    		if (enabled_ppuA12ToggleTimer)
    		{
    			ppu_cycles_timer++;
    		}
    	}

    	internal virtual void OnPPUA12RaisingEdge()
    	{
    	}

    	internal virtual void OnPPUScanlineTick()
    	{
    	}

    	internal virtual void OnAPUClockDuration()
    	{
    	}

    	internal virtual void OnAPUClockEnvelope()
    	{
    	}

    	internal virtual void OnAPUClockSingle()
    	{
    	}

    	internal virtual void OnAPUClock()
    	{
    	}

    	internal virtual double APUGetSample()
    	{
    		return 0.0;
    	}

    	internal virtual void APUApplyChannelsSettings()
    	{
    	}

    	internal void SetupGameGenie(bool IsGameGenieActive, GameGenieCode[] GameGenieCodes)
    	{
    		this.IsGameGenieActive = IsGameGenieActive;
    		this.GameGenieCodes = GameGenieCodes;
    	}

    	internal virtual void WriteStateData(ref BinaryWriter bin)
    	{
    		for (int i = 0; i < PRG_RAM.Length; i++)
    		{
    			bin.Write(PRG_RAM[i]);
    		}
    		for (int j = 0; j < PRG_RAM_ENABLED.Length; j++)
    		{
    			bin.Write(PRG_RAM_ENABLED[j]);
    		}
    		for (int k = 0; k < PRG_RAM_WRITABLE.Length; k++)
    		{
    			bin.Write(PRG_RAM_WRITABLE[k]);
    		}
    		for (int l = 0; l < PRG_RAM_BATTERY.Length; l++)
    		{
    			bin.Write(PRG_RAM_BATTERY[l]);
    		}
    		for (int m = 0; m < PRG_AREA_BLK_RAM.Length; m++)
    		{
    			bin.Write(PRG_AREA_BLK_RAM[m]);
    		}
    		for (int n = 0; n < PRG_AREA_BLK_INDEX.Length; n++)
    		{
    			bin.Write(PRG_AREA_BLK_INDEX[n]);
    		}
    		bin.Write(PRG_TMP_INDX);
    		bin.Write(PRG_TMP_AREA);
    		for (int num = 0; num < CHR_RAM.Length; num++)
    		{
    			bin.Write(CHR_RAM[num]);
    		}
    		for (int num2 = 0; num2 < CHR_RAM_ENABLED.Length; num2++)
    		{
    			bin.Write(CHR_RAM_ENABLED[num2]);
    		}
    		for (int num3 = 0; num3 < CHR_RAM_WRITABLE.Length; num3++)
    		{
    			bin.Write(CHR_RAM_WRITABLE[num3]);
    		}
    		for (int num4 = 0; num4 < CHR_RAM_BATTERY.Length; num4++)
    		{
    			bin.Write(CHR_RAM_BATTERY[num4]);
    		}
    		for (int num5 = 0; num5 < CHR_AREA_BLK_RAM.Length; num5++)
    		{
    			bin.Write(CHR_AREA_BLK_RAM[num5]);
    		}
    		for (int num6 = 0; num6 < CHR_AREA_BLK_INDEX.Length; num6++)
    		{
    			bin.Write(CHR_AREA_BLK_INDEX[num6]);
    		}
    		bin.Write(CHR_TMP_INDX);
    		bin.Write(CHR_TMP_AREA);
    		for (int num7 = 0; num7 < NMT_RAM.Length; num7++)
    		{
    			bin.Write(NMT_RAM[num7]);
    		}
    		for (int num8 = 0; num8 < NMT_AREA_BLK_INDEX.Length; num8++)
    		{
    			bin.Write(NMT_AREA_BLK_INDEX[num8]);
    		}
    		bin.Write(NMT_TMP_INDX);
    		bin.Write(NMT_TMP_AREA);
    	}

    	internal virtual void ReadStateData(ref BinaryReader bin)
    	{
    		for (int i = 0; i < PRG_RAM.Length; i++)
    		{
    			bin.Read(PRG_RAM[i], 0, PRG_RAM[i].Length);
    		}
    		for (int j = 0; j < PRG_RAM_ENABLED.Length; j++)
    		{
    			PRG_RAM_ENABLED[j] = bin.ReadBoolean();
    		}
    		for (int k = 0; k < PRG_RAM_WRITABLE.Length; k++)
    		{
    			PRG_RAM_WRITABLE[k] = bin.ReadBoolean();
    		}
    		for (int l = 0; l < PRG_RAM_BATTERY.Length; l++)
    		{
    			PRG_RAM_BATTERY[l] = bin.ReadBoolean();
    		}
    		for (int m = 0; m < PRG_AREA_BLK_RAM.Length; m++)
    		{
    			PRG_AREA_BLK_RAM[m] = bin.ReadBoolean();
    		}
    		for (int n = 0; n < PRG_AREA_BLK_INDEX.Length; n++)
    		{
    			PRG_AREA_BLK_INDEX[n] = bin.ReadInt32();
    		}
    		PRG_TMP_INDX = bin.ReadInt32();
    		PRG_TMP_AREA = bin.ReadInt32();
    		for (int num = 0; num < CHR_RAM.Length; num++)
    		{
    			bin.Read(CHR_RAM[num], 0, CHR_RAM[num].Length);
    		}
    		for (int num2 = 0; num2 < CHR_RAM_ENABLED.Length; num2++)
    		{
    			CHR_RAM_ENABLED[num2] = bin.ReadBoolean();
    		}
    		for (int num3 = 0; num3 < CHR_RAM_WRITABLE.Length; num3++)
    		{
    			CHR_RAM_WRITABLE[num3] = bin.ReadBoolean();
    		}
    		for (int num4 = 0; num4 < CHR_RAM_BATTERY.Length; num4++)
    		{
    			CHR_RAM_BATTERY[num4] = bin.ReadBoolean();
    		}
    		for (int num5 = 0; num5 < CHR_AREA_BLK_RAM.Length; num5++)
    		{
    			CHR_AREA_BLK_RAM[num5] = bin.ReadBoolean();
    		}
    		for (int num6 = 0; num6 < CHR_AREA_BLK_INDEX.Length; num6++)
    		{
    			CHR_AREA_BLK_INDEX[num6] = bin.ReadInt32();
    		}
    		CHR_TMP_INDX = bin.ReadInt32();
    		CHR_TMP_AREA = bin.ReadInt32();
    		for (int num7 = 0; num7 < NMT_RAM.Length; num7++)
    		{
    			bin.Read(NMT_RAM[num7], 0, NMT_RAM[num7].Length);
    		}
    		for (int num8 = 0; num8 < NMT_AREA_BLK_INDEX.Length; num8++)
    		{
    			NMT_AREA_BLK_INDEX[num8] = bin.ReadInt32();
    		}
    		NMT_TMP_INDX = bin.ReadInt32();
    		NMT_TMP_AREA = bin.ReadInt32();
    	}

    	internal void SaveSRAM(Stream stream)
    	{
    		for (int i = 0; i < PRG_RAM_04KB_Count; i++)
    		{
    			if (PRG_RAM_BATTERY[i])
    			{
    				stream.Write(PRG_RAM[i], 0, 4096);
    			}
    		}
    	}

    	internal byte[] GetSRAMBuffer()
    	{
    		List<byte> list = new List<byte>();
    		for (int i = 0; i < PRG_RAM_04KB_Count; i++)
    		{
    			if (PRG_RAM_BATTERY[i])
    			{
    				list.AddRange(PRG_RAM[i]);
    			}
    		}
    		return list.ToArray();
    	}

    	internal void LoadSRAM(Stream stream)
    	{
    		for (int i = 0; i < PRG_RAM_04KB_Count; i++)
    		{
    			if (PRG_RAM_BATTERY[i])
    			{
    				stream.Read(PRG_RAM[i], 0, 4096);
    			}
    		}
    	}

    	internal void LoadSRAM(byte[] buffer)
    	{
    		int num = 0;
    		for (int i = 0; i < PRG_RAM_04KB_Count; i++)
    		{
    			if (PRG_RAM_BATTERY[i])
    			{
    				for (int j = 0; j < 4096; j++)
    				{
    					PRG_RAM[i][j] = buffer[j + num];
    				}
    				num += 4096;
    			}
    		}
    	}
    }
}
