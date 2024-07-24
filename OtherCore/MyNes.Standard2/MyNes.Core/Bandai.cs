using System.IO;

namespace MyNes.Core
{
    internal abstract class Bandai : Board
    {
    	private bool irq_enable;

    	private int irq_counter;

    	private Eprom eprom;

    	internal override void Initialize(IRom rom)
    	{
    		base.Initialize(rom);
    		if (base.BoardType.ToLower().Contains("24c01"))
    		{
    			eprom = new Eprom(128);
    		}
    		else
    		{
    			eprom = new Eprom((base.MapperNumber == 16) ? 256 : 128);
    		}
    	}

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    		irq_enable = false;
    		irq_counter = 0;
    		eprom.HardReset();
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		WritePRG(ref address, ref data);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF)
    		{
    		case 0:
    			Switch01KCHR(data, CHRArea.Area0000);
    			break;
    		case 1:
    			Switch01KCHR(data, CHRArea.Area0400);
    			break;
    		case 2:
    			Switch01KCHR(data, CHRArea.Area0800);
    			break;
    		case 3:
    			Switch01KCHR(data, CHRArea.Area0C00);
    			break;
    		case 4:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 5:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 6:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 7:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		case 8:
    			Switch16KPRG(data, PRGArea.Area8000);
    			break;
    		case 9:
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
    		case 10:
    			irq_enable = (data & 1) == 1;
    			NesEmu.IRQFlags &= -9;
    			break;
    		case 11:
    			irq_counter = (irq_counter & 0xFF00) | data;
    			break;
    		case 12:
    			irq_counter = (irq_counter & 0xFF) | (data << 8);
    			break;
    		case 13:
    			eprom.Write(address, data);
    			break;
    		}
    	}

    	internal override void ReadSRM(ref ushort address, out byte value)
    	{
    		value = eprom.Read(address);
    	}

    	internal override void OnCPUClock()
    	{
    		if (irq_enable)
    		{
    			irq_counter--;
    			if (irq_counter == 0)
    			{
    				NesEmu.IRQFlags |= 8;
    			}
    			if (irq_counter < 0)
    			{
    				irq_counter = 65535;
    			}
    		}
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(irq_enable);
    		stream.Write(irq_counter);
    		eprom.SaveState(stream);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		irq_enable = stream.ReadBoolean();
    		irq_counter = stream.ReadInt32();
    		eprom.LoadState(stream);
    	}
    }
}
