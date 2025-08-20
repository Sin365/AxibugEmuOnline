using MAME.Core;
using System;
using UnityEngine;

public class UniSoundPlayer : MonoBehaviour, ISoundPlayer
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

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!UMAME.bInGame) return;
        int step = channels;
        for (int i = 0; i < data.Length; i += step)
        {
            float rawFloat = lastData;
            float rawData;
            if (_buffer.TryRead(out rawData))
            { 
                rawFloat = rawData;
            }

            data[i] = rawFloat;
            for (int fill = 1; fill < step; fill++)
                data[i + fill] = rawFloat;
            lastData = rawFloat;
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
