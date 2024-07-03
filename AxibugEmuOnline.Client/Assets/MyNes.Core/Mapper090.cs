using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Pirate MMC5-style", 90)]
    [HassIssues]
    internal class Mapper090 : Board
    {
    	protected bool MAPPER90MODE;

    	private int[] prg_reg;

    	private int[] chr_reg;

    	private int[] nt_reg;

    	private int prg_mode;

    	private int chr_mode;

    	private bool chr_block_mode;

    	private int chr_block;

    	private bool chr_m;

    	private bool flag_s;

    	private int irqCounter = 0;

    	private bool IrqEnable = false;

    	private bool irqCountDownMode = false;

    	private bool irqCountUpMode = false;

    	private bool irqFunkyMode = false;

    	private bool irqPrescalerSize = false;

    	private int irqSource = 0;

    	private int irqPrescaler = 0;

    	private int irqPrescalerXOR = 0;

    	private byte irqFunkyModeReg = 0;

    	private byte Dipswitch;

    	private byte multiplication_a;

    	private byte multiplication_b;

    	private ushort multiplication;

    	private byte RAM5803;

    	private bool nt_advanced_enable;

    	private bool nt_rom_only;

    	private int nt_ram_select;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper90;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		MAPPER90MODE = true;
    		prg_reg = new int[4];
    		chr_reg = new int[8];
    		nt_reg = new int[4];
    		prg_mode = (chr_mode = 0);
    		for (int i = 0; i < 4; i++)
    		{
    			prg_reg[i] = i;
    			nt_reg[i] = i;
    		}
    		for (int j = 0; j < 8; j++)
    		{
    			chr_reg[j] = j;
    		}
    		SetupPRG();
    		SetupCHR();
    		Dipswitch = 0;
    		irqCounter = 0;
    		IrqEnable = false;
    		irqCountDownMode = false;
    		irqCountUpMode = false;
    		irqFunkyMode = false;
    		irqPrescalerSize = false;
    		irqSource = 0;
    		irqPrescaler = 0;
    		irqPrescalerXOR = 0;
    		irqFunkyModeReg = 0;
    		RAM5803 = 0;
    		flag_s = false;
    		multiplication_a = 0;
    		multiplication_b = 0;
    		multiplication = 0;
    	}

    	internal override void SoftReset()
    	{
    		base.SoftReset();
    		if (Dipswitch == 0)
    		{
    			Dipswitch = byte.MaxValue;
    		}
    		else
    		{
    			Dipswitch = 0;
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF007)
    		{
    		case 32768:
    		case 32769:
    		case 32770:
    		case 32771:
    		case 32772:
    		case 32773:
    		case 32774:
    		case 32775:
    			prg_reg[address & 3] = data & 0x7F;
    			SetupPRG();
    			break;
    		case 36864:
    		case 36865:
    		case 36866:
    		case 36867:
    		case 36868:
    		case 36869:
    		case 36870:
    		case 36871:
    			chr_reg[address & 7] = (chr_reg[address & 7] & 0xFF00) | data;
    			SetupCHR();
    			break;
    		case 40960:
    		case 40961:
    		case 40962:
    		case 40963:
    		case 40964:
    		case 40965:
    		case 40966:
    		case 40967:
    			chr_reg[address & 7] = (chr_reg[address & 7] & 0xFF) | (data << 8);
    			SetupCHR();
    			break;
    		case 45056:
    		case 45057:
    		case 45058:
    		case 45059:
    			nt_reg[address & 3] = (nt_reg[address & 3] & 0xFF00) | data;
    			break;
    		case 45060:
    		case 45061:
    		case 45062:
    		case 45063:
    			nt_reg[address & 3] = (nt_reg[address & 3] & 0xFF) | (data << 8);
    			break;
    		case 49152:
    			IrqEnable = (data & 1) == 1;
    			if (!IrqEnable)
    			{
    				NesEmu.IRQFlags &= -9;
    			}
    			break;
    		case 49153:
    			irqCountDownMode = (data & 0x80) == 128;
    			irqCountUpMode = (data & 0x40) == 64;
    			irqFunkyMode = (data & 8) == 8;
    			irqPrescalerSize = (data & 4) == 4;
    			irqSource = data & 3;
    			break;
    		case 49154:
    			IrqEnable = false;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 49155:
    			IrqEnable = true;
    			break;
    		case 49156:
    			irqPrescaler = data ^ irqPrescalerXOR;
    			break;
    		case 49157:
    			irqCounter = data ^ irqPrescalerXOR;
    			break;
    		case 49158:
    			irqPrescalerXOR = data;
    			break;
    		case 49159:
    			irqFunkyModeReg = data;
    			break;
    		case 53248:
    			flag_s = (data & 0x80) == 128;
    			prg_mode = data & 7;
    			chr_mode = (data >> 3) & 3;
    			nt_advanced_enable = (data & 0x20) == 32;
    			nt_rom_only = (data & 0x40) == 64;
    			SetupPRG();
    			SetupCHR();
    			break;
    		case 53249:
    			switch (data & 3)
    			{
    			case 0:
    				Switch01KNMTFromMirroring(Mirroring.Vert);
    				break;
    			case 1:
    				Switch01KNMTFromMirroring(Mirroring.Horz);
    				break;
    			case 2:
    				Switch01KNMTFromMirroring(Mirroring.OneScA);
    				break;
    			case 3:
    				Switch01KNMTFromMirroring(Mirroring.OneScB);
    				break;
    			}
    			break;
    		case 53250:
    			nt_ram_select = data & 0x80;
    			break;
    		case 53251:
    			chr_m = (data & 0x80) == 128;
    			chr_block_mode = (data & 0x20) == 32;
    			chr_block = (data & 0x1F) << 8;
    			SetupCHR();
    			break;
    		}
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    	}

    	internal override void ReadSRM(ref ushort address, out byte data)
    	{
    		if (flag_s)
    		{
    			base.ReadSRM(ref address, out data);
    		}
    		else
    		{
    			data = 0;
    		}
    	}

    	internal override void ReadEX(ref ushort address, out byte data)
    	{
    		switch (address)
    		{
    		case 20480:
    			data = Dipswitch;
    			break;
    		case 22528:
    			data = (byte)(multiplication & 0xFFu);
    			break;
    		case 22529:
    			data = (byte)((multiplication & 0xFF00) >> 8);
    			break;
    		case 22531:
    			data = RAM5803;
    			break;
    		default:
    			data = 0;
    			break;
    		}
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 22528:
    			multiplication_a = data;
    			multiplication = (ushort)(multiplication_a * multiplication_b);
    			break;
    		case 22529:
    			multiplication_b = data;
    			multiplication = (ushort)(multiplication_a * multiplication_b);
    			break;
    		case 22531:
    			RAM5803 = data;
    			break;
    		case 22530:
    			break;
    		}
    	}

    	internal override void ReadNMT(ref ushort address, out byte data)
    	{
    		if (MAPPER90MODE)
    		{
    			data = NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF];
    		}
    		if (!nt_advanced_enable)
    		{
    			data = NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF];
    		}
    		else if (nt_rom_only)
    		{
    			data = CHR_ROM[nt_reg[(address >> 10) & 3]][address & 0x3FF];
    		}
    		else if ((nt_reg[(address >> 10) & 3] & 0x80) != nt_ram_select)
    		{
    			data = CHR_ROM[nt_reg[(address >> 10) & 3]][address & 0x3FF];
    		}
    		else
    		{
    			data = NMT_RAM[nt_reg[(address >> 10) & 3] & 1][address & 0x3FF];
    		}
    	}

    	internal override void WriteNMT(ref ushort address, ref byte data)
    	{
    		if (MAPPER90MODE)
    		{
    			NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF] = data;
    		}
    		else if (!nt_advanced_enable)
    		{
    			NMT_RAM[NMT_AREA_BLK_INDEX[(address >> 10) & 3]][address & 0x3FF] = data;
    		}
    		else if (!nt_rom_only && (nt_reg[(address >> 10) & 3] & 0x80) == nt_ram_select)
    		{
    			NMT_RAM[nt_reg[(address >> 10) & 3] & 1][address & 0x3FF] = data;
    		}
    	}

    	private void SetupPRG()
    	{
    		switch (prg_mode)
    		{
    		case 0:
    			Switch08KPRG(prg_reg[3] * 4 + 3, PRGArea.Area6000);
    			Switch32KPRG(PRG_ROM_32KB_Mask, PRGArea.Area8000);
    			break;
    		case 1:
    			Switch08KPRG(prg_reg[3] * 2 + 1, PRGArea.Area6000);
    			Switch16KPRG(prg_reg[1], PRGArea.Area8000);
    			Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    			break;
    		case 2:
    			Switch08KPRG(prg_reg[3], PRGArea.Area6000);
    			Switch08KPRG(prg_reg[0], PRGArea.Area8000);
    			Switch08KPRG(prg_reg[1], PRGArea.AreaA000);
    			Switch08KPRG(prg_reg[2], PRGArea.AreaC000);
    			Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    			break;
    		case 3:
    			Switch08KPRG(ReverseByte(prg_reg[3]), PRGArea.Area6000);
    			Switch08KPRG(ReverseByte(prg_reg[0]), PRGArea.Area8000);
    			Switch08KPRG(ReverseByte(prg_reg[1]), PRGArea.AreaA000);
    			Switch08KPRG(ReverseByte(prg_reg[2]), PRGArea.AreaC000);
    			Switch08KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaE000);
    			break;
    		case 4:
    			Switch08KPRG(prg_reg[3] * 4 + 3, PRGArea.Area6000);
    			Switch32KPRG(prg_reg[3], PRGArea.Area8000);
    			break;
    		case 5:
    			Switch08KPRG(prg_reg[3] * 2 + 1, PRGArea.Area6000);
    			Switch16KPRG(prg_reg[1], PRGArea.Area8000);
    			Switch16KPRG(prg_reg[3], PRGArea.AreaC000);
    			break;
    		case 6:
    			Switch08KPRG(prg_reg[3], PRGArea.Area6000);
    			Switch08KPRG(prg_reg[0], PRGArea.Area8000);
    			Switch08KPRG(prg_reg[1], PRGArea.AreaA000);
    			Switch08KPRG(prg_reg[2], PRGArea.AreaC000);
    			Switch08KPRG(prg_reg[3], PRGArea.AreaE000);
    			break;
    		case 7:
    			Switch08KPRG(ReverseByte(prg_reg[3]), PRGArea.Area6000);
    			Switch08KPRG(ReverseByte(prg_reg[0]), PRGArea.Area8000);
    			Switch08KPRG(ReverseByte(prg_reg[1]), PRGArea.AreaA000);
    			Switch08KPRG(ReverseByte(prg_reg[2]), PRGArea.AreaC000);
    			Switch08KPRG(ReverseByte(prg_reg[3]), PRGArea.AreaE000);
    			break;
    		}
    	}

    	private void SetupCHR()
    	{
    		switch (chr_mode)
    		{
    		case 0:
    			if (chr_block_mode)
    			{
    				Switch08KCHR(chr_reg[0]);
    			}
    			else
    			{
    				Switch08KCHR((chr_reg[0] & 0xFF) | chr_block);
    			}
    			break;
    		case 1:
    			if (chr_block_mode)
    			{
    				Switch04KCHR(chr_reg[0], CHRArea.Area0000);
    				Switch04KCHR(chr_reg[4], CHRArea.Area1000);
    			}
    			else
    			{
    				Switch04KCHR((chr_reg[0] & 0xFF) | chr_block, CHRArea.Area0000);
    				Switch04KCHR((chr_reg[4] & 0xFF) | chr_block, CHRArea.Area1000);
    			}
    			break;
    		case 2:
    			if (chr_block_mode)
    			{
    				Switch02KCHR(chr_reg[0], CHRArea.Area0000);
    				Switch02KCHR(chr_m ? chr_reg[0] : chr_reg[2], CHRArea.Area0800);
    				Switch02KCHR(chr_reg[4], CHRArea.Area1000);
    				Switch02KCHR(chr_reg[6], CHRArea.Area1800);
    			}
    			else
    			{
    				Switch02KCHR((chr_reg[0] & 0xFF) | chr_block, CHRArea.Area0000);
    				Switch02KCHR(((chr_m ? chr_reg[0] : chr_reg[2]) & 0xFF) | chr_block, CHRArea.Area0800);
    				Switch02KCHR((chr_reg[4] & 0xFF) | chr_block, CHRArea.Area1000);
    				Switch02KCHR((chr_reg[6] & 0xFF) | chr_block, CHRArea.Area1800);
    			}
    			break;
    		case 3:
    			if (chr_block_mode)
    			{
    				Switch01KCHR(chr_reg[0], CHRArea.Area0000);
    				Switch01KCHR(chr_reg[1], CHRArea.Area0400);
    				Switch01KCHR(chr_m ? chr_reg[0] : chr_reg[2], CHRArea.Area0800);
    				Switch01KCHR(chr_m ? chr_reg[1] : chr_reg[3], CHRArea.Area0C00);
    				Switch01KCHR(chr_reg[4], CHRArea.Area1000);
    				Switch01KCHR(chr_reg[5], CHRArea.Area1400);
    				Switch01KCHR(chr_reg[6], CHRArea.Area1800);
    				Switch01KCHR(chr_reg[7], CHRArea.Area1C00);
    			}
    			else
    			{
    				Switch01KCHR((chr_reg[0] & 0xFF) | chr_block, CHRArea.Area0000);
    				Switch01KCHR((chr_reg[1] & 0xFF) | chr_block, CHRArea.Area0400);
    				Switch01KCHR(((chr_m ? chr_reg[0] : chr_reg[2]) & 0xFF) | chr_block, CHRArea.Area0800);
    				Switch01KCHR(((chr_m ? chr_reg[1] : chr_reg[3]) & 0xFF) | chr_block, CHRArea.Area0C00);
    				Switch01KCHR((chr_reg[4] & 0xFF) | chr_block, CHRArea.Area1000);
    				Switch01KCHR((chr_reg[5] & 0xFF) | chr_block, CHRArea.Area1400);
    				Switch01KCHR((chr_reg[6] & 0xFF) | chr_block, CHRArea.Area1800);
    				Switch01KCHR((chr_reg[7] & 0xFF) | chr_block, CHRArea.Area1C00);
    			}
    			break;
    		}
    	}

    	private byte ReverseByte(int value)
    	{
    		return (byte)((uint)(((value & 0x40) >> 6) | ((value & 0x20) >> 4) | ((value & 0x10) >> 2)) | ((uint)value & 8u) | (uint)((value & 4) << 2) | (uint)((value & 2) << 4) | (uint)((value & 1) << 6));
    	}

    	internal override void OnCPUClock()
    	{
    		if (irqSource != 0)
    		{
    			return;
    		}
    		if (irqPrescalerSize)
    		{
    			irqPrescaler = (irqPrescaler & 0xF8) | (((irqPrescaler & 7) + 1) & 7);
    			if ((irqPrescaler & 7) == 7)
    			{
    				ClockIRQCounter();
    			}
    		}
    		else
    		{
    			irqPrescaler++;
    			if (irqPrescaler == 255)
    			{
    				ClockIRQCounter();
    			}
    		}
    	}

    	internal override void OnPPUAddressUpdate(ref ushort address)
    	{
    		if (irqSource != 1)
    		{
    			return;
    		}
    		old_vram_address = new_vram_address;
    		new_vram_address = address & 0x1000;
    		if (old_vram_address >= new_vram_address)
    		{
    			return;
    		}
    		if (irqPrescalerSize)
    		{
    			irqPrescaler = (irqPrescaler & 0xF8) | (((irqPrescaler & 7) + 1) & 7);
    			if ((irqPrescaler & 7) == 7)
    			{
    				ClockIRQCounter();
    			}
    		}
    		else
    		{
    			irqPrescaler++;
    			if (irqPrescaler == 255)
    			{
    				ClockIRQCounter();
    			}
    		}
    	}

    	private void ClockIRQCounter()
    	{
    		if (irqCountDownMode && irqCountUpMode)
    		{
    			return;
    		}
    		if (irqCountDownMode)
    		{
    			irqCounter--;
    			if (irqCounter == 0)
    			{
    				irqCounter = 255;
    				if (IrqEnable)
    				{
    					NesEmu.IRQFlags |= 8;
    				}
    			}
    		}
    		else
    		{
    			if (!irqCountUpMode)
    			{
    				return;
    			}
    			irqCounter++;
    			if (irqCounter == 255)
    			{
    				irqCounter = 0;
    				if (IrqEnable)
    				{
    					NesEmu.IRQFlags |= 8;
    				}
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		for (int i = 0; i < prg_reg.Length; i++)
    		{
    			stream.Write(prg_reg[i]);
    		}
    		for (int j = 0; j < chr_reg.Length; j++)
    		{
    			stream.Write(chr_reg[j]);
    		}
    		for (int k = 0; k < nt_reg.Length; k++)
    		{
    			stream.Write(nt_reg[k]);
    		}
    		stream.Write(prg_mode);
    		stream.Write(chr_mode);
    		stream.Write(chr_block_mode);
    		stream.Write(chr_block);
    		stream.Write(chr_m);
    		stream.Write(flag_s);
    		stream.Write(irqCounter);
    		stream.Write(IrqEnable);
    		stream.Write(irqCountDownMode);
    		stream.Write(irqCountUpMode);
    		stream.Write(irqFunkyMode);
    		stream.Write(irqPrescalerSize);
    		stream.Write(irqSource);
    		stream.Write(irqPrescaler);
    		stream.Write(irqPrescalerXOR);
    		stream.Write(irqFunkyModeReg);
    		stream.Write(Dipswitch);
    		stream.Write(multiplication_a);
    		stream.Write(multiplication_b);
    		stream.Write(multiplication);
    		stream.Write(RAM5803);
    		stream.Write(nt_advanced_enable);
    		stream.Write(nt_rom_only);
    		stream.Write(nt_ram_select);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		for (int i = 0; i < prg_reg.Length; i++)
    		{
    			prg_reg[i] = stream.ReadInt32();
    		}
    		for (int j = 0; j < chr_reg.Length; j++)
    		{
    			chr_reg[j] = stream.ReadInt32();
    		}
    		for (int k = 0; k < nt_reg.Length; k++)
    		{
    			nt_reg[k] = stream.ReadInt32();
    		}
    		prg_mode = stream.ReadInt32();
    		chr_mode = stream.ReadInt32();
    		chr_block_mode = stream.ReadBoolean();
    		chr_block = stream.ReadInt32();
    		chr_m = stream.ReadBoolean();
    		flag_s = stream.ReadBoolean();
    		irqCounter = stream.ReadInt32();
    		IrqEnable = stream.ReadBoolean();
    		irqCountDownMode = stream.ReadBoolean();
    		irqCountUpMode = stream.ReadBoolean();
    		irqFunkyMode = stream.ReadBoolean();
    		irqPrescalerSize = stream.ReadBoolean();
    		irqSource = stream.ReadInt32();
    		irqPrescaler = stream.ReadInt32();
    		irqPrescalerXOR = stream.ReadInt32();
    		irqFunkyModeReg = stream.ReadByte();
    		Dipswitch = stream.ReadByte();
    		multiplication_a = stream.ReadByte();
    		multiplication_b = stream.ReadByte();
    		multiplication = stream.ReadUInt16();
    		RAM5803 = stream.ReadByte();
    		nt_advanced_enable = stream.ReadBoolean();
    		nt_rom_only = stream.ReadBoolean();
    		nt_ram_select = stream.ReadInt32();
    	}
    }
}
