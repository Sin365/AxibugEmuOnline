#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using IP = UnityEngine.InputSystem.InputSystem;
using IPDevice = UnityEngine.InputSystem.InputDevice;
using IPKeyboard = UnityEngine.InputSystem.Keyboard;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary> InputSystem对接 </summary>
    public class InputSystemResolver : InputResolver
    {
        DualWayDictionary<IPDevice, InputDevice> m_devices = new DualWayDictionary<IPDevice, InputDevice>();

        protected override void OnInit()
        {
            foreach (var device in IP.devices)
            {
                AddDevice(device);
            }

            IP.onDeviceChange += IP_onDeviceChange;
        }

        private void AddDevice(IPDevice ipdev)
        {
            InputDevice newDevice = null;
            if (ipdev is IPKeyboard) newDevice = new KeyBoard(this);

            if (newDevice != null)
            {
                m_devices.Add(ipdev, newDevice);
                RaiseDeviceConnected(newDevice);
            }
        }

        private void RemoveDevice(IPDevice ipdev)
        {
            if (m_devices.TryGetValue(ipdev, out var device))
            {
                m_devices.Remove(ipdev);
                RaiseDeviceLost(device);
            }
        }

        public override bool CheckOnline(InputDevice device)
        {
            return m_devices.TryGetKey(device, out var _);
        }

        private void IP_onDeviceChange(IPDevice device, UnityEngine.InputSystem.InputDeviceChange changeType)
        {
            switch (changeType)
            {
                case UnityEngine.InputSystem.InputDeviceChange.Added: AddDevice(device); break;
                case UnityEngine.InputSystem.InputDeviceChange.Removed: RemoveDevice(device); break;
            }
        }

        public override IEnumerable<InputDevice> GetDevices()
        {
            return m_devices.Values;
        }
    }
}
#endif