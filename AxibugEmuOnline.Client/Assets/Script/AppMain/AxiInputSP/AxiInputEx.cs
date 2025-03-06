using AxiInputSP.Axis;
using AxiInputSP.UGUI;
using UnityEngine;

namespace AxiInputSP
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

        public static bool GetKeyDown(this AxiInput axiInput)
        {
            switch (axiInput.type)
            {
                case AxiInputType.UNITY_KEYCODE:
                    return Input.GetKeyDown(axiInput.KeyCodeValue);
                case AxiInputType.UNITY_AXIS:
                    return AxiInputAxisCenter.GetKeyDown(axiInput.AxisType);
                case AxiInputType.UNITY_UGUI_BTN:
                    return AxiInputUGUICenter.GetKeyDown(axiInput.UguiBtn);
                default:
                    return false;
            }
        }

        public static bool GetKeyUp(this AxiInput axiInput)
        {
            switch (axiInput.type)
            {
                case AxiInputType.UNITY_KEYCODE:
                    return Input.GetKeyUp(axiInput.KeyCodeValue);
                case AxiInputType.UNITY_AXIS:
                    return AxiInputAxisCenter.GetKeyUp(axiInput.AxisType);
                case AxiInputType.UNITY_UGUI_BTN:
                    return AxiInputUGUICenter.GetKeyUp(axiInput.UguiBtn);
                default:
                    return false;
            }
        }

        public static bool GetKey(this AxiInput axiInput)
        {
            switch (axiInput.type)
            {
                case AxiInputType.UNITY_KEYCODE:
                    return Input.GetKey(axiInput.KeyCodeValue);
                case AxiInputType.UNITY_AXIS: 
                    return AxiInputAxisCenter.GetKey(axiInput.AxisType);
                case AxiInputType.UNITY_UGUI_BTN:
                    return AxiInputUGUICenter.GetKey(axiInput.UguiBtn);
                default:
                    return false;
            }
        }
    }
}
