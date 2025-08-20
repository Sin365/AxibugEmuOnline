public interface IEssgeeLogger
{
    void Debug(string message);
    void Warning(string message);
    void Err(string message);
}
public static class EssgeeLogger
{
    static IEssgeeLogger essgeeLogger;
    public static void Init(IEssgeeLogger logger)
    {
        essgeeLogger = logger;
    }
    public static void WriteLine(string message = null)
    {
        essgeeLogger.Debug(message);
    }
    public static void Err(string message = null)
    {
        essgeeLogger.Err(message);
    }
    public static void EnqueueMessage(string message = null)
    {
        essgeeLogger.Debug(message);
    }
    public static void EnqueueMessageSuccess(string message = null)
    {
        essgeeLogger.Debug(message);
    }
    public static void EnqueueMessageWarning(string message = null)
    {
        essgeeLogger.Warning(message);
    }

    internal static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            essgeeLogger.Debug(message);
        }
    }
}