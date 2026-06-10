namespace sGBA;

public partial class GbaAudio
{
	private void RunPsg( long targetCycles )
	{
		if ( _ch1Playing )
		{
			int period = 4 * (2048 - _ch1Frequency) * TimingFactor;
			if ( period > 0 )
			{
				long diff = targetCycles - _ch1LastUpdate;
				if ( diff >= period )
				{
					int steps = (int)(diff / period);
					_ch1DutyIndex = (_ch1DutyIndex + steps) & 7;
					_ch1LastUpdate += steps * period;
					_ch1Sample = DutyTable[_ch1Duty * 8 + _ch1DutyIndex] * _ch1EnvVolume;
				}
			}
		}

		if ( _ch2Playing )
		{
			int period = 4 * (2048 - _ch2Frequency) * TimingFactor;
			if ( period > 0 )
			{
				long diff = targetCycles - _ch2LastUpdate;
				if ( diff >= period )
				{
					int steps = (int)(diff / period);
					_ch2DutyIndex = (_ch2DutyIndex + steps) & 7;
					_ch2LastUpdate += steps * period;
					_ch2Sample = DutyTable[_ch2Duty * 8 + _ch2DutyIndex] * _ch2EnvVolume;
				}
			}
		}

		if ( _ch3Playing )
		{
			int cycles = 2 * (2048 - _ch3Rate) * TimingFactor;
			if ( cycles > 0 )
			{
				long diff = targetCycles - _ch3NextUpdate;
				if ( diff >= 0 )
				{
					int steps = (int)(diff / cycles) + 1;

					int mask = _ch3Size ? 63 : 31;
					int bankOffset = 0;
					if ( !_ch3Size )
						bankOffset = _ch3Bank ? 16 : 0;

					for ( int i = 0; i < steps; i++ )
					{
						_ch3Window = (_ch3Window + 1) & mask;
					}

					int sampleIdx = _ch3Window;
					int byteIdx = bankOffset + sampleIdx / 2;
					if ( byteIdx >= WaveRam.Length ) byteIdx &= (WaveRam.Length - 1);
					byte dataByte = WaveRam[byteIdx];
					int nibble = (sampleIdx & 1) == 0 ? (dataByte >> 4) : (dataByte & 0xF);

					int volumeShift;
					switch ( _ch3Volume )
					{
						case 0: volumeShift = 4; break;
						case 1: volumeShift = 0; break;
						case 2: volumeShift = 1; break;
						default: volumeShift = 2; break;
					}

					if ( _ch3Volume > 3 )
					{
						nibble += nibble << 1;
					}
					nibble >>= volumeShift;
					_ch3Sample = nibble;

					_ch3NextUpdate += (long)steps * cycles;
				}
			}
		}

		if ( _ch4Playing )
		{
			int noiseCycles = _ch4Ratio != 0 ? 2 * _ch4Ratio : 1;
			noiseCycles <<= _ch4Frequency;
			noiseCycles *= 8 * TimingFactor;

			if ( noiseCycles > 0 )
			{
				long diff = targetCycles - _ch4LastEvent;
				if ( diff >= noiseCycles )
				{
					long last = 0;
					int lsb = 0;
					int coeff = _ch4Power ? 0x4040 : 0x4000;

					for ( ; last + noiseCycles <= diff; last += noiseCycles )
					{
						lsb = (int)((_ch4Lfsr ^ (_ch4Lfsr >> 1) ^ 1) & 1);
						_ch4Lfsr >>= 1;
						if ( lsb != 0 )
							_ch4Lfsr |= (uint)coeff;
						else
							_ch4Lfsr &= ~(uint)coeff;
					}

					_ch4Sample = lsb * _ch4EnvVolume;
					_ch4LastEvent += last;
				}
			}
		}
	}

	private void ClockFrameSequencer()
	{
		if ( !Enable ) return;

		RunPsg( _totalCycles );

		_frameSeqStep = (_frameSeqStep + 1) & 7;

		switch ( _frameSeqStep )
		{
			case 2:
			case 6:
				ClockSweep();
				ClockLengths();
				break;
			case 0:
			case 4:
				ClockLengths();
				break;
			case 7:
				ClockEnvelopes();
				break;
		}
	}

