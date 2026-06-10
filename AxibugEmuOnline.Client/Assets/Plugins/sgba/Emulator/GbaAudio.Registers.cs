namespace sGBA;

public partial class GbaAudio
{
	public void WriteRegister( uint offset, ushort value )
	{
		if ( offset >= 0x60 && offset < 0x80 && !Enable )
			return;

		if ( offset != 0x82 )
			FlushSamples();

		switch ( offset )
		{
			case 0x60:
				Sound1CntL = value;
				WriteNR10( (byte)(value & 0xFF) );
				break;
			case 0x62:
				Sound1CntH = value;
				WriteNR11( (byte)(value & 0xFF) );
				WriteNR12( (byte)(value >> 8) );
				break;
			case 0x64:
				Sound1CntX = value;
				WriteNR13( (byte)(value & 0xFF) );
				WriteNR14( (byte)(value >> 8) );
				break;

			case 0x68:
				Sound2CntL = value;
				WriteNR21( (byte)(value & 0xFF) );
				WriteNR22( (byte)(value >> 8) );
				break;
			case 0x6C:
				Sound2CntH = value;
				WriteNR23( (byte)(value & 0xFF) );
				WriteNR24( (byte)(value >> 8) );
				break;

			case 0x70:
				Sound3CntL = value;
				WriteNR30( (byte)(value & 0xFF) );
				break;
			case 0x72:
				Sound3CntH = value;
				WriteNR31( (byte)(value & 0xFF) );
				_ch3Volume = (value >> 13) & 7;
				break;
			case 0x74:
				Sound3CntX = value;
				WriteNR33( (byte)(value & 0xFF) );
				WriteNR34( (byte)(value >> 8) );
				break;

			case 0x78:
				Sound4CntL = value;
				WriteNR41( (byte)(value & 0xFF) );
				WriteNR42( (byte)(value >> 8) );
				break;
			case 0x7C:
				Sound4CntH = value;
				WriteNR43( (byte)(value & 0xFF) );
				WriteNR44( (byte)(value >> 8) );
				break;

			case 0x80:
				SoundCntL = value;
				WriteNR50( (byte)(value & 0xFF) );
				WriteNR51( (byte)(value >> 8) );
				break;
			case 0x82:
				WriteSoundCntH( value );
				break;
			case 0x84:
				WriteSoundCntX( value );
				break;
			case 0x88:
				SoundBias = value;
				break;
		}
	}

