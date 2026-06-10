namespace sGBA;

public partial class ArmCore
{
	private void ExecuteThumb()
	{
		uint opcode = _prefetch0 & 0xFFFF;
		_prefetch0 = _prefetch1;
		_prefetch1 = Memory.Load16( Gprs[15] );
		OpenBusPrefetch = _prefetch1 | (_prefetch1 << 16);
		Cycles += 1 + Memory.WaitstatesSeq16[(Gprs[15] >> 24) & 0xF];

		uint top = opcode >> 8;

		switch ( opcode >> 12 )
		{
			case 0:
				ThumbShiftImm( opcode );
				break;
			case 1:
				if ( (opcode & 0x1800) == 0x1800 )
					ThumbAddSub( opcode );
				else
					ThumbShiftImm( opcode );
				break;
			case 2:
				ThumbImmOp( opcode );
				break;
			case 3:
				ThumbImmOp( opcode );
				break;
			case 4:
				if ( (opcode & 0x0800) == 0 )
				{
					if ( (opcode & 0x0400) == 0 )
						ThumbAluOp( opcode );
					else
						ThumbHiRegBx( opcode );
				}
				else
					ThumbPcRelLoad( opcode );
				break;
			case 5:
				if ( (opcode & 0xF200) == 0x5000 )
					ThumbLoadStoreReg( opcode );
				else if ( (opcode & 0xF200) == 0x5200 )
					ThumbLoadStoreSignedHalf( opcode );
				else
					ThumbLoadStoreImmWord( opcode );
				break;
			case 6:
			case 7:
				ThumbLoadStoreImmWord( opcode );
				break;
			case 8:
				ThumbLoadStoreHalf( opcode );
				break;
			case 9:
				ThumbSpRelLoadStore( opcode );
				break;
			case 10:
				ThumbLoadAddress( opcode );
				break;
			case 11:
				if ( (opcode & 0x0600) == 0 )
					ThumbSpOffset( opcode );
				else if ( (opcode & 0x0600) == 0x0400 )
					ThumbPushPop( opcode );
				else if ( (opcode & 0xFF00) == 0xBE00 )
				{ /* BKPT - ignore */ }
				break;
			case 12:
				ThumbBlockTransfer( opcode );
				break;
			case 13:
				if ( (opcode & 0x0F00) == 0x0F00 )
					ThumbSwi( opcode );
				else
					ThumbCondBranch( opcode );
				break;
			case 14:
				ThumbBranch( opcode );
				break;
			case 15:
				ThumbBranchLink( opcode );
				break;
		}

		if ( !_prefetchFlushed )
			Gprs[15] += 2;
	}

	private void ThumbShiftImm( uint opcode )
	{
		uint rd = opcode & 7;
		uint rm = (opcode >> 3) & 7;
		uint amount = (opcode >> 6) & 0x1F;
		uint op = (opcode >> 11) & 3;
		uint val = Gprs[rm];

		switch ( op )
		{
			case 0:
				if ( amount == 0 ) { Gprs[rd] = val; }
				else { FlagC = ((val >> (int)(32 - amount)) & 1) != 0; Gprs[rd] = val << (int)amount; }
				break;
			case 1:
				if ( amount == 0 ) { FlagC = (val & 0x80000000) != 0; Gprs[rd] = 0; }
				else { FlagC = ((val >> (int)(amount - 1)) & 1) != 0; Gprs[rd] = val >> (int)amount; }
				break;
			case 2:
				if ( amount == 0 ) { FlagC = (val & 0x80000000) != 0; Gprs[rd] = FlagC ? 0xFFFFFFFF : 0; }
				else { FlagC = ((val >> (int)(amount - 1)) & 1) != 0; Gprs[rd] = (uint)((int)val >> (int)amount); }
				break;
		}

		FlagN = (Gprs[rd] & 0x80000000) != 0;
		FlagZ = Gprs[rd] == 0;
	}

