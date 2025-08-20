﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AxiInputSP
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct AxiInput
    {
        [FieldOffset(0)]
        public UInt64 all;
        [FieldOffset(0)]
        public AxiInputType type;
        [FieldOffset(1)]
        public KeyCode KeyCodeValue;
        [FieldOffset(1)]
        public AxiInputAxisType AxisType;
        [FieldOffset(1)]
        public AxiInputUGuiBtnType UguiBtn;
    }

    public enum AxiInputType : byte
    {
        UNITY_KEYCODE,
        UNITY_AXIS,//Input.GetAxis
        UNITY_UGUI_BTN,//UGUI 的BTN事件，
    }

    public enum AxiInputAxisType : byte
    {
        LEFT,
        RIGHT,
        UP,
        DOWN,
    }

    public enum AxiInputUGuiBtnType : byte
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        BTN_A,
        BTN_B,
        BTN_C,
        BTN_D,
        BTN_E,
        BTN_F,
        OPTION_1,
        OPTION_2,
        OPTION_3,
        OPTION_4,
        HOME,
    }
}
