using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class IJoypadConnecter
    {
    	protected byte DATA;

    	public abstract void Update();

    	public virtual void Destroy()
    	{
    	}

    	public virtual byte GetData()
    	{
    		return DATA;
    	}
    }
}
