using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CommandListener : ICommandListener
    {
        //Dictionary<KeyCode, EnumCommand> m_keyMapper = new Dictionary<KeyCode, EnumCommand>();
        SingleKeysSetting singleKeysSetting;
        Dictionary<EnumCommand, bool> m_dictLastState = new Dictionary<EnumCommand, bool>();
        EnumCommand[] m_checkCmds;
        List<CommandState> m_commands = new List<CommandState>();
        long CheckFrame = -1;
        IEnumerable<CommandState> GetCommand()
        {
            //foreach (var item in m_keyMapper)
            //{
            //    if (Input.GetKeyDown(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = false });
            //    if (Input.GetKeyUp(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = true });
            //}

            if (CheckFrame == Time.frameCount)
                return m_commands;
            CheckFrame = Time.frameCount;

            m_commands.Clear();

            //不再依赖KeyDown KeyUp的做法，兼容UGUI，或者Axis
            if (m_checkCmds != null)
            {
                foreach (var cmd in m_checkCmds)
                {
                    if (singleKeysSetting.GetKeyDown((ulong)cmd)) m_commands.Add(new CommandState { Cmd = cmd, Cancel = false });
                    if (singleKeysSetting.GetKeyUp((ulong)cmd)) m_commands.Add(new CommandState { Cmd = cmd, Cancel = true });

                    //if (m_dictLastState[cmd] && !singleKeysSetting.GetKey((ulong)cmd))
                    //{
                    //    m_commands.Add(new CommandState { Cmd = cmd, Cancel = true });
                    //    m_dictLastState[cmd] = false;
                    //}
                    //else if (!m_dictLastState[cmd] && singleKeysSetting.GetKey((ulong)cmd))
                    //{ 
                    //    m_commands.Add(new CommandState { Cmd = cmd, Cancel = false });
                    //    m_dictLastState[cmd] = true;
                    //}

                    //bool oldstate = m_dictLastState[cmd];
                    //bool newstate = singleKeysSetting.GetKey((ulong)cmd);
                    //m_dictLastState[cmd] = newstate;
                    //if (oldstate != newstate)
                    //{
                    //    m_commands.Add(new CommandState { Cmd = cmd, Cancel = !newstate });
                    //}


                }
            }


            //foreach (var item in m_keyMapper)
            //{
            //    if (Input.GetKeyDown(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = false });
            //    if (Input.GetKeyUp(item.Key)) m_commands.Add(new CommandState { Cmd = item.Value, Cancel = true });
            //}

            return m_commands;
        }
        public void ApplyKeyMapper(IKeyMapperChanger changer)
        {
            //var cfg = (Dictionary<KeyCode, EnumCommand>)changer.GetConfig();
            singleKeysSetting = changer.GetKeySetting();
            App.log.Debug($"CommandListener ApplyKeyMapper | {Time.frameCount} {changer.Name}");
            EnumCommand[] arr = changer.GetConfig();
            //不要直接清m_dictLastState.Clear()，同样的Key维持状态，避免造成多余CommandState
            EnumCommand[] temp = m_dictLastState.Keys.ToArray();
            //仅添加新增
            foreach (var cmd in arr)
            {
                if(!m_dictLastState.ContainsKey(cmd))
                    m_dictLastState[cmd] = true;
            }
            m_checkCmds = arr;
        }

        public void Update(IEnumerable<CommandExecuter> executers)
        {
            foreach (var cmd in GetCommand())
            {
                foreach (var executer in executers)
                {
                    //App.log.Debug($"CommandListener GetCommand | {Time.frameCount}|{executer.name}| {cmd.Cmd}|{cmd.Cancel}");
                    executer.ExecuteCommand(cmd.Cmd, cmd.Cancel);
                }
            }
        }
    }
}
