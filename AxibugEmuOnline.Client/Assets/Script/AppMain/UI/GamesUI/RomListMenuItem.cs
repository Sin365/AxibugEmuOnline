using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class RomListMenuItem : VirtualSubMenuItem
    {
        [SerializeField]
        protected bool StarRom;
        [SerializeField]
        protected RomPlatformType Platform;

        private RomLib RomLib
        {
            get
            {
                if (StarRom) return App.starRomLib;
                else return App.GetRomLib(Platform);
            }
        }

        private List<OptionMenu> m_options;

        protected override void Awake()
        {
            base.Awake();

            m_options = new List<OptionMenu>()
            {
                new OptMenu_Search(this),
                new OptMenu_ShowAll(this),
                new OptMenu_Fav(this),
            };
        }

        public string SearchKey;
        protected override void GetVirtualListDatas(VirtualListDataHandle callback)
        {
            RomLib.FetchRomCount((roms) => callback.Invoke(roms, 0), SearchKey);
        }

        public override bool OnEnterItem()
        {
            var res = base.OnEnterItem();
            if (res) CommandDispatcher.Instance.RegistController(this);

            return true;
        }

        public override bool OnExitItem()
        {
            var res = base.OnExitItem();
            if (res) CommandDispatcher.Instance.UnRegistController(this);

            return false;
        }


        protected override void OnCmdOptionMenu()
        {
            var thirdMenuGroup = SubMenuItemGroup as ThirdMenuRoot;
            if (thirdMenuGroup.itemGroup.DataList == null || thirdMenuGroup.itemGroup.DataList.Count == 0) return;

            OverlayManager.PopSideBar(m_options);
        }

        public class OptMenu_Search : ExecuteMenu
        {
            private RomListMenuItem m_romListUI;
            public override string Name => "搜索";

            public OptMenu_Search(RomListMenuItem romListUI)
            {
                m_romListUI = romListUI;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                OverlayManager.Input(OnSearchCommit, "输入Rom名称", m_romListUI.SearchKey);
            }

            private void OnSearchCommit(string text)
            {
                m_romListUI.SearchKey = text;
                m_romListUI.RefreshUI();
            }
        }

        public class OptMenu_ShowAll : ExecuteMenu
        {
            private RomListMenuItem m_ui;

            public override string Name => "显示全部";
            public override bool Visible => !string.IsNullOrWhiteSpace(m_ui.SearchKey);

            public OptMenu_ShowAll(RomListMenuItem romListUI)
            {
                m_ui = romListUI;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                m_ui.SearchKey = null;
                m_ui.RefreshUI();
            }
        }

        public class OptMenu_Fav : ExecuteMenu
        {
            private RomListMenuItem m_romListUI;
            private ThirdMenuRoot m_romListSub;

            private RomItem m_currentSelect => m_romListSub.GetItemUIByIndex(m_romListSub.SelectIndex) as RomItem;

            public override bool Visible => m_currentSelect.RomInfoReady;

            public override string Name { get { return m_currentSelect.IsStar ? "取消收藏" : "收藏"; } }

            public OptMenu_Fav(RomListMenuItem romListUI)
            {
                m_romListUI = romListUI;
                m_romListSub = m_romListUI.SubMenuItemGroup as ThirdMenuRoot;
            }

            public override void OnExcute(OptionUI optionUI, ref bool cancelHide)
            {
                var romItem = m_currentSelect;
                if (!romItem.IsStar)
                    App.share.SendGameStar(romItem.RomID, 1);
                else
                    App.share.SendGameStar(romItem.RomID, 0);
            }
        }
    }
}
