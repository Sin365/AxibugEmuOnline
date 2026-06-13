using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using System;
using UnityEngine;

public class SGSoundPlayer : MonoBehaviour, AxiAudioPull
{
    [SerializeField]
    private AudioSource m_as;

    // 大幅加大缓冲 + 预留安全余量
    private RingBuffer<float> _buffer = new RingBuffer<float>(44100 * 2); // 约 270ms 缓冲

    private float lastSample = 0f;

    [HideInInspector]
    public int sampleRate = 44100;
    [HideInInspector]
    public int channels = 2;


    private void Awake()
    {
        return;
    }

    private void OnEnable()
    {
        App.audioMgr.RegisterStream(nameof(UStoicGoose), sampleRate, this);
    }

    private void OnDisable()
    {
        App.audioMgr.ClearAudioData(nameof(UStoicGoose));
    }

    public unsafe void PullAudio(float[] data, int channels)
    {
        fixed (float* pData = data)
        {
            float* outputPtr = pData;

            float lastSample = 0;
            //for (int i = 0; i < data.Length; i += channels)
            for (int i = 0; i < data.Length; i++)
            {
                float sample;

                if (!_buffer.TryRead(out sample))
                    sample = 0f;
                else
                    lastSample = sample;

                outputPtr[i] = lastSample;
                //for (int ch = 0; ch < channels; ch++)
                //    outputPtr[i + ch] = sample; // 单声道复制到所有通道
            }
        }
    }

    /// <summary>
    /// 模拟器核心推送音频（关键优化）
    /// </summary>
    internal unsafe void EnqueueSamples(short[] buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            _buffer.Write(buffer[i] / 32767.0f);
        }

        // 固定 short[]，拿到 short*
        fixed (short* pShort = buffer)
        {
            App.audioMgr.WriteToRecord(pShort, buffer.Length);
        }
    }

    public void Initialize()
    {
        //if (m_as != null && !m_as.isPlaying)
        //    m_as.Play();
    }

    public void StopPlay()
    {
        //if (m_as != null && m_as.isPlaying)
        //    m_as.Stop();
    }

    public void SetVolume(float volume)
    {
        //if (m_as) m_as.volume = Mathf.Clamp01(volume);
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