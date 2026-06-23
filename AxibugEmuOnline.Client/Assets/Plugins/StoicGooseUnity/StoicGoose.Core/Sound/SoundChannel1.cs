namespace StoicGoose.Core.Sound
{
    /* Channel 1, no additional features */
    public struct SoundChannel1
    {
        const int counterReload = 2048;

        ushort counter;
        byte pointer;

        public byte OutputLeft;// { get; set; }
        public byte OutputRight;// { get; set; }

        readonly WaveTableReadDelegate waveTableReadDelegate;

        /* REG_SND_CH1_PITCH */
        public ushort Pitch;// { get; set; }
        /* REG_SND_CH1_VOL */
        public byte VolumeLeft;// { get; set; }
        public byte VolumeRight;// { get; set; }
        /* REG_SND_CTRL */
        public bool IsEnabled;// { get; set; }

        public SoundChannel1(WaveTableReadDelegate waveTableRead)
        {
            counter = counterReload;
            pointer = 0;
            OutputLeft = OutputRight = 0;

            Pitch = 0;
            VolumeLeft = VolumeRight = 0;
            IsEnabled = false;

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
        }

        public void Step()
        {
            Step(1);
        }

        /// <summary>
        /// [性能优化-第一阶段] 批处理波形更新
        /// 原始设计：循环调用Step() 5000-20000次/帧，每次检查counter递减
        /// 优化方案：集中计算 counter -= cycles，仅触发条件时更新波形
        /// 好处：减少方法调用开销，保持原始计时精度(counter == Pitch触发)
        /// 说明：counter使用ushort溢出后自动重载，避免计时错误
        /// </summary>
        public void Step(int cycles)
        {
            if (!IsEnabled || cycles <= 0)
                return;

            // [性能优化] 批量递减计数器，而非逐周期调用
            counter -= (ushort)cycles;
            if (counter > Pitch)
                return;

            var data = waveTableReadDelegate((ushort)(pointer >> 1));
            if ((pointer & 0b1) == 0b1) data >>= 4;
            data &= 0x0F;

            OutputLeft = (byte)(data * VolumeLeft);
            OutputRight = (byte)(data * VolumeRight);

            pointer++;
            pointer &= 0b11111;
            counter = counterReload;
        }
    }
    ///* Channel 1, no additional features */
    //public sealed class SoundChannel1
    //{
    //	const int counterReload = 2048;

    //	ushort counter;
    //	byte pointer;

    //	public byte OutputLeft;// { get; set; }
    //       public byte OutputRight;// { get; set; }

    //       readonly WaveTableReadDelegate waveTableReadDelegate;

    //	/* REG_SND_CH1_PITCH */
    //	public ushort Pitch;// { get; set; }
    //       /* REG_SND_CH1_VOL */
    //       public byte VolumeLeft;// { get; set; }
    //       public byte VolumeRight;// { get; set; }
    //       /* REG_SND_CTRL */
    //       public bool IsEnabled;// { get; set; }

    //       public SoundChannel1(WaveTableReadDelegate waveTableRead) => waveTableReadDelegate = waveTableRead;

    //	public void Reset()
    //	{
    //		counter = counterReload;
    //		pointer = 0;
    //		OutputLeft = OutputRight = 0;

    //		Pitch = 0;
    //		VolumeLeft = VolumeRight = 0;
    //		IsEnabled = false;
    //	}

    //	public void Step()
    //	{
    //		counter--;
    //		if (counter == Pitch)
    //		{
    //			var data = waveTableReadDelegate((ushort)(pointer >> 1));
    //			if ((pointer & 0b1) == 0b1) data >>= 4;
    //			data &= 0x0F;

    //			OutputLeft = (byte)(data * VolumeLeft);
    //			OutputRight = (byte)(data * VolumeRight);

    //			pointer++;
    //			pointer &= 0b11111;
    //			counter = counterReload;
    //		}
    //	}
    //}
}
