using MAME.Core;

public class UniTimeSpan : ITimeSpan
{
    public ulong tick;
    double tickDetailus = 16666.666667;
    object tickLock = new object();

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
            return true;
        }
    }

    public bool QueryPerformanceFrequency(ref long PerformanceFrequency)
    {
        PerformanceFrequency = (long)(1000000 / tickDetailus);
        return true;
    }
}
