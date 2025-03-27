#if ENABLE_INPUT_SYSTEM
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AxibugEmuOnline.Client.InputDevices.ForInputSystem
{
    /// <summary> 基于UnityInputSystem实现的输入解决器 </summary>
    public class InputSystemResolver : InputResolver
    {
        DualWayDictionary<InputDevice, InputDevice_D> m_devices = new DualWayDictionary<InputDevice, InputDevice_D>();

        protected override void OnInit()
        {
            foreach (var device in InputSystem.devices) AddDevice(device);

            InputSystem.onDeviceChange += IP_onDeviceChange;
        }

        private void AddDevice(InputDevice ipdev)
        {
            InputDevice_D newDevice = null;
            if (ipdev is Keyboard)
            {
                newDevice = new Keyboard_D(this);
                AddKeyboardMapper((Keyboard_D)newDevice, (Keyboard)ipdev);
            }
            else if (ipdev is Gamepad) newDevice = new GamePad_D(this);

            if (newDevice != null)
            {
                m_devices.Add(ipdev, newDevice);
                RaiseDeviceConnected(newDevice);
            }
        }

        private void RemoveDevice(InputDevice ipdev)
        {
            if (m_devices.TryGetValue(ipdev, out var device))
            {
                m_devices.Remove(ipdev);
                RaiseDeviceLost(device);

                if (device is Keyboard_D)
                {
                    RemoveKeyboardMapper((Keyboard_D)device);
                }
            }
        }

        private T GetInputSystemDevice<T>(InputDevice_D device) where T : InputDevice
        {
            m_devices.TryGetKey(device, out var ipDev);
            return ipDev as T;
        }

        public override string GetDeviceName(InputDevice_D inputDevice)
        {
            var ipdev = GetInputSystemDevice<InputDevice>(inputDevice);
            Debug.Assert(ipdev != null, "不能对已离线的设备获取名称");

            return $"{ipdev.description.deviceClass}_{ipdev.description.interfaceName}_{ipdev.deviceId}";
        }

        public override bool CheckOnline(InputDevice_D device)
        {
            return m_devices.TryGetKey(device, out var _);
        }

        private void IP_onDeviceChange(InputDevice device, InputDeviceChange changeType)
        {
            switch (changeType)
            {
                case InputDeviceChange.Added: AddDevice(device); break;
                case InputDeviceChange.Removed: RemoveDevice(device); break;
            }
        }

        public override IEnumerable<InputDevice_D> GetDevices()
        {
            return m_devices.Values;
        }

        public override bool CheckPerforming<CONTROLLER>(CONTROLLER control)
        {
            if (control.Device is Keyboard_D keyboard)
            {
                var ipKeyboard = GetInputSystemDevice<Keyboard>(keyboard);
                var k = GetIPKeyboardKey(ipKeyboard, keyboard, control);
                return k.isPressed;
            }

            throw new System.NotImplementedException();
        }

        public override Vector2 GetVector2<CONTROLLER>(CONTROLLER control)
        {
            throw new System.NotImplementedException();
        }

        public override float GetFloat<CONTROLLER>(CONTROLLER control)
        {
            throw new System.NotImplementedException();
        }

        ButtonControl GetIPKeyboardKey(Keyboard ipKeyboard, Keyboard_D keyboard, InputControl_C key)
        {
            var mapper = m_keyboardMapper[keyboard];
            mapper.TryGetValue(key, out ButtonControl inputBtn);
            if (inputBtn != null)
                return inputBtn;
            else
            {
                throw new System.Exception($"not found keyboard mapper setting : {key}");
            }
        }
        Dictionary<Keyboard_D, Dictionary<InputControl_C, ButtonControl>> m_keyboardMapper = new Dictionary<Keyboard_D, Dictionary<InputControl_C, ButtonControl>>();
        void AddKeyboardMapper(Keyboard_D keyboard_d, Keyboard ipkeyboard)
        {
            m_keyboardMapper.Add(keyboard_d, new Dictionary<InputControl_C, ButtonControl>());
            var mapper = m_keyboardMapper[keyboard_d];

            mapper.Add(keyboard_d.A, ipkeyboard.aKey);
            mapper.Add(keyboard_d.B, ipkeyboard.bKey);
            mapper.Add(keyboard_d.C, ipkeyboard.cKey);
            mapper.Add(keyboard_d.D, ipkeyboard.dKey);
            mapper.Add(keyboard_d.E, ipkeyboard.eKey);
            mapper.Add(keyboard_d.F, ipkeyboard.fKey);
            mapper.Add(keyboard_d.G, ipkeyboard.gKey);
            mapper.Add(keyboard_d.H, ipkeyboard.hKey);
            mapper.Add(keyboard_d.I, ipkeyboard.iKey);
            mapper.Add(keyboard_d.J, ipkeyboard.jKey);
            mapper.Add(keyboard_d.K, ipkeyboard.kKey);
            mapper.Add(keyboard_d.L, ipkeyboard.lKey);
            mapper.Add(keyboard_d.M, ipkeyboard.mKey);
            mapper.Add(keyboard_d.N, ipkeyboard.nKey);
            mapper.Add(keyboard_d.O, ipkeyboard.oKey);
            mapper.Add(keyboard_d.P, ipkeyboard.pKey);
            mapper.Add(keyboard_d.Q, ipkeyboard.qKey);
            mapper.Add(keyboard_d.R, ipkeyboard.rKey);
            mapper.Add(keyboard_d.S, ipkeyboard.sKey);
            mapper.Add(keyboard_d.T, ipkeyboard.tKey);
            mapper.Add(keyboard_d.U, ipkeyboard.uKey);
            mapper.Add(keyboard_d.V, ipkeyboard.vKey);
            mapper.Add(keyboard_d.W, ipkeyboard.wKey);
            mapper.Add(keyboard_d.X, ipkeyboard.xKey);
            mapper.Add(keyboard_d.Y, ipkeyboard.yKey);
            mapper.Add(keyboard_d.Z, ipkeyboard.zKey);
            mapper.Add(keyboard_d.Alpha0, ipkeyboard.digit0Key);
            mapper.Add(keyboard_d.Alpha1, ipkeyboard.digit1Key);
            mapper.Add(keyboard_d.Alpha2, ipkeyboard.digit2Key);
            mapper.Add(keyboard_d.Alpha3, ipkeyboard.digit3Key);
            mapper.Add(keyboard_d.Alpha4, ipkeyboard.digit4Key);
            mapper.Add(keyboard_d.Alpha5, ipkeyboard.digit5Key);
            mapper.Add(keyboard_d.Alpha6, ipkeyboard.digit6Key);
            mapper.Add(keyboard_d.Alpha7, ipkeyboard.digit7Key);
            mapper.Add(keyboard_d.Alpha8, ipkeyboard.digit8Key);
            mapper.Add(keyboard_d.Alpha9, ipkeyboard.digit9Key);
            mapper.Add(keyboard_d.Keypad0, ipkeyboard.numpad0Key);
            mapper.Add(keyboard_d.Keypad1, ipkeyboard.numpad1Key);
            mapper.Add(keyboard_d.Keypad2, ipkeyboard.numpad2Key);
            mapper.Add(keyboard_d.Keypad3, ipkeyboard.numpad3Key);
            mapper.Add(keyboard_d.Keypad4, ipkeyboard.numpad4Key);
            mapper.Add(keyboard_d.Keypad5, ipkeyboard.numpad5Key);
            mapper.Add(keyboard_d.Keypad6, ipkeyboard.numpad6Key);
            mapper.Add(keyboard_d.Keypad7, ipkeyboard.numpad7Key);
            mapper.Add(keyboard_d.Keypad8, ipkeyboard.numpad8Key);
            mapper.Add(keyboard_d.Keypad9, ipkeyboard.numpad9Key);
            mapper.Add(keyboard_d.KeypadPeriod, ipkeyboard.numpadPeriodKey);
            mapper.Add(keyboard_d.KeypadDivide, ipkeyboard.numpadDivideKey);
            mapper.Add(keyboard_d.KeypadMultiply, ipkeyboard.numpadMultiplyKey);
            mapper.Add(keyboard_d.KeypadMinus, ipkeyboard.numpadMinusKey);
            mapper.Add(keyboard_d.KeypadPlus, ipkeyboard.numpadPlusKey);
            mapper.Add(keyboard_d.KeypadEnter, ipkeyboard.numpadEnterKey);
            mapper.Add(keyboard_d.F1, ipkeyboard.f1Key);
            mapper.Add(keyboard_d.F2, ipkeyboard.f2Key);
            mapper.Add(keyboard_d.F3, ipkeyboard.f3Key);
            mapper.Add(keyboard_d.F4, ipkeyboard.f4Key);
            mapper.Add(keyboard_d.F5, ipkeyboard.f5Key);
            mapper.Add(keyboard_d.F6, ipkeyboard.f6Key);
            mapper.Add(keyboard_d.F7, ipkeyboard.f7Key);
            mapper.Add(keyboard_d.F8, ipkeyboard.f8Key);
            mapper.Add(keyboard_d.F9, ipkeyboard.f9Key);
            mapper.Add(keyboard_d.F10, ipkeyboard.f10Key);
            mapper.Add(keyboard_d.F11, ipkeyboard.f11Key);
            mapper.Add(keyboard_d.F12, ipkeyboard.f12Key);
            mapper.Add(keyboard_d.UpArrow, ipkeyboard.upArrowKey);
            mapper.Add(keyboard_d.DownArrow, ipkeyboard.downArrowKey);
            mapper.Add(keyboard_d.LeftArrow, ipkeyboard.leftArrowKey);
            mapper.Add(keyboard_d.RightArrow, ipkeyboard.rightArrowKey);
            mapper.Add(keyboard_d.Space, ipkeyboard.spaceKey);
            mapper.Add(keyboard_d.Backspace, ipkeyboard.backspaceKey);
            mapper.Add(keyboard_d.Tab, ipkeyboard.tabKey);
            mapper.Add(keyboard_d.Return, ipkeyboard.enterKey);
            mapper.Add(keyboard_d.Escape, ipkeyboard.escapeKey);
            mapper.Add(keyboard_d.LeftShift, ipkeyboard.leftShiftKey);
            mapper.Add(keyboard_d.RightShift, ipkeyboard.rightShiftKey);
            mapper.Add(keyboard_d.LeftControl, ipkeyboard.leftCtrlKey);
            mapper.Add(keyboard_d.RightControl, ipkeyboard.rightCtrlKey);
            mapper.Add(keyboard_d.LeftAlt, ipkeyboard.leftAltKey);
            mapper.Add(keyboard_d.RightAlt, ipkeyboard.rightAltKey);
            mapper.Add(keyboard_d.LeftCommand, ipkeyboard.leftCommandKey);
            mapper.Add(keyboard_d.RightCommand, ipkeyboard.rightCommandKey);
            mapper.Add(keyboard_d.CapsLock, ipkeyboard.capsLockKey);
            mapper.Add(keyboard_d.Numlock, ipkeyboard.numLockKey);
            mapper.Add(keyboard_d.ScrollLock, ipkeyboard.scrollLockKey);
            mapper.Add(keyboard_d.Print, ipkeyboard.printScreenKey);
            mapper.Add(keyboard_d.Pause, ipkeyboard.pauseKey);
            mapper.Add(keyboard_d.Insert, ipkeyboard.insertKey);
            mapper.Add(keyboard_d.Home, ipkeyboard.homeKey);
            mapper.Add(keyboard_d.End, ipkeyboard.endKey);
            mapper.Add(keyboard_d.PageUp, ipkeyboard.pageUpKey);
            mapper.Add(keyboard_d.PageDown, ipkeyboard.pageDownKey);
            mapper.Add(keyboard_d.Delete, ipkeyboard.deleteKey);
            mapper.Add(keyboard_d.Comma, ipkeyboard.commaKey);
            mapper.Add(keyboard_d.Period, ipkeyboard.periodKey);
            mapper.Add(keyboard_d.Slash, ipkeyboard.slashKey);
            mapper.Add(keyboard_d.BackQuote, ipkeyboard.backquoteKey);
            mapper.Add(keyboard_d.Minus, ipkeyboard.minusKey);
            mapper.Add(keyboard_d.Equals_k, ipkeyboard.equalsKey);
            mapper.Add(keyboard_d.LeftBracket, ipkeyboard.leftBracketKey);
            mapper.Add(keyboard_d.RightBracket, ipkeyboard.rightBracketKey);
            mapper.Add(keyboard_d.Backslash, ipkeyboard.backslashKey);
            mapper.Add(keyboard_d.Semicolon, ipkeyboard.semicolonKey);
            mapper.Add(keyboard_d.Quote, ipkeyboard.quoteKey);
        }
        void RemoveKeyboardMapper(Keyboard_D keyboard_d)
        {
            m_keyboardMapper.Remove(keyboard_d);
        }
    }

}
#endif