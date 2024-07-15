namespace MyNes.Core
{
    [BoardInfo("Unknown", 222)]
    internal class Mapper222 : Board
    {
    	internal override string Issues => MNInterfaceLanguage.IssueMapper222;

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		switch (address & 0xF003)
    		{
    		case 32768:
    			Switch08KPRG(data, PRGArea.Area8000);
    			break;
    		case 36864:
    			Switch01KNMTFromMirroring(((data & 1) == 1) ? Mirroring.Horz : Mirroring.Vert);
    			break;
    		case 40960:
    			Switch08KPRG(data, PRGArea.AreaA000);
    			break;
    		case 45056:
    			Switch01KCHR(data, CHRArea.Area0000);
    			break;
    		case 45058:
    			Switch01KCHR(data, CHRArea.Area0400);
    			break;
    		case 49152:
    			Switch01KCHR(data, CHRArea.Area0800);
    			break;
    		case 49154:
    			Switch01KCHR(data, CHRArea.Area0C00);
    			break;
    		case 53248:
    			Switch01KCHR(data, CHRArea.Area1000);
    			break;
    		case 53250:
    			Switch01KCHR(data, CHRArea.Area1400);
    			break;
    		case 57344:
    			Switch01KCHR(data, CHRArea.Area1800);
    			break;
    		case 57346:
    			Switch01KCHR(data, CHRArea.Area1C00);
    			break;
    		}
    	}
    }
}
