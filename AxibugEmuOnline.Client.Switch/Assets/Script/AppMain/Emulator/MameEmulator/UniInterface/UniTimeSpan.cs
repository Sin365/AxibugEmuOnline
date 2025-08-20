using MAME.Core;
using System.Diagnostics;

public class UniTimeSpan : ITimeSpan
{
    public ulong tick;
    double startUs;
    double tickDetailus = 16666.666667;
    //double tickDetailus = 16.666667;//16微秒 （越小越决定tick精度越小）
    object tickLock = new object();

    public void InitStandTime()
    {
        startUs = GetCurrUs();
    }

    /// <summary>
    /// 获取当前毫秒
    /// </summary>
    /// <returns></returns>
    public double GetCurrUs()
    {
        return (double)UMAME.sw.ElapsedTicks * 1000000 / Stopwatch.Frequency;
    }

    /// <summary>
    /// 获取当前毫秒
    /// </summary>
    /// <returns></returns>
    public double GetRunUs()
    {
        return ((double)UMAME.sw.ElapsedTicks * 1000000 / Stopwatch.Frequency) - startUs;
    }

    public void SetTick(ulong nexttick)
    {
        //lock (tickLock)
        {
            tick = nexttick;
        }
    }

    //这个函数无意义
    public uint GetTickCount()
    {
        //lock (tickLock)
        {
            //return (uint)(tick * tickDetail);
            return 0;
        }
    }

    public bool QueryPerformanceCounter(ref long lpPerformanceCount)
    {
        lock (tickLock)
        {
            lpPerformanceCount = (long)tick;
            //lpPerformanceCount = (long)(GetRunUs() / tickDetailus);
            return true;
        }
    }

    public bool QueryPerformanceFrequency(ref long PerformanceFrequency)
    {
        PerformanceFrequency = (long)(1000000 / tickDetailus);
        return true;
    }
}
