using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Manager;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace AxibugEmuOnline.Client
{
    public class CommandListener : ICommandListener
    {
        //Dictionary<KeyCode, EnumCommand> m_keyMapper = new Dictionary<KeyCode, EnumCommand>();
        SingleKeysSetting singleKeysSetting;
        Dictionary<EnumCommand, bool> m_dictLastState = new Dictionary<EnumCommand, bool>();
        EnumCommand[] m_checkCmds;
        List<CommandState> m_commands = new List<CommandState>();
        IEnumerable<CommandState> GetCommand()
        {
            m_commands.Clear();
            //foreach (var item in m_keyMapper)
            //{
            //    if (Input.GetKeyDown(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = false });
            //    if (Input.GetKeyUp(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = true });
            //}
            if (m_checkCmds != null)
            {
                foreach (var cmd in m_checkCmds)
                {
                    bool oldstate = m_dictLastState[cmd];
                    bool newstate = singleKeysSetting.GetKey((ulong)cmd);
                    m_dictLastState[cmd] = newstate;
                    if (oldstate != newstate)
                    {
                        m_commands.Add(new CommandState { Cmd = cmd, Cancel = !newstate });
                    }
                }
            }

            return m_commands;
        }
        public void ApplyKeyMapper(IKeyMapperChanger changer)
        {
            //var cfg = (Dictionary<KeyCode, EnumCommand>)changer.GetConfig();
            singleKeysSetting = changer.GetKeySetting();
            m_dictLastState.Clear();
            EnumCommand[] arr = changer.GetConfig();
            for (int i = 0; i < arr.Length; i++)
            {
                m_dictLastState[arr[i]] = false;
            }
            m_checkCmds = arr;
        }

        public void Update(IEnumerable<CommandExecuter> executers)
        {
            foreach (var cmd in GetCommand())
            {
                foreach (var executer in executers)
                {
                    App.log.Debug($"{cmd.Cmd}|{cmd.Cancel}");
                    executer.ExecuteCommand(cmd.Cmd, cmd.Cancel);
                }
            }
        }
    }
}
