namespace MyNes.Core
{
    public abstract class IVSUnisystemDIPConnecter
    {
    	public abstract void Update();

    	public virtual void OnEmuShutdown()
    	{
    	}

    	public virtual byte GetData4016()
    	{
    		return 0;
    	}

    	public virtual byte GetData4017()
    	{
    		return 0;
    	}

    	public virtual void Write4020(ref byte data)
    	{
    	}
    }
}
