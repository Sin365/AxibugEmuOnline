using System.Collections.Generic;
using UnityEngine;

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

        public override bool GetKey(KeyBoard keyboard, KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public override string GetDeviceName(InputDevice inputDevice)
        {
            Debug.Assert(inputDevice == m_psvController, "只支持psv控制器");

            return nameof(PSVController);
        }
    }
}
