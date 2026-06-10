namespace sGBA;

public partial class GbaBios
{
	public Gba Gba { get; }

	public bool HleActive;

	public int BiosStall;

	public GbaBios( Gba gba )
	{
		Gba = gba;
	}

	public bool HandleSwi( uint comment )
	{
		switch ( comment )
		{
			case 0xF0:
				Gba.Cpu.Registers[11] = (uint)BiosStall;
				return true;
			case 0xFA:
				Gba.Memory.FlushAgbPrint();
				return true;
		}

		HleActive = true;
		BiosStall = 0;
		bool useStall = false;

		switch ( comment )
		{
			case 0x00: // SoftReset
				HleActive = false;
				return false;
			case 0x01: RegisterRamReset(); break;
			case 0x02: // Halt
				HleActive = false;
				return false;
			case 0x03: Stop(); break;
			case 0x04: // IntrWait
			case 0x05: // VBlankIntrWait
				HleActive = false;
				return false;
			case 0x06:
				useStall = true;
				Div();
				break;
			case 0x07:
				useStall = true;
				DivArm();
				break;
			case 0x08:
				useStall = true;
				Sqrt();
				break;
			case 0x09:
				useStall = true;
				ArcTan();
				break;
			case 0x0A:
				useStall = true;
				ArcTan2();
				break;
			case 0x0B: // CpuSet
			case 0x0C: // CpuFastSet
				if ( (Gba.Cpu.Registers[0] >> 24) < 2 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Cannot CpuSet from BIOS" );
					break;
				}

				uint alignmentMask = (Gba.Cpu.Registers[2] & (1u << 26)) != 0 ? 3u : 1u;
				if ( (Gba.Cpu.Registers[0] & alignmentMask) != 0 )
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Misaligned CpuSet source" );
				if ( (Gba.Cpu.Registers[1] & alignmentMask) != 0 )
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Misaligned CpuSet destination" );

				HleActive = false;
				return false;
			case 0x0D: GetBiosChecksum(); break;
			case 0x0E: BgAffineSet(); break;
			case 0x0F: ObjAffineSet(); break;
			case 0x10:
				if ( Gba.Cpu.Registers[0] < 0x02000000 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad BitUnPack source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad BitUnPack destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						BitUnPack();
						break;
				}
				break;
			case 0x11:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad LZ77 source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad LZ77 destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						useStall = true;
						LZ77UnCompWram();
						break;
				}
				break;
			case 0x12:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad LZ77 source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad LZ77 destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						useStall = true;
						LZ77UnCompVram();
						break;
				}
				break;
			case 0x13:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad Huffman source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad Huffman destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						HuffmanUnComp();
						break;
				}
				break;
			case 0x14:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad RL source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad RL destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						RLUnCompWram();
						break;
				}
				break;
			case 0x15:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad RL source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad RL destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						RLUnCompVram();
						break;
				}
				break;
			case 0x16:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad UnFilter source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad UnFilter destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						Diff8BitUnFilterWram();
						break;
				}
				break;
			case 0x17:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad UnFilter source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad UnFilter destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						Diff8BitUnFilterVram();
						break;
				}
				break;
			case 0x18:
				if ( (Gba.Cpu.Registers[0] & 0x0E000000) == 0 )
				{
					GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad UnFilter source" );
					break;
				}

				switch ( Gba.Cpu.Registers[1] >> 24 )
				{
					default:
						GbaLog.Write( LogCategory.GBABIOS, LogLevel.GameError, "Bad UnFilter destination" );
						goto case 0x2;
					case 0x2:
					case 0x3:
					case 0x6:
						Diff16BitUnFilter();
						break;
				}
				break;
			case 0x19:
				GbaLog.Write( LogCategory.GBABIOS, LogLevel.Stub, "Stub software interrupt: SoundBias (19)" );
				break;
			case 0x1F: MidiKey2Freq(); break;
			case 0x2A: // SoundDriverGetJumpList
				HleActive = false;
				return false;
			default:
				GbaLog.Write( LogCategory.GBABIOS, LogLevel.Stub, "Stub software interrupt: {0:X2}", comment );
				break;
		}

		if ( useStall && BiosStall >= 18 )
		{
			BiosStall -= 18;
			Gba.Cpu.Cycles += BiosStall & 3;
			BiosStall &= ~3;
			HleActive = false;
			return false;
		}

		if ( useStall )
		{
			Gba.Cpu.Cycles += BiosStall;
		}

		HleActive = false;

		int region = (int)((Gba.Cpu.Gprs[15] >> 24) & 0xF);
		Gba.Cpu.Cycles += 45 + Gba.Memory.WaitstatesNonseq16[region];
		if ( Gba.Cpu.ThumbMode )
			Gba.Cpu.Cycles += Gba.Memory.WaitstatesNonseq16[region] + Gba.Memory.WaitstatesSeq16[region];
		else
			Gba.Cpu.Cycles += Gba.Memory.WaitstatesNonseq32[region] + Gba.Memory.WaitstatesSeq32[region];

		Gba.Memory.BiosPrefetch = 0xE3A02004;

		return true;
	}

	private void GetBiosChecksum()
	{
		Gba.Cpu.Registers[0] = 0xBAAE187F;
		Gba.Cpu.Registers[1] = 1;
		Gba.Cpu.Registers[3] = 0x4000;
	}

	private void RegisterRamReset()
	{
		uint flags = Gba.Cpu.Registers[0];
		var io = Gba.Io;

		io.Write16( 0x000, 0x0080 );

		if ( (flags & 0x01) != 0 )
			Gba.Memory.Wram.AsSpan( 0, 0x40000 ).Clear();
		if ( (flags & 0x02) != 0 )
			Gba.Memory.Iwram.AsSpan( 0, 0x7E00 ).Clear();
		if ( (flags & 0x04) != 0 )
			Array.Clear( Gba.Memory.PaletteRam );
		if ( (flags & 0x08) != 0 )
			Gba.Memory.Vram.AsSpan( 0, 0x18000 ).Clear();
		if ( (flags & 0x10) != 0 )
			Array.Clear( Gba.Memory.Oam );
		if ( (flags & 0x20) != 0 )
		{
			io.Write16( 0x128, 0 );
			io.Write16( 0x134, 0x8000 );
			io.Write16( 0x120, 0 );
			io.Write16( 0x140, 0 );
			io.Write16( 0x150, 0 );
			io.Write16( 0x152, 0 );
			io.Write16( 0x154, 0 );
			io.Write16( 0x156, 0 );
		}
		if ( (flags & 0x40) != 0 )
		{
			var apu = Gba.Audio;
			apu.WriteRegister( 0x60, 0 );
			apu.WriteRegister( 0x62, 0 );
			apu.WriteRegister( 0x64, 0 );
			apu.WriteRegister( 0x68, 0 );
			apu.WriteRegister( 0x6C, 0 );
			apu.WriteRegister( 0x70, 0 );
			apu.WriteRegister( 0x72, 0 );
			apu.WriteRegister( 0x74, 0 );
			apu.WriteRegister( 0x78, 0 );
			apu.WriteRegister( 0x7C, 0 );
			apu.WriteRegister( 0x80, 0 );
			apu.WriteRegister( 0x82, 0 );
			apu.WriteRegister( 0x84, 0 );
			apu.WriteRegister( 0x088, 0x0200 );
			Array.Clear( apu.WaveRam );
		}
		if ( (flags & 0x80) != 0 )
		{
			io.Write16( 0x004, 0 );
			io.Write16( 0x006, 0 );
			io.Write16( 0x008, 0 );
			io.Write16( 0x00A, 0 );
			io.Write16( 0x00C, 0 );
			io.Write16( 0x00E, 0 );
			io.Write16( 0x010, 0 );
			io.Write16( 0x012, 0 );
			io.Write16( 0x014, 0 );
			io.Write16( 0x016, 0 );
			io.Write16( 0x018, 0 );
			io.Write16( 0x01A, 0 );
			io.Write16( 0x01C, 0 );
			io.Write16( 0x01E, 0 );
			io.Write16( 0x020, 0x0100 );
			io.Write16( 0x022, 0 );
			io.Write16( 0x024, 0 );
			io.Write16( 0x026, 0x0100 );
			io.Write16( 0x028, 0 );
			io.Write16( 0x02A, 0 );
			io.Write16( 0x02C, 0 );
			io.Write16( 0x02E, 0 );
			io.Write16( 0x030, 0x0100 );
			io.Write16( 0x032, 0 );
			io.Write16( 0x034, 0 );
			io.Write16( 0x036, 0x0100 );
			io.Write16( 0x038, 0 );
			io.Write16( 0x03A, 0 );
			io.Write16( 0x03C, 0 );
			io.Write16( 0x03E, 0 );
			io.Write16( 0x040, 0 );
			io.Write16( 0x042, 0 );
			io.Write16( 0x044, 0 );
			io.Write16( 0x046, 0 );
			io.Write16( 0x048, 0 );
			io.Write16( 0x04A, 0 );
			io.Write16( 0x04C, 0 );
			io.Write16( 0x050, 0 );
			io.Write16( 0x052, 0 );
			io.Write16( 0x054, 0 );
			for ( uint ch = 0; ch < 4; ch++ )
			{
				uint b = 0x0B0 + ch * 12;
				io.Write16( b, 0 );
				io.Write16( b + 2, 0 );
				io.Write16( b + 4, 0 );
				io.Write16( b + 6, 0 );
				io.Write16( b + 8, 0 );
				io.Write16( b + 10, 0 );
			}
			for ( uint t = 0; t < 4; t++ )
			{
				io.Write16( 0x100 + t * 4, 0 );
				io.Write16( 0x102 + t * 4, 0 );
			}
			io.Write16( 0x200, 0 );
			io.Write16( 0x202, 0xFFFF );
			io.Write16( 0x204, 0 );
			io.Write16( 0x208, 0 );
		}
	}

	private void Stop()
	{
		Gba.Cpu.Halted = true;
	}
}
