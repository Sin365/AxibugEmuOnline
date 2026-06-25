using AxibugEmuOnline.Client.ClientCore;
using AxibugProtobuf;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary>
    /// 实现模拟器输出画面的比例调整类
    /// </summary>
    public class ScreenScaler
    {
        #region 给每个RomID单独存储缩放配置
        string RomID2ScalerSettingPath => App.PersistentDataRootPath() + "/RomDispSet";
        Dictionary<int, EnumScalerMode?> dictSettingCache = new Dictionary<int, EnumScalerMode?>();

        string GetRomID2ScalerSettingFileName(int romID)
        {
            return romID + ".sclr";
        }

        string GetRomID2ScalerSettingPath(int romID)
        {
            return RomID2ScalerSettingPath + "/" + GetRomID2ScalerSettingFileName(romID);
        }
        public EnumScalerMode GetRomIDScalerMode(int romID)
        {
            if (!dictSettingCache.ContainsKey(romID))
                dictSettingCache[romID] = LoadScalerModeFromFile(romID);
            EnumScalerMode? val = dictSettingCache[romID];
            return val.HasValue ?  val.Value : GlobalMode;
        }

        public void SetScalerMode(int romID, EnumScalerMode mode)
        {
            if (dictSettingCache.ContainsKey(romID) && dictSettingCache[romID] == mode)
                return;
            dictSettingCache[romID] = mode;
            SaveScalerModeToFile(romID, mode);
        }


        EnumScalerMode? LoadScalerModeFromFile(int romID)
        {
            string path = GetRomID2ScalerSettingPath(romID);
            if (!AxiIO.File.Exists(path))
                return null;
            else
                return (EnumScalerMode)BitConverter.ToInt32(AxiIO.File.ReadAllBytes(path));
        }

        void SaveScalerModeToFile(int romID, EnumScalerMode mode)
        {
            if(!AxiIO.Directory.Exists(RomID2ScalerSettingPath))
                AxiIO.Directory.CreateDirectory(RomID2ScalerSettingPath);

            string path = GetRomID2ScalerSettingPath(romID);
            AxiIO.File.WriteAllBytes(path, BitConverter.GetBytes((int)mode));
        }
        #endregion

        string key_GlobalMode = nameof(ScreenScaler) + ".GlobalMode";
        ////Dictionary<RomPlatformType, string> cache_PlatMode = new Dictionary<RomPlatformType, string>();
        //string get_key_PlatMode(RomPlatformType platform)
        //{
        //    if (cache_PlatMode.ContainsKey(platform))
        //        return cache_PlatMode[platform];
        //    string val = nameof(ScreenScaler) + ".PlatMode." + platform;
        //    cache_PlatMode[platform] = val;
        //    return val;
        //}

        /// <summary>
        /// 全局设置的缩放模式
        /// </summary>
        public EnumScalerMode GlobalMode
        {
            //get => (EnumScalerMode)AxiPlayerPrefs.GetInt($"{nameof(ScreenScaler)}.GlobalMode", 0);
            //set => AxiPlayerPrefs.SetInt($"{nameof(ScreenScaler)}.GlobalMode", (int)value);
            get => (EnumScalerMode)AxiPlayerPrefs.GetInt(key_GlobalMode, (int)EnumScalerMode.Fix);
            set => AxiPlayerPrefs.SetInt(key_GlobalMode, (int)value);
        }

        /*
        /// <summary>
        /// 获得指定平台设置的缩放模式
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public EnumScalerMode GetMode(RomPlatformType platform)
        {
            int setVal = AxiPlayerPrefs.GetInt(get_key_PlatMode(platform), -1);
            if (setVal == -1)
                return GlobalMode;
            else
                return (EnumScalerMode)setVal;
        }*/

        /*
        public bool IsSetMode(RomPlatformType platform)
        {
            int setVal = AxiPlayerPrefs.GetInt(get_key_PlatMode(platform), -1);
            return setVal != -1;
        }

        public void SetMode(RomPlatformType platform, EnumScalerMode? mode)
        {
            int setVal = mode == null ? -1 : (int)mode;
            AxiPlayerPrefs.SetInt(get_key_PlatMode(platform), setVal);
        }*/

        /// <summary>
        /// 根据缩放模式设置UI的缩放
        /// </summary>
        /// <param name="m_rawImg"></param>
        /// <param name="platform">不指定模拟器平台时,使用全局设置的缩放模式</param>
        public void CalcScale(RawImage rawImg, Vector3 srcEulerAngles, RomPlatformType? platform = null, int? RomID = null)
        {
            var targetMode = RomID == null ? GlobalMode : GetRomIDScalerMode(RomID.Value);
            var resolution = GetRawResolution(platform == null ? RomPlatformType.Nes : platform.Value);
            var canvasRect = (rawImg.canvas.transform as RectTransform).rect;
            switch (targetMode)
            {
                case EnumScalerMode.Raw:
                    {
                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Raw_x2:
                    {
                        int pr = 2;
                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * pr);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * pr);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Raw_x3:
                    {
                        int pr = 3;
                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * pr);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * pr);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Raw_x4:
                    {
                        int pr = 4;
                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * pr);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * pr);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Raw_x5:
                    {
                        int pr = 5;
                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * pr);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * pr);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Raw_x6:
                    {
                        int pr = 6;
                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * pr);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * pr);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Fix:
                    {
                        bool stretchWidth = rawImg.canvas.pixelRect.width <= rawImg.canvas.pixelRect.height;
                        //bool stretchWidth = Mathf.Abs(resolution.x - rawImg.canvas.pixelRect.width) <= Mathf.Abs(resolution.y - rawImg.canvas.pixelRect.height);
                        if (stretchWidth)
                        {
                            var needWidth = rawImg.canvas.pixelRect.width;
                            var factor = needWidth / resolution.x;
                            resolution.x = (int)needWidth;
                            resolution.y = (int)(resolution.y * factor);
                        }
                        else
                        {
                            var needHeight = rawImg.canvas.pixelRect.height;
                            var factor = needHeight / resolution.y;
                            resolution.y = (int)needHeight;
                            resolution.x = (int)(resolution.x * factor);
                        }

                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.FullScreen:
                    {
                        rawImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        rawImg.rectTransform.anchorMin = new Vector2(0, 0);
                        rawImg.rectTransform.anchorMax = new Vector2(1, 1);
                        rawImg.rectTransform.sizeDelta = new Vector2(0, 0);
                        rawImg.rectTransform.anchoredPosition = new Vector2(0, 0);
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles;
                    }
                    break;
                case EnumScalerMode.Rotate_90:
                    {

                        bool stretchWidth = rawImg.canvas.pixelRect.width <= rawImg.canvas.pixelRect.height;
                        //bool stretchWidth = Mathf.Abs(resolution.x - rawImg.canvas.pixelRect.width) <= Mathf.Abs(resolution.y - rawImg.canvas.pixelRect.height);
                        if (stretchWidth)
                        {
                            var needWidth = rawImg.canvas.pixelRect.width;
                            var factor = needWidth / resolution.x;
                            resolution.x = (int)needWidth;
                            resolution.y = (int)(resolution.y * factor);
                        }
                        else
                        {
                            var needHeight = rawImg.canvas.pixelRect.height;
                            var factor = needHeight / resolution.y;
                            resolution.y = (int)needHeight;
                            resolution.x = (int)(resolution.x * factor);
                        }

                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;

                        float newwidth = height;
                        float newheight = height * (height / width);

                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newwidth);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newheight);

                        //旋转 90°
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles + new Vector3(0, 0, 90);
                    }
                    break;
                case EnumScalerMode.Rotate_270:
                    {
                        bool stretchWidth = rawImg.canvas.pixelRect.width <= rawImg.canvas.pixelRect.height;
                        //bool stretchWidth = Mathf.Abs(resolution.x - rawImg.canvas.pixelRect.width) <= Mathf.Abs(resolution.y - rawImg.canvas.pixelRect.height);
                        if (stretchWidth)
                        {
                            var needWidth = rawImg.canvas.pixelRect.width;
                            var factor = needWidth / resolution.x;
                            resolution.x = (int)needWidth;
                            resolution.y = (int)(resolution.y * factor);
                        }
                        else
                        {
                            var needHeight = rawImg.canvas.pixelRect.height;
                            var factor = needHeight / resolution.y;
                            resolution.y = (int)needHeight;
                            resolution.x = (int)(resolution.x * factor);
                        }

                        float width = resolution.x / rawImg.canvas.pixelRect.width * canvasRect.width;
                        float height = resolution.y / rawImg.canvas.pixelRect.height * canvasRect.height;

                        float newwidth = height;
                        float newheight = height * (height / width);

                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newwidth);
                        rawImg.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newheight);

                        //旋转 270°
                        rawImg.rectTransform.localEulerAngles = srcEulerAngles + new Vector3(0, 0, 270);
                    }
                    break;
            }
        }

        public Vector2Int GetRawResolution(RomPlatformType platform)
        {
            switch (platform)
            {
                case RomPlatformType.Nes:
                    return new Vector2Int(256, 240);
                case RomPlatformType.Cps1:
                case RomPlatformType.Cps2:
                case RomPlatformType.Neogeo:
                case RomPlatformType.Igs:
                case RomPlatformType.ArcadeOld:
                    return UMAME.instance.mUniVideoPlayer.mScreenSize;
                case RomPlatformType.MasterSystem:
                case RomPlatformType.GameGear:
                case RomPlatformType.GameBoy:
                case RomPlatformType.GameBoyColor:
                case RomPlatformType.ColecoVision:
                case RomPlatformType.Sc3000:
                case RomPlatformType.Sg1000:
                    return UEssgee.instance.graphicsHandler.mScreenSize;
                case RomPlatformType.WonderSwan:
                case RomPlatformType.WonderSwanColor:
                    return new Vector2Int(224, 144);
                //return UStoicGoose.instance.graphicsHandler.ScreenSize;
                default: throw new System.NotImplementedException($"未实现的平台:{platform}");
            }
        }

        /// <summary> 缩放模式 </summary>
        public enum EnumScalerMode
        {
            /// <summary> 全屏 </summary>
            FullScreen,
            /// <summary> 适应 </summary>
            Fix,
            /// <summary> 原始 </summary>
            Raw,
            Raw_x2,
            Raw_x3,
            Raw_x4,
            Raw_x5,
            Raw_x6,
            Rotate_270,
            Rotate_90,
        };
    }
}
