namespace sGBA;

public partial class GbaBios
{
	private void LZ77UnCompWram() { LZ77Decompress( 1 ); }
	private void LZ77UnCompVram() { LZ77Decompress( 2 ); }

	private void LZ77Decompress( int width )
	{
		uint src = Gba.Cpu.Registers[0];
		uint dst = Gba.Cpu.Registers[1];
		int cycles = 20;

		uint header = Gba.Memory.Load32( src );
		int remaining = (int)((header & 0xFFFFFF00) >> 8);
		src += 4;

		int blocksRemaining = 0;
		int blockHeader = 0;
		int halfword = 0;

		while ( remaining > 0 )
		{
			cycles += 14;

			if ( blocksRemaining > 0 )
			{
				cycles += 18;

				if ( (blockHeader & 0x80) != 0 )
				{
					int block = Gba.Memory.Load8( src + 1 ) | (Gba.Memory.Load8( src ) << 8);
					src += 2;
					uint disp = dst - (uint)(block & 0x0FFF) - 1;
					int bytes = (block >> 12) + 3;

					while ( bytes-- > 0 )
					{
						cycles += 10;
						if ( remaining > 0 )
						{
							--remaining;
						}

						if ( width == 2 )
						{
							int val = (short)Gba.Memory.Load16( disp & ~1u );
							if ( (dst & 1) != 0 )
							{
								val >>= (int)(disp & 1) * 8;
								halfword |= val << 8;
								Gba.Memory.Store16( dst ^ 1, (ushort)halfword );
							}
							else
							{
								val >>= (int)(disp & 1) * 8;
								halfword = val & 0xFF;
							}
							cycles += 4;
						}
						else
						{
							int val = Gba.Memory.Load8( disp );
							Gba.Memory.Store8( dst, (byte)val );
						}

						++disp;
						++dst;
					}
				}
				else
				{
					int val = Gba.Memory.Load8( src );
					++src;

					if ( width == 2 )
					{
						if ( (dst & 1) != 0 )
						{
							halfword |= val << 8;
							Gba.Memory.Store16( dst ^ 1, (ushort)halfword );
						}
						else
						{
							halfword = val;
						}
					}
					else
					{
						Gba.Memory.Store8( dst, (byte)val );
					}

					++dst;
					--remaining;
				}

				blockHeader <<= 1;
				--blocksRemaining;
			}
			else
			{
				blockHeader = Gba.Memory.Load8( src );
				++src;
				blocksRemaining = 8;
			}
		}

		Gba.Cpu.Registers[0] = src;
		Gba.Cpu.Registers[1] = dst;
		Gba.Cpu.Registers[3] = 0;
		BiosStall = cycles;
	}

	private void HuffmanUnComp()
	{
		uint src = Gba.Cpu.Registers[0] & 0xFFFFFFFC;
		uint dst = Gba.Cpu.Registers[1];

		uint header = Gba.Memory.Load32( src );
		int bitSize = (int)(header & 0xF);
		int decompSize = (int)(header >> 8);

		if ( bitSize == 0 )
			bitSize = 8;
		if ( 32 % bitSize != 0 || bitSize == 1 )
			return;

		src += 4;

		uint treeSize = Gba.Memory.Load8( src ) * 2u + 1;
		uint treeBase = src + 1;
		src += treeSize + 1;

		int written = 0;
		uint outBuffer = 0;
		int outBits = 0;
		uint bits = 0;
		int bitsLeft = 0;

		uint treeNode = treeBase;
		while ( written < decompSize )
		{
			if ( bitsLeft == 0 )
			{
				bits = Gba.Memory.Load32( src );
				src += 4;
				bitsLeft = 32;
			}

			bool goRight = (bits & 0x80000000) != 0;
			bits <<= 1;
			bitsLeft--;

			byte nodeVal = Gba.Memory.Load8( treeNode );
			uint childOffset = (treeNode & ~1u) + (uint)(nodeVal & 0x3F) * 2 + 2;

			if ( goRight )
				childOffset++;

			int endFlag = goRight ? ((nodeVal >> 6) & 1) : (nodeVal >> 7);

			if ( endFlag != 0 )
			{
				byte data = Gba.Memory.Load8( childOffset );
				outBuffer |= (uint)(data & ((1 << bitSize) - 1)) << outBits;
				outBits += bitSize;

				if ( outBits >= 32 )
				{
					Gba.Memory.Store32( dst, outBuffer );
					dst += 4;
					written += 4;
					outBuffer = 0;
					outBits = 0;
				}

				treeNode = treeBase;
			}
			else
			{
				treeNode = childOffset;
			}
		}

		Gba.Cpu.Registers[0] = src;
		Gba.Cpu.Registers[1] = dst;
	}

