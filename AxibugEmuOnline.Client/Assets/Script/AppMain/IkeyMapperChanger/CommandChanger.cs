using AxibugEmuOnline.Client.Manager;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public abstract class CommandChanger : IKeyMapperChanger
    {
        public string Name => GetType().Name;
        public abstract EnumCommand[] GetConfig();

        public abstract SingleKeysSetting GetKeySetting();
    }
}
