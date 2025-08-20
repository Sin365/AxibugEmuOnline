using System;

namespace StoicGooseUnity
{
    [Flags]
    public enum StoicGooseKey : ushort
    {
        X1 = 1,
        X2 = 2,
        X3 = 4,
        X4 = 8,
        Y1 = 16,
        Y2 = 32,
        Y3 = 64,
        Y4 = 128,
        Start = 256,
        A = 512,
        B = 1024
    }
}
