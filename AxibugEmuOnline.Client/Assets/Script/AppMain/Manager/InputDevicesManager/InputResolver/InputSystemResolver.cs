#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using IP = UnityEngine.InputSystem.InputSystem;
using IPDevice = UnityEngine.InputSystem.InputDevice;
using IPKeyboard = UnityEngine.InputSystem.Keyboard;

namespace AxibugEmuOnline.Client.InputDevices.ForInputSystem
{
    /// <summary> InputSystem对接类 </summary>
    public partial class InputSystemResolver : InputResolver
    {
        DualWayDictionary<IPDevice, InputDevice> m_devices = new DualWayDictionary<IPDevice, InputDevice>();

        protected override void OnInit()
        {
            foreach (var device in IP.devices) AddDevice(device);

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

        public override string GetDeviceName(InputDevice inputDevice)
        {
            m_devices.TryGetKey(inputDevice, out var ipdev);
            Debug.Assert(ipdev != null, "不能对已离线的设备获取名称");

            return $"{ipdev.description.deviceClass}_{ipdev.description.interfaceName}_{ipdev.deviceId}";
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

        public override bool GetKey(KeyBoard keyboard, KeyCode key)
        {
            if (m_devices.TryGetKey(keyboard, out var ipdev))
            {
                var ipKeyboard = ipdev as IPKeyboard;
                if (ipKeyboard == null) return false;

                var k = GetIPKeyboardKey(ipKeyboard, key);
                return k.isPressed;
            }

            return false;
        }
    }

    public partial class InputSystemResolver : InputResolver
    {
        static ButtonControl GetIPKeyboardKey(IPKeyboard keyboard, KeyCode key)
        {
            switch (key)
            {
                // 字母键（A-Z）
                case KeyCode.A: return keyboard.aKey;
                case KeyCode.B: return keyboard.bKey;
                case KeyCode.C: return keyboard.cKey;
                case KeyCode.D: return keyboard.dKey;
                case KeyCode.E: return keyboard.eKey;
                case KeyCode.F: return keyboard.fKey;
                case KeyCode.G: return keyboard.gKey;
                case KeyCode.H: return keyboard.hKey;
                case KeyCode.I: return keyboard.iKey;
                case KeyCode.J: return keyboard.jKey;
                case KeyCode.K: return keyboard.kKey;
                case KeyCode.L: return keyboard.lKey;
                case KeyCode.M: return keyboard.mKey;
                case KeyCode.N: return keyboard.nKey;
                case KeyCode.O: return keyboard.oKey;
                case KeyCode.P: return keyboard.pKey;
                case KeyCode.Q: return keyboard.qKey;
                case KeyCode.R: return keyboard.rKey;
                case KeyCode.S: return keyboard.sKey;
                case KeyCode.T: return keyboard.tKey;
                case KeyCode.U: return keyboard.uKey;
                case KeyCode.V: return keyboard.vKey;
                case KeyCode.W: return keyboard.wKey;
                case KeyCode.X: return keyboard.xKey;
                case KeyCode.Y: return keyboard.yKey;
                case KeyCode.Z: return keyboard.zKey;

                // 数字键（0-9）
                case KeyCode.Alpha0: return keyboard.digit0Key;
                case KeyCode.Alpha1: return keyboard.digit1Key;
                case KeyCode.Alpha2: return keyboard.digit2Key;
                case KeyCode.Alpha3: return keyboard.digit3Key;
                case KeyCode.Alpha4: return keyboard.digit4Key;
                case KeyCode.Alpha5: return keyboard.digit5Key;
                case KeyCode.Alpha6: return keyboard.digit6Key;
                case KeyCode.Alpha7: return keyboard.digit7Key;
                case KeyCode.Alpha8: return keyboard.digit8Key;
                case KeyCode.Alpha9: return keyboard.digit9Key;

                // 小键盘
                case KeyCode.Keypad0: return keyboard.numpad0Key;
                case KeyCode.Keypad1: return keyboard.numpad1Key;
                case KeyCode.Keypad2: return keyboard.numpad2Key;
                case KeyCode.Keypad3: return keyboard.numpad3Key;
                case KeyCode.Keypad4: return keyboard.numpad4Key;
                case KeyCode.Keypad5: return keyboard.numpad5Key;
                case KeyCode.Keypad6: return keyboard.numpad6Key;
                case KeyCode.Keypad7: return keyboard.numpad7Key;
                case KeyCode.Keypad8: return keyboard.numpad8Key;
                case KeyCode.Keypad9: return keyboard.numpad9Key;
                case KeyCode.KeypadPeriod: return keyboard.numpadPeriodKey;
                case KeyCode.KeypadDivide: return keyboard.numpadDivideKey;
                case KeyCode.KeypadMultiply: return keyboard.numpadMultiplyKey;
                case KeyCode.KeypadMinus: return keyboard.numpadMinusKey;
                case KeyCode.KeypadPlus: return keyboard.numpadPlusKey;
                case KeyCode.KeypadEnter: return keyboard.numpadEnterKey;
                case KeyCode.KeypadEquals: return keyboard.numpadEqualsKey;

                // 功能键（F1-F15）
                case KeyCode.F1: return keyboard.f1Key;
                case KeyCode.F2: return keyboard.f2Key;
                case KeyCode.F3: return keyboard.f3Key;
                case KeyCode.F4: return keyboard.f4Key;
                case KeyCode.F5: return keyboard.f5Key;
                case KeyCode.F6: return keyboard.f6Key;
                case KeyCode.F7: return keyboard.f7Key;
                case KeyCode.F8: return keyboard.f8Key;
                case KeyCode.F9: return keyboard.f9Key;
                case KeyCode.F10: return keyboard.f10Key;
                case KeyCode.F11: return keyboard.f11Key;
                case KeyCode.F12: return keyboard.f12Key;

                // 方向键
                case KeyCode.UpArrow: return keyboard.upArrowKey;
                case KeyCode.DownArrow: return keyboard.downArrowKey;
                case KeyCode.LeftArrow: return keyboard.leftArrowKey;
                case KeyCode.RightArrow: return keyboard.rightArrowKey;

                // 符号键
                case KeyCode.Space: return keyboard.spaceKey;
                case KeyCode.Backspace: return keyboard.backspaceKey;
                case KeyCode.Tab: return keyboard.tabKey;
                case KeyCode.Return: return keyboard.enterKey;
                case KeyCode.Escape: return keyboard.escapeKey;
                case KeyCode.LeftShift: return keyboard.leftShiftKey;
                case KeyCode.RightShift: return keyboard.rightShiftKey;
                case KeyCode.LeftControl: return keyboard.leftCtrlKey;
                case KeyCode.RightControl: return keyboard.rightCtrlKey;
                case KeyCode.LeftAlt: return keyboard.leftAltKey;
                case KeyCode.RightAlt: return keyboard.rightAltKey;
                case KeyCode.LeftCommand: return keyboard.leftCommandKey; // macOS Command键
                case KeyCode.RightCommand: return keyboard.rightCommandKey;
                case KeyCode.CapsLock: return keyboard.capsLockKey;
                case KeyCode.Numlock: return keyboard.numLockKey;
                case KeyCode.ScrollLock: return keyboard.scrollLockKey;
                case KeyCode.Print: return keyboard.printScreenKey;
                case KeyCode.Pause: return keyboard.pauseKey;
                case KeyCode.Insert: return keyboard.insertKey;
                case KeyCode.Home: return keyboard.homeKey;
                case KeyCode.End: return keyboard.endKey;
                case KeyCode.PageUp: return keyboard.pageUpKey;
                case KeyCode.PageDown: return keyboard.pageDownKey;
                case KeyCode.Delete: return keyboard.deleteKey;
                case KeyCode.Comma: return keyboard.commaKey;
                case KeyCode.Period: return keyboard.periodKey;
                case KeyCode.Slash: return keyboard.slashKey;
                case KeyCode.BackQuote: return keyboard.backquoteKey;
                case KeyCode.Minus: return keyboard.minusKey;
                case KeyCode.Equals: return keyboard.equalsKey;
                case KeyCode.LeftBracket: return keyboard.leftBracketKey;
                case KeyCode.RightBracket: return keyboard.rightBracketKey;
                case KeyCode.Backslash: return keyboard.backslashKey;
                case KeyCode.Semicolon: return keyboard.semicolonKey;
                case KeyCode.Quote: return keyboard.quoteKey;
                default:
                    throw new System.NotImplementedException($"Not Find KeyCode Mapper Code from {key}");
            }
        }
    }
}
#endif