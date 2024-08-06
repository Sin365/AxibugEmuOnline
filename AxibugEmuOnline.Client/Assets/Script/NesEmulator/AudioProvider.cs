using System;
using System.Diagnostics;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{

    public class AudioProvider : MonoBehaviour
    {
        [SerializeField]
        private AudioSource m_as;

        private SoundBuffer _buffer = new SoundBuffer(4096);

        public void Initialize()
        {
            var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);

            dummy.SetData(new float[] { 1 }, 0);
            m_as.clip = dummy; //just to let unity play the audiosource
            m_as.loop = true;
            m_as.spatialBlend = 1;
            m_as.Play();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            int step = channels;

            var bufferCount = _buffer.Available();

            for (int i = 0; i < data.Length; i += step)
            {
                float rawFloat = 0;
                if (_buffer.TryRead(out byte rawData))
                    rawFloat = rawData / 255f;

                data[i] = rawFloat;
                for (int fill = 1; fill < step; fill++)
                    data[i + fill] = rawFloat;

            }
        }

        public void ProcessSound(NES nes)
        {
            nes.apu.Process(_buffer, (uint)(Supporter.Config.sound.nRate * Time.deltaTime));
        }
    }

}