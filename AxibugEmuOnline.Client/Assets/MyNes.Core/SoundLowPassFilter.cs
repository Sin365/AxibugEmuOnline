using System;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal class SoundLowPassFilter
    {
    	private double K;

    	private double K_1;

    	private double y;

    	private double y_1;

    	private double x;

    	private double x_1;

    	public SoundLowPassFilter(double k)
    	{
    		K = k;
    		K_1 = 1.0 - k;
    	}

    	public void Reset(double k)
    	{
    		y = (y_1 = (x = (x_1 = 0.0)));
    		K = k;
    		K_1 = 1.0 - k;
    	}

    	public void DoFiltering(double sample, out double filtered)
    	{
    		filtered = K * sample + K_1 * y_1;
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
