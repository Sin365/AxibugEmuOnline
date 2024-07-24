namespace MyNes.Core
{
    public abstract class IZapperConnecter
    {
    	protected bool Trigger;

    	protected bool State;

    	public abstract void Update();

    	public virtual byte GetData()
    	{
    		return (byte)((Trigger ? 16u : 0u) | (State ? 8u : 0u));
    	}
    }
}
