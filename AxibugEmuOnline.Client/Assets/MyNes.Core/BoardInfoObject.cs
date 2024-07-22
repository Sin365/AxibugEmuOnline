using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class BoardInfoObject
    {
    	public string Name { get; internal set; }

    	public int MapperNumber { get; internal set; }

    	public bool IsSupported { get; internal set; }

    	public string Issues { get; internal set; }

    	public bool HasIssues { get; internal set; }
    }
}
