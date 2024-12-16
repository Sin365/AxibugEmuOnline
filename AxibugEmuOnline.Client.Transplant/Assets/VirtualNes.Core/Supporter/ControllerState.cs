using System;

namespace VirtualNes.Core
{
    public struct ControllerState
    {
        public uint raw0;
        public uint raw1;
        public uint raw2;
        public uint raw3;

        public bool valid;

        public ControllerState(
            EnumButtonType player0_buttons,
            EnumButtonType player1_buttons,
            EnumButtonType player2_buttons,
            EnumButtonType player3_buttons)
        {
            raw0 = (uint)player0_buttons;
            raw1 = (uint)player1_buttons;
            raw2 = (uint)player2_buttons;
            raw3 = (uint)player3_buttons;
            valid = true;
        }

        public static bool operator ==(ControllerState left, ControllerState right)
        {
            return
                left.raw0 == right.raw0 &&
                left.raw1 == right.raw1 &&
                left.raw2 == right.raw2 &&
                left.raw3 == right.raw3;
        }

        public static bool operator !=(ControllerState left, ControllerState right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{raw0}|{raw1}|{raw2}|{raw3}";
        }

        public bool HasButton(int player, EnumButtonType button)
        {
            uint raw = 0;
            switch (player)
            {
                case 0: raw = raw0; break;
                case 1: raw = raw1; break;
                case 2: raw = raw2; break;
                case 3: raw = raw3; break;
            }
            return (raw & (uint)button) == (uint)button;
        }
    }

    [Flags]
    public enum EnumButtonType
    {
        NONE = 0,
        UP = 1,
        DOWN = 2,
        LEFT = 4,
        RIGHT = 8,
        A = 16,
        B = 32,
        SELECT = 64,
        START = 128,
        MIC = 256
    }
}
