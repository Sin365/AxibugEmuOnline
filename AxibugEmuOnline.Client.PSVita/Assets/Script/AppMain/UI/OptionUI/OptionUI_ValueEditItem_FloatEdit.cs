using System;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ValueEditItem_FloatEdit : MonoBehaviour, IValueEditControl
    {
        [SerializeField]
        Slider slider;
        [SerializeField]
        Text txt_value;

        float m_step;
        private ValueSetMenu m_valueMenu;

        private void Awake()
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            txt_value.text = $"{value:.00}";
            m_valueMenu.OnValueChanged(value);
        }

        public void SetData(ValueSetMenu valueMenu)
        {
            m_valueMenu = valueMenu;
            slider.minValue = (float)valueMenu.Min;
            slider.maxValue = (float)valueMenu.Max;
            slider.value = (float)valueMenu.ValueRaw;
            m_step = (slider.maxValue - slider.minValue) * 0.05f;
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