	public void WriteRegisterByte( uint regOffset, bool highByte, byte value )
	{
		if ( regOffset != 0x82 )
			FlushSamples();

		switch ( regOffset )
		{
			case 0x60:
				if ( !highByte )
				{
					Sound1CntL = (ushort)((Sound1CntL & 0xFF00) | value);
					WriteNR10( value );
				}
				break;
			case 0x62:
				if ( !highByte )
				{
					Sound1CntH = (ushort)((Sound1CntH & 0xFF00) | value);
					WriteNR11( value );
				}
				else
				{
					Sound1CntH = (ushort)((Sound1CntH & 0x00FF) | (value << 8));
					WriteNR12( value );
				}
				break;
			case 0x64:
				if ( !highByte )
				{
					Sound1CntX = (ushort)((Sound1CntX & 0xFF00) | value);
					WriteNR13( value );
				}
				else
				{
					Sound1CntX = (ushort)((Sound1CntX & 0x00FF) | (value << 8));
					WriteNR14( value );
				}
				break;
			case 0x68:
				if ( !highByte )
				{
					Sound2CntL = (ushort)((Sound2CntL & 0xFF00) | value);
					WriteNR21( value );
				}
				else
				{
					Sound2CntL = (ushort)((Sound2CntL & 0x00FF) | (value << 8));
					WriteNR22( value );
				}
				break;
			case 0x6C:
				if ( !highByte )
				{
					Sound2CntH = (ushort)((Sound2CntH & 0xFF00) | value);
					WriteNR23( value );
				}
				else
				{
					Sound2CntH = (ushort)((Sound2CntH & 0x00FF) | (value << 8));
					WriteNR24( value );
				}
				break;
			case 0x70:
				if ( !highByte )
				{
					Sound3CntL = (ushort)((Sound3CntL & 0xFF00) | value);
					WriteNR30( value );
				}
				break;
			case 0x72:
				if ( !highByte )
				{
					Sound3CntH = (ushort)((Sound3CntH & 0xFF00) | value);
					WriteNR31( value );
				}
				else
				{
					Sound3CntH = (ushort)((Sound3CntH & 0x00FF) | (value << 8));
					_ch3Volume = (value >> 5) & 7;
				}
				break;
			case 0x74:
				if ( !highByte )
				{
					Sound3CntX = (ushort)((Sound3CntX & 0xFF00) | value);
					WriteNR33( value );
				}
				else
				{
					Sound3CntX = (ushort)((Sound3CntX & 0x00FF) | (value << 8));
					WriteNR34( value );
				}
				break;
			case 0x78:
				if ( !highByte )
				{
					Sound4CntL = (ushort)((Sound4CntL & 0xFF00) | value);
					WriteNR41( value );
				}
				else
				{
					Sound4CntL = (ushort)((Sound4CntL & 0x00FF) | (value << 8));
					WriteNR42( value );
				}
				break;
			case 0x7C:
				if ( !highByte )
				{
					Sound4CntH = (ushort)((Sound4CntH & 0xFF00) | value);
					WriteNR43( value );
				}
				else
				{
					Sound4CntH = (ushort)((Sound4CntH & 0x00FF) | (value << 8));
					WriteNR44( value );
				}
				break;
			case 0x80:
				if ( !highByte )
				{
					SoundCntL = (ushort)((SoundCntL & 0xFF00) | value);
					WriteNR50( value );
				}
				else
				{
					SoundCntL = (ushort)((SoundCntL & 0x00FF) | (value << 8));
					WriteNR51( value );
				}
				break;
			case 0x82:
				if ( !highByte )
				{
					SoundCntH = (ushort)((SoundCntH & 0xFF00) | value);
					_psgVolume = value & 3;
					_volumeChA = (value & 4) != 0;
					_volumeChB = (value & 8) != 0;
				}
				else
				{
					SoundCntH = (ushort)((SoundCntH & 0x00FF) | (value << 8));
					_chARight = (value & 1) != 0;
					_chALeft = (value & 2) != 0;
					_chATimer = (value & 4) != 0;
					if ( (value & 8) != 0 )
					{
						_fifoA.Write = _fifoA.Read = 0;
					}
					_chBRight = (value & 0x10) != 0;
					_chBLeft = (value & 0x20) != 0;
					_chBTimer = (value & 0x40) != 0;
					if ( (value & 0x80) != 0 )
					{
						_fifoB.Write = _fifoB.Read = 0;
					}
				}
				break;
			case 0x84:
				if ( !highByte )
				{
					WriteSoundCntX( value );
				}
				break;
			case 0x88:
				if ( !highByte )
					SoundBias = (ushort)((SoundBias & 0xFF00) | value);
				else
					SoundBias = (ushort)((SoundBias & 0x00FF) | (value << 8));
				break;
		}
	}

