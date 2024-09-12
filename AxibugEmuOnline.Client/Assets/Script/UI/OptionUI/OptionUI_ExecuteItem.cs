using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ExecuteItem : MonoBehaviour
    {
        [SerializeField] Text m_MenuNameTxt;
        [SerializeField] Image m_Icon;

        public void SetData(ExecuteMenu executeMenu)
        {
            m_MenuNameTxt.text = executeMenu.Name;
            if (executeMenu.Icon == null) m_Icon.gameObject.SetActiveEx(false);
            else
            {
                m_Icon.gameObject.SetActiveEx(true);
                m_Icon.sprite = executeMenu.Icon;
            }
        }
    }
}
