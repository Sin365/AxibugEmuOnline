using System;

namespace Essgee.EventArguments
{
    public class SendLogMessageEventArgs : EventArgs
    {
        public string Message { get; private set; }


        public static SendLogMessageEventArgs Create(string message)
        {
            var eventArgs = ObjectPoolAuto.Acquire<SendLogMessageEventArgs>();
            eventArgs.Message = message;
            return eventArgs;
        }
    }
    public static class SendLogMessageEventArgsEx
    {
        public static void Release(this SendLogMessageEventArgs eventArgs)
        {
            ObjectPoolAuto.Release(eventArgs);
        }
    }
}
