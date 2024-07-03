namespace MyNes.Core
{
    public class BoardInfoObject
    {
    	public string Name { get; internal set; }

    	public int MapperNumber { get; internal set; }

    	public bool IsSupported { get; internal set; }

    	public string Issues { get; internal set; }

    	public bool HasIssues { get; internal set; }
    }
}
