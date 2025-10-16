using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UEGSoundPlayer : MonoBehaviour, AxiAudioPull
{
    [SerializeField]
    private AudioSource m_as;
    private RingBuffer<float> _buffer = new RingBuffer<float>(44100 * 2);
    private TimeSpan lastElapsed;
    public double audioFPS { get; private set; }
    public bool IsRecording { get; private set; }
    [HideInInspector]
    public int sampleRate = 44100;
    [HideInInspector]
    public int channle = 2;

    void Awake()
    {
        return;
        //// 获取当前音频配置
        //AudioConfiguration config = AudioSettings.GetConfiguration();
        //// 设置目标音频配置
        //config.sampleRate = sampleRate;       // 采样率为 44100Hz
        //config.numRealVoices = 32;      // 设置最大音频源数量（可选）
        //config.numVirtualVoices = 512;   // 设置虚拟音频源数量（可选）
        //config.dspBufferSize = 1024;     // 设置 DSP 缓冲区大小（可选）
        //config.speakerMode = AudioSpeakerMode.Stereo; // 设置为立体声（2 声道）
        //App.audioMgr.SetAudioConfig(config);
    }


    private void OnEnable()
    {
        App.audioMgr.RegisterStream(nameof(UEssgee), sampleRate, this);
    }

    void OnDisable()
    {
        App.audioMgr.ClearAudioData(nameof(UEssgee));
    }

    private Queue<float> sampleQueue = new Queue<float>();


    public unsafe void PullAudio(float[] data, int channels)
    {
        fixed (float* pData = data)
        {
            float* outputPtr = pData; // 指向数组起始位置的指针
            int dataLength = data.Length;
            for (int i = 0; i < dataLength; i++)
            {
                float rawData;
                if (_buffer.TryRead(out rawData))
                    *outputPtr = rawData;
                else
                    *outputPtr = 0; // 无数据时静音

                outputPtr++; // 指针移动到下一个位置
            }
        }

        /* 非指针版本，代码保留
        for (int i = 0; i < data.Length; i++)
        {
            if (_buffer.TryRead(out float rawData))
                data[i] = rawData;
            else
                data[i] = 0; // 无数据时静音
        }*/
    }

    //// Unity 音频线程回调
    //void OnAudioFilterRead(float[] data, int channels)
    //{
    //    for (int i = 0; i < data.Length; i++)
    //    {
    //        if (_buffer.TryRead(out float rawData))
    //            data[i] = rawData;
    //        else
    //            data[i] = 0; // 无数据时静音
    //    }
    //}


    public void Initialize()
    {
        if (!m_as.isPlaying)
        {
            m_as.Play();
        }
    }

    public void StopPlay()
    {
        if (m_as.isPlaying)
        {
            m_as.Stop();
        }
    }

    public unsafe void SubmitSamples(short* buffer, short*[] ChannelSamples, int samples_a)
    {
        var current = UEssgee.sw.Elapsed;
        var delta = current - lastElapsed;
        lastElapsed = current;
        audioFPS = 1d / delta.TotalSeconds;

        for (int i = 0; i < samples_a; i += 1)
        {
            _buffer.Write(buffer[i] / 32767.0f);
        }
        //App.audioMgr.WriteToRecord(buffer, samples_a);
    }
    public void BufferWirte(int Off, byte[] Data)
    {
    }

    public void GetCurrentPosition(out int play_position, out int write_position)
    {
        play_position = 0;
        write_position = 0;
    }

    public void SetVolume(int Vol)
    {
        //TODO 音量
        if (m_as)
            return;
        m_as.volume = Vol;
    }

}
