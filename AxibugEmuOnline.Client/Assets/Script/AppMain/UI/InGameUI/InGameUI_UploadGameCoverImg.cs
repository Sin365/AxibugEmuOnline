using AxibugEmuOnline.Client.ClientCore;

namespace AxibugEmuOnline.Client
{
    public class InGameUI_UploadGameCoverImg : ExecuteMenu
    {
        private InGameUI m_gameUI;
        public override string Name => "上传封面图";

        public InGameUI_UploadGameCoverImg(InGameUI gameUI)
        {
            m_gameUI = gameUI;
        }

        public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
        {
            var tex = m_gameUI.Core.OutputPixel;
            var screenData = tex.ToJPG(m_gameUI.Core.DrawLocalScale);
            App.share.SendUpLoadGameScreenCover(m_gameUI.RomFile.ID, screenData);
        }
    }
}