	private void RLUnCompWram() { RLDecompress( false ); }
	private void RLUnCompVram() { RLDecompress( true ); }

	private void RLDecompress( bool vram )
	{
		uint src = Gba.Cpu.Registers[0];
		uint dst = Gba.Cpu.Registers[1];

		uint header = Gba.Memory.Load32( src & 0xFFFFFFFC );
		src += 4;
		int remaining = (int)(header >> 8);
		int padding = (4 - remaining) & 0x3;

		int halfword = 0;

		while ( remaining > 0 )
		{
			byte flag = Gba.Memory.Load8( src++ );
			if ( (flag & 0x80) != 0 )
			{
				int length = (flag & 0x7F) + 3;
				byte data = Gba.Memory.Load8( src++ );
				for ( int i = 0; i < length && remaining > 0; i++ )
				{
					remaining--;
					if ( vram )
					{
						if ( (dst & 1) != 0 )
						{
							halfword |= data << 8;
							Gba.Memory.Store16( dst ^ 1, (ushort)halfword );
						}
						else
						{
							halfword = data;
						}
					}
					else
					{
						Gba.Memory.Store8( dst, data );
					}
					dst++;
				}
			}
			else
			{
				int length = (flag & 0x7F) + 1;
				for ( int i = 0; i < length && remaining > 0; i++ )
				{
					byte data = Gba.Memory.Load8( src++ );
					remaining--;
					if ( vram )
					{
						if ( (dst & 1) != 0 )
						{
							halfword |= data << 8;
							Gba.Memory.Store16( dst ^ 1, (ushort)halfword );
						}
						else
						{
							halfword = data;
						}
					}
					else
					{
						Gba.Memory.Store8( dst, data );
					}
					dst++;
				}
			}
		}

		if ( vram )
		{
			if ( (dst & 1) != 0 )
			{
				padding--;
				dst++;
			}
			for ( ; padding > 0; padding -= 2, dst += 2 )
				Gba.Memory.Store16( dst, 0 );
		}
		else
		{
			for ( ; padding > 0; padding-- )
				Gba.Memory.Store8( dst++, 0 );
		}

		Gba.Cpu.Registers[0] = src;
		Gba.Cpu.Registers[1] = dst;
	}

	private void Diff8BitUnFilterWram() { DiffUnFilter( 1, 1 ); }
	private void Diff8BitUnFilterVram() { DiffUnFilter( 1, 2 ); }
	private void Diff16BitUnFilter() { DiffUnFilter( 2, 2 ); }

	private void DiffUnFilter( int inWidth, int outWidth )
	{
		uint src = Gba.Cpu.Registers[0] & 0xFFFFFFFC;
		uint dst = Gba.Cpu.Registers[1];

		uint header = Gba.Memory.Load32( src );
		int remaining = (int)(header >> 8);
		ushort halfword = 0;
		ushort old = 0;
		src += 4;

		while ( remaining > 0 )
		{
			ushort next;
			if ( inWidth == 1 )
				next = Gba.Memory.Load8( src );
			else
				next = Gba.Memory.Load16( src );
			next = (ushort)(next + old);

			if ( outWidth > inWidth )
			{
				halfword >>= 8;
				halfword |= (ushort)(next << 8);
				if ( (src & 1) != 0 )
				{
					Gba.Memory.Store16( dst, halfword );
					dst += (uint)outWidth;
					remaining -= outWidth;
				}
			}
			else if ( outWidth == 1 )
			{
				Gba.Memory.Store8( dst, (byte)next );
				dst += (uint)outWidth;
				remaining -= outWidth;
			}
			else
			{
				Gba.Memory.Store16( dst, next );
				dst += (uint)outWidth;
				remaining -= outWidth;
			}

			old = next;
			src += (uint)inWidth;
		}

		Gba.Cpu.Registers[0] = src;
		Gba.Cpu.Registers[1] = dst;
	}

	private void MidiKey2Freq()
	{
		uint waveDataPtr = Gba.Cpu.Registers[0];
		uint key = Gba.Memory.Load32( waveDataPtr + 4 );
		int midiKey = (int)Gba.Cpu.Registers[1];
		int pitchAdj = (int)Gba.Cpu.Registers[2];
		float exponent = (180f - midiKey - pitchAdj / 256f) / 12f;
		Gba.Cpu.Registers[0] = (uint)(key / MathF.Pow( 2f, exponent ));
	}
}
