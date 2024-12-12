using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ValueEditItem_EnumEdit : MonoBehaviour, IValueEditControl
    {
        [SerializeField]
        Text txt_value;

        private ValueSetMenu m_valueMenu;
        private List<Enum> m_enumValues = new List<Enum>();
        private int m_valueIndex;

        public void SetData(ValueSetMenu valueMenu)
        {
            m_valueMenu = valueMenu;
            txt_value.text = valueMenu.ValueRaw.ToString();

            foreach (Enum enumValue in Enum.GetValues(valueMenu.ValueType))
            {
                m_enumValues.Add(enumValue);
            }
            m_valueIndex = m_enumValues.IndexOf((Enum)valueMenu.ValueRaw);
        }

        public void OnLeft()
        {
            m_valueIndex--;

            if (m_valueIndex < 0) m_valueIndex = m_enumValues.Count - 1;

            var value = m_enumValues[m_valueIndex];
            txt_value.text = value.ToString();
            m_valueMenu.OnValueChanged(value);
        }

        public void OnRight()
        {
            m_valueIndex++;

            if (m_valueIndex >= m_enumValues.Count) m_valueIndex = 0;

            var value = m_enumValues[m_valueIndex];
            txt_value.text = value.ToString();
            m_valueMenu.OnValueChanged(value);
        }

        public void OnExecute()
        {
            OnRight();
        }
    }
}