	public ushort ReadRegister( uint offset )
	{
		switch ( offset )
		{
			case 0x60: return (ushort)(Sound1CntL & 0x007F);
			case 0x62: return (ushort)(Sound1CntH & 0xFFC0);
			case 0x64: return (ushort)(Sound1CntX & 0x4000);
			case 0x68: return (ushort)(Sound2CntL & 0xFFC0);
			case 0x6C: return (ushort)(Sound2CntH & 0x4000);
			case 0x70: return (ushort)(Sound3CntL & 0x00E0);
			case 0x72: return (ushort)(Sound3CntH & 0xE000);
			case 0x74: return (ushort)(Sound3CntX & 0x4000);
			case 0x78: return (ushort)(Sound4CntL & 0xFF00);
			case 0x7C: return (ushort)(Sound4CntH & 0x40FF);
			case 0x80: return (ushort)(SoundCntL & 0xFF77);
			case 0x82: return (ushort)(SoundCntH & 0x770F);
			case 0x84:
				{
					ushort status = (ushort)(Enable ? 0x80 : 0);
					if ( _ch1Playing ) status |= 1;
					if ( _ch2Playing ) status |= 2;
					if ( _ch3Playing ) status |= 4;
					if ( _ch4Playing ) status |= 8;
					return status;
				}
			case 0x88: return SoundBias;
			default: return 0;
		}
	}

	private void WriteNR10( byte value )
	{
		_ch1SweepShift = value & 7;
		bool oldDir = _ch1SweepDirection;
		_ch1SweepDirection = (value & 8) != 0;
		if ( _ch1SweepOccurred && oldDir && !_ch1SweepDirection )
		{
			_ch1Playing = false;
		}
		_ch1SweepOccurred = false;
		_ch1SweepTime = (value >> 4) & 7;
		if ( _ch1SweepTime == 0 ) _ch1SweepTime = 8;
	}

	private void WriteNR11( byte value )
	{
		_ch1Duty = (value >> 6) & 3;
		_ch1Length = 64 - (value & 0x3F);
	}

	private void WriteNR12( byte value )
	{
		_ch1EnvStepTime = value & 7;
		_ch1EnvDirection = (value & 8) != 0;
		_ch1EnvInitVolume = (value >> 4) & 0xF;

		if ( _ch1EnvStepTime == 0 )
		{
			_ch1EnvVolume &= 0xF;
		}

		UpdateEnvelopeDead( ref _ch1EnvVolume, ref _ch1EnvStepTime, ref _ch1EnvDirection,
		ref _ch1EnvInitVolume, ref _ch1EnvDead, ref _ch1EnvNextStep );

		if ( _ch1EnvInitVolume == 0 && !_ch1EnvDirection )
		{
			_ch1Playing = false;
		}
	}

	private void WriteNR13( byte value )
	{
		_ch1Frequency = (_ch1Frequency & 0x700) | value;
	}

	private void WriteNR14( byte value )
	{
		_ch1Frequency = (_ch1Frequency & 0xFF) | ((value & 7) << 8);
		bool wasStop = _ch1Stop;
		_ch1Stop = (value & 0x40) != 0;

		if ( !wasStop && _ch1Stop && _ch1Length > 0 && (_frameSeqStep & 1) == 0 )
		{
			_ch1Length--;
			if ( _ch1Length == 0 )
				_ch1Playing = false;
		}

		if ( (value & 0x80) != 0 )
		{
			_ch1Playing = ResetEnvelope( ref _ch1EnvVolume, ref _ch1EnvStepTime,
			ref _ch1EnvDirection, ref _ch1EnvInitVolume, ref _ch1EnvDead, ref _ch1EnvNextStep );

			_ch1SweepRealFreq = _ch1Frequency;
			_ch1SweepStep = _ch1SweepTime;
			_ch1SweepEnable = (_ch1SweepTime != 8) || _ch1SweepShift != 0;
			_ch1SweepOccurred = false;

			if ( _ch1Playing && _ch1SweepShift > 0 )
			{
				_ch1Playing = UpdateSweep( true );
			}

			if ( _ch1Length == 0 )
			{
				_ch1Length = 64;
				if ( _ch1Stop && (_frameSeqStep & 1) == 0 )
					_ch1Length--;
			}

			_ch1Sample = DutyTable[_ch1Duty * 8 + _ch1DutyIndex] * _ch1EnvVolume;
			_ch1LastUpdate = _totalCycles;
		}
	}

