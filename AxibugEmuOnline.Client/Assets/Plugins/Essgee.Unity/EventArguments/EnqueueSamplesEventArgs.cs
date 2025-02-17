using System;

namespace Essgee.EventArguments
{
    public unsafe class EnqueueSamplesEventArgs : EventArgs
    {
        public int NumChannels { get; set; }
        //public short[][] ChannelSamples { get; set; }
        public short*[] ChannelSamples { get; set; }
        public bool[] IsChannelMuted { get; set; }
        public short* MixedSamples { get; set; }
        public int MixedSamplesLength { get; set; }

        //public EnqueueSamplesEventArgs(int numChannels, short[][] channelSamples, bool[] isMuted, short[] mixedSamples)
        //{
        //	NumChannels = numChannels;
        //	ChannelSamples = channelSamples;
        //	IsChannelMuted = isMuted;
        //	MixedSamples = mixedSamples;
        //}

        //public static EnqueueSamplesEventArgs Create(int numChannels, short[][] channelSamples, bool[] isMuted, short[] mixedSamples)
        public static EnqueueSamplesEventArgs Create(int numChannels, short*[] channelSamples, bool[] isMuted, short* mixedSamples, int mixedSamplesLength)
        {
            var eventArgs = ObjectPoolAuto.Acquire<EnqueueSamplesEventArgs>();
            eventArgs.NumChannels = numChannels;
            eventArgs.ChannelSamples = channelSamples;
            eventArgs.IsChannelMuted = isMuted;
            eventArgs.MixedSamples = mixedSamples;
            eventArgs.MixedSamplesLength = mixedSamplesLength;
            return eventArgs;
        }
    }
    public unsafe static class EnqueueSamplesEventArgsEx
    {
        public static void Release(this EnqueueSamplesEventArgs eventArgs)
        {
            eventArgs.NumChannels = 1;
            eventArgs.ChannelSamples = null;
            eventArgs.IsChannelMuted = null;
            eventArgs.MixedSamples = null;
            ObjectPoolAuto.Release(eventArgs);
        }
    }
}
