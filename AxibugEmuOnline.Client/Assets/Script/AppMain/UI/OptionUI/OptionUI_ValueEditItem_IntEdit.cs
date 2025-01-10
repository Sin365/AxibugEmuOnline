using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ValueEditItem_IntEdit : MonoBehaviour, IValueEditControl
    {
        [SerializeField]
        Slider slider;
        [SerializeField]
        Text txt_value;

        int m_step;
        private ValueSetMenu m_valueMenu;

        private void Awake()
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            int intValue = (int)value;
            txt_value.text = $"{intValue}";

            if (!m_dataSetting) m_valueMenu.OnValueChanged(intValue);
        }

        bool m_dataSetting;
        public void SetData(ValueSetMenu valueMenu)
        {
            m_dataSetting = true;
            m_valueMenu = valueMenu;
            slider.minValue = (int)valueMenu.Min;
            slider.maxValue = (int)valueMenu.Max;
            slider.value = (int)valueMenu.ValueRaw;
            slider.wholeNumbers = true;
            m_step = 1;
            m_dataSetting = false;
        }

        public void OnLeft()
        {
            var newValue = Mathf.Clamp(slider.value - m_step, slider.minValue, slider.maxValue);
            slider.value = newValue;
        }

        public void OnRight()
        {
            var newValue = Mathf.Clamp(slider.value + m_step, slider.minValue, slider.maxValue);
            slider.value = newValue;
        }

        public void OnExecute()
        {
        }
    }
}