	private void ThumbAddSub( uint opcode )
	{
		uint rd = opcode & 7;
		uint rn = (opcode >> 3) & 7;
		bool isImm = ((opcode >> 10) & 1) != 0;
		bool isSub = ((opcode >> 9) & 1) != 0;

		uint operand = isImm ? (opcode >> 6) & 7 : Gprs[(opcode >> 6) & 7];
		uint a = Gprs[rn];

		if ( isSub )
		{
			Gprs[rd] = a - operand;
			SetSubFlags( a, operand, Gprs[rd] );
		}
		else
		{
			Gprs[rd] = a + operand;
			SetAddFlags( a, operand, Gprs[rd] );
		}
	}

	private void ThumbImmOp( uint opcode )
	{
		uint rd = (opcode >> 8) & 7;
		uint imm = opcode & 0xFF;
		uint op = (opcode >> 11) & 3;

		switch ( op )
		{
			case 0:
				Gprs[rd] = imm;
				FlagN = false;
				FlagZ = imm == 0;
				break;
			case 1:
				uint cmpResult = Gprs[rd] - imm;
				SetSubFlags( Gprs[rd], imm, cmpResult );
				break;
			case 2:
				{
					uint old = Gprs[rd];
					Gprs[rd] += imm;
					SetAddFlags( old, imm, Gprs[rd] );
				}
				break;
			case 3:
				{
					uint old = Gprs[rd];
					Gprs[rd] -= imm;
					SetSubFlags( old, imm, Gprs[rd] );
				}
				break;
		}
	}

	private void ThumbAluOp( uint opcode )
	{
		uint rd = opcode & 7;
		uint rm = (opcode >> 3) & 7;
		uint op = (opcode >> 6) & 0xF;
		uint a = Gprs[rd], b = Gprs[rm];
		uint result;

		switch ( op )
		{
			case 0x0: result = a & b; SetLogicFlags( result, FlagC ); Gprs[rd] = result; break;
			case 0x1: result = a ^ b; SetLogicFlags( result, FlagC ); Gprs[rd] = result; break;
			case 0x2:
				{
					uint shift = b & 0xFF;
					if ( shift == 0 ) { result = a; }
					else if ( shift < 32 ) { FlagC = ((a >> (int)(32 - shift)) & 1) != 0; result = a << (int)shift; }
					else if ( shift == 32 ) { FlagC = (a & 1) != 0; result = 0; }
					else { FlagC = false; result = 0; }
					FlagN = (result & 0x80000000) != 0; FlagZ = result == 0;
					Gprs[rd] = result;
					int lslCr = (int)((Gprs[15] >> 24) & 0xF); Cycles += Memory.WaitstatesNonseq16[lslCr] - Memory.WaitstatesSeq16[lslCr];
				}
				break;
			case 0x3:
				{
					uint shift = b & 0xFF;
					if ( shift == 0 ) { result = a; }
					else if ( shift < 32 ) { FlagC = ((a >> (int)(shift - 1)) & 1) != 0; result = a >> (int)shift; }
					else if ( shift == 32 ) { FlagC = (a & 0x80000000) != 0; result = 0; }
					else { FlagC = false; result = 0; }
					FlagN = (result & 0x80000000) != 0; FlagZ = result == 0;
					Gprs[rd] = result;
					int lsrCr = (int)((Gprs[15] >> 24) & 0xF); Cycles += Memory.WaitstatesNonseq16[lsrCr] - Memory.WaitstatesSeq16[lsrCr];
				}
				break;
			case 0x4:
				{
					uint shift = b & 0xFF;
					if ( shift == 0 ) { result = a; }
					else if ( shift < 32 ) { FlagC = ((a >> (int)(shift - 1)) & 1) != 0; result = (uint)((int)a >> (int)shift); }
					else { FlagC = (a & 0x80000000) != 0; result = FlagC ? 0xFFFFFFFF : 0u; }
					FlagN = (result & 0x80000000) != 0; FlagZ = result == 0;
					Gprs[rd] = result;
					int asrCr = (int)((Gprs[15] >> 24) & 0xF); Cycles += Memory.WaitstatesNonseq16[asrCr] - Memory.WaitstatesSeq16[asrCr];
				}
				break;
			case 0x5: result = a + b + (FlagC ? 1u : 0); SetAdcFlags( a, b, FlagC ); Gprs[rd] = result; break;
			case 0x6: result = a - b - (FlagC ? 0u : 1u); SetSbcFlags( a, b, FlagC ); Gprs[rd] = result; break;
			case 0x7:
				{
					uint shift = b & 0xFF;
					if ( shift == 0 ) { result = a; }
					else { shift &= 31; if ( shift == 0 ) { FlagC = (a & 0x80000000) != 0; result = a; } else { result = Ror( a, (int)shift ); FlagC = (result & 0x80000000) != 0; } }
					FlagN = (result & 0x80000000) != 0; FlagZ = result == 0;
					Gprs[rd] = result;
					int rorCr = (int)((Gprs[15] >> 24) & 0xF); Cycles += Memory.WaitstatesNonseq16[rorCr] - Memory.WaitstatesSeq16[rorCr];
				}
				break;
			case 0x8: result = a & b; SetLogicFlags( result, FlagC ); break;
			case 0x9: result = 0 - b; SetSubFlags( 0, b, result ); Gprs[rd] = result; break;
			case 0xA: result = a - b; SetSubFlags( a, b, result ); break;
			case 0xB: result = a + b; SetAddFlags( a, b, result ); break;
			case 0xC: result = a | b; SetLogicFlags( result, FlagC ); Gprs[rd] = result; break;
			case 0xD:
				result = a * b;
				FlagN = (result & 0x80000000) != 0;
				FlagZ = result == 0;
				Gprs[rd] = result;
				Cycles += Memory.MemoryStall( Gprs[15], MultiplyExtraCycles( a ) );
				{ int thumbMulCr = (int)((Gprs[15] >> 24) & 0xF); Cycles += Memory.WaitstatesNonseq16[thumbMulCr] - Memory.WaitstatesSeq16[thumbMulCr]; }
				break;
			case 0xE: result = a & ~b; SetLogicFlags( result, FlagC ); Gprs[rd] = result; break;
			case 0xF: result = ~b; SetLogicFlags( result, FlagC ); Gprs[rd] = result; break;
		}
	}

