namespace MyNes.Core
{
    public abstract class IRom
    {
    	public bool IsValid { get; set; }

    	public int PRGCount { get; set; }

    	public int CHRCount { get; set; }

    	public int MapperNumber { get; set; }

    	public Mirroring Mirroring { get; set; }

    	public bool HasTrainer { get; set; }

    	public byte[] PRG { get; set; }

    	public byte[] CHR { get; set; }

    	public byte[] Trainer { get; set; }

    	public string SHA1 { get; set; }

    	public virtual void Load(string fileName, bool loadDumps)
    	{
    	}
    }
}
