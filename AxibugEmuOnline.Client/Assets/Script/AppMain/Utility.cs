using AxibugEmuOnline.Client.ClientCore;
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

        public static byte[] ToJPG(this Texture texture)
        {
            Texture2D outputTex = null;
            if (texture is RenderTexture rt)
            {
                outputTex = ConvertFromRenderTexture(rt);
            }
            else if (texture is Texture2D)
            {
                outputTex = texture as Texture2D;
            }

            return outputTex.EncodeToJPG();
        }

        private static Texture2D ConvertFromRenderTexture(RenderTexture rt)
        {
            // 创建临时RenderTexture并拷贝内容
            RenderTexture tempRT = RenderTexture.GetTemporary(rt.width, rt.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(rt, tempRT);

            // 读取到Texture2D
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
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
