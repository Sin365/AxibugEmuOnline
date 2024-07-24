using System;
using System.IO;

namespace MyNes.Core
{
    [BoardInfo("MMC1", 1, 4, 64)]
    internal class Mapper001 : Board
    {
    	private int address_reg;

    	private byte[] reg = new byte[4];

    	private byte shift;

    	private byte buffer;

    	private bool flag_p;

    	private bool flag_c;

    	private bool flag_s;

    	private bool enable_wram_enable;

    	private int prg_hijackedbit;

    	private bool use_hijacked;

    	private bool use_sram_switch;

    	private int sram_switch_mask;

    	private int cpuCycles;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		cpuCycles = 0;
    		address_reg = 0;
    		reg = new byte[4];
    		reg[0] = 12;
    		flag_c = false;
    		flag_s = (flag_p = true);
    		prg_hijackedbit = 0;
    		reg[1] = (reg[2] = (reg[3] = 0));
    		buffer = 0;
    		shift = 0;
    		if (base.Chips.Contains("MMC1B") || base.Chips.Contains("MMC1B2"))
    		{
    			TogglePRGRAMEnable(enable: false);
    			Console.WriteLine("MMC1: SRAM Disabled.");
    		}
    		enable_wram_enable = !base.Chips.Contains("MMC1A");
    		Console.WriteLine("MMC1: enable_wram_enable = " + enable_wram_enable);
    		use_hijacked = (PRG_ROM_16KB_Mask & 0x10) == 16;
    		if (use_hijacked)
    		{
    			prg_hijackedbit = 16;
    		}
    		use_sram_switch = false;
    		if (PRG_RAM_08KB_Count > 0)
    		{
    			use_sram_switch = true;
    			sram_switch_mask = (use_hijacked ? 8 : 24);
    			sram_switch_mask &= PRG_RAM_08KB_Mask << 3;
    			if (sram_switch_mask == 0)
    			{
    				use_sram_switch = false;
    			}
    		}
    		Switch16KPRG(0xF | prg_hijackedbit, PRGArea.AreaC000);
    		Console.WriteLine("MMC1: use_hijacked = " + use_hijacked);
    		Console.WriteLine("MMC1: use_sram_switch = " + use_sram_switch);
    		Console.WriteLine("MMC1: sram_switch_mask = " + sram_switch_mask.ToString("X2"));
    	}

    	internal override void WritePRG(ref ushort address, ref byte value)
    	{
    		if (cpuCycles > 0)
    		{
    			return;
    		}
    		cpuCycles = 3;
    		if ((value & 0x80) == 128)
    		{
    			reg[0] |= 12;
    			flag_s = (flag_p = true);
    			shift = (buffer = 0);
    			return;
    		}
    		if ((value & 1) == 1)
    		{
    			buffer |= (byte)(1 << (int)shift);
    		}
    		if (++shift < 5)
    		{
    			return;
    		}
    		address_reg = (address & 0x7FFF) >> 13;
    		reg[address_reg] = buffer;
    		shift = (buffer = 0);
    		switch (address_reg)
    		{
    		case 0:
    			flag_c = (reg[0] & 0x10) != 0;
    			flag_p = (reg[0] & 8) != 0;
    			flag_s = (reg[0] & 4) != 0;
    			UpdatePRG();
    			UpdateCHR();
    			switch (reg[0] & 3)
    			{
    			case 0:
    				Switch01KNMTFromMirroring(Mirroring.OneScA);
    				break;
    			case 1:
    				Switch01KNMTFromMirroring(Mirroring.OneScB);
    				break;
    			case 2:
    				Switch01KNMTFromMirroring(Mirroring.Vert);
    				break;
    			case 3:
    				Switch01KNMTFromMirroring(Mirroring.Horz);
    				break;
    			}
    			break;
    		case 1:
    			if (!flag_c)
    			{
    				Switch08KCHR(reg[1] >> 1);
    			}
    			else
    			{
    				Switch04KCHR(reg[1], CHRArea.Area0000);
    			}
    			if (use_sram_switch)
    			{
    				Switch08KPRG((reg[1] & sram_switch_mask) >> 3, PRGArea.Area6000);
    			}
    			if (use_hijacked)
    			{
    				prg_hijackedbit = reg[1] & 0x10;
    				UpdatePRG();
    			}
    			break;
    		case 2:
    			if (flag_c)
    			{
    				Switch04KCHR(reg[2], CHRArea.Area1000);
    			}
    			if (use_sram_switch)
    			{
    				Switch08KPRG((reg[2] & sram_switch_mask) >> 3, PRGArea.Area6000);
    			}
    			if (use_hijacked)
    			{
    				prg_hijackedbit = reg[2] & 0x10;
    				UpdatePRG();
    			}
    			break;
    		case 3:
    			if (enable_wram_enable)
    			{
    				TogglePRGRAMEnable((reg[3] & 0x10) == 0);
    			}
    			UpdatePRG();
    			break;
    		}
    	}

    	private void UpdateCHR()
    	{
    		if (!flag_c)
    		{
    			Switch08KCHR(reg[1] >> 1);
    		}
    		else
    		{
    			Switch04KCHR(reg[1], CHRArea.Area0000);
    			Switch04KCHR(reg[2], CHRArea.Area1000);
    		}
    		if (use_sram_switch)
    		{
    			Switch08KPRG((reg[1] & sram_switch_mask) >> 3, PRGArea.Area6000);
    		}
    	}

    	private void UpdatePRG()
    	{
    		if (!flag_p)
    		{
    			Switch32KPRG(((reg[3] & 0xF) | prg_hijackedbit) >> 1, PRGArea.Area8000);
    		}
    		else if (flag_s)
    		{
    			Switch16KPRG((reg[3] & 0xF) | prg_hijackedbit, PRGArea.Area8000);
    			Switch16KPRG(0xF | prg_hijackedbit, PRGArea.AreaC000);
    		}
    		else
    		{
    			Switch16KPRG(prg_hijackedbit, PRGArea.Area8000);
    			Switch16KPRG((reg[3] & 0xF) | prg_hijackedbit, PRGArea.AreaC000);
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (cpuCycles > 0)
    		{
    			cpuCycles--;
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(reg);
    		stream.Write(shift);
    		stream.Write(buffer);
    		stream.Write(flag_p);
    		stream.Write(flag_c);
    		stream.Write(flag_s);
    		stream.Write(enable_wram_enable);
    		stream.Write(prg_hijackedbit);
    		stream.Write(use_hijacked);
    		stream.Write(use_sram_switch);
    		stream.Write(cpuCycles);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		stream.Read(reg, 0, reg.Length);
    		shift = stream.ReadByte();
    		buffer = stream.ReadByte();
    		flag_p = stream.ReadBoolean();
    		flag_c = stream.ReadBoolean();
    		flag_s = stream.ReadBoolean();
    		enable_wram_enable = stream.ReadBoolean();
    		prg_hijackedbit = stream.ReadInt32();
    		use_hijacked = stream.ReadBoolean();
    		use_sram_switch = stream.ReadBoolean();
    		cpuCycles = stream.ReadInt32();
    	}
    }
}
