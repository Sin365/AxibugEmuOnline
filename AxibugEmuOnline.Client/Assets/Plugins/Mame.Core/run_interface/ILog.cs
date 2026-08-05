namespace MAME.Core
{
    public enum MAME_LOG_LEVEL
    {
        DEBUG,
        INFO,
        WARN,
        ERR,
    }
    public interface ILog
    {
        void Log(string msg, MAME_LOG_LEVEL MAMELOGLEVEL);
    }
}