	private void ClockLengths()
	{
		if ( _ch1Length > 0 && _ch1Stop )
		{
			_ch1Length--;
			if ( _ch1Length == 0 )
				_ch1Playing = false;
		}
		if ( _ch2Length > 0 && _ch2Stop )
		{
			_ch2Length--;
			if ( _ch2Length == 0 )
				_ch2Playing = false;
		}
		if ( _ch3Length > 0 && _ch3Stop )
		{
			_ch3Length--;
			if ( _ch3Length == 0 )
				_ch3Playing = false;
		}
		if ( _ch4Length > 0 && _ch4Stop )
		{
			_ch4Length--;
			if ( _ch4Length == 0 )
				_ch4Playing = false;
		}
	}

	private void ClockSweep()
	{
		if ( !_ch1SweepEnable ) return;

		_ch1SweepStep--;
		if ( _ch1SweepStep == 0 )
		{
			if ( !UpdateSweep( false ) )
			{
				_ch1Playing = false;
			}
		}
	}

	private void ClockEnvelopes()
	{
		if ( _ch1Playing && _ch1EnvDead == 0 )
		{
			TickEnvelope( ref _ch1EnvVolume, ref _ch1EnvStepTime,
			ref _ch1EnvDirection, ref _ch1EnvDead, ref _ch1EnvNextStep );
			_ch1Sample = DutyTable[_ch1Duty * 8 + _ch1DutyIndex] * _ch1EnvVolume;
		}

		if ( _ch2Playing && _ch2EnvDead == 0 )
		{
			TickEnvelope( ref _ch2EnvVolume, ref _ch2EnvStepTime,
			ref _ch2EnvDirection, ref _ch2EnvDead, ref _ch2EnvNextStep );
			_ch2Sample = DutyTable[_ch2Duty * 8 + _ch2DutyIndex] * _ch2EnvVolume;
		}

		if ( _ch4Playing && _ch4EnvDead == 0 )
		{
			int oldSample = _ch4Sample;
			TickEnvelope( ref _ch4EnvVolume, ref _ch4EnvStepTime,
			ref _ch4EnvDirection, ref _ch4EnvDead, ref _ch4EnvNextStep );
			_ch4Sample = (oldSample > 0 ? 1 : 0) * _ch4EnvVolume;
		}
	}

	private void SamplePsg( out int left, out int right )
	{
		int sampleLeft = 0;
		int sampleRight = 0;

		if ( _psgCh1Left ) sampleLeft += _ch1Sample;
		if ( _psgCh1Right ) sampleRight += _ch1Sample;
		if ( _psgCh2Left ) sampleLeft += _ch2Sample;
		if ( _psgCh2Right ) sampleRight += _ch2Sample;
		if ( _psgCh3Left ) sampleLeft += _ch3Sample;
		if ( _psgCh3Right ) sampleRight += _ch3Sample;

		sampleLeft <<= 3;
		sampleRight <<= 3;

		int ch4Out = _ch4Sample << 3;
		if ( _psgCh4Left ) sampleLeft += ch4Out;
		if ( _psgCh4Right ) sampleRight += ch4Out;

		left = sampleLeft * (1 + _volumeLeft);
		right = sampleRight * (1 + _volumeRight);
	}

	private void MixSample( out short left, out short right )
	{
		RunPsg( _totalCycles );

		SamplePsg( out int psgLeft, out int psgRight );

		int psgShift = 4 - _psgVolume;
		psgLeft >>= psgShift;
		psgRight >>= psgShift;

		int mixLeft = psgLeft;
		int mixRight = psgRight;

		int dmaA = _fifoA.Sample << 2;
		if ( !_volumeChA ) dmaA >>= 1;
		if ( _chALeft ) mixLeft += dmaA;
		if ( _chARight ) mixRight += dmaA;

		int dmaB = _fifoB.Sample << 2;
		if ( !_volumeChB ) dmaB >>= 1;
		if ( _chBLeft ) mixLeft += dmaB;
		if ( _chBRight ) mixRight += dmaB;

		int bias = SoundBias & 0x3FF;
		mixLeft = ApplyBias( mixLeft, bias );
		mixRight = ApplyBias( mixRight, bias );

		left = (short)Math.Clamp( mixLeft, -32768, 32767 );
		right = (short)Math.Clamp( mixRight, -32768, 32767 );
	}

	private static int ApplyBias( int sample, int bias )
	{
		sample += bias;
		if ( sample >= 0x400 ) sample = 0x3FF;
		else if ( sample < 0 ) sample = 0;
		sample -= bias;
		return sample * 48;
	}
}