	private void WriteNR21( byte value )
	{
		_ch2Duty = (value >> 6) & 3;
		_ch2Length = 64 - (value & 0x3F);
	}

	private void WriteNR22( byte value )
	{
		_ch2EnvStepTime = value & 7;
		_ch2EnvDirection = (value & 8) != 0;
		_ch2EnvInitVolume = (value >> 4) & 0xF;

		if ( _ch2EnvStepTime == 0 )
		{
			_ch2EnvVolume &= 0xF;
		}

		UpdateEnvelopeDead( ref _ch2EnvVolume, ref _ch2EnvStepTime, ref _ch2EnvDirection,
		ref _ch2EnvInitVolume, ref _ch2EnvDead, ref _ch2EnvNextStep );

		if ( _ch2EnvInitVolume == 0 && !_ch2EnvDirection )
		{
			_ch2Playing = false;
		}
	}

	private void WriteNR23( byte value )
	{
		_ch2Frequency = (_ch2Frequency & 0x700) | value;
	}

	private void WriteNR24( byte value )
	{
		_ch2Frequency = (_ch2Frequency & 0xFF) | ((value & 7) << 8);
		bool wasStop = _ch2Stop;
		_ch2Stop = (value & 0x40) != 0;

		if ( !wasStop && _ch2Stop && _ch2Length > 0 && (_frameSeqStep & 1) == 0 )
		{
			_ch2Length--;
			if ( _ch2Length == 0 )
				_ch2Playing = false;
		}

		if ( (value & 0x80) != 0 )
		{
			_ch2Playing = ResetEnvelope( ref _ch2EnvVolume, ref _ch2EnvStepTime,
			ref _ch2EnvDirection, ref _ch2EnvInitVolume, ref _ch2EnvDead, ref _ch2EnvNextStep );

			if ( _ch2Length == 0 )
			{
				_ch2Length = 64;
				if ( _ch2Stop && (_frameSeqStep & 1) == 0 )
					_ch2Length--;
			}

			_ch2Sample = DutyTable[_ch2Duty * 8 + _ch2DutyIndex] * _ch2EnvVolume;
			_ch2LastUpdate = _totalCycles;
		}
	}

	private void WriteNR30( byte value )
	{
		_ch3Size = (value & 0x20) != 0;
		_ch3Bank = (value & 0x40) != 0;
		_ch3Enable = (value & 0x80) != 0;
		if ( !_ch3Enable )
		{
			_ch3Playing = false;
		}
	}

	private void WriteNR31( byte value )
	{
		_ch3Length = 256 - value;
	}

	private void WriteNR33( byte value )
	{
		_ch3Rate = (_ch3Rate & 0x700) | value;
	}

	private void WriteNR34( byte value )
	{
		_ch3Rate = (_ch3Rate & 0xFF) | ((value & 7) << 8);
		bool wasStop = _ch3Stop;
		_ch3Stop = (value & 0x40) != 0;

		if ( !wasStop && _ch3Stop && _ch3Length > 0 && (_frameSeqStep & 1) == 0 )
		{
			_ch3Length--;
			if ( _ch3Length == 0 )
				_ch3Playing = false;
		}

		if ( (value & 0x80) != 0 )
		{
			_ch3Playing = _ch3Enable;

			if ( _ch3Length == 0 )
			{
				_ch3Length = 256;
				if ( _ch3Stop && (_frameSeqStep & 1) == 0 )
					_ch3Length--;
			}

			_ch3Window = 0;

			if ( _ch3Playing )
			{
				_ch3NextUpdate = _totalCycles + (6 + 2 * (2048 - _ch3Rate)) * TimingFactor;
			}
		}
	}

	private void WriteNR41( byte value )
	{
		_ch4Length = 64 - (value & 0x3F);
	}

