#if ENABLE_INPUT_SYSTEM

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WSA || PACKAGE_DOCS_GENERATION
#define DUALSHOCK_SUPPORT
#endif

#if UNITY_EDITOR || UNITY_SWITCH || PACKAGE_DOCS_GENERATION
#define JOYCON_SUPPORT
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

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
            if (ipdev is Keyboard) newDevice = new Keyboard_D(this);

#if DUALSHOCK_SUPPORT
            else if (ipdev is DualShockGamepad)
            {
                if (ipdev is DualShock3GamepadHID) newDevice = new DualShockController_D(this, ps3: true);
                else if (ipdev is DualShock4GamepadHID) newDevice = new DualShockController_D(this, ps4: true);
                else if (ipdev is DualSenseGamepadHID) newDevice = new DualShockController_D(this, ps5: true);
                else newDevice = new DualShockController_D(this);
            }
#endif
            else if (ipdev is XInputController)
            {
                newDevice = new XboxController_D(this);
            }
#if JOYCON_SUPPORT
            else if (ipdev is NPad)
            {
                newDevice = new SwitchJoyCon_D(this);
            }
#endif
            else if (ipdev is Gamepad) newDevice = new GamePad_D(this); //注意Gamepad的优先级,因为任何手柄,Inputsystem中的基类都是GamePad

            if (newDevice != null)
            {
                m_devices.Add(ipdev, newDevice);
                AddDeviceMapper(newDevice, ipdev);
                RaiseDeviceConnected(newDevice);
            }
        }

        private void RemoveDevice(InputDevice ipdev)
        {
            if (m_devices.TryGetValue(ipdev, out var device))
            {
                RemoveDeviceMapper(device);
                RaiseDeviceLost(device);
                m_devices.Remove(ipdev);
            }
        }

        private T GetInputSystemDevice<T>(InputDevice_D device) where T : InputDevice
        {
            m_devices.TryGetKey(device, out var ipDev);
            return ipDev as T;
        }

        protected override string OnGetDeviceName(InputDevice_D inputDevice)
        {
            var ipdev = GetInputSystemDevice<InputDevice>(inputDevice);
            Debug.Assert(ipdev != null, "不能对已离线的设备获取名称");

            return $"{ipdev.description}_{ipdev.deviceId}";
        }

        protected override bool OnCheckOnline(InputDevice_D device)
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

        protected override IEnumerable<InputDevice_D> OnGetDevices()
        {
            return m_devices.Values;
        }

        protected override bool OnCheckPerforming<CONTROLLER>(CONTROLLER control)
        {
            var ipControl = GetInputSystemControl(control);
            return ipControl.IsPressed();
        }

        protected override Vector2 OnGetVector2<CONTROLLER>(CONTROLLER control)
        {
            var ipControl = GetInputSystemControl(control);
            return (ipControl as InputControl<Vector2>).ReadValue();
        }

        protected override float OnGetFloat<CONTROLLER>(CONTROLLER control)
        {
            var ipControl = GetInputSystemControl(control);
            return (ipControl as InputControl<float>).ReadValue();
        }

        InputControl GetInputSystemControl(InputControl_C key)
        {
            var device_d = key.Device;
            var mapper = m_deviceMapper[device_d];
            mapper.TryGetValue(key, out InputControl inputBtn);
            if (inputBtn != null)
                return inputBtn;
            else
            {
                throw new System.Exception($"not found mapper setting : {key}");
            }
        }
        Dictionary<InputDevice_D, Dictionary<InputControl_C, InputControl>> m_deviceMapper = new Dictionary<InputDevice_D, Dictionary<InputControl_C, InputControl>>();
        void AddDeviceMapper(InputDevice_D device_d, InputDevice ipdevice)
        {
            m_deviceMapper.Add(device_d, new Dictionary<InputControl_C, InputControl>());
            var mapper = m_deviceMapper[device_d];

            if (device_d is Keyboard_D keyboard_d)
            {
                var ipKeyboard = ipdevice as Keyboard;

                mapper[keyboard_d.A] = ipKeyboard.aKey;
                mapper[keyboard_d.B] = ipKeyboard.bKey;
                mapper[keyboard_d.C] = ipKeyboard.cKey;
                mapper[keyboard_d.D] = ipKeyboard.dKey;
                mapper[keyboard_d.E] = ipKeyboard.eKey;
                mapper[keyboard_d.F] = ipKeyboard.fKey;
                mapper[keyboard_d.G] = ipKeyboard.gKey;
                mapper[keyboard_d.H] = ipKeyboard.hKey;
                mapper[keyboard_d.I] = ipKeyboard.iKey;
                mapper[keyboard_d.J] = ipKeyboard.jKey;
                mapper[keyboard_d.K] = ipKeyboard.kKey;
                mapper[keyboard_d.L] = ipKeyboard.lKey;
                mapper[keyboard_d.M] = ipKeyboard.mKey;
                mapper[keyboard_d.N] = ipKeyboard.nKey;
                mapper[keyboard_d.O] = ipKeyboard.oKey;
                mapper[keyboard_d.P] = ipKeyboard.pKey;
                mapper[keyboard_d.Q] = ipKeyboard.qKey;
                mapper[keyboard_d.R] = ipKeyboard.rKey;
                mapper[keyboard_d.S] = ipKeyboard.sKey;
                mapper[keyboard_d.T] = ipKeyboard.tKey;
                mapper[keyboard_d.U] = ipKeyboard.uKey;
                mapper[keyboard_d.V] = ipKeyboard.vKey;
                mapper[keyboard_d.W] = ipKeyboard.wKey;
                mapper[keyboard_d.X] = ipKeyboard.xKey;
                mapper[keyboard_d.Y] = ipKeyboard.yKey;
                mapper[keyboard_d.Z] = ipKeyboard.zKey;
                mapper[keyboard_d.Alpha0] = ipKeyboard.digit0Key;
                mapper[keyboard_d.Alpha1] = ipKeyboard.digit1Key;
                mapper[keyboard_d.Alpha2] = ipKeyboard.digit2Key;
                mapper[keyboard_d.Alpha3] = ipKeyboard.digit3Key;
                mapper[keyboard_d.Alpha4] = ipKeyboard.digit4Key;
                mapper[keyboard_d.Alpha5] = ipKeyboard.digit5Key;
                mapper[keyboard_d.Alpha6] = ipKeyboard.digit6Key;
                mapper[keyboard_d.Alpha7] = ipKeyboard.digit7Key;
                mapper[keyboard_d.Alpha8] = ipKeyboard.digit8Key;
                mapper[keyboard_d.Alpha9] = ipKeyboard.digit9Key;
                mapper[keyboard_d.Keypad0] = ipKeyboard.numpad0Key;
                mapper[keyboard_d.Keypad1] = ipKeyboard.numpad1Key;
                mapper[keyboard_d.Keypad2] = ipKeyboard.numpad2Key;
                mapper[keyboard_d.Keypad3] = ipKeyboard.numpad3Key;
                mapper[keyboard_d.Keypad4] = ipKeyboard.numpad4Key;
                mapper[keyboard_d.Keypad5] = ipKeyboard.numpad5Key;
                mapper[keyboard_d.Keypad6] = ipKeyboard.numpad6Key;
                mapper[keyboard_d.Keypad7] = ipKeyboard.numpad7Key;
                mapper[keyboard_d.Keypad8] = ipKeyboard.numpad8Key;
                mapper[keyboard_d.Keypad9] = ipKeyboard.numpad9Key;
                mapper[keyboard_d.KeypadPeriod] = ipKeyboard.numpadPeriodKey;
                mapper[keyboard_d.KeypadDivide] = ipKeyboard.numpadDivideKey;
                mapper[keyboard_d.KeypadMultiply] = ipKeyboard.numpadMultiplyKey;
                mapper[keyboard_d.KeypadMinus] = ipKeyboard.numpadMinusKey;
                mapper[keyboard_d.KeypadPlus] = ipKeyboard.numpadPlusKey;
                mapper[keyboard_d.KeypadEnter] = ipKeyboard.numpadEnterKey;
                mapper[keyboard_d.F1] = ipKeyboard.f1Key;
                mapper[keyboard_d.F2] = ipKeyboard.f2Key;
                mapper[keyboard_d.F3] = ipKeyboard.f3Key;
                mapper[keyboard_d.F4] = ipKeyboard.f4Key;
                mapper[keyboard_d.F5] = ipKeyboard.f5Key;
                mapper[keyboard_d.F6] = ipKeyboard.f6Key;
                mapper[keyboard_d.F7] = ipKeyboard.f7Key;
                mapper[keyboard_d.F8] = ipKeyboard.f8Key;
                mapper[keyboard_d.F9] = ipKeyboard.f9Key;
                mapper[keyboard_d.F10] = ipKeyboard.f10Key;
                mapper[keyboard_d.F11] = ipKeyboard.f11Key;
                mapper[keyboard_d.F12] = ipKeyboard.f12Key;
                mapper[keyboard_d.UpArrow] = ipKeyboard.upArrowKey;
                mapper[keyboard_d.DownArrow] = ipKeyboard.downArrowKey;
                mapper[keyboard_d.LeftArrow] = ipKeyboard.leftArrowKey;
                mapper[keyboard_d.RightArrow] = ipKeyboard.rightArrowKey;
                mapper[keyboard_d.Space] = ipKeyboard.spaceKey;
                mapper[keyboard_d.Backspace] = ipKeyboard.backspaceKey;
                mapper[keyboard_d.Tab] = ipKeyboard.tabKey;
                mapper[keyboard_d.Return] = ipKeyboard.enterKey;
                mapper[keyboard_d.Escape] = ipKeyboard.escapeKey;
                mapper[keyboard_d.LeftShift] = ipKeyboard.leftShiftKey;
                mapper[keyboard_d.RightShift] = ipKeyboard.rightShiftKey;
                mapper[keyboard_d.LeftControl] = ipKeyboard.leftCtrlKey;
                mapper[keyboard_d.RightControl] = ipKeyboard.rightCtrlKey;
                mapper[keyboard_d.LeftAlt] = ipKeyboard.leftAltKey;
                mapper[keyboard_d.RightAlt] = ipKeyboard.rightAltKey;
                mapper[keyboard_d.LeftCommand] = ipKeyboard.leftCommandKey;
                mapper[keyboard_d.RightCommand] = ipKeyboard.rightCommandKey;
                mapper[keyboard_d.CapsLock] = ipKeyboard.capsLockKey;
                mapper[keyboard_d.Numlock] = ipKeyboard.numLockKey;
                mapper[keyboard_d.ScrollLock] = ipKeyboard.scrollLockKey;
                mapper[keyboard_d.Print] = ipKeyboard.printScreenKey;
                mapper[keyboard_d.Pause] = ipKeyboard.pauseKey;
                mapper[keyboard_d.Insert] = ipKeyboard.insertKey;
                mapper[keyboard_d.Home] = ipKeyboard.homeKey;
                mapper[keyboard_d.End] = ipKeyboard.endKey;
                mapper[keyboard_d.PageUp] = ipKeyboard.pageUpKey;
                mapper[keyboard_d.PageDown] = ipKeyboard.pageDownKey;
                mapper[keyboard_d.Delete] = ipKeyboard.deleteKey;
                mapper[keyboard_d.Comma] = ipKeyboard.commaKey;
                mapper[keyboard_d.Period] = ipKeyboard.periodKey;
                mapper[keyboard_d.Slash] = ipKeyboard.slashKey;
                mapper[keyboard_d.BackQuote] = ipKeyboard.backquoteKey;
                mapper[keyboard_d.Minus] = ipKeyboard.minusKey;
                mapper[keyboard_d.Equals_k] = ipKeyboard.equalsKey;
                mapper[keyboard_d.LeftBracket] = ipKeyboard.leftBracketKey;
                mapper[keyboard_d.RightBracket] = ipKeyboard.rightBracketKey;
                mapper[keyboard_d.Backslash] = ipKeyboard.backslashKey;
                mapper[keyboard_d.Semicolon] = ipKeyboard.semicolonKey;
                mapper[keyboard_d.Quote] = ipKeyboard.quoteKey;
            }
#if DUALSHOCK_SUPPORT
            else if (device_d is DualShockController_D ds_d)
            {
                var ipDsGamePad = ipdevice as DualShockGamepad;
                mapper[ds_d.Circle] = ipDsGamePad.circleButton;
                mapper[ds_d.Triangle] = ipDsGamePad.triangleButton;
                mapper[ds_d.Cross] = ipDsGamePad.crossButton;
                mapper[ds_d.Square] = ipDsGamePad.squareButton;
                mapper[ds_d.Up] = ipDsGamePad.dpad.up;
                mapper[ds_d.Down] = ipDsGamePad.dpad.down;
                mapper[ds_d.Left] = ipDsGamePad.dpad.left;
                mapper[ds_d.Right] = ipDsGamePad.dpad.right;
                mapper[ds_d.L1] = ipDsGamePad.L1;
                mapper[ds_d.L2] = ipDsGamePad.L2;
                mapper[ds_d.L3] = ipDsGamePad.L3;
                mapper[ds_d.R1] = ipDsGamePad.R1;
                mapper[ds_d.R2] = ipDsGamePad.R2;
                mapper[ds_d.R3] = ipDsGamePad.R3;
                mapper[ds_d.Share] = ipDsGamePad.shareButton;
                mapper[ds_d.Options] = ipDsGamePad.optionsButton;
                mapper[ds_d.TouchpadBtn] = ipDsGamePad.touchpadButton;
                mapper[ds_d.LeftStick] = ipDsGamePad.leftStick;
                mapper[ds_d.RightStick] = ipDsGamePad.rightStick;
            }
#endif
            else if (device_d is XboxController_D xbox_d)
            {
                var ipXInputGamePad = ipdevice as XInputController;
                mapper[xbox_d.X] = ipXInputGamePad.xButton;
                mapper[xbox_d.Y] = ipXInputGamePad.yButton;
                mapper[xbox_d.A] = ipXInputGamePad.aButton;
                mapper[xbox_d.B] = ipXInputGamePad.bButton;
                mapper[xbox_d.Up] = ipXInputGamePad.dpad.up;
                mapper[xbox_d.Down] = ipXInputGamePad.dpad.down;
                mapper[xbox_d.Left] = ipXInputGamePad.dpad.left;
                mapper[xbox_d.Right] = ipXInputGamePad.dpad.right;
                mapper[xbox_d.View] = ipXInputGamePad.view;
                mapper[xbox_d.Menu] = ipXInputGamePad.menu;
                mapper[xbox_d.LeftBumper] = ipXInputGamePad.leftShoulder;
                mapper[xbox_d.LeftTrigger] = ipXInputGamePad.leftTrigger;
                mapper[xbox_d.LeftStickPress] = ipXInputGamePad.leftStickButton;
                mapper[xbox_d.RightBumper] = ipXInputGamePad.rightShoulder;
                mapper[xbox_d.RightTrigger] = ipXInputGamePad.rightTrigger;
                mapper[xbox_d.RightStickPress] = ipXInputGamePad.rightStickButton;
                mapper[xbox_d.LeftStick] = ipXInputGamePad.leftStick;
                mapper[xbox_d.RightStick] = ipXInputGamePad.rightStick;
            }
#if JOYCON_SUPPORT
            else if (device_d is SwitchJoyCon_D joycon_d)
            {
                var ipdevice_joycon = ipdevice as NPad;
                mapper[joycon_d.LeftSL] = ipdevice_joycon.leftSL;
                mapper[joycon_d.LeftSR] = ipdevice_joycon.leftSR;
                mapper[joycon_d.RightSL] = ipdevice_joycon.rightSL;
                mapper[joycon_d.RightSR] = ipdevice_joycon.rightSR;
                mapper[joycon_d.B] = ipdevice_joycon.bButton;
                mapper[joycon_d.A] = ipdevice_joycon.aButton;
                mapper[joycon_d.Y] = ipdevice_joycon.yButton;
                mapper[joycon_d.X] = ipdevice_joycon.xButton;
                mapper[joycon_d.Up] = ipdevice_joycon.dpad.up;
                mapper[joycon_d.Down] = ipdevice_joycon.dpad.down;
                mapper[joycon_d.Left] = ipdevice_joycon.dpad.left;
                mapper[joycon_d.Right] = ipdevice_joycon.dpad.right;
                mapper[joycon_d.Minus] = ipdevice_joycon.selectButton;
                mapper[joycon_d.Plus] = ipdevice_joycon.startButton;
                mapper[joycon_d.LeftStick] = ipdevice_joycon.leftStick;
                mapper[joycon_d.RightStick] = ipdevice_joycon.rightStick;
                mapper[joycon_d.RightStickPress] = ipdevice_joycon.rightStickButton;
                mapper[joycon_d.LeftStickPress] = ipdevice_joycon.leftStickButton;
            }
#endif
            else if (device_d is GamePad_D gamepad_d)
            {
                var ipGamepad = ipdevice as Gamepad;
                mapper[gamepad_d.Up] = ipGamepad.dpad.up;
                mapper[gamepad_d.Down] = ipGamepad.dpad.down;
                mapper[gamepad_d.Left] = ipGamepad.dpad.left;
                mapper[gamepad_d.Right] = ipGamepad.dpad.right;
                mapper[gamepad_d.Select] = ipGamepad.selectButton;
                mapper[gamepad_d.Start] = ipGamepad.startButton;
                mapper[gamepad_d.North] = ipGamepad.buttonNorth;
                mapper[gamepad_d.South] = ipGamepad.buttonSouth;
                mapper[gamepad_d.West] = ipGamepad.buttonWest;
                mapper[gamepad_d.East] = ipGamepad.buttonEast;
                mapper[gamepad_d.LeftShoulder] = ipGamepad.leftShoulder;
                mapper[gamepad_d.RightShoulder] = ipGamepad.rightShoulder;
                mapper[gamepad_d.LeftTrigger] = ipGamepad.leftTrigger;
                mapper[gamepad_d.RightTrigger] = ipGamepad.rightTrigger;
                mapper[gamepad_d.LeftStickPress] = ipGamepad.leftStickButton;
                mapper[gamepad_d.RightStickPress] = ipGamepad.rightStickButton;
                mapper[gamepad_d.LeftStick] = ipGamepad.leftStick;
                mapper[gamepad_d.RightStick] = ipGamepad.rightStick;

            }
            else throw new System.NotImplementedException($"初始化设备失败,未实现的物理按键映射 for {device_d.GetType()}");
        }
        void RemoveDeviceMapper(InputDevice_D keyboard_d)
        {
            m_deviceMapper.Remove(keyboard_d);
        }
    }
}

#endif