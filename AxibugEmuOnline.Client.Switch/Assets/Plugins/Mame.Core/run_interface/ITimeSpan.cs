namespace MAME.Core
{
    public interface ITimeSpan
    {
        /// <summary>
        /// 启动以来的毫秒数
        /// </summary>
        /// <returns></returns>
        uint GetTickCount();

        /// <summary>
        /// 计数器周期
        /// </summary>
        /// <param name="lpPerformanceCount"></param>
        /// <returns></returns>
        bool QueryPerformanceCounter(ref long lpPerformanceCount);

        /// <summary>
        /// 计数器间隔(Hz)
        /// </summary>
        /// <param name="PerformanceFrequency"></param>
        /// <returns></returns>
        bool QueryPerformanceFrequency(ref long PerformanceFrequency);
    }
}
