namespace MyNes.Core
{
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
