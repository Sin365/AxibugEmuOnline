using AxibugEmuOnline.Client.ClientCore;
using System;
using UnityEngine;

namespace AxibugEmuOnline.Client.GBA.Unity
{
    public class GBAAudioProvider : MonoBehaviour, AxiAudioPull
    {
        // 大幅加大缓冲 + 预留安全余量
        private RingBuffer<float> _buffer = new RingBuffer<float>(sampleRate * 2);//4幀音頻數據為最大緩衝
        private RingBuffer<float> _buffer_2nd = new RingBuffer<float>(1024);
        private TimeSpan lastElapsed;
        public int SampleRate => sampleRate;
        public int Channels => channels;
        public double audioFPS { get; private set; }

#if UNITY_SWITCH //Switch 貌似无法设置32768为DSP采样率，所以 手动用32768重采样到48000

        const int sampleRate = 48000;

        const int channels = 2;

        // Resampling state
        private double _inputFrac = 0.0; // accumulator for source->output stepping
        private double _resampleRatio = 1.0; // sourceRate / outputRate
        private float _lastLeft = 0f;
        private float _lastRight = 0f;

        public void PullAudio(float[] data, int channels)
        {
            // data is the Unity audio buffer (interleaved if channels==2)
            int outFrames = data.Length / Math.Max(1, channels);
            _resampleRatio = 32768f / sampleRate;
            double r = _resampleRatio; // how many source frames per output frame
            for (int f = 0; f < outFrames; f++)
            {
                // advance input accumulator by ratio
                _inputFrac += r;

                // consume whole source frames as needed
                while (_inputFrac >= 1.0)
                {
                    // each source frame contains 'channels' floats interleaved
                    // try to read a full frame from the FIFO
                    if (_buffer.TryRead(out float s0))
                    {
                        float s1 = s0;
                        if (channels > 1)
                        {
                            if (_buffer.TryRead(out float s1Read)) s1 = s1Read;
                        }
                        // update last frame
                        _lastLeft = s0;
                        _lastRight = s1;
                    }
                    else
                    {
                        // underrun: no more source frames available
                        // stop consuming and break; remaining output frames will reuse last samples
                        _inputFrac = 0.0;
                        break;
                    }

                    _inputFrac -= 1.0;
                }

                // write output for this frame
                if (channels == 2)
                {
                    int baseIdx = f * 2;
                    data[baseIdx + 0] = _lastLeft;
                    data[baseIdx + 1] = _lastRight;
                }
                else
                {
                    // mono output: average stereo
                    data[f] = 0.5f * (_lastLeft + _lastRight);
                }
            }
        }
#else

        const int sampleRate = 32768;

        const int channels = 2;
        public void PullAudio(float[] data, int channels)
        {
            int step = channels;
            step = 1;
            float lastdata = 0;
            for (int i = 0; i < data.Length; i += step)
            {
                //if (_buffer.TryRead(out float rawData))
                //    lastdata = rawData;

                //data[i] = lastdata;
                if (_buffer.TryRead(out float rawData))
                    data[i] = rawData;
                else
                    break;
            }
        }
#endif
        public void Awake()
        {
        }
        private void OnEnable()
        {
            App.audioMgr.RegisterStream(nameof(GBAAudioProvider), sampleRate, this);
        }

        private void OnDisable()
        {
            App.audioMgr.ClearAudioData(nameof(GBAAudioProvider));
        }
        public void Initialize()
        {
        }

        public void AudioReady(float[] data)
        {
            if (!GBAEmulator.instance.EnableAudio) return;

            var current = GBAEmulator.sw.Elapsed;
            var delta = current - lastElapsed;
            lastElapsed = current;
            audioFPS = 1d / delta.TotalSeconds;
            //二級缓冲，尝试跨帧效果
            _buffer_2nd.CopyTo(_buffer);
            _buffer_2nd.Write(data, 0, data.Length);
        }

    }
}
