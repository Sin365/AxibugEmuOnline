namespace StoicGoose.Core.Sound
{
    /* Channel 4, supports noise */
    public struct SoundChannel4
    {
        const int counterReload = 2048;

        readonly static byte[] noiseLfsrTaps = { 14, 10, 13, 4, 8, 6, 9, 11 };

        ushort counter;
        byte pointer;

        public byte OutputLeft;// { get; set; }
        public byte OutputRight;// { get; set; }

        readonly WaveTableReadDelegate waveTableReadDelegate;

        /* REG_SND_CH4_PITCH */
        public ushort Pitch;// { get; set; }
        /* REG_SND_CH4_VOL */
        public byte VolumeLeft;// { get; set; }
        public byte VolumeRight;// { get; set; }
        /* REG_SND_CTRL */
        public bool IsEnabled;// { get; set; }
        public bool IsNoiseEnabled;// { get; set; }

        /* REG_SND_NOISE */
        public byte NoiseMode;// { get; set; }
        public bool NoiseReset;// { get; set; }
        public bool NoiseEnable;// { get; set; }
        /* REG_SND_RANDOM */
        public ushort NoiseLfsr;// { get; set; }

        public SoundChannel4(WaveTableReadDelegate waveTableRead)
        {
            counter = counterReload;
            pointer = 0;
            OutputLeft = OutputRight = 0;

            Pitch = 0;
            VolumeLeft = VolumeRight = 0;
            IsEnabled = false;

            IsNoiseEnabled = false;

            NoiseMode = 0;
            NoiseReset = NoiseEnable = false;
            NoiseLfsr = 0;

            waveTableReadDelegate = waveTableRead;
        }

        public void Reset()
        {
            counter = counterReload;
            pointer = 0;
            OutputLeft = OutputRight = 0;

            Pitch = 0;
            VolumeLeft = VolumeRight = 0;
            IsEnabled = false;

            IsNoiseEnabled = false;

            NoiseMode = 0;
            NoiseReset = NoiseEnable = false;
            NoiseLfsr = 0;
        }

        public void Step()
        {
            Step(1);
        }

        /// <summary>
        /// [性能优化-第一阶段] 批处理波形/噪声更新
        /// 原始设计：循环调用Step() 5000-20000次/帧，每次检查counter递减
        /// 优化方案：集中计算 counter -= cycles，仅触发条件时更新波形或LFSR
        /// 好处：减少方法调用开销，保持原始计时精度(counter == Pitch触发)
        /// 说明：支持波形模式和噪声(LFSR)模式，噪声使用线性反馈移位寄存器生成
        /// </summary>
        public void Step(int cycles)
        {
            if (!IsEnabled || cycles <= 0)
                return;

            // [性能优化] 批量递减计数器，而非逐周期调用
            counter -= (ushort)cycles;
            if (counter > Pitch)
                return;

            if (NoiseEnable)
            {
                var tap = noiseLfsrTaps[NoiseMode];
                var noise = (1 ^ (NoiseLfsr >> 7) ^ (NoiseLfsr >> tap)) & 0b1;
                NoiseLfsr = (ushort)(((NoiseLfsr << 1) | noise) & 0x7FFF);
            }

            var data = IsNoiseEnabled ? ((NoiseLfsr & 0b1) * 0x0F) : waveTableReadDelegate((ushort)(pointer >> 1));
            if (!IsNoiseEnabled)
            {
                if ((pointer & 0b1) == 0b1) data >>= 4;
                data &= 0x0F;
            }

            OutputLeft = (byte)(data * VolumeLeft);
            OutputRight = (byte)(data * VolumeRight);

            if (NoiseReset)
            {
                NoiseLfsr = 0;
                OutputLeft = OutputRight = 0;
                NoiseReset = false;
            }

            pointer++;
            pointer &= 0b11111;
            counter = counterReload;
        }
    }

    ///* Channel 4, supports noise */
    //public sealed class SoundChannel4
    //{
    //	const int counterReload = 2048;

    //	readonly static byte[] noiseLfsrTaps = { 14, 10, 13, 4, 8, 6, 9, 11 };

    //	ushort counter;
    //	byte pointer;

    //	public byte OutputLeft;// { get; set; }
    //       public byte OutputRight;// { get; set; }

    //       readonly WaveTableReadDelegate waveTableReadDelegate;

    //	/* REG_SND_CH4_PITCH */
    //	public ushort Pitch;// { get; set; }
    //       /* REG_SND_CH4_VOL */
    //       public byte VolumeLeft;// { get; set; }
    //       public byte VolumeRight;// { get; set; }
    //       /* REG_SND_CTRL */
    //       public bool IsEnabled;// { get; set; }
    //       public bool IsNoiseEnabled;// { get; set; }

    //       /* REG_SND_NOISE */
    //       public byte NoiseMode;// { get; set; }
    //       public bool NoiseReset;// { get; set; }
    //       public bool NoiseEnable;// { get; set; }
    //       /* REG_SND_RANDOM */
    //       public ushort NoiseLfsr;// { get; set; }

    //       public SoundChannel4(WaveTableReadDelegate waveTableRead) => waveTableReadDelegate = waveTableRead;

    //	public void Reset()
    //	{
    //		counter = counterReload;
    //		pointer = 0;
    //		OutputLeft = OutputRight = 0;

    //		Pitch = 0;
    //		VolumeLeft = VolumeRight = 0;
    //		IsEnabled = false;

    //		IsNoiseEnabled = false;

    //		NoiseMode = 0;
    //		NoiseReset = NoiseEnable = false;
    //		NoiseLfsr = 0;
    //	}

    //	public void Step()
    //	{
    //		counter--;
    //		if (counter == Pitch)
    //		{
    //			if (NoiseEnable)
    //			{
    //				var tap = noiseLfsrTaps[NoiseMode];
    //				var noise = (1 ^ (NoiseLfsr >> 7) ^ (NoiseLfsr >> tap)) & 0b1;
    //				NoiseLfsr = (ushort)(((NoiseLfsr << 1) | noise) & 0x7FFF);
    //			}

    //			var data = IsNoiseEnabled ? ((NoiseLfsr & 0b1) * 0x0F) : waveTableReadDelegate((ushort)(pointer >> 1));
    //			if (!IsNoiseEnabled)
    //			{
    //				if ((pointer & 0b1) == 0b1) data >>= 4;
    //				data &= 0x0F;
    //			}

    //			OutputLeft = (byte)(data * VolumeLeft);
    //			OutputRight = (byte)(data * VolumeRight);

    //			if (NoiseReset)
    //			{
    //				NoiseLfsr = 0;
    //				OutputLeft = OutputRight = 0;
    //				NoiseReset = false;
    //			}

    //			pointer++;
    //			pointer &= 0b11111;
    //			counter = counterReload;
    //		}
    //	}
    //}
}
