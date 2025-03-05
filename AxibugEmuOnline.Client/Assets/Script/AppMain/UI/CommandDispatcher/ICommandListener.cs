using AxibugEmuOnline.Client.Manager;
using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public interface IKeyMapperChanger
    {
        string Name { get; }
        EnumCommand[] GetConfig();
        SingleKeysSetting GetKeySetting();
    }
    public interface ICommandListener
    {
        /// <summary>
        /// 应用键位设置
        /// </summary>
        /// <param name="changer"></param>
        void ApplyKeyMapper(IKeyMapperChanger changer);
        void Update(IEnumerable<CommandExecuter> commands);
    }

    public struct CommandState
    {
        public EnumCommand Cmd;
        public bool Cancel;
    }
}
