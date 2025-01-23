using MAME.Core;
using UnityEngine;

public class UniMouse : MonoBehaviour, IMouse
{
    static int mX, mY;
    public byte[] buttons = new byte[2];
    void Update()
    {
        mX = (int)Input.mousePosition.x;
        mY = (int)Input.mousePosition.y;
        buttons[0] = Input.GetMouseButton(0) ? (byte)1 : (byte)0;
        buttons[1] = Input.GetMouseButton(1) ? (byte)1 : (byte)0;
    }

    public void MouseXY(out int X, out int Y, out byte[] MouseButtons)
    {
        X = mX;
        Y = mY * -1;
        MouseButtons = buttons;
    }

}
