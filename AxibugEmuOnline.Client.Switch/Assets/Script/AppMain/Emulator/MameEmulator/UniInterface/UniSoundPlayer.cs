using AxibugEmuOnline.Client;
using AxibugEmuOnline.Client.ClientCore;
using MAME.Core;
using System;
using UnityEngine;

public class UniSoundPlayer : MonoBehaviour, ISoundPlayer /*, AxiAudioPull*/
{
    [SerializeField]
    private AudioSource m_as;
    private RingBuffer<float> _buffer = new RingBuffer<float>(4096);
    private TimeSpan lastElapsed;
    public double audioFPS { get; private set; }
    float lastData = 0;

    void Awake()
    {
        //TODO 采样率需要更准确，而且和clip并没有关系
        var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);
        dummy.SetData(new float[] { 1 }, 0);
        m_as.clip = dummy; //just to let unity play the audiosource
        m_as.loop = true;
        m_as.spatialBlend = 1;
        m_as.Play();
    }
    //private void OnEnable()
    //{
    //    App.audioMgr.RegisterStream(nameof(UMAME), AudioSettings.outputSampleRate, this);
    //}
    //void OnDisable()
    //{
    //    App.audioMgr.ClearAudioData(nameof(UMAME));
    //}
    //public unsafe void PullAudio(float[] data, int channels)
    //{
    //    if (!UMAME.bInGame) return;

    //    //fixed (float* pData = data)
    //    //{
    //    //    float* current = pData;
    //    //    float* end = pData + data.Length;
    //    //    float currentSample = lastData;

    //    //    while (current < end)
    //    //    {
    //    //        // 尝试从缓冲区读取新样本
    //    //        float newSample;
    //    //        if (_buffer.TryRead(out newSample))
    //    //        {
    //    //            currentSample = newSample;
    //    //        }

    //    //        // 为所有声道写入相同样本
    //    //        for (int channel = 0; channel < channels; channel++)
    //    //        {
    //    //            *current = currentSample;
    //    //            current++;
    //    //        }
    //    //    }

    //    //    // 保存最后一个样本用于下次调用
    //    //    lastData = currentSample;
    //    //}
    //    // 非指针版本
    //    int step = channels;
    //    for (int i = 0; i < data.Length; i += step)
    //    {
    //        float rawFloat = lastData;
    //        float rawData;
    //        if (_buffer.TryRead(out rawData))
    //        {
    //            rawFloat = rawData;
    //        }

    //        data[i] = rawFloat;
    //        for (int fill = 1; fill < step; fill++)
    //            data[i + fill] = rawFloat;
    //        lastData = rawFloat;
    //    }
    //}
    public void GetAudioParams(out int frequency, out int channels)
    {
        frequency = m_as.clip.samples;
        channels = m_as.clip.channels;
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

    unsafe void OnAudioFilterRead(float[] data, int channels)
    {
        if (!UMAME.bInGame) return;
        int step = channels;
        int length = data.Length;
        fixed (float* dataptr = &data[0])
        {
            float* dataptr_index = &dataptr[0];
            for (int i = 0; i < length; i += step)
            {
                float rawFloat = lastData;
                float rawData;
                if (_buffer.TryRead(out rawData))
                    rawFloat = rawData;
                for (int fill = 0; fill < step; fill++)
                {
                    *dataptr_index = rawFloat;
                    dataptr_index++;
                }
                lastData = rawFloat;
            }
        }
    }

    public void SubmitSamples(byte[] buffer, int samples_a)
    {
        var current = UMAME.sw.Elapsed;
        var delta = current - lastElapsed;
        lastElapsed = current;
        audioFPS = 1d / delta.TotalSeconds;


        for (int i = 0; i < samples_a; i++)
        {
            short left = BitConverter.ToInt16(buffer, i * 2 * 2);
            //short right = BitConverter.ToInt16(buffer, i * 2 * 2 + 2);
            _buffer.Write(left / 32767.0f);
            //_buffer.Write(right / 32767.0f);
        }
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
        if (m_as)
            return;
        m_as.volume = Vol;
    }
}
