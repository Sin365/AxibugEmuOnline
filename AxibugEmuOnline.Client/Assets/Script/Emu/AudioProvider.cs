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
        private AudioSource m_as;

        private Stopwatch sw = Stopwatch.StartNew();
        private Queue<short> _buffer = new Queue<short>(2048);

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

            for (int i = 0; i < data.Length; i += step)
            {
                var rawFloat = _buffer.Count <= 0 ? lastData : _buffer.Dequeue() / 124f;
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

            if (_buffer.Count > 2048)
            {
                _buffer.Clear();
            }

            for (int i = 0; i < samples_a; i++)
            {
                _buffer.Enqueue(buffer[i]);
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
