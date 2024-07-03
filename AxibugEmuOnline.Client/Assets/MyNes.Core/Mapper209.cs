namespace MyNes.Core
{
    [BoardInfo("Pirate MMC5-style", 209)]
    internal class Mapper209 : Mapper090
    {
    	internal override void HardReset()
    	{
    		MAPPER90MODE = false;
    		base.HardReset();
    	}
    }
}
