using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class OptionUI_ValueEditItem : OptionUI_MenuItem<ValueSetMenu>
    {
        [SerializeField]
        OptionUI_ValueEditItem_FloatEdit com_floatEdit;
        [SerializeField]
        OptionUI_ValueEditItem_IntEdit com_intEdit;
        [SerializeField]
        OptionUI_ValueEditItem_BoolEdit com_boolEdit;
        [SerializeField]
        OptionUI_ValueEditItem_EnumEdit com_enumEdit;

        IValueEditControl m_currentCom;

        protected override void OnSetData(OptionMenu menuData)
        {
            com_floatEdit.gameObject.SetActive(false);
            com_intEdit.gameObject.SetActive(false);
            com_boolEdit.gameObject.SetActive(false);
            com_enumEdit.gameObject.SetActive(false);

            if (menuData is ValueSetMenu)
            {
                var valueMenu = (ValueSetMenu)menuData;
                if (valueMenu.ValueType == typeof(float))
                {
                    m_currentCom = com_floatEdit;
                }
                else if(valueMenu.ValueType == typeof(int))
                {
                    m_currentCom = com_intEdit;
                }
                else if(valueMenu.ValueType == typeof(bool))
                {
                    m_currentCom = com_boolEdit;
                }
                else if (valueMenu.ValueType.IsEnum)
                {
                    m_currentCom = com_enumEdit;
                }
                else
                {
                    App.log.Error($"尚未支持的数据类型:{valueMenu.ValueType}");
                    return;
                }


                m_currentCom.gameObject.SetActiveEx(true);
                m_currentCom.SetData(valueMenu);
            }

            base.OnSetData(menuData);
        }

        public override void OnExecute(OptionUI optionUI, ref bool cancelHide)
        {
            cancelHide = true;
            m_currentCom?.OnExecute();
        }

        public override void OnLeft()
        {
            m_currentCom?.OnLeft();
        }

        public override void OnRight()
        {
            m_currentCom?.OnRight();
        }
    }

    public interface IValueEditControl
    {
        void SetData(ValueSetMenu valueMenu);
        GameObject gameObject { get; }
        void OnLeft();
        void OnRight();
        void OnExecute();
    }
}
