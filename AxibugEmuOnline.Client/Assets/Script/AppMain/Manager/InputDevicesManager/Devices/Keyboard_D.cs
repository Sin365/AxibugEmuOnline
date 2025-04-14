using System.Collections.Generic;

namespace AxibugEmuOnline.Client.InputDevices
{
    /// <summary>
    /// 通用键盘设备
    /// </summary>
    public class Keyboard_D : InputDevice_D
    {
        public Button_C A;
        public Button_C B;
        public Button_C C;
        public Button_C D;
        public Button_C E;
        public Button_C F;
        public Button_C G;
        public Button_C H;
        public Button_C I;
        public Button_C J;
        public Button_C K;
        public Button_C L;
        public Button_C M;
        public Button_C N;
        public Button_C O;
        public Button_C P;
        public Button_C Q;
        public Button_C R;
        public Button_C S;
        public Button_C T;
        public Button_C U;
        public Button_C V;
        public Button_C W;
        public Button_C X;
        public Button_C Y;
        public Button_C Z;
        public Button_C Alpha0;
        public Button_C Alpha1;
        public Button_C Alpha2;
        public Button_C Alpha3;
        public Button_C Alpha4;
        public Button_C Alpha5;
        public Button_C Alpha6;
        public Button_C Alpha7;
        public Button_C Alpha8;
        public Button_C Alpha9;
        public Button_C F1;
        public Button_C F2;
        public Button_C F3;
        public Button_C F4;
        public Button_C F5;
        public Button_C F6;
        public Button_C F7;
        public Button_C F8;
        public Button_C F9;
        public Button_C F10;
        public Button_C F11;
        public Button_C F12;
        public Button_C UpArrow;
        public Button_C DownArrow;
        public Button_C LeftArrow;
        public Button_C RightArrow;
        public Button_C Space;
        public Button_C Return;
        public Button_C Escape;
        public Button_C Tab;
        public Button_C Backspace;
        public Button_C CapsLock;
        public Button_C LeftShift;
        public Button_C RightShift;
        public Button_C LeftControl;
        public Button_C RightControl;
        public Button_C LeftAlt;
        public Button_C RightAlt;
        public Button_C LeftCommand;
        public Button_C RightCommand;
        public Button_C Comma;
        public Button_C Period;
        public Button_C Slash;
        public Button_C BackQuote;
        public Button_C Quote;
        public Button_C Semicolon;
        public Button_C LeftBracket;
        public Button_C RightBracket;
        public Button_C Backslash;
        public Button_C Minus;
        public Button_C Equals_k;
        public Button_C Keypad0;
        public Button_C Keypad1;
        public Button_C Keypad2;
        public Button_C Keypad3;
        public Button_C Keypad4;
        public Button_C Keypad5;
        public Button_C Keypad6;
        public Button_C Keypad7;
        public Button_C Keypad8;
        public Button_C Keypad9;
        public Button_C KeypadPeriod;
        public Button_C KeypadDivide;
        public Button_C KeypadMultiply;
        public Button_C KeypadMinus;
        public Button_C KeypadPlus;
        public Button_C KeypadEnter;
        public Button_C Numlock;
        public Button_C Print;
        public Button_C Insert;
        public Button_C Delete;
        public Button_C Home;
        public Button_C End;
        public Button_C PageUp;
        public Button_C PageDown;
        public Button_C Pause;
        public Button_C ScrollLock;

        public Keyboard_D(InputResolver resolver) : base(resolver) { }
        public override bool Exclusive => false;
    }
}