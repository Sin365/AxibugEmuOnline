namespace MyNes.Core
{
    public class RendererSettings : ISettings
    {
    	public string Video_ProviderID = "";

    	public bool Vid_AutoStretch = true;

    	public bool Vid_Res_Upscale = true;

    	public int Vid_Res_W = 640;

    	public int Vid_Res_H = 480;

    	public int Vid_StretchMultiply = 2;

    	public bool Vid_KeepAspectRatio = true;

    	public bool Vid_ShowFPS = false;

    	public bool Vid_Fullscreen = false;

    	public bool Vid_HardwareVertexProcessing = false;

    	public bool Vid_VSync = false;

    	public bool Vid_ShowNotifications = true;

    	public int Vid_Filter = 1;

    	public bool FrameSkipEnabled = false;

    	public int FrameSkipInterval = 2;

    	public bool UseEmuThread = true;

    	public string Audio_ProviderID = "";

    	public bool Audio_EnableFilters = true;

    	public int Audio_Volume = 100;

    	public bool Audio_SoundEnabled = true;

    	public int Audio_Frequency = 44100;

    	public int Audio_InternalSamplesCount = 4096;

    	public int Audio_InternalPeekLimit = 124;

    	public int Audio_PlaybackAmplitude = 200;

    	public int Audio_PlaybackBufferSizeInKB = 8;

    	public bool Audio_UseDefaultMixer = true;

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

    	public RendererSettings(string path)
    		: base(path)
    	{
    	}
    }
}
