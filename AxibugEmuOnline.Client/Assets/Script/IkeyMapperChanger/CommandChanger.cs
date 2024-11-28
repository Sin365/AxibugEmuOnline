using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public abstract class CommandChanger : IKeyMapperChanger
    {
        public string Name { get; private set; }

        public abstract object GetConfig();
    }
}
