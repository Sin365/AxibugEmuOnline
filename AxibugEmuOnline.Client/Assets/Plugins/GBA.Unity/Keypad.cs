using static OptimeGBA.Bits;

namespace OptimeGBA
{
    public sealed class Keypad
    {
        public bool A;
        public bool B;
        public bool Start;
        public bool Select;
        public bool Right;
        public bool Left;
        public bool Up;
        public bool Down;
        public bool R;
        public bool L;
        public byte ReadHwio8(uint addr)
        {
            byte val = 0;
            switch (addr)
            {
                case 0x4000130: // KEYINPUT B0
                    if (!A) val = BitSet(val, 0);
                    if (!B) val = BitSet(val, 1);
                    if (!Select) val = BitSet(val, 2);
                    if (!Start) val = BitSet(val, 3);
                    if (!Right) val = BitSet(val, 4);
                    if (!Left) val = BitSet(val, 5);
                    if (!Up) val = BitSet(val, 6);
                    if (!Down) val = BitSet(val, 7);
                    break;
                case 0x4000131: // KEYINPUT B1
                    if (!R) val = BitSet(val, 8 - 8);
                    if (!L) val = BitSet(val, 9 - 8);
                    break;

                case 0x4000137: // EXTKEYIN B1
                    val = 0;
                    break;
            }

            return val;
        }
    }
}