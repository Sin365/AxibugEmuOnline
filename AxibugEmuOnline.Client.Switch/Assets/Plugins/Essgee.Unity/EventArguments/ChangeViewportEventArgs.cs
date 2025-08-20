using System;

namespace Essgee.EventArguments
{
    public class ChangeViewportEventArgs : EventArgs
    {
        public (int X, int Y, int Width, int Height) Viewport { get; private set; }

        //public ChangeViewportEventArgs((int, int, int, int) viewport)
        //{
        //	Viewport = viewport;
        //}

        public static ChangeViewportEventArgs Create((int, int, int, int) viewport)
        {
            var eventArgs = ObjectPoolAuto.Acquire<ChangeViewportEventArgs>();
            eventArgs.Viewport = viewport;
            return eventArgs;
        }
    }
    public static class ChangeViewportEventArgsEx
    {
        public static void Release(this ChangeViewportEventArgs eventArgs)
        {
            ObjectPoolAuto.Release(eventArgs);
        }
    }
}
