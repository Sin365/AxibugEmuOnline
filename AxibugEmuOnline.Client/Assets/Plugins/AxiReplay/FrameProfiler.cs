using System;
using System.Diagnostics;

namespace AxiReplay
{
    public partial class FrameProfiler
    {
        private int m_headFrame;
        private int m_cacheCount;
        private int m_targetFrameRate;
        private RingBuffer<double> m_timePoints;
        private double m_lastTime;

        private Stopwatch sw;

        public void InputHead(int headFrame)
        {
            m_headFrame = headFrame;
            var currentTimeMs = GetCurrTime();

            if (m_timePoints.Available() == 60)
                CalcCacheCount();
            m_timePoints.Write(currentTimeMs - m_lastTime);

            m_lastTime = currentTimeMs;
        }
        public void Reset(int targetFrameRate = 60)
        {
            if (sw != null) sw.Stop();

            sw = Stopwatch.StartNew();
            m_timePoints = new RingBuffer<double>(targetFrameRate);
            m_lastTime = 0;
            m_targetFrameRate = targetFrameRate;
        }

        /// <summary>
        /// 临时方法(暂行）
        /// </summary>
        /// <param name="mRemoteForwardCount"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int TempFrameCount(int mRemoteForwardCount)
        {
            if (mRemoteForwardCount == 0)
                return 0;
            if (mRemoteForwardCount < 5)
                return 1;
            else
                return (int)Math.Ceiling(mRemoteForwardCount / 5f);
        }

        void CalcCacheCount()
        {
            double deltaMax = 0;
            double delta;
            while (m_timePoints.TryRead(out delta))
            {
                deltaMax = Math.Max(deltaMax, delta);
            }

            int minCacheCount = (int)Math.Ceiling(deltaMax * m_targetFrameRate);
            m_cacheCount = minCacheCount;
        }

        double GetCurrTime()
        {
            if (sw == null) return 0;
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}