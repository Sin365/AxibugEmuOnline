using MyNes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class AudioProvider : IAudioProvider
    {
        public string Name => nameof(AudioProvider);

        public string ID => Name.GetHashCode().ToString();

        public bool AllowBufferChange => true;

        public bool AllowFrequencyChange => true;

        private bool m_isPlaying;
        private AudioSource m_as;
        private int samples_added;


        private Queue<float> _buffer = new Queue<float>();

        public void Initialize()
        {
            m_as = NesCoreProxy.Instance.AS;
            m_as.clip = AudioClip.Create("nes wav", 48000 * 2, 1, 48000, true, OnAudioFilterRead);
            m_as.loop = true;
            m_as.playOnAwake = false;
            m_as.spatialBlend = 0f;

            m_as.Play();
        }

        public void Update() { }


        private void OnAudioFilterRead(float[] data)
        {
            lock (_buffer)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = _buffer.Count > 0 ? _buffer.Dequeue() : 0;
                }
            }
        }

        public void SubmitSamples(ref short[] buffer, ref int samples_a)
        {
            lock (_buffer)
            {
                foreach (var a in buffer.Take(samples_a).ToArray())
                {
                    var floatData = (float)a / 124;
                    _buffer.Enqueue(floatData);
                }
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
