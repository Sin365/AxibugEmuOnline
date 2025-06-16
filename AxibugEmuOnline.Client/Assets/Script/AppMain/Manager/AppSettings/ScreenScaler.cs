using AxibugProtobuf;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client.Settings
{
    /// <summary>
    /// 实现模拟器输出画面的比例调整类
    /// </summary>
    public class ScreenScaler
    {
        /// <summary>
        /// 全局设置的缩放模式
        /// </summary>
        public EnumScalerMode GlobalMode
        {
            get => (EnumScalerMode)AxiPlayerPrefs.GetInt($"{nameof(ScreenScaler)}.GlobalMode", 0);
            set => AxiPlayerPrefs.SetInt($"{nameof(ScreenScaler)}.GlobalMode", (int)value);
        }

        /// <summary>
        /// 获得指定平台设置的缩放模式
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public EnumScalerMode GetMode(RomPlatformType platform)
        {
            int setVal = AxiPlayerPrefs.GetInt($"{nameof(ScreenScaler)}.PlatMode.{platform}", -1);
            if (setVal == -1)
                return GlobalMode;
            else
                return (EnumScalerMode)setVal;
        }

        public bool IsSetMode(RomPlatformType platform)
        {
            int setVal = AxiPlayerPrefs.GetInt($"{nameof(ScreenScaler)}.PlatMode.{platform}", -1);
            return setVal != -1;
        }

        public void SetMode(RomPlatformType platform, EnumScalerMode? mode)
        {
            int setVal = mode == null ? -1 : (int)mode;
            AxiPlayerPrefs.SetInt($"{nameof(ScreenScaler)}.PlatMode.{platform}", setVal);
        }

        /// <summary>
        /// 根据缩放模式设置UI的缩放
        /// </summary>
        /// <param name="m_rawImg"></param>
        /// <param name="platform">不指定模拟器平台时,使用全局设置的缩放模式</param>
        public void CalcScale(RawImage rawImg, RomPlatformType? platform = null)
        {
            var targetMode = platform == null ? GlobalMode : GetMode(platform.Value);
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
                    }
                    break;
                case EnumScalerMode.FullScreen:
                    {
                        rawImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        rawImg.rectTransform.anchorMin = new Vector2(0, 0);
                        rawImg.rectTransform.anchorMax = new Vector2(1, 1);
                        rawImg.rectTransform.sizeDelta = new Vector2(0, 0);
                        rawImg.rectTransform.anchoredPosition = new Vector2(0, 0);
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
            Raw
        };
    }
}
