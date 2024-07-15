using System.IO;

namespace MyNes.Core
{
    [BoardInfo("NES-EVENT", 105)]
    [HassIssues]
    internal class Mapper105 : Board
    {
    	private int DipSwitchNumber;

    	private byte[] reg = new byte[4];

    	private byte shift;

    	private byte buffer;

    	private bool flag_p;

    	private bool flag_s;

    	private bool flag_o;

    	private int reg_a;

    	private int reg_b;

    	private bool irq_control;

    	private bool initialized;

    	private int irq_counter;

    	private int dipswitches;

    	internal override string Issues => MNInterfaceLanguage.IssueMapper105;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		TogglePRGRAMEnable(enable: true);
    		reg = new byte[4];
    		reg[0] = 12;
    		flag_s = (flag_p = true);
    		reg[1] = (reg[2] = (reg[3] = 0));
    		buffer = 0;
    		shift = 0;
    		initialized = false;
    		DipSwitchNumber = 0;
    		dipswitches = 0x20000000 | (DipSwitchNumber << 22);
    	}

    	internal override void SoftReset()
    	{
    		DipSwitchNumber = (DipSwitchNumber + 1) & 0xF;
    		dipswitches = 0x20000000 | (DipSwitchNumber << 22);
    	}

    	internal override void WritePRG(ref ushort address, ref byte value)
    	{
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
    		address = (ushort)((address & 0x7FFF) >> 13);
    		reg[address] = buffer;
    		shift = (buffer = 0);
    		switch (address)
    		{
    		case 0:
    			flag_p = (reg[0] & 8) != 0;
    			flag_s = (reg[0] & 4) != 0;
    			UpdatePRG();
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
    			irq_control = (reg[1] & 0x10) == 16;
    			if (irq_control)
    			{
    				initialized = true;
    				irq_counter = 0;
    				NesEmu.IRQFlags &= -9;
    			}
    			else
    			{
    				Switch32KPRG(0, PRGArea.Area8000);
    			}
    			flag_o = (reg[1] & 8) == 8;
    			reg_a = (reg[1] >> 1) & 3;
    			UpdatePRG();
    			break;
    		case 3:
    			TogglePRGRAMEnable((reg[3] & 0x10) == 0);
    			reg_b = reg[3] & 0xF;
    			UpdatePRG();
    			break;
    		case 2:
    			break;
    		}
    	}

    	private void UpdatePRG()
    	{
    		if (initialized)
    		{
    			if (!flag_o)
    			{
    				Switch32KPRG(reg_a, PRGArea.Area8000);
    			}
    			else if (!flag_p)
    			{
    				Switch32KPRG((reg_b >> 1) + 4, PRGArea.Area8000);
    			}
    			else if (!flag_s)
    			{
    				Switch16KPRG(8, PRGArea.Area8000);
    				Switch16KPRG(reg_b + 8, PRGArea.AreaC000);
    			}
    			else
    			{
    				Switch16KPRG(reg_b + 8, PRGArea.Area8000);
    				Switch16KPRG(15, PRGArea.AreaC000);
    			}
    		}
    	}

    	internal override void OnCPUClock()
    	{
    		if (!irq_control)
    		{
    			irq_counter++;
    			if (irq_counter == dipswitches)
    			{
    				irq_counter = 0;
    				NesEmu.IRQFlags |= 8;
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(DipSwitchNumber);
    		stream.Write(reg);
    		stream.Write(shift);
    		stream.Write(buffer);
    		stream.Write(flag_p);
    		stream.Write(flag_s);
    		stream.Write(flag_o);
    		stream.Write(reg_a);
    		stream.Write(reg_b);
    		stream.Write(irq_control);
    		stream.Write(initialized);
    		stream.Write(irq_counter);
    		stream.Write(dipswitches);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		DipSwitchNumber = stream.ReadInt32();
    		stream.Read(reg, 0, reg.Length);
    		shift = stream.ReadByte();
    		buffer = stream.ReadByte();
    		flag_p = stream.ReadBoolean();
    		flag_s = stream.ReadBoolean();
    		flag_o = stream.ReadBoolean();
    		reg_a = stream.ReadInt32();
    		reg_b = stream.ReadInt32();
    		irq_control = stream.ReadBoolean();
    		initialized = stream.ReadBoolean();
    		irq_counter = stream.ReadInt32();
    		dipswitches = stream.ReadInt32();
    	}
    }
}
