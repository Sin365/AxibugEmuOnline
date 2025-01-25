using MAME.Core;

namespace MAME.Core
{
    public class Mouse
    {
        public static int deltaX, deltaY, oldX, oldY;
        public static byte[] buttons;
        static IMouse iMouse;
        public static void InitialMouse(IMouse im)
        {
            iMouse = im;
        }

        public static void Update()
        {
            int X, Y;
            iMouse.MouseXY(out X, out Y, out byte[] MouseButtons);
            deltaX = X - oldX;
            deltaY = Y - oldY;
            oldX = X;
            oldY = Y;
            buttons = MouseButtons;

            //MouseState mouseState = mouseDevice.CurrentMouseState;
            //deltaX = mouseState.X - oldX;
            //deltaY = mouseState.Y - oldY;
            //oldX = mouseState.X;
            //oldY = mouseState.Y;
            //buttons = mouseState.GetMouseButtons();
        }
    }
}
