using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public abstract class OptionUI_MenuItem : MonoBehaviour
    {
        [SerializeField] Text m_MenuNameTxt;
        [SerializeField] Image m_Icon;

        public Image IconUI => m_Icon;

        public bool Visible => m_Menu.Visible;
        public OptionUI OptionUI { get; private set; }
        protected OptionMenu m_Menu;

        public void SetData(OptionUI optionUI, OptionMenu menuData)
        {
            OptionUI = optionUI;
            m_Menu = menuData;
            m_MenuNameTxt.text = menuData.Name;
            if (menuData.Icon == null) m_Icon.gameObject.SetActiveEx(false);
            else
            {
                m_Icon.gameObject.SetActiveEx(true);
                m_Icon.SetMaterial(null);
                m_Icon.sprite = menuData.Icon;
            }

            OnSetData(menuData);
        }

        public bool IsExpandMenu => m_Menu is ExpandMenu;

        protected abstract void OnSetData(OptionMenu menuData);

        public abstract void OnExecute(OptionUI optionUI, ref bool cancelHide);
        public virtual void OnLeft() { }
        public virtual void OnRight() { }
        public abstract void OnFocus();
        public virtual void OnHide() { }
    }

    public abstract class OptionUI_MenuItem<T> : OptionUI_MenuItem
        where T : OptionMenu
    {
        protected T MenuData => m_Menu as T;

        protected override void OnSetData(OptionMenu menuData)
        {
            MenuData.OnShow(this);
        }

        public override void OnFocus()
        {
            MenuData.OnFocus();
        }

        public override void OnHide()
        {
            MenuData.OnHide();
        }
    }
}