	private void WriteNR42( byte value )
	{
		_ch4EnvStepTime = value & 7;
		_ch4EnvDirection = (value & 8) != 0;
		_ch4EnvInitVolume = (value >> 4) & 0xF;

		if ( _ch4EnvStepTime == 0 )
		{
			_ch4EnvVolume &= 0xF;
		}

		UpdateEnvelopeDead( ref _ch4EnvVolume, ref _ch4EnvStepTime, ref _ch4EnvDirection,
		ref _ch4EnvInitVolume, ref _ch4EnvDead, ref _ch4EnvNextStep );

		if ( _ch4EnvInitVolume == 0 && !_ch4EnvDirection )
		{
			_ch4Playing = false;
		}
	}

	private void WriteNR43( byte value )
	{
		_ch4Ratio = value & 7;
		_ch4Power = (value & 8) != 0;
		_ch4Frequency = (value >> 4) & 0xF;
	}

	private void WriteNR44( byte value )
	{
		bool wasStop = _ch4Stop;
		_ch4Stop = (value & 0x40) != 0;

		if ( !wasStop && _ch4Stop && _ch4Length > 0 && (_frameSeqStep & 1) == 0 )
		{
			_ch4Length--;
			if ( _ch4Length == 0 )
				_ch4Playing = false;
		}

		if ( (value & 0x80) != 0 )
		{
			_ch4Playing = ResetEnvelope( ref _ch4EnvVolume, ref _ch4EnvStepTime,
			ref _ch4EnvDirection, ref _ch4EnvInitVolume, ref _ch4EnvDead, ref _ch4EnvNextStep );

			_ch4Lfsr = 0;

			if ( _ch4Length == 0 )
			{
				_ch4Length = 64;
				if ( _ch4Stop && (_frameSeqStep & 1) == 0 )
					_ch4Length--;
			}

			if ( _ch4Playing )
			{
				_ch4LastEvent = _totalCycles;
			}
		}
	}

	private void WriteNR50( byte value )
	{
		_volumeRight = value & 7;
		_volumeLeft = (value >> 4) & 7;
	}

	private void WriteNR51( byte value )
	{
		_psgCh1Right = (value & 1) != 0;
		_psgCh2Right = (value & 2) != 0;
		_psgCh3Right = (value & 4) != 0;
		_psgCh4Right = (value & 8) != 0;
		_psgCh1Left = (value & 0x10) != 0;
		_psgCh2Left = (value & 0x20) != 0;
		_psgCh3Left = (value & 0x40) != 0;
		_psgCh4Left = (value & 0x80) != 0;
	}

	private void WriteSoundCntH( ushort value )
	{
		SoundCntH = value;
		_psgVolume = value & 3;
		_volumeChA = (value & 4) != 0;
		_volumeChB = (value & 8) != 0;
		_chARight = (value & (1 << 8)) != 0;
		_chALeft = (value & (1 << 9)) != 0;
		_chATimer = (value & (1 << 10)) != 0;
		_chBRight = (value & (1 << 12)) != 0;
		_chBLeft = (value & (1 << 13)) != 0;
		_chBTimer = (value & (1 << 14)) != 0;

		if ( (value & (1 << 11)) != 0 )
		{
			_fifoA.Write = _fifoA.Read = 0;
		}
		if ( (value & (1 << 15)) != 0 )
		{
			_fifoB.Write = _fifoB.Read = 0;
		}
	}