	private void ThumbHiRegBx( uint opcode )
	{
		uint op = (opcode >> 8) & 3;
		uint rd = (opcode & 7) | ((opcode >> 4) & 8);
		uint rm = (opcode >> 3) & 0xF;

		switch ( op )
		{
			case 0:
				Gprs[rd] += Gprs[rm];
				if ( rd == 15 ) _prefetchFlushed = true;
				break;
			case 1:
				{
					uint result = Gprs[rd] - Gprs[rm];
					SetSubFlags( Gprs[rd], Gprs[rm], result );
				}
				break;
			case 2:
				Gprs[rd] = Gprs[rm];
				if ( rd == 15 ) _prefetchFlushed = true;
				break;
			case 3:
				ThumbMode = (Gprs[rm] & 1) != 0;
				Gprs[15] = Gprs[rm] & ~1u;
				_prefetchFlushed = true;
				break;
		}
	}

	private void ThumbPcRelLoad( uint opcode )
	{
		uint rd = (opcode >> 8) & 7;
		uint offset = (opcode & 0xFF) << 2;
		uint addr = (Gprs[15] & ~3u) + offset;
		Gprs[rd] = Memory.Load32( addr );

		int dr = (int)((addr >> 24) & 0xF);
		int wait = Memory.WaitstatesNonseq32[dr] + 2;
		if ( dr < 8 )
			wait = Memory.MemoryStall( Gprs[15], wait );
		Cycles += wait;
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq16[cr] - Memory.WaitstatesSeq16[cr];
	}

