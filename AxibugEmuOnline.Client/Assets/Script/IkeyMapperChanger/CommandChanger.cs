using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class CommandChanger : IKeyMapperChanger
    {
        public string Name => GetType().Name;
        public abstract object GetConfig();
    }
}
