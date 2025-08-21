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

        public static byte[] ToJPG(this Texture texture, Vector2 scale)
        {
            Texture2D outputTex = ConvertFromRenderTexture(texture, scale);

            return outputTex.EncodeToJPG();
        }

        private static Texture2D ConvertFromRenderTexture(Texture src, Vector2 scale)
        {
            float offsetX = (scale.x < 0) ? 1 : 0;
            float offsetY = (scale.y < 0) ? 1 : 0;

            var offset = new Vector2(offsetX, offsetY);

            // 创建临时RenderTexture并拷贝内容
            RenderTexture tempRT = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, tempRT, scale, offset);

            // 读取到Texture2D
            Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            RenderTexture.active = tempRT;
            tex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            tex.Apply();

            // 释放资源
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tempRT);
            return tex;
        }
    }
}
