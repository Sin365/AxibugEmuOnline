using System;

namespace MAME.Core
{
    public static class EmuLogger
    {

        #region 抽象出去
        static Action<string, MAME_LOG_LEVEL> Act_Log;

        public static void BindFunc(ILog ilog)
        {
            Act_Log -= Act_Log;

            Act_Log += ilog.Log;
        }

        public static void Debug(string msg)
        {
            Act_Log?.Invoke(msg,MAME_LOG_LEVEL.DEBUG);
        }

        public static void Info(string msg)
        {
            Act_Log?.Invoke(msg, MAME_LOG_LEVEL.INFO);
        }

        public static void Warn(string msg)
        {
            Act_Log?.Invoke(msg, MAME_LOG_LEVEL.WARN);
        }

        public static void Err(string msg)
        {
            Act_Log?.Invoke(msg, MAME_LOG_LEVEL.ERR);
        }

        public static void Assert(bool conditional, string msg)
        {
            if (conditional)
                return;
            Act_Log?.Invoke(msg,MAME_LOG_LEVEL.DEBUG);
        }
        #endregion
    }
}
