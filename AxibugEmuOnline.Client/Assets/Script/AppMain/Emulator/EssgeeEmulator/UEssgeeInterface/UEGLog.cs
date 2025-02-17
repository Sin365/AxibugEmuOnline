using AxibugEmuOnline.Client.ClientCore;

public class UEGLog : IEssgeeLogger
{
    public void Debug(string message)
    {
        App.log.Debug(message);
        UnityEngine.Debug.Log(message);
    }

    public void Warning(string message)
    {
        App.log.Warning(message);
    }
    public void Err(string message)
    {
        App.log.Error(message);
    }
}
