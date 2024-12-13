using AxibugEmuOnline.Client.ClientCore;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class CoreDebuger : IDebugerImpl
    {
        public void Log(string message)
        {
            App.log.Info(message);
        }

        public void LogError(string message)
        {
            App.log.Error(message);
        }
    }
}
