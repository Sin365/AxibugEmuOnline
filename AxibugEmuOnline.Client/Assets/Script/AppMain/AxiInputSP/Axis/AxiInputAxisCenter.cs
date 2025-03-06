using System.Collections.Generic;
using UnityEngine;

namespace AxiInputSP.Axis
{
    public static class AxiInputAxisCenter
    {
        static long LastCheckFrame = -1;

        enum AxisState
        {
            None,
            KeyUp,
            KeyDown,
            KeyHold
        }

        static Dictionary<AxiInputAxisType, AxisState> mAxis2State = new Dictionary<AxiInputAxisType, AxisState>()
        {
            {AxiInputAxisType.RIGHT,AxisState.None},
            {AxiInputAxisType.LEFT,AxisState.None },
            {AxiInputAxisType.UP,AxisState.None},
            {AxiInputAxisType.DOWN,AxisState.None},
        };

        public static bool GetKey(AxiInputAxisType axisType)
        {
            UpdateState();
            return mAxis2State[axisType] >= AxisState.KeyDown;
        }
        public static bool GetKeyDown(AxiInputAxisType axisType)
        {
            UpdateState();
            return mAxis2State[axisType] == AxisState.KeyDown;
        }
        public static bool GetKeyUp(AxiInputAxisType axisType)
        {
            UpdateState();
            return mAxis2State[axisType] == AxisState.KeyUp;
        }

        public static void UpdateState()
        {
            if (LastCheckFrame == Time.frameCount)
                return;
            LastCheckFrame = Time.frameCount;
            RecheckSingleState(AxiInputAxisType.RIGHT);
            RecheckSingleState(AxiInputAxisType.LEFT);
            RecheckSingleState(AxiInputAxisType.UP);
            RecheckSingleState(AxiInputAxisType.DOWN);
        }

        static void RecheckSingleState(AxiInputAxisType axisType)
        {
            bool bKey = false;
            switch (axisType)
            {
                case AxiInputAxisType.RIGHT: bKey = Input.GetAxis("Horizontal") > 0; break;
                case AxiInputAxisType.LEFT: bKey = Input.GetAxis("Horizontal") < 0; break;
                case AxiInputAxisType.UP: bKey = Input.GetAxis("Vertical") > 0; break;
                case AxiInputAxisType.DOWN: bKey = Input.GetAxis("Vertical") < 0; break;
            }
            //按下
            if (bKey)
            {
                //如果之前帧是KeyUp或None，则为KeyDown|KeyHold
                if (mAxis2State[axisType] <= AxisState.KeyUp)
                    mAxis2State[axisType] = AxisState.KeyDown;
                //如果之前帧是KeyDown，则为KeyHold
                else if (mAxis2State[axisType] == AxisState.KeyDown)
                    mAxis2State[axisType] = AxisState.KeyHold;
            }
            //未按下
            else
            {
                //如果之前帧是KeyDown|KeyHold，则为KeyUp|None
                if (mAxis2State[axisType] >= AxisState.KeyDown)
                    mAxis2State[axisType] = AxisState.KeyUp;
                //如果之前帧是KeyUp，则为None
                else if (mAxis2State[axisType] == AxisState.KeyUp)
                    mAxis2State[axisType] = AxisState.None;
            }
        }
    }
}
