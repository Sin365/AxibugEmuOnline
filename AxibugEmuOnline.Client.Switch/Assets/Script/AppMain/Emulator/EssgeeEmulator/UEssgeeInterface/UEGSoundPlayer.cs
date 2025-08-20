using AxibugEmuOnline.Client.ClientCore;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UEGSoundPlayer : MonoBehaviour
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
        // ��ȡ��ǰ��Ƶ����
        AudioConfiguration config = AudioSettings.GetConfiguration();
        // ����Ŀ����Ƶ����
        config.sampleRate = sampleRate;       // ������Ϊ 44100Hz
        config.numRealVoices = 32;      // ���������ƵԴ��������ѡ��
        config.numVirtualVoices = 512;   // ����������ƵԴ��������ѡ��
        config.dspBufferSize = 1024;     // ���� DSP ��������С����ѡ��
        config.speakerMode = AudioSpeakerMode.Stereo; // ����Ϊ��������2 ������
        App.audioMgr.SetAudioConfig(config);
    }

    private Queue<float> sampleQueue = new Queue<float>();


    // Unity ��Ƶ�̻߳ص�
    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (_buffer.TryRead(out float rawData))
                data[i] = rawData;
            else
                data[i] = 0; // ������ʱ����
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
        App.audioMgr.WriteToRecord(buffer, samples_a);
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
        //TODO ����
        if (m_as)
            return;
        m_as.volume = Vol;
    }
}
