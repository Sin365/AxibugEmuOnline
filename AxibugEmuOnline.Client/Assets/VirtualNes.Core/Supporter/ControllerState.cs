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
            uint raw;

			switch (player)
            {
                case 0: raw = raw0; break;
				case 1: raw = raw1; break;
				case 2: raw = raw2; break;
				case 3: raw = raw3; break;
                default:
                    raw = 0;
                    break;
			}
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
            //return HashCode.Combine(raw0, raw1, raw2, raw3, valid);
            return ComputeHashCode(raw0, raw1, raw2, raw3, valid);
		}

		public static int ComputeHashCode(uint raw0, uint raw1, uint raw2, uint raw3, bool valid)
		{
			unchecked // 允许溢出，使得哈希码计算更加合理
			{
				int hash = 17; // 选择一个非零的初始值

				// 将每个 uint 类型的值转换为 int 并合并到哈希码中
				hash = hash * 31 + (int)raw0;
				hash = hash * 31 + (int)raw1;
				hash = hash * 31 + (int)raw2;
				hash = hash * 31 + (int)raw3;

				// 将 bool 类型的值转换为 int 并合并到哈希码中
				hash = hash * 31 + (valid ? 1 : 0);

				return hash;
			}
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
