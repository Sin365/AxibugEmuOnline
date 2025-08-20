using System;

namespace Essgee.EventArguments
{
    public class SizeScreenEventArgs : EventArgs
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public static SizeScreenEventArgs Create(int width, int height)
        {
            var eventArgs = ObjectPoolAuto.Acquire<SizeScreenEventArgs>();
            eventArgs.Width = width;
            eventArgs.Height = height;
            return eventArgs;
        }
    }
    public static class SizeScreenEventArgsEx
    {
        public static void Release(this SizeScreenEventArgs eventArgs)
        {
            ObjectPoolAuto.Release(eventArgs);
        }
    }
}
