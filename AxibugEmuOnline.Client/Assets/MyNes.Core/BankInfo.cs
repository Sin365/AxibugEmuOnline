namespace MyNes.Core
{
    internal struct BankInfo
    {
    	public bool IsRAM;

    	public bool Enabled;

    	public bool Writable;

    	public bool IsBattery;

    	public string ID;

    	public byte[] DATA;

    	public BankInfo(string ID, bool IsRAM, bool Writable, bool Enabled, bool IsBattery, byte[] DATA)
    	{
    		this.ID = ID;
    		this.IsRAM = IsRAM;
    		this.Writable = Writable;
    		this.Enabled = Enabled;
    		this.DATA = DATA;
    		this.IsBattery = IsBattery;
    	}
    }
}
