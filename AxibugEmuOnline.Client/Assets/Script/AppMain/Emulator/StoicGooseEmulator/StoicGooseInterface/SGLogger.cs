using AxibugEmuOnline.Client.ClientCore;
using StoicGoose.Common.Utilities;
using System;

public class SGLogger : IStoicGooseLogger
{
    public void Debug(string message)
    {
        App.log.Debug(message);
    }

    public void Err(string message)
    {
        App.log.Error(message);
    }

    public void Log(StoicGoose.Common.Utilities.LogType logtype, string message)
    {
        switch (logtype)
        {
            case LogType.Debug:
                Debug(message);
                break;
            case LogType.Warning:
                Warning(message);
                break;
            case LogType.Error:
                Err(message);
                break;
        }
    }

    public void Warning(string message)
    {
        App.log.Warning(message);
    }
}
