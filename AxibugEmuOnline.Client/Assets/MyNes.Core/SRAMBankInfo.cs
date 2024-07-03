namespace MyNes.Core
{
    public struct SRAMBankInfo
    {
    	public int id;

    	public string SIZE;

    	public bool BATTERY;

    	public SRAMBankInfo(int id, string SIZE, bool BATTERY)
    	{
    		this.SIZE = SIZE;
    		if (SIZE == "0kb")
    		{
    			SIZE = "8kb";
    		}
    		this.BATTERY = BATTERY;
    		this.id = id;
    	}
    }
}
