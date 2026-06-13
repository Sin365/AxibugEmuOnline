using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using System;
using UnityEngine;

public class SGSoundPlayer : MonoBehaviour, AxiAudioPull
{
    [SerializeField]
    private AudioSource m_as;

    // 大幅加大缓冲 + 预留安全余量
    private RingBuffer<float> _buffer = new RingBuffer<float>(44100 * 12); // 约 270ms 缓冲

    private float lastSample = 0f;

    [HideInInspector]
    public int sampleRate = 44100;
    [HideInInspector]
    public int channels = 2;

    // 重采样
    private double resampleAccumulator = 0.0;
    private const double TargetSampleRate = 44100.0;
    private const double SourceSampleRate = 24000.0;

    // 新增：音频统计和动态调整
    private int totalWritten = 0;
    private int totalPulled = 0;
    private float underrunCount = 0;

    private void Awake()
    {
        if (m_as == null)
            m_as = GetComponent<AudioSource>();

        // 推荐设置
        var config = AudioSettings.GetConfiguration();
        config.sampleRate = 44100;
        config.speakerMode = AudioSpeakerMode.Stereo;
        config.dspBufferSize = 512;      // 关键：不要太小（256容易爆，1024延迟大）
        AudioSettings.Reset(config);
    }

    private void OnEnable()
    {
        App.audioMgr.RegisterStream(nameof(UStoicGoose), 24000, this);
    }

    private void OnDisable()
    {
        App.audioMgr.ClearAudioData(nameof(UStoicGoose));
    }

    /// <summary>
    /// Unity 音频拉取
    /// </summary>
    public unsafe void PullAudio(float[] data, int channels)
    {
        fixed (float* pData = data)
        {
            float* ptr = pData;
            int length = data.Length;

            for (int i = 0; i < length; i++)
            {
                if (_buffer.TryRead(out float sample))
                {
                    *ptr = sample;
                    lastSample = sample;
                }
                else
                {
                    *ptr = lastSample * 0.92f;   // 更激进淡出
                    lastSample *= 0.92f;
                    underrunCount += 1;
                }
                ptr++;
            }
        }

        totalPulled += data.Length;
    }

    /// <summary>
    /// 模拟器核心推送音频（关键优化）
    /// </summary>
    internal void EnqueueSamples(short[] buffer)
    {
        double ratio = SourceSampleRate / TargetSampleRate;

        for (int i = 0; i < buffer.Length; i++)
        {
            float sample = buffer[i] / 32767.0f;

            resampleAccumulator += ratio;

            while (resampleAccumulator >= 1.0)
            {
                _buffer.Write(sample);
                resampleAccumulator -= 1.0;
                totalWritten++;
            }
        }
    }

    // ====================== 调试信息 ======================
    //private void Update()
    //{
    //    if (Time.frameCount % 60 == 0 && totalPulled > 0)
    //    {
    //        float usage = (float)_buffer.Count / _buffer.Capacity * 100f;
    //        float underrunRate = underrunCount / (totalPulled / 44100f); // 每秒 underrun 次数

    //        Debug.Log($"[Audio] Buffer: {usage:F1}% | Underrun: {underrunRate:F1}/s | FPS: {audioFPS:F1}");

    //        // 如果 underrun 太多，自动加大缓冲（可选）
    //        if (underrunRate > 8f && _buffer.Capacity < 44100 * 20)
    //        {
    //            Debug.LogWarning("Underrun too high, consider increasing buffer");
    //        }

    //        underrunCount = 0;
    //    }
    //}

    public void Initialize()
    {
        if (m_as != null && !m_as.isPlaying)
            m_as.Play();
    }

    public void StopPlay()
    {
        if (m_as != null && m_as.isPlaying)
            m_as.Stop();
    }

    public void SetVolume(float volume)
    {
        if (m_as) m_as.volume = Mathf.Clamp01(volume);
    }

    // 空实现
    public void BufferWirte(int Off, byte[] Data) { }
    public void GetCurrentPosition(out int play_position, out int write_position)
    {
        play_position = write_position = 0;
    }
    internal void Unpause() { }
    internal void Pause() { }
}