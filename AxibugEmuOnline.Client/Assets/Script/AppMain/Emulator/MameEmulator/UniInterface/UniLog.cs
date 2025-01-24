using AxibugEmuOnline.Client.ClientCore;
using MAME.Core;

public class UniLog : ILog
{
    public void Log(string msg)
    {
        App.log.Debug(msg);
    }
}
