using AxibugEmuOnline.Client.Settings;

namespace AxibugEmuOnline.Client
{
    public class AppSettings
    {
        /// <summary> 背景颜色设定 </summary>
        public BgColorSettings BgColor { get; private set; }
        /// <summary> 滤镜设置 </summary>
        public FilterManager Filter { get; private set; }
        /// <summary> 画面比例设置 </summary>
        public ScreenScaler ScreenScaler { get; private set; }
        /// <summary> 键位设置 </summary>
        public KeyMapperSetting KeyMapper { get; private set; }

        public AppSettings()
        {
            BgColor = new BgColorSettings();
            Filter = new FilterManager();
            ScreenScaler = new ScreenScaler();
            KeyMapper = new KeyMapperSetting();
        }
    }
}
