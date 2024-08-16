using UnityEngine;
using VirtualNes.Core.Debug;

namespace AxibugEmuOnline.Client
{
    public class CoreDebuger : IDebugerImpl
    {

        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}
