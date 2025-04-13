using UnityEngine;
using UnityEngine.UI;
using static AxibugEmuOnline.Client.InGameUI_SaveStateMenu;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_SavSlotItem : OptionUI_MenuItem<SaveSlotMenu>
    {
        public RawImage UI_ScreenShot;
        public Image UI_Empty;
        public Text UI_SavTime;

        Texture2D m_screenTex;

        protected override void OnSetData(InternalOptionMenu menuData)
        {
            base.OnSetData(menuData);

            RefreshUI();

            MenuData.SavFile.OnSavSuccessed += RefreshUI;
        }

        private void RefreshUI()
        {
            bool isEmpty = MenuData.SavFile.IsEmpty;
            UI_ScreenShot.gameObject.SetActiveEx(!isEmpty);
            UI_Empty.gameObject.SetActiveEx(isEmpty);
            UI_SavTime.gameObject.SetActiveEx(true);

            if (isEmpty)
            {
                UI_SavTime.text = "没有数据";
                if (m_screenTex)
                {
                    Destroy(m_screenTex);
                    m_screenTex = null;
                }
            }
            else
            {
                var savTime = MenuData.SavFile.GetSavTimeUTC().ToLocalTime();
                UI_SavTime.text = $"{savTime.Year}/{savTime.Month:00}/{savTime.Day:00}\n{savTime.Hour}:{savTime.Minute}:{savTime.Second}";
                MenuData.SavFile.GetSavData(out byte[] _, out byte[] screenShotData);

                if (!m_screenTex) m_screenTex = new Texture2D(1, 1);

                m_screenTex.LoadImage(screenShotData);
                UI_ScreenShot.texture = m_screenTex;
            }
        }

        public override void OnHide()
        {
            base.OnHide();

            if (m_screenTex)
            {
                Destroy(m_screenTex);
                m_screenTex = null;
            }

            MenuData.SavFile.OnSavSuccessed -= RefreshUI;
        }

        public override void OnExecute(OptionUI optionUI, ref bool cancelHide)
        {
            MenuData.OnExcute(optionUI, ref cancelHide);
        }
    }
}
