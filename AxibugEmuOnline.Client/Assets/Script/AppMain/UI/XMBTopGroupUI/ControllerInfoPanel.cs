using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Manager;
using AxibugProtobuf;
using System.Collections.Generic;
using UnityEngine;

public class ControllerInfoPanel : MonoBehaviour
{
    private void OnEnable()
    {
        Eventer.Instance.RegisterEvent<int>(EEvent.OnLocalJoyDesireInvert, OnLocalJoyDesireInvert);
    }

    private void OnDisable()
    {
        Eventer.Instance.UnregisterEvent<int>(EEvent.OnLocalJoyDesireInvert, OnLocalJoyDesireInvert);
    }

    static List<int> s_freeSlots = new List<int>(4);
    private void OnLocalJoyDesireInvert(int joyIndex)
    {
        if (App.roomMgr.InRoom)
        {
            if (!App.roomMgr.mineRoomMiniInfo.GetFreeSlot(ref s_freeSlots)) return;

            //找到第一个空闲手柄插槽
            var freeSlotIndex = s_freeSlots[0];

            //TODO 手柄类型
            GamePadType gpType = GamePadType.GlobalGamePad;

            App.roomMgr.SendChangePlaySlotIdxWithJoyIdx((uint)joyIndex, (uint)freeSlotIndex, gpType);
        }
        else //不在房间中,直接设置
        {
            if (App.emu.Core == null) return;
            var setuper = App.emu.Core.GetControllerSetuper();
            if (setuper == null) return;

            var freeSlotIndex = setuper.GetFreeSlotIndex();
            if (freeSlotIndex == null) return;

            setuper.LetControllerConnect(joyIndex, freeSlotIndex.Value);
        }
    }
}
