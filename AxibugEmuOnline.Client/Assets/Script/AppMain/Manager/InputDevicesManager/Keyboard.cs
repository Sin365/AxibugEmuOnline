using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client.InputDevices
{
    public partial class KeyBoard : InputDevice
    {
        public override string UniqueName => nameof(KeyBoard);
        public KeyBoard(InputResolver resolver) : base(resolver) { }

        protected override IEnumerable<InputControl> DefineControls()
        {
            var keys = s_keyboardKeys.Select(kc => new KeyboardKey(kc) as InputControl);
            return keys;
        }

        public class KeyboardKey : InputControl
        {
            KeyCode m_keycode;

            public override bool Start => Device.Resolver.GetKeyDown(Device as KeyBoard, m_keycode);
            public override bool Release => Device.Resolver.GetKeyUp(Device as KeyBoard, m_keycode);
            public override bool Performing => Device.Resolver.GetKey(Device as KeyBoard, m_keycode);

            public KeyboardKey(KeyCode listenKey)
            {
                m_keycode = listenKey;
            }

            public override string ControlName => m_keycode.ToString();

            public override Vector2 GetVector2()
            {
                return default(Vector2);
            }

            public override float GetFlaot()
            {
                return Performing ? 1 : 0;
            }
        }
    }

    #region HardCodeForKeyboard
    public partial class KeyBoard : InputDevice
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
            KeyCode.F13, KeyCode.F14, KeyCode.F15,

            // 方向键
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,

            // 控制键
            KeyCode.Space, KeyCode.Return, KeyCode.Escape, KeyCode.Tab, KeyCode.Backspace,
            KeyCode.CapsLock, KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl,
            KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftCommand,
            KeyCode.RightCommand, KeyCode.Menu,

            // 符号键
            KeyCode.Comma, KeyCode.Period, KeyCode.Slash, KeyCode.BackQuote, KeyCode.Quote,
            KeyCode.Semicolon, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash,
            KeyCode.Minus, KeyCode.Equals, KeyCode.Tilde,

            // 小键盘
            KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
            KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
            KeyCode.KeypadPeriod, KeyCode.KeypadDivide, KeyCode.KeypadMultiply,
            KeyCode.KeypadMinus, KeyCode.KeypadPlus, KeyCode.KeypadEnter, KeyCode.Numlock,

            // 其他标准键
            KeyCode.Print,  KeyCode.Insert, KeyCode.Delete, KeyCode.Home,
            KeyCode.End, KeyCode.PageUp, KeyCode.PageDown, KeyCode.Pause, KeyCode.ScrollLock,
            KeyCode.Clear
        };

        // 字母键 A-Z
        public KeyboardKey A { get; private set; } = new KeyboardKey(KeyCode.A);
        public KeyboardKey B { get; private set; } = new KeyboardKey(KeyCode.B);
        public KeyboardKey C { get; private set; } = new KeyboardKey(KeyCode.C);
        public KeyboardKey D { get; private set; } = new KeyboardKey(KeyCode.D);
        public KeyboardKey E { get; private set; } = new KeyboardKey(KeyCode.E);
        public KeyboardKey F { get; private set; } = new KeyboardKey(KeyCode.F);
        public KeyboardKey G { get; private set; } = new KeyboardKey(KeyCode.G);
        public KeyboardKey H { get; private set; } = new KeyboardKey(KeyCode.H);
        public KeyboardKey I { get; private set; } = new KeyboardKey(KeyCode.I);
        public KeyboardKey J { get; private set; } = new KeyboardKey(KeyCode.J);
        public KeyboardKey K { get; private set; } = new KeyboardKey(KeyCode.K);
        public KeyboardKey L { get; private set; } = new KeyboardKey(KeyCode.L);
        public KeyboardKey M { get; private set; } = new KeyboardKey(KeyCode.M);
        public KeyboardKey N { get; private set; } = new KeyboardKey(KeyCode.N);
        public KeyboardKey O { get; private set; } = new KeyboardKey(KeyCode.O);
        public KeyboardKey P { get; private set; } = new KeyboardKey(KeyCode.P);
        public KeyboardKey Q { get; private set; } = new KeyboardKey(KeyCode.Q);
        public KeyboardKey R { get; private set; } = new KeyboardKey(KeyCode.R);
        public KeyboardKey S { get; private set; } = new KeyboardKey(KeyCode.S);
        public KeyboardKey T { get; private set; } = new KeyboardKey(KeyCode.T);
        public KeyboardKey U { get; private set; } = new KeyboardKey(KeyCode.U);
        public KeyboardKey V { get; private set; } = new KeyboardKey(KeyCode.V);
        public KeyboardKey W { get; private set; } = new KeyboardKey(KeyCode.W);
        public KeyboardKey X { get; private set; } = new KeyboardKey(KeyCode.X);
        public KeyboardKey Y { get; private set; } = new KeyboardKey(KeyCode.Y);
        public KeyboardKey Z { get; private set; } = new KeyboardKey(KeyCode.Z);

        // 数字键 0-9
        public KeyboardKey Alpha0 { get; private set; } = new KeyboardKey(KeyCode.Alpha0);
        public KeyboardKey Alpha1 { get; private set; } = new KeyboardKey(KeyCode.Alpha1);
        public KeyboardKey Alpha2 { get; private set; } = new KeyboardKey(KeyCode.Alpha2);
        public KeyboardKey Alpha3 { get; private set; } = new KeyboardKey(KeyCode.Alpha3);
        public KeyboardKey Alpha4 { get; private set; } = new KeyboardKey(KeyCode.Alpha4);
        public KeyboardKey Alpha5 { get; private set; } = new KeyboardKey(KeyCode.Alpha5);
        public KeyboardKey Alpha6 { get; private set; } = new KeyboardKey(KeyCode.Alpha6);
        public KeyboardKey Alpha7 { get; private set; } = new KeyboardKey(KeyCode.Alpha7);
        public KeyboardKey Alpha8 { get; private set; } = new KeyboardKey(KeyCode.Alpha8);
        public KeyboardKey Alpha9 { get; private set; } = new KeyboardKey(KeyCode.Alpha9);

        // 功能键 F1-F15
        public KeyboardKey F1 { get; private set; } = new KeyboardKey(KeyCode.F1);
        public KeyboardKey F2 { get; private set; } = new KeyboardKey(KeyCode.F2);
        public KeyboardKey F3 { get; private set; } = new KeyboardKey(KeyCode.F3);
        public KeyboardKey F4 { get; private set; } = new KeyboardKey(KeyCode.F4);
        public KeyboardKey F5 { get; private set; } = new KeyboardKey(KeyCode.F5);
        public KeyboardKey F6 { get; private set; } = new KeyboardKey(KeyCode.F6);
        public KeyboardKey F7 { get; private set; } = new KeyboardKey(KeyCode.F7);
        public KeyboardKey F8 { get; private set; } = new KeyboardKey(KeyCode.F8);
        public KeyboardKey F9 { get; private set; } = new KeyboardKey(KeyCode.F9);
        public KeyboardKey F10 { get; private set; } = new KeyboardKey(KeyCode.F10);
        public KeyboardKey F11 { get; private set; } = new KeyboardKey(KeyCode.F11);
        public KeyboardKey F12 { get; private set; } = new KeyboardKey(KeyCode.F12);
        public KeyboardKey F13 { get; private set; } = new KeyboardKey(KeyCode.F13);
        public KeyboardKey F14 { get; private set; } = new KeyboardKey(KeyCode.F14);
        public KeyboardKey F15 { get; private set; } = new KeyboardKey(KeyCode.F15);

        // 方向键
        public KeyboardKey UpArrow { get; private set; } = new KeyboardKey(KeyCode.UpArrow);
        public KeyboardKey DownArrow { get; private set; } = new KeyboardKey(KeyCode.DownArrow);
        public KeyboardKey LeftArrow { get; private set; } = new KeyboardKey(KeyCode.LeftArrow);
        public KeyboardKey RightArrow { get; private set; } = new KeyboardKey(KeyCode.RightArrow);

        // 控制键
        public KeyboardKey Space { get; private set; } = new KeyboardKey(KeyCode.Space);
        public KeyboardKey Return { get; private set; } = new KeyboardKey(KeyCode.Return);
        public KeyboardKey Escape { get; private set; } = new KeyboardKey(KeyCode.Escape);
        public KeyboardKey Tab { get; private set; } = new KeyboardKey(KeyCode.Tab);
        public KeyboardKey Backspace { get; private set; } = new KeyboardKey(KeyCode.Backspace);
        public KeyboardKey CapsLock { get; private set; } = new KeyboardKey(KeyCode.CapsLock);
        public KeyboardKey LeftShift { get; private set; } = new KeyboardKey(KeyCode.LeftShift);
        public KeyboardKey RightShift { get; private set; } = new KeyboardKey(KeyCode.RightShift);
        public KeyboardKey LeftControl { get; private set; } = new KeyboardKey(KeyCode.LeftControl);
        public KeyboardKey RightControl { get; private set; } = new KeyboardKey(KeyCode.RightControl);
        public KeyboardKey LeftAlt { get; private set; } = new KeyboardKey(KeyCode.LeftAlt);
        public KeyboardKey RightAlt { get; private set; } = new KeyboardKey(KeyCode.RightAlt);
        public KeyboardKey LeftCommand { get; private set; } = new KeyboardKey(KeyCode.LeftCommand);
        public KeyboardKey RightCommand { get; private set; } = new KeyboardKey(KeyCode.RightCommand);
        public KeyboardKey Menu { get; private set; } = new KeyboardKey(KeyCode.Menu);

        // 符号键
        public KeyboardKey Comma { get; private set; } = new KeyboardKey(KeyCode.Comma);
        public KeyboardKey Period { get; private set; } = new KeyboardKey(KeyCode.Period);
        public KeyboardKey Slash { get; private set; } = new KeyboardKey(KeyCode.Slash);
        public KeyboardKey BackQuote { get; private set; } = new KeyboardKey(KeyCode.BackQuote);
        public KeyboardKey Quote { get; private set; } = new KeyboardKey(KeyCode.Quote);
        public KeyboardKey Semicolon { get; private set; } = new KeyboardKey(KeyCode.Semicolon);
        public KeyboardKey LeftBracket { get; private set; } = new KeyboardKey(KeyCode.LeftBracket);
        public KeyboardKey RightBracket { get; private set; } = new KeyboardKey(KeyCode.RightBracket);
        public KeyboardKey Backslash { get; private set; } = new KeyboardKey(KeyCode.Backslash);
        public KeyboardKey Minus { get; private set; } = new KeyboardKey(KeyCode.Minus);
        public KeyboardKey Equals_k { get; private set; } = new KeyboardKey(KeyCode.Equals);
        public KeyboardKey Tilde { get; private set; } = new KeyboardKey(KeyCode.Tilde);

        // 小键盘
        public KeyboardKey Keypad0 { get; private set; } = new KeyboardKey(KeyCode.Keypad0);
        public KeyboardKey Keypad1 { get; private set; } = new KeyboardKey(KeyCode.Keypad1);
        public KeyboardKey Keypad2 { get; private set; } = new KeyboardKey(KeyCode.Keypad2);
        public KeyboardKey Keypad3 { get; private set; } = new KeyboardKey(KeyCode.Keypad3);
        public KeyboardKey Keypad4 { get; private set; } = new KeyboardKey(KeyCode.Keypad4);
        public KeyboardKey Keypad5 { get; private set; } = new KeyboardKey(KeyCode.Keypad5);
        public KeyboardKey Keypad6 { get; private set; } = new KeyboardKey(KeyCode.Keypad6);
        public KeyboardKey Keypad7 { get; private set; } = new KeyboardKey(KeyCode.Keypad7);
        public KeyboardKey Keypad8 { get; private set; } = new KeyboardKey(KeyCode.Keypad8);
        public KeyboardKey Keypad9 { get; private set; } = new KeyboardKey(KeyCode.Keypad9);
        public KeyboardKey KeypadPeriod { get; private set; } = new KeyboardKey(KeyCode.KeypadPeriod);
        public KeyboardKey KeypadDivide { get; private set; } = new KeyboardKey(KeyCode.KeypadDivide);
        public KeyboardKey KeypadMultiply { get; private set; } = new KeyboardKey(KeyCode.KeypadMultiply);
        public KeyboardKey KeypadMinus { get; private set; } = new KeyboardKey(KeyCode.KeypadMinus);
        public KeyboardKey KeypadPlus { get; private set; } = new KeyboardKey(KeyCode.KeypadPlus);
        public KeyboardKey KeypadEnter { get; private set; } = new KeyboardKey(KeyCode.KeypadEnter);
        public KeyboardKey Numlock { get; private set; } = new KeyboardKey(KeyCode.Numlock);

        // 其他标准键
        public KeyboardKey Print { get; private set; } = new KeyboardKey(KeyCode.Print);
        public KeyboardKey Insert { get; private set; } = new KeyboardKey(KeyCode.Insert);
        public KeyboardKey Delete { get; private set; } = new KeyboardKey(KeyCode.Delete);
        public KeyboardKey Home { get; private set; } = new KeyboardKey(KeyCode.Home);
        public KeyboardKey End { get; private set; } = new KeyboardKey(KeyCode.End);
        public KeyboardKey PageUp { get; private set; } = new KeyboardKey(KeyCode.PageUp);
        public KeyboardKey PageDown { get; private set; } = new KeyboardKey(KeyCode.PageDown);
        public KeyboardKey Pause { get; private set; } = new KeyboardKey(KeyCode.Pause);
        public KeyboardKey ScrollLock { get; private set; } = new KeyboardKey(KeyCode.ScrollLock);
        public KeyboardKey Clear { get; private set; } = new KeyboardKey(KeyCode.Clear);
    }
    #endregion
}