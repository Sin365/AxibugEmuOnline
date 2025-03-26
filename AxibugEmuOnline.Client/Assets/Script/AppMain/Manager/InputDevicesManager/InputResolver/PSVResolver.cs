using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using static Essgee.Emulation.Audio.DMGAudio;
using static VirtualNes.Core.APU_INTERNAL;

namespace AxibugEmuOnline.Client.InputDevices.ForPSV
{
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
                if (control is PSVController.Button button)
                {
                    if (button == psvCon.Cross) return Input.GetKey(KeyCode.Joystick1Button0);
                    else if (button == psvCon.Circle) return Input.GetKey(KeyCode.Joystick1Button1);
                    else if (button == psvCon.Square) return Input.GetKey(KeyCode.Joystick1Button2);
                    else if (button == psvCon.Triangle) return Input.GetKey(KeyCode.Joystick1Button3);
                    else if (button == psvCon.L) return Input.GetKey(KeyCode.Joystick1Button4);
                    else if (button == psvCon.R) return Input.GetKey(KeyCode.Joystick1Button5);
                    else if (button == psvCon.Select) return Input.GetKey(KeyCode.Joystick1Button6);
                    else if (button == psvCon.Start) return Input.GetKey(KeyCode.Joystick1Button7);
                    else if (button == psvCon.Up) return Input.GetKey(KeyCode.Joystick1Button8);
                    else if (button == psvCon.Right) return Input.GetKey(KeyCode.Joystick1Button9);
                    else if (button == psvCon.Down) return Input.GetKey(KeyCode.Joystick1Button10);
                    else if (button == psvCon.Left) return Input.GetKey(KeyCode.Joystick1Button11);
                }
                else if (control is PSVController.Stick stick)
                {
                    var vec2 = stick.GetVector2();
                    return vec2.x != 0 || vec2.y != 0;
                }
            }

            throw new System.NotImplementedException();
        }

        public override Vector2 GetVector2<CONTROLLER>(CONTROLLER control)
        {
            if (control.Device is PSVController)
            {
                if (control is PSVController.Stick stick)
                {
                    Vector2 result = Vector2.zero;

                    if (stick.m_left)
                    {
                        result.x = Input.GetAxis("Joy1 Axis X");
                        result.y = Input.GetAxis("Joy1 Axis Y");
                    }
                    else
                    {
                        result.x = Input.GetAxis("Joy1 Axis 4");
                        result.y = Input.GetAxis("Joy1 Axis 5");
                    }

                    return result;
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
