namespace MyNes.Core
{
    [BoardInfo("FFE F8xxx", 17)]
    internal class Mapper017 : FFE
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Switch16KPRG(PRG_ROM_16KB_Mask, PRGArea.AreaC000);
    	}

    	internal override void WriteEX(ref ushort address, ref byte data)
    	{
    		switch (address)
    		{
    		case 17668:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		case 17669:
    			Switch08KPRG(data, PRGArea.AreaA000);
    			break;
    		case 17670:
    			Switch08KPRG(data, PRGArea.AreaC000);
    			break;
    		case 17671:
    			Switch08KPRG(data, PRGArea.AreaE000);
    			break;
    		case 17680:
    			Switch01KCHR(data, CHRArea.Area0000);
    			break;
    		case 17681:
    			Switch01KCHR(data, CHRArea.Area0400);
    			break;
    		case 17682:
    			Switch01KCHR(data, CHRArea.Area0800);
    			break;
    		case 17683:
    			Switch01KCHR(data, CHRArea.Area0C00);
    			break;
    		case 17684:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 17685:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 17686:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 17687:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		case 17672:
    		case 17673:
    		case 17674:
    		case 17675:
    		case 17676:
    		case 17677:
    		case 17678:
    		case 17679:
    			break;
    		}
    	}
    }
}
