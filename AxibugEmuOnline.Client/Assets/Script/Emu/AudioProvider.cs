using MyNes.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Profiling;

namespace AxibugEmuOnline.Client
{
    public class AudioProvider : IAudioProvider
    {
        public string Name => nameof(AudioProvider);

        public string ID => Name.GetHashCode().ToString();

        public bool AllowBufferChange => true;

        public bool AllowFrequencyChange => true;

        private bool m_isPlaying;
        private int samples_added;

        private Queue<(float[], int)> queues = new Queue<(float[], int)>();
        public void Initialize()
        {
        }

        public void SubmitSamples(ref short[] buffer, ref int samples_a)
        {
            NesCoreProxy.Instance.DO.Play(buffer.Take(samples_a).SelectMany(s => toBytes(s)).ToArray());
        }

        public byte[] toBytes(short value)
        {
            byte[] temp = new byte[2];
            //temp[0] = (byte)(value >> 8);
            temp[0] = temp[1] = (byte)((uint)value & 0xFFu);

            return temp;
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
