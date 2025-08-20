using AxibugProtobuf;
using AxiInputSP;
using AxiInputSP.UGUI;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    public class ScreenGamepad_D : InputDevice_D
    {
        public override GamePadType PadType =>  GamePadType.TouchPanel;

        public Button_C UP;
        public Button_C DOWN;
        public Button_C LEFT;
        public Button_C RIGHT;
        public Button_C BTN_A;
        public Button_C BTN_B;
        public Button_C BTN_C;
        public Button_C BTN_D;
        public Button_C BTN_E;
        public Button_C BTN_F;
        public Button_C OPTION_1;
        public Button_C OPTION_2;
        public Button_C OPTION_3;
        public Button_C OPTION_4;
        public Button_C HOME;
        public Stick_C JOYSTICK;

        AxiScreenGamepad m_linkUnityImpl;
        Dictionary<Button_C, AxiInputUGuiBtnType> m_buttonTypes = new Dictionary<Button_C, AxiInputUGuiBtnType>();

        public ScreenGamepad_D(AxiScreenGamepad linkMono, InputResolver resolver) : base(resolver)
        {
            m_linkUnityImpl = linkMono;
            m_buttonTypes[UP] = AxiInputUGuiBtnType.UP;
            m_buttonTypes[DOWN] = AxiInputUGuiBtnType.DOWN;
            m_buttonTypes[LEFT] = AxiInputUGuiBtnType.LEFT;
            m_buttonTypes[RIGHT] = AxiInputUGuiBtnType.RIGHT;
            m_buttonTypes[BTN_A] = AxiInputUGuiBtnType.BTN_A;
            m_buttonTypes[BTN_B] = AxiInputUGuiBtnType.BTN_B;
            m_buttonTypes[BTN_C] = AxiInputUGuiBtnType.BTN_C;
            m_buttonTypes[BTN_D] = AxiInputUGuiBtnType.BTN_D;
            m_buttonTypes[BTN_E] = AxiInputUGuiBtnType.BTN_E;
            m_buttonTypes[BTN_F] = AxiInputUGuiBtnType.BTN_F;
            m_buttonTypes[OPTION_1] = AxiInputUGuiBtnType.OPTION_1;
            m_buttonTypes[OPTION_2] = AxiInputUGuiBtnType.OPTION_2;
            m_buttonTypes[OPTION_3] = AxiInputUGuiBtnType.OPTION_3;
            m_buttonTypes[OPTION_4] = AxiInputUGuiBtnType.OPTION_4;
            m_buttonTypes[HOME] = AxiInputUGuiBtnType.HOME;
        }

        public bool CheckPerforming<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C
        {
            if (control is Button_C)
            {
                var type = m_buttonTypes[control as Button_C];
                return m_linkUnityImpl.GetKey(type);
            }
            else if (control is Stick_C)
            {
                var vec2 = GetVector2(control);
                return vec2.x != 0 || vec2.y != 0;
            }
            else return false;
        }

        public Vector2 GetVector2<CONTROLLER>(CONTROLLER control) where CONTROLLER : InputControl_C
        {
            if (control is Stick_C)
            {
                return m_linkUnityImpl.GetJoystickValue();
            }
            else return default;
        }
    }
}