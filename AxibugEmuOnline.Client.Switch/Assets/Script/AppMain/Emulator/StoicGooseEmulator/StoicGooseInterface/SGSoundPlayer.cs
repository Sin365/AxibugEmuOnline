using System;
using System.Collections.Generic;
using UnityEngine;

public class SGSoundPlayer : MonoBehaviour//, ISoundPlayer
{
    [SerializeField]
    private AudioSource m_as;
    private RingBuffer<float> _buffer = new RingBuffer<float>(44100 * 2);
    private TimeSpan lastElapsed;
    public double audioFPS { get; private set; }

    void Awake()
    {
        // 获取当前音频配置
        AudioConfiguration config = AudioSettings.GetConfiguration();

        // 设置目标音频配置
        config.sampleRate = 44100;       // 采样率为 44100Hz
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

    }

    private Queue<float> sampleQueue = new Queue<float>();


    // Unity 音频线程回调
    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (_buffer.TryRead(out float rawData))
                data[i] = rawData;
            else
                data[i] = 0; // 无数据时静音
        }
    }


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

    //public void SubmitSamples(short[] buffer, short[][] ChannelSamples, int samples_a)
    //{
    //    var current = UStoicGoose.sw.Elapsed;
    //    var delta = current - lastElapsed;
    //    lastElapsed = current;
    //    audioFPS = 1d / delta.TotalSeconds;

    //    for (int i = 0; i < samples_a; i += 1)
    //    {
    //        _buffer.Write(buffer[i] / 32767.0f);

    //    }
    //}
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


    internal void EnqueueSamples(short[] buffer)
    {
        var current = UStoicGoose.sw.Elapsed;
        var delta = current - lastElapsed;
        lastElapsed = current;
        audioFPS = 1d / delta.TotalSeconds;

        for (int i = 0; i < buffer.Length; i += 1)
        {
            _buffer.Write(buffer[i] / 32767.0f);
        }
    }

    internal void Unpause()
    {
        throw new NotImplementedException();
    }

    internal void Pause()
    {
        throw new NotImplementedException();
    }

}
