using System.Collections.Generic;

namespace VirtualNes.Core.Debug
{
    public static class Debuger
    {
        private static IDebugerImpl s_debuger;
        public static void Setup(IDebugerImpl debuger)
        {
            s_debuger = debuger;
        }
        public static void Log(string message)
        {
            s_debuger.Log(message);
        }

        public static void LogError(string message)
        {
            s_debuger.LogError(message);
        }
    }

    public interface IDebugerImpl
    {
        void Log(string message);
        void LogError(string message);
    }
}
