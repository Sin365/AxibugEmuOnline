using static AxibugEmuOnline.Client.FilterManager;

namespace AxibugEmuOnline.Client
{
    public class AppSettings
    {
        /// <summary> 背景颜色设定 </summary>
        public BgColorSettings BgColor { get; private set; }
        /// <summary> 滤镜设置 </summary>
        public FilterManager Filter { get; private set; }

        public AppSettings(Initer initer)
        {
            BgColor = new BgColorSettings();
            Filter = new FilterManager(initer.FilterPreview, initer.XMBBg);
        }
    }
}
