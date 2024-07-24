namespace MyNes.Core
{
    [BoardInfo("Unknown", 163)]
    internal class Mapper163 : Board
    {
    	internal byte prg_reg;

    	internal byte security_reg;

    	internal bool security_trigger;

    	internal byte security_reg101;

    	internal bool do_chr_ram;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		security_reg101 = 1;
    		security_trigger = false;
    		do_chr_ram = true;
    		Switch32KPRG(15, PRGArea.Area8000);
    		Toggle08KCHR_RAM(ram: true);
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		if (address == 20737)
    		{
    			if (security_reg101 != 0 && data == 0)
    			{
    				security_trigger = true;
    			}
    			security_reg101 = data;
    		}
    		else
    		{
    			if (address < 20480)
    			{
    				return;
    			}
    			switch (address & 0x7300)
    			{
    			case 20480:
    				prg_reg = (byte)((prg_reg & 0xF0u) | (data & 0xFu));
    				Switch32KPRG(prg_reg, PRGArea.Area8000);
    				do_chr_ram = (data & 0x80) != 0;
    				if (!do_chr_ram && NesEmu.ppu_clock_v < 128)
    				{
    					Switch08KCHR(0);
    				}
    				break;
    			case 20736:
    				if (data == 6)
    				{
    					Switch32KPRG(3, PRGArea.Area8000);
    				}
    				break;
    			case 20992:
    				prg_reg = (byte)((prg_reg & 0xFu) | (uint)((data & 0xF) << 4));
    				Switch32KPRG(prg_reg, PRGArea.Area8000);
    				break;
    			case 21248:
    				security_reg = data;
    				break;
    			}
    		}
    	}

    	internal override void ReadEX(ref ushort addr, out byte val)
    	{
    		val = 0;
    		if (addr < 20480)
    		{
    			return;
    		}
    		switch (addr & 0x1E14)
    		{
    		case 20736:
    			val = security_reg;
    			break;
    		case 21760:
    			if (security_trigger)
    			{
    				val = security_reg;
    			}
    			break;
    		}
    	}

    	internal override void OnPPUScanlineTick()
    	{
    		base.OnPPUScanlineTick();
    		if (do_chr_ram && NesEmu.IsRenderingOn())
    		{
    			if (NesEmu.ppu_clock_v == 127)
    			{
    				Switch04KCHR(1, CHRArea.Area0000);
    				Switch04KCHR(1, CHRArea.Area1000);
    			}
    			if (NesEmu.ppu_clock_v == 237)
    			{
    				Switch04KCHR(0, CHRArea.Area0000);
    				Switch04KCHR(0, CHRArea.Area1000);
    			}
    		}
    	}
    }
}
