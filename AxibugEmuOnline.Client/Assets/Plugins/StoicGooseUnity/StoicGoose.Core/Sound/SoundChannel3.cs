namespace StoicGoose.Core.Sound
{
    /* Channel 3, has optional sweep */
    public struct SoundChannel3
    {
        const int counterReload = 2048;
        const int sweepReload = 32 * 256;

        ushort counter;
        byte pointer;

        public byte OutputLeft;// { get; set; }
        public byte OutputRight;// { get; set; }

        int sweepCounter;
        int sweepCycles;

        readonly WaveTableReadDelegate waveTableReadDelegate;

        /* REG_SND_CH3_PITCH */
        public ushort Pitch;// { get; set; }
        /* REG_SND_CH3_VOL */
        public byte VolumeLeft;// { get; set; }
        public byte VolumeRight;// { get; set; }
        /* REG_SND_CTRL */
        public bool IsEnabled;// { get; set; }
        public bool IsSweepEnabled;// { get; set; }

        /* REG_SND_SWEEP_VALUE */
        public sbyte SweepValue;// { get; set; }
        /* REG_SND_SWEEP_TIME */
        public byte SweepTime;// { get; set; }

        public SoundChannel3(WaveTableReadDelegate waveTableRead)
        {
            counter = counterReload;
            pointer = 0;
            OutputLeft = OutputRight = 0;

            Pitch = 0;
            VolumeLeft = VolumeRight = 0;
            IsEnabled = false;

            IsSweepEnabled = false;

            sweepCounter = 0;
            sweepCycles = sweepReload;

            SweepValue = 0;
            SweepTime = 0;

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

            IsSweepEnabled = false;

            sweepCounter = 0;
            sweepCycles = sweepReload;

            SweepValue = 0;
            SweepTime = 0;
        }

        public void Step()
        {
            Step(1);
        }

        /// <summary>
        /// [性能优化-第一阶段] 批处理波形/扫描更新
        /// 原始设计：循环调用Step() 5000-20000次/帧，每次检查counter递减
        /// 优化方案：集中计算 counter -= cycles，仅触发条件时更新波形或扫描
        /// 好处：减少方法调用开销，保持原始计时精度(counter == Pitch触发)
        /// 说明：支持波形模式和扫描(Sweep)模式，扫描会动态改变Pitch
        /// </summary>
        public void Step(int cycles)
        {
            if (!IsEnabled || cycles <= 0)
                return;

            if (IsSweepEnabled)
            {
                // [性能优化] Advance sweep timing in a batch.
                sweepCycles -= cycles;
                while (sweepCycles <= 0)
                {
                    sweepCounter--;
                    if (sweepCounter <= 0)
                    {
                        sweepCounter = SweepTime;
                        Pitch = (ushort)(Pitch + SweepValue);
                    }
                    sweepCycles += sweepReload;
                }
            }

            // [性能优化] 批量推进：仅在counter达到Pitch时执行输出更新
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
    ///* Channel 3, has optional sweep */
    //public sealed class SoundChannel3
    //{
    //	const int counterReload = 2048;
    //	const int sweepReload = 32 * 256;

    //	ushort counter;
    //	byte pointer;

    //	public byte OutputLeft;// { get; set; }
    //       public byte OutputRight;// { get; set; }

    //       int sweepCounter;
    //	int sweepCycles;

    //	readonly WaveTableReadDelegate waveTableReadDelegate;

    //	/* REG_SND_CH3_PITCH */
    //	public ushort Pitch;// { get; set; }
    //       /* REG_SND_CH3_VOL */
    //       public byte VolumeLeft;// { get; set; }
    //       public byte VolumeRight;// { get; set; }
    //       /* REG_SND_CTRL */
    //       public bool IsEnabled;// { get; set; }
    //       public bool IsSweepEnabled;// { get; set; }

    //       /* REG_SND_SWEEP_VALUE */
    //       public sbyte SweepValue;// { get; set; }
    //       /* REG_SND_SWEEP_TIME */
    //       public byte SweepTime;// { get; set; }

    //	public SoundChannel3(WaveTableReadDelegate waveTableRead) => waveTableReadDelegate = waveTableRead;

    //	public void Reset()
    //	{
    //		counter = counterReload;
    //		pointer = 0;
    //		OutputLeft = OutputRight = 0;

    //		Pitch = 0;
    //		VolumeLeft = VolumeRight = 0;
    //		IsEnabled = false;

    //		IsSweepEnabled = false;

    //		sweepCounter = 0;
    //		sweepCycles = sweepReload;

    //		SweepValue = 0;
    //		SweepTime = 0;
    //	}

    //	public void Step()
    //	{
    //		if (IsSweepEnabled)
    //		{
    //			sweepCycles--;
    //			if (sweepCycles == 0)
    //			{
    //				sweepCounter--;
    //				if (sweepCounter <= 0)
    //				{
    //					sweepCounter = SweepTime;
    //					Pitch = (ushort)(Pitch + SweepValue);
    //				}
    //			}
    //			sweepCycles = sweepReload;
    //		}

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
