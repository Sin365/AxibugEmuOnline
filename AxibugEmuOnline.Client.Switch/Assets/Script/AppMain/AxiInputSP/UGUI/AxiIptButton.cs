using UnityEngine;
using UnityEngine.EventSystems;

namespace AxiInputSP.UGUI
{
    public class AxiIptButton : UnityEngine.UI.Button
    {
        enum AxiButtonState
        {
            None,
            KeyUp,
            KeyDown,
            KeyHold
        }

        AxiButtonState m_state = AxiButtonState.None;
        /// <summary>
        /// 键值（支持组合键）
        /// </summary>
        [SerializeField]
        public AxiInputUGuiBtnType[] axiBtnTypeList;

        public bool GetKey()
        {
            return m_state >= AxiButtonState.KeyDown;
        }
        public bool GetKeyUp()
        {
            return m_state == AxiButtonState.KeyUp;
        }
        public bool GetKeyDown()
        {
            return m_state == AxiButtonState.KeyDown;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            //如果之前帧是KeyUp或None，则为KeyDown|KeyHold
            if (m_state <= AxiButtonState.KeyUp)
                m_state = AxiButtonState.KeyDown;
            //如果之前帧是KeyDown，则为KeyHold
            else if (m_state == AxiButtonState.KeyDown)
                m_state = AxiButtonState.KeyHold;
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            //如果之前帧是KeyDown|KeyHold，则为KeyUp|None
            if (m_state >= AxiButtonState.KeyDown)
                m_state = AxiButtonState.KeyUp;
            //如果之前帧是KeyUp，则为None
            else if (m_state == AxiButtonState.KeyUp)
                m_state = AxiButtonState.None;
        }
    }
}
