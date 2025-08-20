using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices.ForPSV
{
    /// <summary> PSV特化输入解决器,只能用于PSV平台,并且只支持PSV控制器 </summary>
    public class PSVResolver : InputResolver
    {
        List<InputDevice_D> m_devices = new List<InputDevice_D>();
        PSVController_D m_psvController;

        protected override void OnInit()
        {
            m_psvController = new PSVController_D(this);
            m_devices.Add(m_psvController);
        }

        protected override IEnumerable<InputDevice_D> OnGetDevices()
        {
            return m_devices;
        }

        protected override bool OnCheckOnline(InputDevice_D device)
        {
            return device == m_psvController;
        }

        protected override string OnGetDeviceName(InputDevice_D inputDevice)
        {
            Debug.Assert(inputDevice == m_psvController, "只支持psv控制器");

            return nameof(PSVController_D);
        }

        protected override bool OnCheckPerforming<CONTROLLER>(CONTROLLER control)
        {
            if (control.Device is PSVController_D psvCon)
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

        protected override Vector2 OnGetVector2<CONTROLLER>(CONTROLLER control)
        {
            if (control.Device is PSVController_D psvCon)
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

        protected override float OnGetFloat<CONTROLLER>(CONTROLLER control)
        {
            throw new System.NotImplementedException();
        }
    }
}
