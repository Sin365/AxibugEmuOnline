using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Manager;

namespace AxibugEmuOnline.Client
{
    public class GamingChanger : CommandChanger
    {
        public override EnumCommand[] GetConfig()
        {
            return App.input.gaming.controllers[0].GetAllCmd<EnumCommand>();
        }

        public override SingleKeysSetting GetKeySetting()
        {
            return App.input.gaming.controllers[0];
        }

        //Dictionary<KeyCode, EnumCommand> m_uiKeyMapper = new Dictionary<KeyCode, EnumCommand>();
        //public GamingChanger()
        //{
        //    m_uiKeyMapper[KeyCode.Escape] = EnumCommand.OptionMenu;

        //    if (Application.platform == RuntimePlatform.PSP2)
        //    {
        //        m_uiKeyMapper[Common.PSVitaKey.L] = EnumCommand.OptionMenu;
        //        m_uiKeyMapper[Common.PSVitaKey.R] = EnumCommand.OptionMenu;
        //    }

        //    //PC XBOX
        //    m_uiKeyMapper[Common.PC_XBOXKEY.Y] = EnumCommand.OptionMenu;
        //}

        //public override object GetConfig() => m_uiKeyMapper;
    }
}
