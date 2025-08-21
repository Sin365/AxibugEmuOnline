using System.Collections.Generic;
using UnityEngine;

namespace AxiInputSP.UGUI
{
    public class AxiScreenGamepad : MonoBehaviour
    {
        public delegate void OnAxiScreenGamepadActiveHandle(AxiScreenGamepad sender);
        public delegate void OnAxiScreenGamepadDisactiveHandle(AxiScreenGamepad sender);

        public static event OnAxiScreenGamepadActiveHandle OnGamepadActive;
        public static event OnAxiScreenGamepadActiveHandle OnGamepadDisactive;

        AxiIptButton[] m_buttons;
        FloatingJoystick m_joystick;
        HashSet<AxiInputUGuiBtnType> m_pressBtns = new HashSet<AxiInputUGuiBtnType>();
        Vector2 m_joyStickRaw;

        public bool GetKey(AxiInputUGuiBtnType btnType)
        {
            return m_pressBtns.Contains(btnType);
        }

        public Vector2 GetJoystickValue()
        {
            return m_joyStickRaw;
        }

        private void Update()
        {
            m_joyStickRaw = m_joystick.GetJoyRaw();
            m_pressBtns.Clear();
            foreach (var btn in m_buttons)
            {
                if (btn.GetKey())
                {
                    foreach (var btnType in btn.axiBtnTypeList)
                        m_pressBtns.Add(btnType);
                }
            }
        }

        private void Awake()
        {
            m_buttons = GetComponentsInChildren<AxiIptButton>(true);
            m_joystick = GetComponentInChildren<FloatingJoystick>(true);
        }

        private void OnEnable()
        {
            m_joyStickRaw = Vector2.zero;
            m_pressBtns.Clear();

            OnGamepadActive?.Invoke(this);
        }

        private void OnDisable()
        {
            OnGamepadDisactive?.Invoke(this);
        }
    }
}
