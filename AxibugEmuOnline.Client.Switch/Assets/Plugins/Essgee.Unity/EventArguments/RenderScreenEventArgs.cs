using System;

namespace Essgee.EventArguments
{
    public class RenderScreenEventArgs : EventArgs
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        //public byte[] FrameData { get; private set; }
        public IntPtr FrameDataPtr { get; private set; }

        //      public RenderScreenEventArgs(int width, int height, IntPtr ptr)
        //{
        //	Width = width;
        //	Height = height;
        //	//FrameData = data;
        //	FrameDataPtr = ptr;
        //}

        public static RenderScreenEventArgs Create(int width, int height, IntPtr ptr)
        {
            var eventArgs = ObjectPoolAuto.Acquire<RenderScreenEventArgs>();
            eventArgs.Width = width;
            eventArgs.Height = height;
            //FrameData = data;
            eventArgs.FrameDataPtr = ptr;
            return eventArgs;
        }
    }
    public static class RenderScreenEventArgsEx
    {
        public static void Release(this RenderScreenEventArgs eventArgs)
        {
            ObjectPoolAuto.Release(eventArgs);
        }
    }
}
