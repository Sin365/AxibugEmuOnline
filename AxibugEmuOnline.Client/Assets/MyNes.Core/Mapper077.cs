namespace MyNes.Core
{
    [BoardInfo("Irem", 77)]
    internal class Mapper077 : Board
    {
    	internal override void HardReset()
    	{
    		base.HardReset();
    		Toggle02KCHR_RAM(ram: true, CHRArea.Area0800);
    		Switch02KCHR(0, CHRArea.Area0800);
    		Toggle02KCHR_RAM(ram: true, CHRArea.Area1000);
    		Switch02KCHR(1, CHRArea.Area1000);
    		Toggle02KCHR_RAM(ram: true, CHRArea.Area1800);
    		Switch02KCHR(2, CHRArea.Area1800);
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		Switch02KCHR((data >> 4) & 0xF, CHRArea.Area0000);
    		Switch32KPRG(data & 0xF, PRGArea.Area8000);
    	}
    }
}
