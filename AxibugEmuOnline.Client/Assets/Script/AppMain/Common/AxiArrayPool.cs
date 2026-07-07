using System.Buffers;

namespace AxibugEmuOnline.Client.Common
{
    public static class AxiArrayPool
    {
        public static byte[] RentBuffer(int minSize)
        {
            return ArrayPool<byte>.Shared.Rent(minSize);
        }

        public static void ReturnBuffer(byte[] buffer)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        public static T[] RentBuffer<T>(int minSize)
        {
            return ArrayPool<T>.Shared.Rent(minSize);
        }

        public static void ReturnBuffer<T>(T[] buffer)
        {
            ArrayPool<T>.Shared.Return(buffer);
        }
    }
}
