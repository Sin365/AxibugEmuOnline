using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 通用键盘设备
    /// </summary>
    public partial class Keyboard_D : InputDevice_D
    {
        Dictionary<KeyCode, KeyboardKey> m_keyControllerMap = new Dictionary<KeyCode, KeyboardKey>();

        public Keyboard_D(InputResolver resolver) : base(resolver) { }

        protected override List<InputControl_D> DefineControls()
        {
            var keys = s_keyboardKeys.Select(kc => new KeyboardKey(kc, this) as InputControl_D).ToList();
            foreach (KeyboardKey key in keys)
            {
                m_keyControllerMap.Add(key.m_keycode, key);
            }
            return keys;
        }

        public class KeyboardKey : Button_C
        {
            internal KeyCode m_keycode;

            internal KeyboardKey(KeyCode listenKey, Keyboard_D keyboard)
                : base(keyboard, listenKey.ToString())
            {
                m_keycode = listenKey;
            }
        }
    }

    #region HardCodeForKeyboard
    public partial class Keyboard_D : InputDevice_D
    {
        static readonly List<KeyCode> s_keyboardKeys = new List<KeyCode>
        {
            // 字母键 A-Z
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
            KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
            KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
            KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,

            // 数字键 0-9
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,

            // 功能键 F1-F15
            KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6,
            KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,

            // 方向键
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,

            // 控制键
            KeyCode.Space, KeyCode.Return, KeyCode.Escape, KeyCode.Tab, KeyCode.Backspace,
            KeyCode.CapsLock, KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl,
            KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftCommand,
            KeyCode.RightCommand,

            // 符号键
            KeyCode.Comma, KeyCode.Period, KeyCode.Slash, KeyCode.BackQuote, KeyCode.Quote,
            KeyCode.Semicolon, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash,
            KeyCode.Minus, KeyCode.Equals, 

            // 小键盘
            KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
            KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
            KeyCode.KeypadPeriod, KeyCode.KeypadDivide, KeyCode.KeypadMultiply,
            KeyCode.KeypadMinus, KeyCode.KeypadPlus, KeyCode.KeypadEnter, KeyCode.Numlock,

            // 其他标准键
            KeyCode.Print,  KeyCode.Insert, KeyCode.Delete, KeyCode.Home,
            KeyCode.End, KeyCode.PageUp, KeyCode.PageDown, KeyCode.Pause, KeyCode.ScrollLock,
        };

        public KeyboardKey A => m_keyControllerMap[KeyCode.A];
        public KeyboardKey B => m_keyControllerMap[KeyCode.B];
        public KeyboardKey C => m_keyControllerMap[KeyCode.C];
        public KeyboardKey D => m_keyControllerMap[KeyCode.D];
        public KeyboardKey E => m_keyControllerMap[KeyCode.E];
        public KeyboardKey F => m_keyControllerMap[KeyCode.F];
        public KeyboardKey G => m_keyControllerMap[KeyCode.G];
        public KeyboardKey H => m_keyControllerMap[KeyCode.H];
        public KeyboardKey I => m_keyControllerMap[KeyCode.I];
        public KeyboardKey J => m_keyControllerMap[KeyCode.J];
        public KeyboardKey K => m_keyControllerMap[KeyCode.K];
        public KeyboardKey L => m_keyControllerMap[KeyCode.L];
        public KeyboardKey M => m_keyControllerMap[KeyCode.M];
        public KeyboardKey N => m_keyControllerMap[KeyCode.N];
        public KeyboardKey O => m_keyControllerMap[KeyCode.O];
        public KeyboardKey P => m_keyControllerMap[KeyCode.P];
        public KeyboardKey Q => m_keyControllerMap[KeyCode.Q];
        public KeyboardKey R => m_keyControllerMap[KeyCode.R];
        public KeyboardKey S => m_keyControllerMap[KeyCode.S];
        public KeyboardKey T => m_keyControllerMap[KeyCode.T];
        public KeyboardKey U => m_keyControllerMap[KeyCode.U];
        public KeyboardKey V => m_keyControllerMap[KeyCode.V];
        public KeyboardKey W => m_keyControllerMap[KeyCode.W];
        public KeyboardKey X => m_keyControllerMap[KeyCode.X];
        public KeyboardKey Y => m_keyControllerMap[KeyCode.Y];
        public KeyboardKey Z => m_keyControllerMap[KeyCode.Z];
        public KeyboardKey Alpha0 => m_keyControllerMap[KeyCode.Alpha0];
        public KeyboardKey Alpha1 => m_keyControllerMap[KeyCode.Alpha1];
        public KeyboardKey Alpha2 => m_keyControllerMap[KeyCode.Alpha2];
        public KeyboardKey Alpha3 => m_keyControllerMap[KeyCode.Alpha3];
        public KeyboardKey Alpha4 => m_keyControllerMap[KeyCode.Alpha4];
        public KeyboardKey Alpha5 => m_keyControllerMap[KeyCode.Alpha5];
        public KeyboardKey Alpha6 => m_keyControllerMap[KeyCode.Alpha6];
        public KeyboardKey Alpha7 => m_keyControllerMap[KeyCode.Alpha7];
        public KeyboardKey Alpha8 => m_keyControllerMap[KeyCode.Alpha8];
        public KeyboardKey Alpha9 => m_keyControllerMap[KeyCode.Alpha9];
        public KeyboardKey F1 => m_keyControllerMap[KeyCode.F1];
        public KeyboardKey F2 => m_keyControllerMap[KeyCode.F2];
        public KeyboardKey F3 => m_keyControllerMap[KeyCode.F3];
        public KeyboardKey F4 => m_keyControllerMap[KeyCode.F4];
        public KeyboardKey F5 => m_keyControllerMap[KeyCode.F5];
        public KeyboardKey F6 => m_keyControllerMap[KeyCode.F6];
        public KeyboardKey F7 => m_keyControllerMap[KeyCode.F7];
        public KeyboardKey F8 => m_keyControllerMap[KeyCode.F8];
        public KeyboardKey F9 => m_keyControllerMap[KeyCode.F9];
        public KeyboardKey F10 => m_keyControllerMap[KeyCode.F10];
        public KeyboardKey F11 => m_keyControllerMap[KeyCode.F11];
        public KeyboardKey F12 => m_keyControllerMap[KeyCode.F12];
        public KeyboardKey F13 => m_keyControllerMap[KeyCode.F13];
        public KeyboardKey F14 => m_keyControllerMap[KeyCode.F14];
        public KeyboardKey F15 => m_keyControllerMap[KeyCode.F15];
        public KeyboardKey UpArrow => m_keyControllerMap[KeyCode.UpArrow];
        public KeyboardKey DownArrow => m_keyControllerMap[KeyCode.DownArrow];
        public KeyboardKey LeftArrow => m_keyControllerMap[KeyCode.LeftArrow];
        public KeyboardKey RightArrow => m_keyControllerMap[KeyCode.RightArrow];
        public KeyboardKey Space => m_keyControllerMap[KeyCode.Space];
        public KeyboardKey Return => m_keyControllerMap[KeyCode.Return];
        public KeyboardKey Escape => m_keyControllerMap[KeyCode.Escape];
        public KeyboardKey Tab => m_keyControllerMap[KeyCode.Tab];
        public KeyboardKey Backspace => m_keyControllerMap[KeyCode.Backspace];
        public KeyboardKey CapsLock => m_keyControllerMap[KeyCode.CapsLock];
        public KeyboardKey LeftShift => m_keyControllerMap[KeyCode.LeftShift];
        public KeyboardKey RightShift => m_keyControllerMap[KeyCode.RightShift];
        public KeyboardKey LeftControl => m_keyControllerMap[KeyCode.LeftControl];
        public KeyboardKey RightControl => m_keyControllerMap[KeyCode.RightControl];
        public KeyboardKey LeftAlt => m_keyControllerMap[KeyCode.LeftAlt];
        public KeyboardKey RightAlt => m_keyControllerMap[KeyCode.RightAlt];
        public KeyboardKey LeftCommand => m_keyControllerMap[KeyCode.LeftCommand];
        public KeyboardKey RightCommand => m_keyControllerMap[KeyCode.RightCommand];
        public KeyboardKey Comma => m_keyControllerMap[KeyCode.Comma];
        public KeyboardKey Period => m_keyControllerMap[KeyCode.Period];
        public KeyboardKey Slash => m_keyControllerMap[KeyCode.Slash];
        public KeyboardKey BackQuote => m_keyControllerMap[KeyCode.BackQuote];
        public KeyboardKey Quote => m_keyControllerMap[KeyCode.Quote];
        public KeyboardKey Semicolon => m_keyControllerMap[KeyCode.Semicolon];
        public KeyboardKey LeftBracket => m_keyControllerMap[KeyCode.LeftBracket];
        public KeyboardKey RightBracket => m_keyControllerMap[KeyCode.RightBracket];
        public KeyboardKey Backslash => m_keyControllerMap[KeyCode.Backslash];
        public KeyboardKey Minus => m_keyControllerMap[KeyCode.Minus];
        public KeyboardKey Equals_k => m_keyControllerMap[KeyCode.Equals];
        public KeyboardKey Keypad0 => m_keyControllerMap[KeyCode.Keypad0];
        public KeyboardKey Keypad1 => m_keyControllerMap[KeyCode.Keypad1];
        public KeyboardKey Keypad2 => m_keyControllerMap[KeyCode.Keypad2];
        public KeyboardKey Keypad3 => m_keyControllerMap[KeyCode.Keypad3];
        public KeyboardKey Keypad4 => m_keyControllerMap[KeyCode.Keypad4];
        public KeyboardKey Keypad5 => m_keyControllerMap[KeyCode.Keypad5];
        public KeyboardKey Keypad6 => m_keyControllerMap[KeyCode.Keypad6];
        public KeyboardKey Keypad7 => m_keyControllerMap[KeyCode.Keypad7];
        public KeyboardKey Keypad8 => m_keyControllerMap[KeyCode.Keypad8];
        public KeyboardKey Keypad9 => m_keyControllerMap[KeyCode.Keypad9];
        public KeyboardKey KeypadPeriod => m_keyControllerMap[KeyCode.KeypadPeriod];
        public KeyboardKey KeypadDivide => m_keyControllerMap[KeyCode.KeypadDivide];
        public KeyboardKey KeypadMultiply => m_keyControllerMap[KeyCode.KeypadMultiply];
        public KeyboardKey KeypadMinus => m_keyControllerMap[KeyCode.KeypadMinus];
        public KeyboardKey KeypadPlus => m_keyControllerMap[KeyCode.KeypadPlus];
        public KeyboardKey KeypadEnter => m_keyControllerMap[KeyCode.KeypadEnter];
        public KeyboardKey Numlock => m_keyControllerMap[KeyCode.Numlock];
        public KeyboardKey Print => m_keyControllerMap[KeyCode.Print];
        public KeyboardKey Insert => m_keyControllerMap[KeyCode.Insert];
        public KeyboardKey Delete => m_keyControllerMap[KeyCode.Delete];
        public KeyboardKey Home => m_keyControllerMap[KeyCode.Home];
        public KeyboardKey End => m_keyControllerMap[KeyCode.End];
        public KeyboardKey PageUp => m_keyControllerMap[KeyCode.PageUp];
        public KeyboardKey PageDown => m_keyControllerMap[KeyCode.PageDown];
        public KeyboardKey Pause => m_keyControllerMap[KeyCode.Pause];
        public KeyboardKey ScrollLock => m_keyControllerMap[KeyCode.ScrollLock];
    }
    #endregion
}