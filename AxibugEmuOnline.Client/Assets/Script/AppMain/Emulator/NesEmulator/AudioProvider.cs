using AxibugEmuOnline.Client.ClientCore;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class AudioProvider : MonoBehaviour, AxiAudioPull
    {
        public NesEmulator NesEmu { get; set; }

        [SerializeField]
        private AudioSource m_as;

        private SoundBuffer _buffer = new SoundBuffer(4096);
        public void Start()
        {
            return;
            //// 获取当前音频配置
            //AudioConfiguration config = AudioSettings.GetConfiguration();
            //// 设置目标音频配置
            //config.sampleRate = 44100;       // 采样率
            //config.numRealVoices = 32;      // 设置最大音频源数量（可选）
            //config.numVirtualVoices = 512;   // 设置虚拟音频源数量（可选）
            //config.dspBufferSize = 1024;     // 设置 DSP 缓冲区大小（可选）
            //config.speakerMode = AudioSpeakerMode.Stereo; // 设置为立体声（2 声道）
            //App.audioMgr.SetAudioConfig(new AudioConfiguration());

            ////TODO 采样率需要更准确，而且和clip并没有关系
            //var dummy = AudioClip.Create("dummy", 1, 1, AudioSettings.outputSampleRate, false);
            //dummy.SetData(new float[] { 1 }, 0);
            //m_as.clip = dummy; //just to let unity play the audiosource
            //m_as.loop = true;
            //m_as.spatialBlend = 1;
            //m_as.Play();

        }

        private void OnEnable()
        {
            App.audioMgr.RegisterStream(nameof(NesEmulator), null, this);
        }

        void OnDisable()
        {
            App.audioMgr.ClearAudioData(nameof(NesEmulator));
        }

        public void GetAudioParams(out int frequency, out int channels)
        {
            frequency = m_as.clip.samples;
            channels = m_as.clip.channels;
        }

        public unsafe void PullAudio(float[] data, int channels)
        {
            int step = channels;

            if (NesEmu == null || NesEmu.NesCore == null) return;
            if (NesEmu.IsPause) return;

            ProcessSound(NesEmu.NesCore, (uint)(data.Length / channels));

            fixed (float* pData = data)
            {
                float* outputPtr = pData; // 指向 data 数组起始位置的指针
                int dataLength = data.Length;
                for (int i = 0; i < dataLength; i += step)
                {
                    byte rawData;
                    float rawFloat = 0f;
                    if (_buffer.TryRead(out rawData))
                        rawFloat = rawData / 255f;

                    *outputPtr = rawFloat;
                    outputPtr++; //指针移动到下一个位置

                    for (int fill = 1; fill < step; fill++)
                    {
                        *outputPtr = rawFloat; 
                        outputPtr++; //指针移动到下一个位置
                    }
                }
            }

            /* 非指针版本，代码保留
            for (int i = 0; i < data.Length; i += step)
            {
                float rawFloat = 0;
                byte rawData;
                if (_buffer.TryRead(out rawData))
                    rawFloat = rawData / 255f;

                data[i] = rawFloat;
                for (int fill = 1; fill < step; fill++)
                    data[i + fill] = rawFloat;
            }*/
        }

        //void OnAudioFilterRead(float[] data, int channels)
        //{
        //    int step = channels;

        //    if (NesEmu == null || NesEmu.NesCore == null) return;
        //    if (NesEmu.IsPause) return;

        //    ProcessSound(NesEmu.NesCore, (uint)(data.Length / channels));

        //    for (int i = 0; i < data.Length; i += step)
        //    {
        //        float rawFloat = 0;
        //        byte rawData;
        //        if (_buffer.TryRead(out rawData))
        //            rawFloat = rawData / 255f;

        //        data[i] = rawFloat;
        //        for (int fill = 1; fill < step; fill++)
        //            data[i + fill] = rawFloat;
        //    }
        //}

        void ProcessSound(NES nes, uint feedCount)
        {
            nes.apu.Process(_buffer, feedCount);
        }

    }

}