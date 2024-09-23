using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public interface IEmuCore
    {
        object GetState();
        byte[] GetStateBytes();
        void LoadState(object state);
        void LoadStateFromBytes(byte[] data);
        void Pause();
        void Resume();
        void SetupScheme();        
    }
}
