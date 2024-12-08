using System;

namespace VirtualNes.Core
{
    public struct ControllerState
    {
        private uint raw0;
        private uint raw1;
        private uint raw2;
        private uint raw3;

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
