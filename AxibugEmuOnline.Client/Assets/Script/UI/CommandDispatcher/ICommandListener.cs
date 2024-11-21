using System.Collections.Generic;

namespace AxibugEmuOnline.Client
{
    public interface IKeyMapperChanger
    {
        object GetConfig();
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
