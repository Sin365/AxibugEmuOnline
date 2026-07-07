using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Common;
using System;
using System.Buffers;
using UnityEngine;

public class SGSoundPlayer : MonoBehaviour, AxiAudioPull
{
    [SerializeField]
    private AudioSource m_as;

    /// <summary>
    /// WSC/WS 每幀來的數據
    /// </summary>
    const int WscEverTickBufferLenght = 1168;
    // 大幅加大缓冲 + 预留安全余量
    private RingBuffer<float> _buffer = new RingBuffer<float>(WscEverTickBufferLenght * 4);//4幀音頻數據為最大緩衝
    private RingBuffer<float> _buffer_2nd = new RingBuffer<float>(WscEverTickBufferLenght);

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

            /*
            float lastSample = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float sample;

                if (!_buffer.TryRead(out sample))
                    sample = 0f;
                else
                    lastSample = sample;

                outputPtr[i] = lastSample;
            }*/

            // 一次性批量读取
            int readCount = _buffer.Read(data, 0, data.Length);
            // 2. 如果已经读满，就不用做任何事
            if (readCount == data.Length)
                return;
            // 需要补零/保持最后样本的数量
            int needPadding = data.Length - readCount;
            if (needPadding > 0)
            {
                float lastSample = 0f;
                // 优先使用最后写入的有效样本进行填充（避免爆音）
                if (_buffer.TryGetLast(out float last))
                    lastSample = last;
                // 否则保持 0f（缓冲区完全为空）
                // 填充剩余部分 但是0不用填充
                if (lastSample > 0)
                {
                    for (int i = readCount; i < data.Length; i++)
                    {
                        outputPtr[i] = lastSample;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 模拟器核心推送音频（关键优化）
    /// </summary>
    internal unsafe void EnqueueSamples(short[] buffer, int len)
    {
#if UNITY_EDITOR
        // 固定 short[]，拿到 short*
        fixed (short* pShort = buffer)
        {
            App.audioMgr.WriteToRecord(pShort, len);
        }
#endif
        //if (UStoicGoose.instance.emulatorHandler.CurrVirtualFrameIsSkim)
        //    return;

        //二級缓冲，尝试跨帧效果
        //while (_buffer_2nd.TryRead(out var frombefordata))
        //{
        //    _buffer.Write(frombefordata);
        //}

        //二級缓冲，尝试跨帧效果
        _buffer_2nd.CopyTo(_buffer);
        float[] temp = AxiArrayPool.RentBuffer<float>(len);
        for (int i = 0; i < len; i++)
            temp[i] = buffer[i] / 32767.0f;
        _buffer_2nd.Write(temp, 0, len);
        AxiArrayPool.ReturnBuffer(temp);
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