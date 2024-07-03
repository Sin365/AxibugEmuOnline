namespace MyNes.Core
{
    [BoardInfo("Unknown", 193, 1, 32)]
    [HassIssues]
    internal class Mapper193 : Board
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper193;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch08KPRG(PRG_ROM_08KB_Mask - 2, PRGArea.AreaA000);
    		Switch08KPRG(PRG_ROM_08KB_Mask - 1, PRGArea.AreaC000);
    		Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
    	}

    	internal override void WriteSRM(ref ushort address, ref byte data)
    	{
    		switch (address & 0x6003)
    		{
    		case 24576:
    			Switch04KCHR(data >> 2, CHRArea.Area0000);
    			break;
    		case 24577:
    			Switch02KCHR(data >> 1, CHRArea.Area1000);
    			break;
    		case 24578:
    			Switch02KCHR(data >> 1, CHRArea.Area1800);
    			break;
    		case 24579:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		}
    	}
    }
}
