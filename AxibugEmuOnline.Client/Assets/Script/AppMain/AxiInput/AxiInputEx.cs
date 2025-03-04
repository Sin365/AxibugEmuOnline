using UnityEngine;

namespace Assets.Script.AppMain.AxiInput
{
    public static class AxiInputEx
    {
        public static AxiInput ByKeyCode(KeyCode keycode)
        {
            AxiInput data = new AxiInput();
            data.all = 0;
            data.type = AxiInputType.UNITY_KEYCODE;
            data.KeyCodeValue = keycode;
            return data;
        }
        public static AxiInput ByAxis(AxiInputAxisType axisType)
        {
            AxiInput data = new AxiInput();
            data.all = 0;
            data.type = AxiInputType.UNITY_AXIS;
            data.AxisType = axisType;
            return data;
        }
        public static AxiInput ByUGUIBtn(AxiInputUGuiBtnType btnType)
        {
            AxiInput data = new AxiInput();
            data.all = 0;
            data.type = AxiInputType.UNITY_UGUI_BTN;
            data.UguiBtn = btnType;
            return data;
        }

        public static bool IsKeyDown(this AxiInput axiInput)
        {
            switch (axiInput.type)
            {
                case AxiInputType.UNITY_KEYCODE:
                    return Input.GetKeyDown(axiInput.KeyCodeValue);
                case AxiInputType.UNITY_AXIS://AXIS 不考虑KeyDown情况
                    {
                        switch (axiInput.AxisType)
                        {
                            case AxiInputAxisType.RIGHT: return Input.GetAxis("Horizontal") > 0;
                            case AxiInputAxisType.LEFT: return Input.GetAxis("Horizontal") < 0;
                            case AxiInputAxisType.UP: return Input.GetAxis("Vertical") > 0;
                            case AxiInputAxisType.DOWN: return Input.GetAxis("Vertical") < 0;
                            default: return false;
                        }
                    }
                case AxiInputType.UNITY_UGUI_BTN:
                    return AxiInputUGUICenter.IsKeyDown(axiInput.UguiBtn);
                default:
                    return false;
            }
        }

        public static bool IsKey(this AxiInput axiInput)
        {
            switch (axiInput.type)
            {
                case AxiInputType.UNITY_KEYCODE:
                    return Input.GetKey(axiInput.KeyCodeValue);
                case AxiInputType.UNITY_AXIS://AXIS 不考虑KeyDown情况
                    return false;
                case AxiInputType.UNITY_UGUI_BTN:
                    return AxiInputUGUICenter.IsKeyDown(axiInput.UguiBtn);
                default:
                    return false;
            }
        }
    }
}
