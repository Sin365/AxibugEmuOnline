using MyNes.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class AudioProvider : MonoBehaviour, IAudioProvider
    {
        public string Name => nameof(AudioProvider);

        public string ID => Name.GetHashCode().ToString();

        public bool AllowBufferChange => true;

        public bool AllowFrequencyChange => true;

        private bool m_isPlaying;

        [SerializeField]
        private NesCoreProxy m_coreProxy;
        [SerializeField]
        private AudioSource m_as;

        private Stopwatch sw = Stopwatch.StartNew();
        private RingBuffer<short> _buffer = new RingBuffer<short>(4096);

        public double FPS { get; private set; }

        public void Initialize()
        {
            var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);

            dummy.SetData(new float[] { 1 }, 0);
            m_as.clip = dummy; //just to let unity play the audiosource
            m_as.loop = true;
            m_as.spatialBlend = 1;
            m_as.Play();
        }

        float lastData = 0;
        void OnAudioFilterRead(float[] data, int channels)
        {
            int step = channels;

            var bufferCount = _buffer.Available();
            if (bufferCount < 4096)
            {
                double fps = 1 / 61d;
                NesEmu.SetFramePeriod(ref fps);
            }
            else if (bufferCount > 8124)
            {
                double fps = 1 / 59d;
                NesEmu.SetFramePeriod(ref fps);
            }
            else
            {
                NesEmu.RevertFramePeriod();
            }
            for (int i = 0; i < data.Length; i += step)
            {
                float rawFloat = lastData;
                if (_buffer.TryRead(out short rawData))
                    rawFloat = rawData / 124f;

                data[i] = rawFloat;
                for (int fill = 1; fill < step; fill++)
                    data[i + fill] = rawFloat;

                lastData = rawFloat;
            }
        }

        private TimeSpan lastElapsed;
        public void SubmitSamples(ref short[] buffer, ref int samples_a)
        {
            var current = sw.Elapsed;
            var delta = current - lastElapsed;
            lastElapsed = current;

            FPS = 1d / delta.TotalSeconds;

            for (int i = 0; i < samples_a; i++)
            {
                _buffer.Write(buffer[i]);
            }
        }

        public void TogglePause(bool paused)
        {
            m_isPlaying = !paused;
        }

        public void GetIsPlaying(out bool playing)
        {
            playing = m_isPlaying;
        }



        public void ShutDown()
        {
        }

        public void Reset()
        {
        }

        public void SignalToggle(bool started)
        {
        }

        public void SetVolume(int Vol)
        {
        }
    }
}
