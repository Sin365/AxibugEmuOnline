using MAME.Core;
using System.Collections.Generic;

namespace MAME.Core
{
    public partial class Inptport
    {
        private static List<KeyStruct> lks;
        public static List<Corekey> lk;
        public class KeyStruct
        {
            public Corekey key;
            public char c;
            public KeyStruct(Corekey _key, char _c)
            {
                key = _key;
                c = _c;
            }
        }
        public static char getcharbykey(Corekey key1)
        {
            char c1 = ' ';
            foreach (KeyStruct ks in lks)
            {
                if (ks.key == key1)
                {
                    c1 = ks.c;
                    break;
                }
            }
            return c1;
        }
        public static void input_init()
        {
            lk = new List<Corekey>();
            lk.Add(Corekey.D1);
            lk.Add(Corekey.D2);
            lk.Add(Corekey.D3);
            lk.Add(Corekey.D4);
            lk.Add(Corekey.D5);
            lk.Add(Corekey.D6);
            lk.Add(Corekey.D7);
            lk.Add(Corekey.D8);
            lk.Add(Corekey.D9);
            lk.Add(Corekey.D0);
            lk.Add(Corekey.A);
            lk.Add(Corekey.B);
            lk.Add(Corekey.C);
            lk.Add(Corekey.D);
            lk.Add(Corekey.E);
            lk.Add(Corekey.F);
            lk.Add(Corekey.G);
            lk.Add(Corekey.H);
            lk.Add(Corekey.I);
            lk.Add(Corekey.J);
            lk.Add(Corekey.K);
            lk.Add(Corekey.L);
            lk.Add(Corekey.M);
            lk.Add(Corekey.N);
            lk.Add(Corekey.O);
            lk.Add(Corekey.P);
            lk.Add(Corekey.Q);
            lk.Add(Corekey.R);
            lk.Add(Corekey.S);
            lk.Add(Corekey.T);
            lk.Add(Corekey.U);
            lk.Add(Corekey.V);
            lk.Add(Corekey.W);
            lk.Add(Corekey.X);
            lk.Add(Corekey.Y);
            lk.Add(Corekey.Z);
            lks = new List<KeyStruct>();
            lks.Add(new KeyStruct(Corekey.D1, '1'));
            lks.Add(new KeyStruct(Corekey.D2, '2'));
            lks.Add(new KeyStruct(Corekey.D3, '3'));
            lks.Add(new KeyStruct(Corekey.D4, '4'));
            lks.Add(new KeyStruct(Corekey.D5, '5'));
            lks.Add(new KeyStruct(Corekey.D6, '6'));
            lks.Add(new KeyStruct(Corekey.D7, '7'));
            lks.Add(new KeyStruct(Corekey.D8, '8'));
            lks.Add(new KeyStruct(Corekey.D9, '9'));
            lks.Add(new KeyStruct(Corekey.D0, '0'));
            lks.Add(new KeyStruct(Corekey.A, 'a'));
            lks.Add(new KeyStruct(Corekey.B, 'b'));
            lks.Add(new KeyStruct(Corekey.C, 'c'));
            lks.Add(new KeyStruct(Corekey.D, 'd'));
            lks.Add(new KeyStruct(Corekey.E, 'e'));
            lks.Add(new KeyStruct(Corekey.F, 'f'));
            lks.Add(new KeyStruct(Corekey.G, 'g'));
            lks.Add(new KeyStruct(Corekey.H, 'h'));
            lks.Add(new KeyStruct(Corekey.I, 'i'));
            lks.Add(new KeyStruct(Corekey.J, 'j'));
            lks.Add(new KeyStruct(Corekey.K, 'k'));
            lks.Add(new KeyStruct(Corekey.L, 'l'));
            lks.Add(new KeyStruct(Corekey.M, 'm'));
            lks.Add(new KeyStruct(Corekey.N, 'n'));
            lks.Add(new KeyStruct(Corekey.O, 'o'));
            lks.Add(new KeyStruct(Corekey.P, 'p'));
            lks.Add(new KeyStruct(Corekey.Q, 'q'));
            lks.Add(new KeyStruct(Corekey.R, 'r'));
            lks.Add(new KeyStruct(Corekey.S, 's'));
            lks.Add(new KeyStruct(Corekey.T, 't'));
            lks.Add(new KeyStruct(Corekey.U, 'u'));
            lks.Add(new KeyStruct(Corekey.V, 'v'));
            lks.Add(new KeyStruct(Corekey.W, 'w'));
            lks.Add(new KeyStruct(Corekey.X, 'x'));
            lks.Add(new KeyStruct(Corekey.Y, 'y'));
            lks.Add(new KeyStruct(Corekey.Z, 'z'));
        }
    }
}