	private void ThumbLoadStoreReg( uint opcode )
	{
		uint rd = opcode & 7;
		uint rn = (opcode >> 3) & 7;
		uint rm = (opcode >> 6) & 7;
		uint addr = Gprs[rn] + Gprs[rm];
		bool isLoad = ((opcode >> 11) & 1) != 0;
		bool isByte = ((opcode >> 10) & 1) != 0;

		if ( isLoad )
		{
			if ( isByte )
				Gprs[rd] = Memory.Load8( addr );
			else
				Gprs[rd] = ReadWordRotated( addr );
		}
		else
		{
			if ( isByte )
				Memory.Store8( addr, (byte)Gprs[rd] );
			else
				Memory.Store32( addr, Gprs[rd] );
		}

		int dr = (int)((addr >> 24) & 0xF);
		{
			int wait = isByte ? Memory.WaitstatesNonseq16[dr] : Memory.WaitstatesNonseq32[dr];
			wait += isLoad ? 2 : 1;
			if ( dr < 8 )
				wait = Memory.MemoryStall( Gprs[15], wait );
			Cycles += wait;
		}
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq16[cr] - Memory.WaitstatesSeq16[cr];
	}

	private void ThumbLoadStoreSignedHalf( uint opcode )
	{
		uint rd = opcode & 7;
		uint rn = (opcode >> 3) & 7;
		uint rm = (opcode >> 6) & 7;
		uint addr = Gprs[rn] + Gprs[rm];
		uint op = (opcode >> 10) & 3;

		switch ( op )
		{
			case 0: Memory.Store16( addr, (ushort)Gprs[rd] ); break;
			case 1: Gprs[rd] = (uint)(sbyte)Memory.Load8( addr ); break;
			case 2: Gprs[rd] = Memory.Load16( addr ); break;
			case 3:
				if ( (addr & 1) != 0 )
					Gprs[rd] = (uint)(sbyte)Memory.Load8( addr );
				else
					Gprs[rd] = (uint)(short)Memory.Load16( addr );
				break;
		}

		int dr = (int)((addr >> 24) & 0xF); bool isStore = op == 0;
		int wait = Memory.WaitstatesNonseq16[dr] + (isStore ? 1 : 2);
		if ( dr < 8 )
			wait = Memory.MemoryStall( Gprs[15], wait );
		Cycles += wait;
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq16[cr] - Memory.WaitstatesSeq16[cr];
	}

	private void ThumbLoadStoreImmWord( uint opcode )
	{
		uint top3 = (opcode >> 13) & 7;
		uint rd = opcode & 7;
		uint rn = (opcode >> 3) & 7;
		uint offset5 = (opcode >> 6) & 0x1F;
		bool isLoad = ((opcode >> 11) & 1) != 0;
		uint addr = 0;
		bool isByte = false;

		if ( top3 == 3 )
		{
			isByte = ((opcode >> 12) & 1) != 0;
			if ( isByte )
			{
				addr = Gprs[rn] + offset5;
				if ( isLoad ) Gprs[rd] = Memory.Load8( addr );
				else Memory.Store8( addr, (byte)Gprs[rd] );
			}
			else
			{
				addr = Gprs[rn] + offset5 * 4;
				if ( isLoad ) Gprs[rd] = ReadWordRotated( addr );
				else Memory.Store32( addr, Gprs[rd] );
			}
		}
		else if ( top3 == 2 )
		{
			uint rm = (opcode >> 6) & 7;
			addr = Gprs[rn] + Gprs[rm];
			isByte = ((opcode >> 10) & 1) != 0;
			if ( isByte )
			{
				if ( isLoad ) Gprs[rd] = Memory.Load8( addr );
				else Memory.Store8( addr, (byte)Gprs[rd] );
			}
			else
			{
				if ( isLoad ) Gprs[rd] = ReadWordRotated( addr );
				else Memory.Store32( addr, Gprs[rd] );
			}
		}

		int dr = (int)((addr >> 24) & 0xF);
		int wait = isByte ? Memory.WaitstatesNonseq16[dr] : Memory.WaitstatesNonseq32[dr];
		wait += isLoad ? 2 : 1;
		if ( dr < 8 )
			wait = Memory.MemoryStall( Gprs[15], wait );
		Cycles += wait;
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq16[cr] - Memory.WaitstatesSeq16[cr];
	}

