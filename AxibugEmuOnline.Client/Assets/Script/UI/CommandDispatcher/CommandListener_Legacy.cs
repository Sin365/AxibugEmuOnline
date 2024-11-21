using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CommandListener_Legacy : ICommandListener
    {
        Dictionary<KeyCode, EnumCommand> m_keyMapper = new Dictionary<KeyCode, EnumCommand>();
        List<CommandState> m_commands = new List<CommandState>();
        IEnumerable<CommandState> GetCommand()
        {
            m_commands.Clear();
            foreach (var item in m_keyMapper)
            {
                if (Input.GetKeyDown(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = false });
                if (Input.GetKeyUp(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = true });
            }

            return m_commands;
        }
        public void ApplyKeyMapper(IKeyMapperChanger changer)
        {
            var cfg = (Dictionary<KeyCode, EnumCommand>)changer.GetConfig();
            m_keyMapper = cfg;
        }

        public void Update(IEnumerable<CommandExecuter> executers)
        {
            foreach (var cmd in GetCommand())
            {
                foreach (var executer in executers)
                {
                    executer.ExecuteCommand(cmd.Cmd, cmd.Cancel);
                }
            }
        }
    }
}
