using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.Collections;
using System.Collections.Generic;
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
            if (hostUID == roomInfo.Player1UID) return roomInfo.Player1NickName;
            else if (hostUID == roomInfo.Player2UID) return roomInfo.Player2NickName;
            else if (hostUID == roomInfo.Player3UID) return roomInfo.Player3NickName;
            else if (hostUID == roomInfo.Player4UID) return roomInfo.Player4NickName;
            else return string.Empty;
        }

        public static void GetRoomPlayers(this Protobuf_Room_MiniInfo roomInfo, out int current, out int max)
        {
            current = 0; max = 4;

            if (roomInfo.Player1UID > 0) current++;
            if (roomInfo.Player2UID > 0) current++;
            if (roomInfo.Player3UID > 0) current++;
            if (roomInfo.Player4UID > 0) current++;
        }

        private static Dictionary<int, RomFile> s_RomFileCahcesInRoomInfo = new Dictionary<int, RomFile>();
        public static void FetchRomFileInRoomInfo(this Protobuf_Room_MiniInfo roomInfo, EnumPlatform platform, Action<Protobuf_Room_MiniInfo, RomFile> callback)
        {
            if (s_RomFileCahcesInRoomInfo.TryGetValue(roomInfo.GameRomID, out RomFile romFile))
            {
                callback.Invoke(roomInfo,romFile);
                return;
            }
            switch (platform)
            {
                case EnumPlatform.NES:
                    App.StartCoroutine(App.httpAPI.GetNesRomInfo(roomInfo.GameRomID, (romWebData) =>
                    {
                        RomFile romFile = new RomFile(EnumPlatform.NES, 0, 0);
                        romFile.SetWebData(romWebData);
                        s_RomFileCahcesInRoomInfo[roomInfo.GameRomID] = romFile;
                        
                        callback.Invoke(roomInfo,romFile);
                    }));
                    break;
            }

        }
    }
}