	private void WriteSoundCntX( ushort value )
	{
		bool wasEnabled = Enable;
		bool nowEnabled = (value & 0x80) != 0;
		Enable = nowEnabled;
		SoundCntX = (ushort)((SoundCntX & 0x0F) | (value & 0x80));

		if ( wasEnabled && !nowEnabled )
		{
			ushort[] io = Gba.Memory.Io;
			for ( uint offset = 0x60; offset < 0x82; offset += 2 )
				io[offset >> 1] = 0;

			_ch1Playing = false;
			_ch2Playing = false;
			_ch3Playing = false;
			_ch4Playing = false;

			WriteNR10( 0 );
			WriteNR12( 0 );
			WriteNR13( 0 );
			WriteNR14( 0 );
			WriteNR22( 0 );
			WriteNR23( 0 );
			WriteNR24( 0 );
			WriteNR30( 0 );
			WriteNR33( 0 );
			WriteNR34( 0 );
			WriteNR42( 0 );
			WriteNR43( 0 );
			WriteNR44( 0 );
			WriteNR50( 0 );
			WriteNR51( 0 );

			WriteNR11( 0 );
			WriteNR21( 0 );
			WriteNR31( 0 );
			WriteNR41( 0 );

			Sound1CntL = Sound1CntH = Sound1CntX = 0;
			Sound2CntL = Sound2CntH = 0;
			Sound3CntL = Sound3CntH = Sound3CntX = 0;
			Sound4CntL = Sound4CntH = 0;
			SoundCntL = 0;

			_ch3Size = false;
			_ch3Bank = false;
			_ch3Volume = 0;
			_ch3Sample = 0;

			_psgVolume = 0;
			_volumeChA = false;
			_volumeChB = false;

			SoundCntH &= 0xFF00;
			io[0x82 >> 1] &= 0xFF00;
		}
		else if ( !wasEnabled && nowEnabled )
		{
			_frameSeqStep = 7;
		}
	}

	private static bool ResetEnvelope( ref int volume, ref int stepTime,
	ref bool direction, ref int initVolume, ref int dead, ref int nextStep )
	{
		volume = initVolume;
		nextStep = stepTime;
		UpdateEnvelopeDead( ref volume, ref stepTime, ref direction, ref initVolume, ref dead, ref nextStep );
		return initVolume != 0 || direction;
	}

	private static void UpdateEnvelopeDead( ref int volume, ref int stepTime,
	ref bool direction, ref int initVolume, ref int dead, ref int nextStep )
	{
		if ( stepTime == 0 )
		{
			dead = volume != 0 ? 1 : 2;
		}
		else if ( !direction && volume == 0 )
		{
			dead = 2;
		}
		else if ( direction && volume == 0xF )
		{
			dead = 1;
		}
		else if ( dead != 0 )
		{
			nextStep = stepTime;
			dead = 0;
		}
	}

	private static void TickEnvelope( ref int volume, ref int stepTime,
	ref bool direction, ref int dead, ref int nextStep )
	{
		if ( dead != 0 ) return;
		if ( stepTime == 0 ) return;

		nextStep--;
		if ( nextStep > 0 ) return;

		if ( direction )
		{
			volume++;
			if ( volume >= 15 )
			{
				volume = 15;
				dead = 1;
			}
			else
			{
				nextStep = stepTime;
			}
		}
		else
		{
			volume--;
			if ( volume <= 0 )
			{
				volume = 0;
				dead = 2;
			}
			else
			{
				nextStep = stepTime;
			}
		}
	}

	private bool UpdateSweep( bool initial )
	{
		if ( initial || _ch1SweepTime != 8 )
		{
			int frequency = _ch1SweepRealFreq;
			if ( _ch1SweepDirection )
			{
				frequency -= frequency >> _ch1SweepShift;
				if ( !initial && frequency >= 0 )
				{
					_ch1Frequency = frequency;
					_ch1SweepRealFreq = frequency;
				}
			}
			else
			{
				frequency += frequency >> _ch1SweepShift;
				if ( frequency < 2048 )
				{
					if ( !initial && _ch1SweepShift > 0 )
					{
						_ch1Frequency = frequency;
						_ch1SweepRealFreq = frequency;
						if ( !UpdateSweep( true ) )
							return false;
					}
				}
				else
				{
					return false;
				}
			}
			_ch1SweepOccurred = true;
		}
		_ch1SweepStep = _ch1SweepTime;
		return true;
	}
}
