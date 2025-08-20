using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
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

        public Dictionary<E_SFXTYPE, AudioClip> dictAudioClip = new Dictionary<E_SFXTYPE, AudioClip>();

        private AudioSource mSource;
        private void Awake()
        {
            mSource = this.gameObject.AddComponent<AudioSource>();
            LoadAudioClip();
            PlaySFX(E_SFXTYPE.Launch);
        }

        /// <summary>
        /// 手动设置AudioCfg 主要用于模拟器各核心采样率对齐
        /// </summary>
        /// <param name="config"></param>
        public void SetAudioConfig(AudioConfiguration config)
        {
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
                AudioConfiguration config = AudioSettings.GetConfiguration();
                AudioSettings.Reset(config);
                //TODO 重新播放音效，但是DSP不用，若有UI BGM，后续 这里加重播
            }
        }

        public void LoadAudioClip()
        {
            dictAudioClip[E_SFXTYPE.Cancel] = Resources.Load<AudioClip>("Sound/XMBSFX/cancel");
            dictAudioClip[E_SFXTYPE.Cursor] = Resources.Load<AudioClip>("Sound/XMBSFX/cursor");
            dictAudioClip[E_SFXTYPE.Option] = Resources.Load<AudioClip>("Sound/XMBSFX/option");
            dictAudioClip[E_SFXTYPE.Launch] = Resources.Load<AudioClip>("Sound/XMBSFX/StartPSP");
            dictAudioClip[E_SFXTYPE.system_ng] = Resources.Load<AudioClip>("Sound/XMBSFX/system_ng");
            dictAudioClip[E_SFXTYPE.system_ok] = Resources.Load<AudioClip>("Sound/XMBSFX/system_ok");
        }

        public void PlaySFX(E_SFXTYPE type, bool isLoop = false)
        {
            mSource.clip = dictAudioClip[type];
            mSource.loop = isLoop;
            mSource.Play();
        }

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

            //using (FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            //{
            //    file.Write(waveHeader.GetBytes(), 0, (int)waveHeader.Length());
            //    file.Write(formatChunk.GetBytes(), 0, (int)formatChunk.Length());
            //    file.Write(dataChunk.GetBytes(), 0, (int)dataChunk.Length());
            //}

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
