using MAME.Core;
using System;

namespace MAME.Core
{
    public static class EmuLogger
    {

        #region 抽象出去
        static Action<string> Act_Log;

        public static void BindFunc(ILog ilog)
        {
            Act_Log -= Act_Log;

            Act_Log += ilog.Log;
        }

        public static void Log(string msg)
        {
            Act_Log?.Invoke(msg);
        }


        public static void Assert(bool conditional, string msg)
        {
            if (conditional)
                return;
            Act_Log?.Invoke(msg);
        }
        #endregion
    }
}
