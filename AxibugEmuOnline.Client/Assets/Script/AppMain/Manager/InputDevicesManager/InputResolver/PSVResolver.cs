using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices.ForPSV
{
    /// <summary> PSV特化输入解决器,只能用于PSV平台,并且只支持PSV控制器 </summary>
    public class PSVResolver : InputResolver
    {
        List<InputDevice> m_devices = new List<InputDevice>();
        PSVController m_psvController;

        protected override void OnInit()
        {
            m_psvController = new PSVController(this);
            m_devices.Add(m_psvController);
        }

        public override IEnumerable<InputDevice> GetDevices()
        {
            return m_devices;
        }

        public override bool CheckOnline(InputDevice device)
        {
            return device == m_psvController;
        }

        public override string GetDeviceName(InputDevice inputDevice)
        {
            Debug.Assert(inputDevice == m_psvController, "只支持psv控制器");

            return nameof(PSVController);
        }

        public override bool CheckPerforming<CONTROLLER>(CONTROLLER control)
        {
            if (control.Device is PSVController psvCon)
            {
                if (control == psvCon.Cross) return Input.GetKey(KeyCode.Joystick1Button0);
                else if (control == psvCon.Circle) return Input.GetKey(KeyCode.Joystick1Button1);
                else if (control == psvCon.Square) return Input.GetKey(KeyCode.Joystick1Button2);
                else if (control == psvCon.Triangle) return Input.GetKey(KeyCode.Joystick1Button3);
                else if (control == psvCon.L) return Input.GetKey(KeyCode.Joystick1Button4);
                else if (control == psvCon.R) return Input.GetKey(KeyCode.Joystick1Button5);
                else if (control == psvCon.Select) return Input.GetKey(KeyCode.Joystick1Button6);
                else if (control == psvCon.Start) return Input.GetKey(KeyCode.Joystick1Button7);
                else if (control == psvCon.Up) return Input.GetKey(KeyCode.Joystick1Button8);
                else if (control == psvCon.Right) return Input.GetKey(KeyCode.Joystick1Button9);
                else if (control == psvCon.Down) return Input.GetKey(KeyCode.Joystick1Button10);
                else if (control == psvCon.Left) return Input.GetKey(KeyCode.Joystick1Button11);
                else if (control == psvCon.LeftStick || control == psvCon.RightStick)
                {
                    var vec2 = control.GetVector2();
                    return vec2.x != 0 || vec2.y != 0;
                }
            }

            throw new System.NotImplementedException();
        }

        public override Vector2 GetVector2<CONTROLLER>(CONTROLLER control)
        {
            if (control.Device is PSVController psvCon)
            {
                if (control == psvCon.LeftStick)
                {
                    return new Vector2(Input.GetAxis("Joy1 Axis X"), Input.GetAxis("Joy1 Axis Y"));
                }
                else if (control == psvCon.RightStick)
                {
                    return new Vector2(Input.GetAxis("Joy1 Axis 4"), Input.GetAxis("Joy1 Axis 5"));
                }
            }

            throw new System.NotImplementedException();
        }

        public override float GetFloat<CONTROLLER>(CONTROLLER control)
        {
            throw new System.NotImplementedException();
        }
    }
}
