using System.Diagnostics;

namespace AxibugEmuOnline.Server.Manager
{
    public class TickManager
    {
        public Stopwatch sw;
        public enum TickType
        {
            Interval_16MS,
            Interval_32MS,
            Interval_2000MS,
        }

        System.Timers.Timer mTimer16ms;
        System.Timers.Timer mTimer32ms;
        System.Timers.Timer mTimer2000ms;
        List<AutoResetEvent> mAREList16ms;
        List<AutoResetEvent> mAREList32ms;
        List<AutoResetEvent> mAREList2000ms;

        public TickManager()
        {
            sw = Stopwatch.StartNew();
            mAREList16ms = new List<AutoResetEvent>();
            mAREList32ms = new List<AutoResetEvent>();
            mAREList2000ms = new List<AutoResetEvent>();

            mTimer16ms = new System.Timers.Timer(16);//实例化Timer类，设置间隔时间为10000毫秒；
            mTimer16ms.Elapsed += new System.Timers.ElapsedEventHandler((source, e) => { UpdateARE(mAREList16ms); });//到达时间的时候执行事件；
            mTimer16ms.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            mTimer16ms.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            mTimer16ms.Start();

            mTimer32ms = new System.Timers.Timer(32);//实例化Timer类，设置间隔时间为10000毫秒；
            mTimer32ms.Elapsed += new System.Timers.ElapsedEventHandler((source, e) => { UpdateARE(mAREList32ms); });//到达时间的时候执行事件；
            mTimer32ms.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            mTimer32ms.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            mTimer32ms.Start();

            mTimer2000ms = new System.Timers.Timer(2000);//实例化Timer类，设置间隔时间为10000毫秒；
            mTimer2000ms.Elapsed += new System.Timers.ElapsedEventHandler((source, e) => { UpdateARE(mAREList2000ms); });//到达时间的时候执行事件；
            mTimer2000ms.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            mTimer2000ms.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            mTimer2000ms.Start();
        }

        public AutoResetEvent AddNewARE(TickType tikeType)
        {
            AutoResetEvent are = new AutoResetEvent(false);
            switch (tikeType)
            {
                case TickType.Interval_16MS:
                    mAREList16ms.Add(are);
                    break;
                case TickType.Interval_32MS:
                    mAREList32ms.Add(are);
                    break;
                case TickType.Interval_2000MS:
                    mAREList2000ms.Add(are);
                    break;
            }
            return are;
        }

        public void UpdateARE(List<AutoResetEvent> are)
        {
            for (int i = 0; i < are.Count; i++)
                are[i].Set();
        }
    }
}