public class UEGLog : IEssgeeLogger
{
    public void Debug(string message)
    {
        UnityEngine.Debug.Log(message);
    }


    public void Warning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
    }
    public void Err(string message)
    {
        UnityEngine.Debug.LogError(message);
    }
}
