using System;

namespace MyNes.Core
{
    internal class SoundHighPassFilter
    {
    	private double K;

    	private double y_1;

    	private double x_1;

    	public SoundHighPassFilter(double k)
    	{
    		K = k;
    	}

    	public void Reset()
    	{
    		y_1 = (x_1 = 0.0);
    	}

    	public void DoFiltering(double sample, out double filtered)
    	{
    		filtered = K * y_1 + K * (sample - x_1);
    		x_1 = sample;
    		y_1 = filtered;
    	}

    	public static double GetK(double dt, double fc)
    	{
    		double num = Math.PI * 2.0 * dt * fc;
    		return num / (num + 1.0);
    	}
    }
}
