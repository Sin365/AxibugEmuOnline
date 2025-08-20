using AxibugEmuOnline.Client.Tools;
using System;
using System.Collections.Generic;
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

        public GameObject UI_Disconnect;
        public GameObject UI_DownloadError;
        public GameObject UI_Downloading;
        public GameObject UI_Uploading;
        public GameObject UI_Checking;
        public GameObject UI_Conflict;
        public GameObject UI_Synced;

        Texture2D m_screenTex;

        Dictionary<Type, GameObject> m_stateNodes = new Dictionary<Type, GameObject>();

        private void Awake()
        {
            m_stateNodes[typeof(SaveFile.CheckingState)] = UI_Checking;
            m_stateNodes[typeof(SaveFile.ConflictState)] = UI_Conflict;
            m_stateNodes[typeof(SaveFile.DownloadingState)] = UI_Downloading;
            m_stateNodes[typeof(SaveFile.SyncedState)] = UI_Synced;
            m_stateNodes[typeof(SaveFile.UploadingState)] = UI_Uploading;
            m_stateNodes[typeof(SaveFile.CheckingNetworkState)] = UI_Disconnect;
            m_stateNodes[typeof(SaveFile.SyncFailedState)] = UI_DownloadError;
        }

        protected override void OnSetData(InternalOptionMenu menuData)
        {
            base.OnSetData(menuData);

            RefreshUI();
            MenuData.SavFile.OnSavSuccessed += SavFile_OnSavSuccessed;
            MenuData.SavFile.OnStateChanged += UpdateStateNode;
        }

        private void SavFile_OnSavSuccessed()
        {
            MenuData.SavFile.TrySync();
            RefreshUI();
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

            UpdateStateNode();
        }

        private void UpdateStateNode()
        {
            var stateType = MenuData.SavFile.GetCurrentState().GetType();

            foreach (var item in m_stateNodes)
            {
                var type = item.Key;
                var nodeGo = item.Value;

                nodeGo.SetActiveEx(type == stateType);
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

            MenuData.SavFile.OnSavSuccessed -= SavFile_OnSavSuccessed;
            MenuData.SavFile.OnStateChanged -= UpdateStateNode;
        }

        public override void OnExecute(OptionUI optionUI, ref bool cancelHide)
        {
            MenuData.OnExcute(optionUI, ref cancelHide);
        }
    }
}