	private void ThumbLoadStoreHalf( uint opcode )
	{
		uint rd = opcode & 7;
		uint rn = (opcode >> 3) & 7;
		uint offset = ((opcode >> 6) & 0x1F) << 1;
		uint addr = Gprs[rn] + offset;
		bool isLoad = ((opcode >> 11) & 1) != 0;

		if ( isLoad )
			Gprs[rd] = Memory.Load16( addr );
		else
			Memory.Store16( addr, (ushort)Gprs[rd] );

		int dr = (int)((addr >> 24) & 0xF);
		int wait = Memory.WaitstatesNonseq16[dr] + (isLoad ? 2 : 1);
		if ( dr < 8 )
			wait = Memory.MemoryStall( Gprs[15], wait );
		Cycles += wait;
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq16[cr] - Memory.WaitstatesSeq16[cr];
	}

	private void ThumbSpRelLoadStore( uint opcode )
	{
		uint rd = (opcode >> 8) & 7;
		uint offset = (opcode & 0xFF) << 2;
		uint addr = Gprs[13] + offset;
		bool isLoad = ((opcode >> 11) & 1) != 0;

		if ( isLoad )
			Gprs[rd] = ReadWordRotated( addr );
		else
			Memory.Store32( addr, Gprs[rd] );

		int dr = (int)((addr >> 24) & 0xF);
		int wait = Memory.WaitstatesNonseq32[dr] + (isLoad ? 2 : 1);
		if ( dr < 8 )
			wait = Memory.MemoryStall( Gprs[15], wait );
		Cycles += wait;
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq16[cr] - Memory.WaitstatesSeq16[cr];
	}

	private void ThumbLoadAddress( uint opcode )
	{
		uint rd = (opcode >> 8) & 7;
		uint offset = (opcode & 0xFF) << 2;
		bool useSP = ((opcode >> 11) & 1) != 0;

		if ( useSP )
			Gprs[rd] = Gprs[13] + offset;
		else
			Gprs[rd] = (Gprs[15] & ~3u) + offset;
	}

	private void ThumbSpOffset( uint opcode )
	{
		uint offset = (opcode & 0x7F) << 2;
		if ( (opcode & 0x80) != 0 )
			Gprs[13] -= offset;
		else
			Gprs[13] += offset;
	}

	private void ThumbPushPop( uint opcode )
	{
		bool isLoad = ((opcode >> 11) & 1) != 0;
		bool extraReg = ((opcode >> 8) & 1) != 0;
		byte regList = (byte)(opcode & 0xFF);
		int count = BitCount( regList ) + (extraReg ? 1 : 0);
		int instructionRegion = (int)((Gprs[15] >> 24) & 0xF);

		if ( isLoad )
		{
			uint addr = Gprs[13];
			for ( int i = 0; i < 8; i++ )
			{
				if ( (regList & (1 << i)) != 0 )
				{
					Gprs[i] = Memory.Load32( addr );
					addr += 4;
				}
			}
			if ( extraReg )
			{
				Gprs[15] = Memory.Load32( addr );
				addr += 4;
				_prefetchFlushed = true;
			}
			Gprs[13] = addr;
		}
		else
		{
			uint addr = Gprs[13] - (uint)(count * 4);
			Gprs[13] = addr;
			for ( int i = 0; i < 8; i++ )
			{
				if ( (regList & (1 << i)) != 0 )
				{
					Memory.Store32( addr, Gprs[i] );
					addr += 4;
				}
			}
			if ( extraReg )
			{
				Memory.Store32( addr, Gprs[14] );
			}
		}

		int dr = (int)((Gprs[13] >> 24) & 0xF);
		if ( count > 0 )
		{
			int firstWait = Memory.WaitstatesNonseq32[dr];
			int seqWait = Memory.WaitstatesSeq32[dr];
			int blockWait = seqWait - firstWait + count * (seqWait + 1) + (isLoad ? 1 : 0);
			if ( dr < 8 )
				blockWait = Memory.MemoryStall( Gprs[15], blockWait );
			Cycles += blockWait;
		}
		Cycles += Memory.WaitstatesNonseq16[instructionRegion] - Memory.WaitstatesSeq16[instructionRegion];
	}

