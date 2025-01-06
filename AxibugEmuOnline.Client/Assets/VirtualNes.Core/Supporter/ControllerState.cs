using System;

namespace VirtualNes.Core
{
    public struct ControllerState : IEquatable<ControllerState>
    {
        public uint raw0;
        public uint raw1;
        public uint raw2;
        public uint raw3;

        public bool valid;

        public ControllerState(EnumButtonType[] states)
        {
            raw0 = (uint)states[0];
            raw1 = (uint)states[1];
            raw2 = (uint)states[2];
            raw3 = (uint)states[3];
            valid = true;
        }

        public bool HasButton(int player, EnumButtonType button)
        {
            uint raw = player switch
            {
                0 => raw0,
                1 => raw1,
                2 => raw2,
                3 => raw3,
                _ => 0
            };
            return (raw & (uint)button) == (uint)button;
        }

        public override string ToString()
        {
            return $"{raw0}|{raw1}|{raw2}|{raw3}";
        }

        #region Impl_Equals
        public bool Equals(ControllerState other)
        {
            return raw0 == other.raw0 && raw1 == other.raw1 && raw2 == other.raw2 && raw3 == other.raw3 && valid == other.valid;
        }

        public override bool Equals(object obj)
        {
            return obj is ControllerState other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + raw0.GetHashCode();
            hash = hash * 31 + raw1.GetHashCode();
            hash = hash * 31 + raw2.GetHashCode();
            hash = hash * 31 + raw3.GetHashCode();
            hash = hash * 31 + valid.GetHashCode();
            return hash;
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
        #endregion
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
