using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxiInputSP.UGUI
{
    public class AxiIptJoystick : IDisposable
    {
        enum AxiIptJoystickState
        {
            None,
            KeyUp,
            KeyDown,
            KeyHold
        }
        static long LastCheckFrame = -1;
        static Func<Vector2Int> RawJoy;
        AxiInputUGUIHandle[] handles = new AxiInputUGUIHandle[4];
        static Dictionary<AxiInputUGuiBtnType, AxiIptJoystickState> mAxis2State = new Dictionary<AxiInputUGuiBtnType, AxiIptJoystickState>();

        public AxiIptJoystick(Func<Vector2Int> getRawJoy)
        {
            RawJoy = getRawJoy;
            handles[0] = new AxiInputUGUIHandle(AxiInputUGuiBtnType.UP);
            handles[0].GetKeyHandle = (() => { return GetKey(AxiInputUGuiBtnType.UP); });
            handles[0].GetKeyUpHandle = (() => { return GetKeyUp(AxiInputUGuiBtnType.UP); });
            handles[0].GetKeyDownHandle = (() => { return GetKeyDown(AxiInputUGuiBtnType.UP); });
            mAxis2State[AxiInputUGuiBtnType.UP] = AxiIptJoystickState.None;

            handles[1] = new AxiInputUGUIHandle(AxiInputUGuiBtnType.DOWN);
            handles[1].GetKeyHandle = (() => { return GetKey(AxiInputUGuiBtnType.DOWN); });
            handles[1].GetKeyUpHandle = (() => { return GetKeyUp(AxiInputUGuiBtnType.DOWN); });
            handles[1].GetKeyDownHandle = (() => { return GetKeyDown(AxiInputUGuiBtnType.DOWN); });
            mAxis2State[AxiInputUGuiBtnType.DOWN] = AxiIptJoystickState.None;

            handles[2] = new AxiInputUGUIHandle(AxiInputUGuiBtnType.LEFT);
            handles[2].GetKeyHandle = (() => { return GetKey(AxiInputUGuiBtnType.LEFT); });
            handles[2].GetKeyUpHandle = (() => { return GetKeyUp(AxiInputUGuiBtnType.LEFT); });
            handles[2].GetKeyDownHandle = (() => { return GetKeyDown(AxiInputUGuiBtnType.LEFT); });
            mAxis2State[AxiInputUGuiBtnType.LEFT] = AxiIptJoystickState.None;

            handles[3] = new AxiInputUGUIHandle(AxiInputUGuiBtnType.RIGHT);
            handles[3].GetKeyHandle = (() => { return GetKey(AxiInputUGuiBtnType.RIGHT); });
            handles[3].GetKeyUpHandle = (() => { return GetKeyUp(AxiInputUGuiBtnType.RIGHT); });
            handles[3].GetKeyDownHandle = (() => { return GetKeyDown(AxiInputUGuiBtnType.RIGHT); });
            mAxis2State[AxiInputUGuiBtnType.RIGHT] = AxiIptJoystickState.None;
        }


        public void Dispose()
        {
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i].Dispose();
                handles[i] = null;
            }
            mAxis2State.Clear();
        }

        public static void UpdateState()
        {
            if (LastCheckFrame == Time.frameCount)
                return;
            LastCheckFrame = Time.frameCount;
            RecheckSingleState(AxiInputUGuiBtnType.RIGHT);
            RecheckSingleState(AxiInputUGuiBtnType.LEFT);
            RecheckSingleState(AxiInputUGuiBtnType.UP);
            RecheckSingleState(AxiInputUGuiBtnType.DOWN);
        }

        static void RecheckSingleState(AxiInputUGuiBtnType axisType)
        {
            bool bKey = false;

            Vector2Int inputV2 = RawJoy.Invoke();

            switch (axisType)
            {
                case AxiInputUGuiBtnType.RIGHT: bKey = inputV2.x > 0; break;
                case AxiInputUGuiBtnType.LEFT: bKey = inputV2.x < 0; break;
                case AxiInputUGuiBtnType.UP: bKey = inputV2.y > 0; break;
                case AxiInputUGuiBtnType.DOWN: bKey = inputV2.y < 0; break;
            }
            //按下
            if (bKey)
            {
                //如果之前帧是KeyUp或None，则为KeyDown|KeyHold
                if (mAxis2State[axisType] <= AxiIptJoystickState.KeyUp)
                    mAxis2State[axisType] = AxiIptJoystickState.KeyDown;
                //如果之前帧是KeyDown，则为KeyHold
                else if (mAxis2State[axisType] == AxiIptJoystickState.KeyDown)
                    mAxis2State[axisType] = AxiIptJoystickState.KeyHold;
            }
            //未按下
            else
            {
                //如果之前帧是KeyDown|KeyHold，则为KeyUp|None
                if (mAxis2State[axisType] >= AxiIptJoystickState.KeyDown)
                    mAxis2State[axisType] = AxiIptJoystickState.KeyUp;
                //如果之前帧是KeyUp，则为None
                else if (mAxis2State[axisType] == AxiIptJoystickState.KeyUp)
                    mAxis2State[axisType] = AxiIptJoystickState.None;
            }
        }
        bool GetKey(AxiInputUGuiBtnType key)
        {
            UpdateState();
            return mAxis2State[key] >= AxiIptJoystickState.KeyDown;
        }
        bool GetKeyUp(AxiInputUGuiBtnType key)
        {
            UpdateState();
            return mAxis2State[key] == AxiIptJoystickState.KeyUp;
        }
        bool GetKeyDown(AxiInputUGuiBtnType key)
        {
            UpdateState();
            return mAxis2State[key] == AxiIptJoystickState.KeyDown;
        }
    }
}
