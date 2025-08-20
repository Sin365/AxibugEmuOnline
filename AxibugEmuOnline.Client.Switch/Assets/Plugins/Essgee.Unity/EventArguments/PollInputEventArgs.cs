using System;
using System.Collections.Generic;

namespace Essgee.EventArguments
{
    public class PollInputEventArgs : EventArgs
    {
        public List<EssgeeMotionKey> Keyboard { get; set; }

        public MouseButtons MouseButtons { get; set; }
        public (int X, int Y) MousePosition { get; set; }

        //public ControllerState ControllerState { get; set; }

        //public PollInputEventArgs()
        //{
        //	Keyboard = new List<MotionKey>();

        //	MouseButtons = MouseButtons.None;
        //	MousePosition = (0, 0);

        //	ControllerState = new ControllerState();
        //}

        public static PollInputEventArgs Create()
        {
            var eventArgs = ObjectPoolAuto.Acquire<PollInputEventArgs>();
            //eventArgs.Keyboard = new List<MotionKey>();
            eventArgs.Keyboard = ObjectPoolAuto.AcquireList<EssgeeMotionKey>();
            eventArgs.MouseButtons = MouseButtons.None;
            eventArgs.MousePosition = (0, 0);

            //eventArgs.ControllerState = new ControllerState();
            //eventArgs.ControllerState = ObjectPoolAuto.Acquire<ControllerState>();
            return eventArgs;
        }
    }
    public static class PollInputEventArgsEx
    {
        public static void Release(this PollInputEventArgs eventArgs)
        {
            ObjectPoolAuto.Release(eventArgs.Keyboard);
            eventArgs.Keyboard = null;
            eventArgs.MouseButtons = MouseButtons.None;
            eventArgs.MousePosition = (0, 0);
            //ObjectPoolAuto.Release(eventArgs.ControllerState);
            //eventArgs.ControllerState = null;

            ObjectPoolAuto.Release(eventArgs);
        }
    }
}
