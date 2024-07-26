using System.Runtime.CompilerServices;

namespace VirtualNes.Core
{
    public static class MemoryUtility
    {
        public static void ZEROMEMORY(byte[] array, uint length)
        {
            memset(array, 0, length);
        }

        public static void memset(byte[] array, byte value, uint length)
        {
            Unsafe.InitBlock(ref array[0], value, length);
        }
    }
}
