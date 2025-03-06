using Assets.Script.AppMain.AxiInput;
using Assets.Script.AppMain.AxiInput.Settings;
using UnityEngine.UIElements.Experimental;

namespace AxibugEmuOnline.Client.Manager
{
    public class AppInput
    {
        public XMBMultiKeysSetting xmb;
        public GamingMultiKeysSetting gaming;
        public UMAMEMultiKeysSetting mame;
        public AppInput()
        {
            xmb = new XMBMultiKeysSetting();
            gaming = new GamingMultiKeysSetting();
            mame = new UMAMEMultiKeysSetting();
            LoadDefaultSetting();
        }

        public void LoadDefaultSetting()
        {
            xmb.LoadDefaultSetting();
            gaming.LoadDefaultSetting();
            mame.LoadDefaultSetting();
        }
    }

    public interface MultiKeysSetting
    {
        bool HadAnyKeyDown(int index);
        void ClearAll();
        void LoadDefaultSetting();
    }

    public interface SingleKeysSetting
    {
        void ClearAll();
        void SetKey(ulong Key, AxiInput input);
        bool GetKey(ulong Key);
        bool GetKeyDown(ulong Key);
        bool GetKeyUp(ulong Key);
        void ColletAllKey();
        bool HadAnyKeyDown();
    }

}