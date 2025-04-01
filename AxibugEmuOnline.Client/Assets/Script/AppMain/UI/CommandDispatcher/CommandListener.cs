using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CommandListener
    {
        EnumCommand[] m_checkCmds;
        List<CommandState> m_commands = new List<CommandState>();
        long CheckFrame = -1;
        XMBKeyBinding m_keyBinder;
        public ScheduleType Schedule { get; set; }

        public CommandListener()
        {
            m_keyBinder = App.settings.KeyMapper.GetBinder<XMBKeyBinding>();
            m_checkCmds = Enum.GetValues(typeof(EnumCommand)) as EnumCommand[];
        }

        IEnumerable<CommandState> GetCommand()
        {
            if (CheckFrame == Time.frameCount)
                return m_commands;
            CheckFrame = Time.frameCount;

            m_commands.Clear();

            int controllerIndex = (int)Schedule;

            foreach (var cmd in m_checkCmds)
            {
                if (m_keyBinder.Start(cmd, controllerIndex)) m_commands.Add(new CommandState { Cmd = cmd, Cancel = false });
                if (m_keyBinder.Release(cmd, controllerIndex)) m_commands.Add(new CommandState { Cmd = cmd, Cancel = true });
            }


            return m_commands;
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

        public enum ScheduleType
        {
            Normal,
            Gaming
        }
    }
}
