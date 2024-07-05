using System.IO;

namespace MyNes.Core
{
    public class RendererSettings : ISettings
    {
    	public string Video_ProviderID = "";

    	public bool Vid_AutoStretch = true;

    	public int Vid_StretchMultiply = 3;

    	public bool Vid_KeepAspectRatio;

    	public bool Vid_ShowFPS;

    	public bool Vid_HideLines = true;

    	public bool Vid_Fullscreen;

    	public bool Vid_HardwareVertexProcessing;

    	public bool Vid_VSync;

    	public bool Vid_ShowNotifications = true;

    	public int Vid_Filter = 1;

    	public bool FrameSkipEnabled;

    	public int FrameSkipInterval = 2;

    	public string Audio_ProviderID = "";

    	public bool Audio_EnableFilters = true;

    	public int Audio_Volume = 100;

    	public bool Audio_SoundEnabled = true;

    	public int Audio_Frequency = 48000;

    	public int Audio_InternalSamplesCount = 1024;

    	public int Audio_InternalPeekLimit = 124;

    	public int Audio_PlaybackAmplitude = 200;

    	public int Audio_PlaybackBufferSizeInKB = 16;

    	public bool Audio_UseDefaultMixer;

    	public bool Audio_ChannelEnabled_SQ1 = true;

    	public bool Audio_ChannelEnabled_SQ2 = true;

    	public bool Audio_ChannelEnabled_NOZ = true;

    	public bool Audio_ChannelEnabled_TRL = true;

    	public bool Audio_ChannelEnabled_DMC = true;

    	public bool Audio_ChannelEnabled_MMC5_SQ1 = true;

    	public bool Audio_ChannelEnabled_MMC5_SQ2 = true;

    	public bool Audio_ChannelEnabled_MMC5_PCM = true;

    	public bool Audio_ChannelEnabled_VRC6_SQ1 = true;

    	public bool Audio_ChannelEnabled_VRC6_SQ2 = true;

    	public bool Audio_ChannelEnabled_VRC6_SAW = true;

    	public bool Audio_ChannelEnabled_SUN1 = true;

    	public bool Audio_ChannelEnabled_SUN2 = true;

    	public bool Audio_ChannelEnabled_SUN3 = true;

    	public bool Audio_ChannelEnabled_NMT1 = true;

    	public bool Audio_ChannelEnabled_NMT2 = true;

    	public bool Audio_ChannelEnabled_NMT3 = true;

    	public bool Audio_ChannelEnabled_NMT4 = true;

    	public bool Audio_ChannelEnabled_NMT5 = true;

    	public bool Audio_ChannelEnabled_NMT6 = true;

    	public bool Audio_ChannelEnabled_NMT7 = true;

    	public bool Audio_ChannelEnabled_NMT8 = true;

    	public int Palette_PaletteSetting;

    	public float Palette_NTSC_brightness = 1.075f;

    	public float Palette_NTSC_contrast = 1.016f;

    	public float Palette_NTSC_gamma = 1.975f;

    	public float Palette_NTSC_hue_tweak;

    	public float Palette_NTSC_saturation = 1.496f;

    	public float Palette_PALB_brightness = 1.075f;

    	public float Palette_PALB_contrast = 1.016f;

    	public float Palette_PALB_gamma = 1.975f;

    	public float Palette_PALB_hue_tweak;

    	public float Palette_PALB_saturation = 1.496f;

    	public RendererSettings(string path)
    		: base(path)
    	{
    	}
    }
}
