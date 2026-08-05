using AxibugEmuOnline.Client.ClientCore;
using MAME.Core;

public class UniLog : ILog
{
    public void Log(string msg, MAME_LOG_LEVEL level)
    {
        switch (level)
        {
            case MAME_LOG_LEVEL.DEBUG:
                App.log.Debug(msg);
                return;
            case MAME_LOG_LEVEL.INFO:
                App.log.Info(msg);
                break;
            case MAME_LOG_LEVEL.WARN:
                App.log.Warning(msg);
                break;
            case MAME_LOG_LEVEL.ERR:
                App.log.Error(msg);
                return;
        }
    }
}
