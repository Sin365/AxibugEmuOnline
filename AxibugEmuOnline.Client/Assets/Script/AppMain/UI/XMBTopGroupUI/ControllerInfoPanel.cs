using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Event;
using AxibugEmuOnline.Client.Manager;
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
        //如果是GBA禁止动态插入第二个插槽
        if (App.emu.Core != null && InGameUI.Instance.RomFile.Platform == AxibugProtobuf.RomPlatformType.GameBoyAdvance)
        {
            if (App.roomMgr.InRoom && App.roomMgr.GetMinePlayerSlotCount() >= 1)
            {
#if UNITY_EDITOR
                App.log.Debug("GBA禁止动态插入第二个手柄插槽");
#endif
                return;
            }
            else
            {
                //TODO 单机也应该禁用
            }
        }

        if (App.roomMgr.InRoom)
        {
            if (!App.roomMgr.mineRoomMiniInfo.GetFreeSlot(ref s_freeSlots)) return;

            //找到第一个空闲手柄插槽
            var freeSlotIndex = s_freeSlots[0];

            App.roomMgr.SendChangePlaySlotIdxWithJoyIdx((uint)joyIndex, (uint)freeSlotIndex);
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
