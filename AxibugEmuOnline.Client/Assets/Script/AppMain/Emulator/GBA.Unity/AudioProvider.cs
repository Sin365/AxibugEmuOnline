using AxibugEmuOnline.Client.ClientCore;
using MAME.Core;
using OptimeGBA;
using System;
using UnityEngine;

namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class AudioProvider : MonoBehaviour, AxiAudioPull
    {
        [SerializeField]
        private AudioSource m_as;
        // 大幅加大缓冲 + 预留安全余量
        private RingBuffer<float> _buffer = new RingBuffer<float>(sampleRate * 4);//4幀音頻數據為最大緩衝
        private RingBuffer<float> _buffer_2nd = new RingBuffer<float>(sampleRate);
        private TimeSpan lastElapsed;

        const int sampleRate = 32768;
        const int channels = 2;
        public int SampleRate => sampleRate;
        public int Channels => channels;
        public double audioFPS { get; private set; }
        float lastData = 0;

        public void PullAudio(float[] data, int channels)
        {
            int step = channels;
            step = 1;
            for (int i = 0; i < data.Length; i += step)
            {
                if (_buffer.TryRead(out float rawData))
                    data[i] = rawData;
                else
                    break;
            }
        }
        public void Awake()
        {
            //AudioClip clip = AudioClip.Create("dummy", GbaAudio.SampleRate * 2, 2, GbaAudio.SampleRate, true);
            //AudioSettings.GetDSPBufferSize(out int bufferLength, out _);
            //_buffer = new RingBuffer<float>(bufferLength * 2 * 2);
            //m_as.clip = clip;
            //m_as.playOnAwake = true;
            ////m_as.loop = true;
            //m_as.spatialBlend = 1;


            ////TODO 采样率需要更准确，而且和clip并没有关系
            //var dummy = AudioClip.Create("dummy", 1, channels, sampleRate, false);
            //dummy.SetData(new float[] { 1 }, 0);
            //m_as.clip = dummy; //just to let unity play the audiosource
            //m_as.loop = true;
            //m_as.spatialBlend = 1;
            //m_as.Play();
        }
        private void OnEnable()
        {
            App.audioMgr.RegisterStream(nameof(UniSoundPlayer), sampleRate, this);
        }

        private void OnDisable()
        {
            App.audioMgr.ClearAudioData(nameof(NesEmulator));
        }
        public void Initialize()
        {
            if (!m_as.isPlaying)
            {
                m_as.Play();
            }
        }

        public void AudioReady(float[] data)
        {
            if (!Emulator.instance.EnableAudio) return;

            var current = Emulator.sw.Elapsed;
            var delta = current - lastElapsed;
            lastElapsed = current;
            audioFPS = 1d / delta.TotalSeconds;

            //for (int i = 0; i < data.Length; i++)
            //{
            //    _buffer.Write(data[i]);
            //}


            //二級缓冲，尝试跨帧效果
            _buffer_2nd.CopyTo(_buffer);
            _buffer_2nd.Write(data, 0, data.Length);
        }

    }
}
