using StoicGoose.Common.Utilities;
using System;

public class SGLogger : IStoicGooseLogger
{
    public void Debug(string message)
    {
        UnityEngine.Debug.Log(message);
    }

    public void Err(string message)
    {
        UnityEngine.Debug.LogError(message);
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
        UnityEngine.Debug.LogWarning(message);
    }
}