	private void ThumbBlockTransfer( uint opcode )
	{
		uint rn = (opcode >> 8) & 7;
		bool isLoad = ((opcode >> 11) & 1) != 0;
		byte regList = (byte)(opcode & 0xFF);
		uint addr = Gprs[rn];
		int instructionRegion = (int)((Gprs[15] >> 24) & 0xF);

		if ( regList == 0 )
		{
			if ( isLoad )
			{
				Gprs[15] = Memory.Load32( addr );
				_prefetchFlushed = true;
			}
			else
			{
				Memory.Store32( addr, Gprs[15] + 2 );
			}
			Gprs[rn] += 0x40;

			int dr0 = (int)((addr >> 24) & 0xF);
			int seqWait = Memory.WaitstatesSeq32[dr0];
			int emptyWait = 2 * seqWait - Memory.WaitstatesNonseq32[dr0] + 16 + (isLoad ? 1 : 0);
			if ( dr0 < 8 )
				emptyWait = Memory.MemoryStall( Gprs[15], emptyWait );
			Cycles += emptyWait;
			Cycles += Memory.WaitstatesNonseq16[instructionRegion] - Memory.WaitstatesSeq16[instructionRegion];
			return;
		}

		int count = BitCount( regList );
		uint startAddr = addr;

		for ( int i = 0; i < 8; i++ )
		{
			if ( (regList & (1 << i)) == 0 ) continue;

			if ( isLoad )
				Gprs[i] = Memory.Load32( addr );
			else
				Memory.Store32( addr, Gprs[i] );
			addr += 4;
		}

		if ( !isLoad || (regList & (1 << (int)rn)) == 0 )
			Gprs[rn] = addr;

		{
			int blockRegion = (int)((startAddr >> 24) & 0xF);
			int firstWait = Memory.WaitstatesNonseq32[blockRegion];
			int seqWait = Memory.WaitstatesSeq32[blockRegion];
			int blockWait = seqWait - firstWait + count * (seqWait + 1) + (isLoad ? 1 : 0);
			uint endAddr = startAddr + (uint)(count * 4);
			int endRegion = (int)((endAddr >> 24) & 0xF);
			if ( endRegion < 8 )
				blockWait = Memory.MemoryStall( Gprs[15], blockWait );
			Cycles += blockWait;
		}
		Cycles += Memory.WaitstatesNonseq16[instructionRegion] - Memory.WaitstatesSeq16[instructionRegion];
	}

	private void ThumbCondBranch( uint opcode )
	{
		uint cond = (opcode >> 8) & 0xF;
		if ( !CheckCondition( cond ) ) return;

		int offset = (sbyte)(opcode & 0xFF);
		offset <<= 1;
		Gprs[15] = (uint)(Gprs[15] + offset);
		_prefetchFlushed = true;
	}

	private void ThumbBranch( uint opcode )
	{
		int offset = (int)(opcode & 0x7FF);
		if ( (offset & 0x400) != 0 ) offset |= unchecked((int)0xFFFFF800);
		offset <<= 1;
		Gprs[15] = (uint)(Gprs[15] + offset);
		_prefetchFlushed = true;
	}

	private void ThumbBranchLink( uint opcode )
	{
		bool isSecond = ((opcode >> 11) & 1) != 0;

		if ( !isSecond )
		{
			int offset = (int)(opcode & 0x7FF);
			if ( (offset & 0x400) != 0 ) offset |= unchecked((int)0xFFFFF800);
			Gprs[14] = (uint)(Gprs[15] + (offset << 12));
		}
		else
		{
			uint temp = Gprs[14] + ((opcode & 0x7FF) << 1);
			Gprs[14] = (Gprs[15] - 2) | 1;
			Gprs[15] = temp;
			_prefetchFlushed = true;
		}
	}

	private void ThumbSwi( uint opcode )
	{
		uint comment = opcode & 0xFF;
		if ( Gba.Bios.HandleSwi( comment ) )
			return;

		uint savedCpsr = GetCpsrRaw();
		SetPrivilegeMode( PrivilegeMode.Supervisor );
		SetSpsr( savedCpsr );
		Gprs[14] = Gprs[15] - 2;
		IrqDisable = true;
		ThumbMode = false;
		Gprs[15] = GbaConstants.BaseSwi;
		_prefetchFlushed = true;
	}
}
