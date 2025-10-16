using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace AxibugEmuOnline.Client
{
    public interface AxiAudioPull
    {
        public void PullAudio(float[] data, int channels);
    }

    public class AudioMgr : MonoBehaviour
    {
        public enum E_SFXTYPE
        {
            Cancel,
            Cursor,
            Option,
            Launch,
            system_ng,
            system_ok
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystem();
        }

        #region 音频资源
        Dictionary<E_SFXTYPE, AudioClip> dictAudioClip = new Dictionary<E_SFXTYPE, AudioClip>();
        void LoadAudioClip()
        {
            dictAudioClip[E_SFXTYPE.Cancel] = Resources.Load<AudioClip>("Sound/XMBSFX/cancel");
            dictAudioClip[E_SFXTYPE.Cursor] = Resources.Load<AudioClip>("Sound/XMBSFX/cursor");
            dictAudioClip[E_SFXTYPE.Option] = Resources.Load<AudioClip>("Sound/XMBSFX/option");
            dictAudioClip[E_SFXTYPE.Launch] = Resources.Load<AudioClip>("Sound/XMBSFX/StartPSP");
            dictAudioClip[E_SFXTYPE.system_ng] = Resources.Load<AudioClip>("Sound/XMBSFX/system_ng");
            dictAudioClip[E_SFXTYPE.system_ok] = Resources.Load<AudioClip>("Sound/XMBSFX/system_ok");
        }
        #endregion

        [SerializeField] private AudioMixerGroup _staticGroup; // 静态音效（UI等）输出组
        [Header("静态音效")]
        [SerializeField] private AudioSource _staticAudioSource; // 用于播放静态音效的源
        AudioStreamData _audioStreams;
        private int _targetOutputSampleRate; // Unity音频系统的输出采样率

        /// <summary>
        /// 初始化音频系统
        /// </summary>
        private void InitializeAudioSystem()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            _targetOutputSampleRate = AudioSettings.outputSampleRate;
            if (_staticAudioSource == null)
            {
                _staticAudioSource = this.gameObject.AddComponent<AudioSource>();
                _staticAudioSource.outputAudioMixerGroup = _staticGroup;
            }

            // 设置初始音量
            SetStaticVolume(0.9f);
            Debug.Log($"Audio System Initialized. Output Sample Rate: {_targetOutputSampleRate}Hz");
            LoadAudioClip();
        }

        /// <summary>
        /// 切换设备（蓝牙）后重新播放背景音乐
        /// </summary>
        /// <param name="deviceWasChanged"></param>
        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            //函数仅处理设备变化的情况，非设备变化不再本函数处理，避免核心采样率变化和本处循环调用
            if (deviceWasChanged)
            {
                ResetAudioCfg(AudioSettings.outputSampleRate);
                //AudioConfiguration config = AudioSettings.GetConfiguration();
                //AudioSettings.Reset(config);
                //TODO 重新播放音效，但是DSP不用，若有UI BGM，后续 这里加重播
            }
        }

        #region 静态音源
        public void PlaySFX(E_SFXTYPE type, bool isLoop = false)
        {
            PlayStaticSound(dictAudioClip[type], 1, 1);
        }
        /// <summary>
        /// 播放静态音频剪辑（UI音效等）
        /// </summary>
        void PlayStaticSound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (clip == null) return;
            _staticAudioSource.pitch = Mathf.Clamp(pitch, 0.5f, 2.0f);
            _staticAudioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        /// <summary>
        /// 设置静态音频音量（线性0.0-1.0）
        /// </summary>
        public void SetStaticVolume(float volumeLinear)
        {
            if (_staticGroup != null && _staticGroup.audioMixer != null)
            {
                float volumeDB = ConvertLinearToDecibel(Mathf.Clamp01(volumeLinear));
                _staticGroup.audioMixer.SetFloat("StaticVolume", volumeDB);
            }
        }
        #endregion

        #region 动态音源（模拟器）
        /// <summary>
        /// 注册一个动态音频流通道（模拟器）
        /// </summary>
        /// <param name="channelId">通道标识符 (e.g., "NES", "MAME")</param>
        /// <param name="inputSampleRate">该通道的原始采样率</param>
        public void RegisterStream(string channelId, int? inputSampleRate, AxiAudioPull audioPullHandle)
        {
            _audioStreams = null;
            _audioStreams = new AudioStreamData(channelId,
                inputSampleRate.HasValue ? inputSampleRate.Value : AudioSettings.outputSampleRate
                , audioPullHandle);
            ResetAudioCfg(inputSampleRate);
        }

        private void ResetAudioCfg(int? inputSampleRate)
        {
            // 获取当前音频配置
            AudioConfiguration config = AudioSettings.GetConfiguration();

            // 设置目标音频配置
            config.sampleRate = inputSampleRate.HasValue ? inputSampleRate.Value : 48000;       // 采样率为 44100Hz
            config.numRealVoices = 32;      // 设置最大音频源数量（可选）
            config.numVirtualVoices = 512;   // 设置虚拟音频源数量（可选）
            config.dspBufferSize = 1024;     // 设置 DSP 缓冲区大小（可选）
            config.speakerMode = AudioSpeakerMode.Stereo; // 设置为立体声（2 声道）

            // 应用新的音频配置
            if (AudioSettings.Reset(config))
            {
                Debug.Log("Audio settings updated successfully.");
                Debug.Log("Sample Rate: " + config.sampleRate + "Hz");
                Debug.Log("Speaker Mode: " + config.speakerMode);
            }
            else
            {
                Debug.LogError("Failed to update audio settings.");
            }
            _staticAudioSource.Play();//只为让DSP继续
        }

        /// <summary>
        /// 清空指定通道的音频数据
        /// </summary>
        public void ClearAudioData(string channelId)
        {
            if (_audioStreams == null || _audioStreams.channelid != channelId)
                return;
            _audioStreams = null;
        }
        #endregion

        #region Core Audio Processing (Called automatically by Unity)
        /// <summary>
        /// Unity音频线程回调：在这里处理和混合所有动态音频流[1](@ref)
        /// </summary>
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (_audioStreams == null) return;
            _audioStreams.AxiAudioPullHandle.PullAudio(data, channels);

            //TODO 如果要处理采样率差异
            if (_audioStreams.NeedsResampling) { }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// 线性音量值转换为分贝值 (dB)[4](@ref)
        /// </summary>
        private float ConvertLinearToDecibel(float linear)
        {
            if (linear <= 0.0001f) return -80.0f; // 避免log10(0)
            return Mathf.Log10(linear) * 20.0f;
        }

        #endregion

        #region 录音功能实现

        WaveHeader waveHeader;
        FormatChunk formatChunk;
        DataChunk dataChunk;

        public bool IsRecording { get; private set; }

        public void BeginRecordGameAudio()
        {
            App.emu.Core.GetAudioParams(out int frequency, out int channels);
            BeginRecording(frequency, channels);
        }

        public void StopRecordGameAudio()
        {
            SaveRecording(GetSavePath());
        }

        string GetSavePath()
        {
            string dir = $"{App.PersistentDataPath(App.emu.Core.Platform)}/AxiSoundRecord";
            if (!AxiIO.Directory.Exists(dir))
            {
                AxiIO.Directory.CreateDirectory(dir);
            }
            return $"{dir}/{App.tick.GetDateTimeStr()}";
        }

        void BeginRecording(int frequency, int channels)
        {
            if (IsRecording)
            {
                OverlayManager.PopTip("已有正在进行的录音……");
                return;
            }
            waveHeader = new WaveHeader();
            formatChunk = new FormatChunk(frequency, channels);
            dataChunk = new DataChunk();
            waveHeader.FileLength += formatChunk.Length();
            IsRecording = true;
            OverlayManager.PopTip("录音开始");
        }

        void SaveRecording(string filename)
        {
            if (!IsRecording)
            {
                OverlayManager.PopTip("没有录音数据");
                return;
            }
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ms.Write(waveHeader.GetBytes(), 0, (int)waveHeader.Length());
                ms.Write(formatChunk.GetBytes(), 0, (int)formatChunk.Length());
                ms.Write(dataChunk.GetBytes(), 0, (int)dataChunk.Length());
                AxiIO.File.WriteAllBytesFromStream(filename, ms);
            }
            IsRecording = false;
            OverlayManager.PopTip("录音结束");
        }

        public unsafe void WriteToRecord(short* stereoBuffer, int lenght)
        {
            if (!IsRecording) return;
            dataChunk.AddSampleData(stereoBuffer, lenght);
            waveHeader.FileLength += (uint)lenght;
        }
        #endregion
    }

    // 用于描述一个动态音频流的数据结构
    public class AudioStreamData
    {
        public string channelid;
        public int SourceSampleRate;
        public bool NeedsResampling;
        public AxiAudioPull AxiAudioPullHandle;
        public AudioStreamData(string channelid, int SourceSampleRate, AxiAudioPull audiohandle)
        {
            this.channelid = channelid;
            this.SourceSampleRate = SourceSampleRate;
            this.AxiAudioPullHandle = audiohandle;
            NeedsResampling = SourceSampleRate != AudioSettings.outputSampleRate;
            AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);
        }
    }
    class WaveHeader
    {
        const string fileTypeId = "RIFF";
        const string mediaTypeId = "WAVE";
        public string FileTypeId { get; private set; }
        public uint FileLength { get; set; }
        public string MediaTypeId { get; private set; }
        public WaveHeader()
        {
            FileTypeId = fileTypeId;
            MediaTypeId = mediaTypeId;
            FileLength = 4;     /* Minimum size is always 4 bytes */
        }
        public byte[] GetBytes()
        {
            List<byte> chunkData = new List<byte>();

            chunkData.AddRange(Encoding.ASCII.GetBytes(FileTypeId));
            chunkData.AddRange(BitConverter.GetBytes(FileLength));
            chunkData.AddRange(Encoding.ASCII.GetBytes(MediaTypeId));

            return chunkData.ToArray();
        }
        public uint Length()
        {
            return (uint)GetBytes().Length;
        }
    }

    class FormatChunk
    {
        const string chunkId = "fmt ";
        ushort bitsPerSample, channels;
        uint frequency;
        public string ChunkId { get; private set; }
        public uint ChunkSize { get; private set; }
        public ushort FormatTag { get; private set; }
        public ushort Channels
        {
            get { return channels; }
            set { channels = value; RecalcBlockSizes(); }
        }
        public uint Frequency
        {
            get { return frequency; }
            set { frequency = value; RecalcBlockSizes(); }
        }
        public uint AverageBytesPerSec { get; private set; }
        public ushort BlockAlign { get; private set; }
        public ushort BitsPerSample
        {
            get { return bitsPerSample; }
            set { bitsPerSample = value; RecalcBlockSizes(); }
        }
        public FormatChunk()
        {
            ChunkId = chunkId;
            ChunkSize = 16;
            FormatTag = 1;          /* MS PCM (Uncompressed wave file) */
            Channels = 2;           /* Default to stereo */
            Frequency = 44100;      /* Default to 44100hz */
            BitsPerSample = 16;     /* Default to 16bits */
            RecalcBlockSizes();
        }
        public FormatChunk(int frequency, int channels) : this()
        {
            Channels = (ushort)channels;
            Frequency = (ushort)frequency;
            RecalcBlockSizes();
        }
        private void RecalcBlockSizes()
        {
            BlockAlign = (ushort)(channels * (bitsPerSample / 8));
            AverageBytesPerSec = frequency * BlockAlign;
        }
        public byte[] GetBytes()
        {
            List<byte> chunkBytes = new List<byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            chunkBytes.AddRange(BitConverter.GetBytes(FormatTag));
            chunkBytes.AddRange(BitConverter.GetBytes(Channels));
            chunkBytes.AddRange(BitConverter.GetBytes(Frequency));
            chunkBytes.AddRange(BitConverter.GetBytes(AverageBytesPerSec));
            chunkBytes.AddRange(BitConverter.GetBytes(BlockAlign));
            chunkBytes.AddRange(BitConverter.GetBytes(BitsPerSample));

            return chunkBytes.ToArray();
        }
        public uint Length()
        {
            return (uint)GetBytes().Length;
        }
    }

    class DataChunk
    {
        const string chunkId = "data";

        public string ChunkId { get; private set; }
        public uint ChunkSize { get; set; }
        public List<short> WaveData { get; private set; }

        public DataChunk()
        {
            ChunkId = chunkId;
            ChunkSize = 0;
            WaveData = new List<short>();
        }

        public byte[] GetBytes()
        {
            //待优化
            List<byte> chunkBytes = new List<byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            byte[] bufferBytes = new byte[WaveData.Count * 2];
            Buffer.BlockCopy(WaveData.ToArray(), 0, bufferBytes, 0, bufferBytes.Length);
            chunkBytes.AddRange(bufferBytes.ToList());

            return chunkBytes.ToArray();
        }

        public uint Length()
        {
            return (uint)GetBytes().Length;
        }

        public unsafe void AddSampleData(short* stereoBuffer, int lenght)
        {
            for (int i = 0; i < lenght; i++)
            {
                WaveData.Add(stereoBuffer[i]);
            }
            ChunkSize += (uint)(lenght * 2);
        }
    }
}
