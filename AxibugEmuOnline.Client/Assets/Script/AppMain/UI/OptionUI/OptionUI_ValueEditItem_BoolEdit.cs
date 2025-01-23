using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ValueEditItem_BoolEdit : MonoBehaviour, IValueEditControl
    {
        [SerializeField]
        Text txt_value;

        private ValueSetMenu m_valueMenu;

        public void SetData(ValueSetMenu valueMenu)
        {
            m_valueMenu = valueMenu;
            txt_value.text = valueMenu.ValueRaw.ToString();
        }

        public void OnLeft()
        {
            OnExecute();
        }

        public void OnRight()
        {
            OnExecute();
        }

        public void OnExecute()
        {
            var value = (bool)m_valueMenu.ValueRaw;
            value = !value;
            txt_value.text = value.ToString();
            m_valueMenu.OnValueChanged(value);
        }
    }
}
