namespace MyNes.Core
{
    internal class SoundDCBlockerFilter
    {
    	private double R;

    	private double y;

    	private double y_1;

    	private double x;

    	private double x_1;

    	public SoundDCBlockerFilter(double R)
    	{
    		this.R = R;
    	}

    	public void Reset()
    	{
    		y = (y_1 = (x = (x_1 = 0.0)));
    	}

    	public void DoFiltering(double sample, out double filtered)
    	{
    		x = sample;
    		filtered = (y = x - x_1 + R * y_1);
    		x_1 = x;
    		y_1 = y;
    	}
    }
}
