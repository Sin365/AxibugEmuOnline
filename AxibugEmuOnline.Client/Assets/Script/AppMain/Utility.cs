﻿using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public static class Utility
    {
        public static void SetActiveEx(this GameObject go, bool active)
        {
            if (active && go.activeSelf) return;
            if (!active && !go.activeSelf) return;

            go.SetActive(active);
        }

        public static string GetHostNickName(this Protobuf_Room_MiniInfo roomInfo)
        {
            var hostUID = roomInfo.HostPlayerUID;
            Protobuf_Room_GamePlaySlot slotdata = roomInfo.GamePlaySlotList.FirstOrDefault(w => w.PlayerUID == hostUID);
            if (slotdata != null)
                return slotdata.PlayerNickName;
            else
                return string.Empty;
        }

        public static void GetRoomPlayers(this Protobuf_Room_MiniInfo roomInfo, out int current, out int max)
        {
            current = 0; max = 4;
            current = roomInfo.GamePlaySlotList.Count(w => w.PlayerUID > 0);
        }

        private static Dictionary<int, RomFile> s_RomFileCahcesInRoomInfo = new Dictionary<int, RomFile>();
        public static void FetchRomFileInRoomInfo(this Protobuf_Room_MiniInfo roomInfo, Action<Protobuf_Room_MiniInfo, RomFile> callback)
        {
            RomFile romFile;

            if (s_RomFileCahcesInRoomInfo.TryGetValue(roomInfo.GameRomID, out romFile))
            {
                callback.Invoke(roomInfo, romFile);
                return;
            }

            App.StartCoroutine(App.httpAPI.GetRomInfo(roomInfo.GameRomID, (romWebData) =>
            {
                RomFile _romFile = new RomFile(0, 0, (RomPlatformType)romWebData.ptype);
                _romFile.SetWebData(romWebData);
                s_RomFileCahcesInRoomInfo[roomInfo.GameRomID] = _romFile;

                callback.Invoke(roomInfo, _romFile);
            }));

        }
    }
}
