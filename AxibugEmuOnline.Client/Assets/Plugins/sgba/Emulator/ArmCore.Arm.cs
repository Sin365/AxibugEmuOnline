namespace sGBA;

public partial class ArmCore
{
	private void ExecuteArm()
	{
		uint opcode = _prefetch0;
		_prefetch0 = _prefetch1;
		_prefetch1 = Memory.Load32( Gprs[15] );
		OpenBusPrefetch = _prefetch1;
		Cycles += 1 + Memory.WaitstatesSeq32[(Gprs[15] >> 24) & 0xF];

		uint cond = opcode >> 28;
		if ( cond != 0xE && !CheckCondition( cond ) )
		{
			Gprs[15] += 4;
			return;
		}

		uint group = (opcode >> 25) & 7;
		switch ( group )
		{
			case 0:
				if ( (opcode & 0x0FC000F0) == 0x00000090 )
					ArmMultiply( opcode );
				else if ( (opcode & 0x0F8000F0) == 0x00800090 )
					ArmMultiplyLong( opcode );
				else if ( (opcode & 0x0FB00FF0) == 0x01000090 )
					ArmSwap( opcode );
				else if ( (opcode & 0x0E000090) == 0x00000090 && (opcode & 0x00000060) != 0 )
					ArmHalfwordTransfer( opcode );
				else if ( (opcode & 0x0FBF0FFF) == 0x010F0000 )
					ArmMrs( opcode );
				else if ( (opcode & 0x0FFFFFF0) == 0x012FFF10 )
					ArmBx( opcode );
				else if ( (opcode & 0x0DB0F000) == 0x0120F000 )
					ArmMsr( opcode );
				else
					ArmDataProcessing( opcode );
				break;

			case 1:
				if ( (opcode & 0x0FB0F000) == 0x0320F000 )
					ArmMsr( opcode );
				else
					ArmDataProcessing( opcode );
				break;

			case 2:
				ArmSingleTransfer( opcode );
				break;

			case 3:
				if ( (opcode & 0x10) != 0 )
				{
					break;
				}
				ArmSingleTransfer( opcode );
				break;

			case 4:
				ArmBlockTransfer( opcode );
				break;

			case 5:
				ArmBranch( opcode );
				break;

			case 6:
				break;

			case 7:
				if ( (opcode & 0x0F000000) == 0x0F000000 )
					ArmSwi( opcode );
				break;
		}

		if ( !_prefetchFlushed )
			Gprs[15] += 4;
	}

	private void ArmDataProcessing( uint opcode )
	{
		uint op = (opcode >> 21) & 0xF;
		bool setFlags = ((opcode >> 20) & 1) != 0;
		uint rn = (opcode >> 16) & 0xF;
		uint rd = (opcode >> 12) & 0xF;

		uint operand1 = Gprs[rn];
		bool isRegShift = ((opcode >> 25) & 1) == 0 && ((opcode >> 4) & 1) != 0;
		if ( rn == 15 && isRegShift ) operand1 += 4;

		uint operand2;
		bool shiftCarry = FlagC;

		if ( ((opcode >> 25) & 1) != 0 )
		{
			uint imm = opcode & 0xFF;
			uint rotate = ((opcode >> 8) & 0xF) * 2;
			if ( rotate != 0 )
			{
				operand2 = Ror( imm, (int)rotate );
				shiftCarry = (operand2 & 0x80000000) != 0;
			}
			else
			{
				operand2 = imm;
			}
		}
		else
		{
			operand2 = GetShifterOperand( opcode, out shiftCarry );
		}

		uint result;
		bool writeDest = true;

		switch ( op )
		{
			case 0x0:
				result = operand1 & operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				break;
			case 0x1:
				result = operand1 ^ operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				break;
			case 0x2:
				result = operand1 - operand2;
				if ( setFlags ) SetSubFlags( operand1, operand2, result );
				break;
			case 0x3:
				result = operand2 - operand1;
				if ( setFlags ) SetSubFlags( operand2, operand1, result );
				break;
			case 0x4:
				result = operand1 + operand2;
				if ( setFlags ) SetAddFlags( operand1, operand2, result );
				break;
			case 0x5:
				result = operand1 + operand2 + (FlagC ? 1u : 0u);
				if ( setFlags ) SetAdcFlags( operand1, operand2, FlagC );
				break;
			case 0x6:
				result = operand1 - operand2 - (FlagC ? 0u : 1u);
				if ( setFlags ) SetSbcFlags( operand1, operand2, FlagC );
				break;
			case 0x7:
				result = operand2 - operand1 - (FlagC ? 0u : 1u);
				if ( setFlags ) SetSbcFlags( operand2, operand1, FlagC );
				break;
			case 0x8:
				result = operand1 & operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				writeDest = false;
				break;
			case 0x9:
				result = operand1 ^ operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				writeDest = false;
				break;
			case 0xA:
				result = operand1 - operand2;
				if ( setFlags ) SetSubFlags( operand1, operand2, result );
				writeDest = false;
				break;
			case 0xB:
				result = operand1 + operand2;
				if ( setFlags ) SetAddFlags( operand1, operand2, result );
				writeDest = false;
				break;
			case 0xC:
				result = operand1 | operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				break;
			case 0xD:
				result = operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				break;
			case 0xE:
				result = operand1 & ~operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				break;
			case 0xF:
				result = ~operand2;
				if ( setFlags ) SetLogicFlags( result, shiftCarry );
				break;
			default:
				result = 0;
				break;
		}

		if ( writeDest )
		{
			Gprs[rd] = result;
			if ( rd == 15 )
			{
				if ( setFlags && PrivilegeMode != PrivilegeMode.User && PrivilegeMode != PrivilegeMode.System )
				{
					uint spsr = GetSpsr();
					SetCpsr( spsr );
				}
				_prefetchFlushed = true;
			}
		}
	}

	private uint GetShifterOperand( uint opcode, out bool carryOut )
	{
		uint rm = opcode & 0xF;
		uint val = Gprs[rm];

		uint shiftType = (opcode >> 5) & 3;
		uint shiftAmount;
		bool regShift = ((opcode >> 4) & 1) != 0;

		if ( rm == 15 && regShift ) val += 4;

		if ( regShift )
		{
			uint rs = (opcode >> 8) & 0xF;
			shiftAmount = Gprs[rs] & 0xFF;
			Cycles += 1;

			if ( shiftAmount == 0 )
			{
				carryOut = FlagC;
				return val;
			}

			switch ( shiftType )
			{
				case 0:
					if ( shiftAmount < 32 ) { carryOut = ((val >> (int)(32 - shiftAmount)) & 1) != 0; return val << (int)shiftAmount; }
					if ( shiftAmount == 32 ) { carryOut = (val & 1) != 0; return 0; }
					carryOut = false; return 0;
				case 1:
					if ( shiftAmount < 32 ) { carryOut = ((val >> (int)(shiftAmount - 1)) & 1) != 0; return val >> (int)shiftAmount; }
					if ( shiftAmount == 32 ) { carryOut = (val & 0x80000000) != 0; return 0; }
					carryOut = false; return 0;
				case 2:
					if ( shiftAmount >= 32 ) { carryOut = (val & 0x80000000) != 0; return carryOut ? 0xFFFFFFFF : 0; }
					carryOut = ((val >> (int)(shiftAmount - 1)) & 1) != 0;
					return (uint)((int)val >> (int)shiftAmount);
				case 3:
					shiftAmount &= 31;
					if ( shiftAmount == 0 ) { carryOut = (val & 0x80000000) != 0; return val; }
					uint ror = Ror( val, (int)shiftAmount );
					carryOut = (ror & 0x80000000) != 0;
					return ror;
			}
		}
		else
		{
			shiftAmount = (opcode >> 7) & 0x1F;

			switch ( shiftType )
			{
				case 0:
					if ( shiftAmount == 0 ) { carryOut = FlagC; return val; }
					carryOut = ((val >> (int)(32 - shiftAmount)) & 1) != 0;
					return val << (int)shiftAmount;
				case 1:
					if ( shiftAmount == 0 ) { carryOut = (val & 0x80000000) != 0; return 0; }
					carryOut = ((val >> (int)(shiftAmount - 1)) & 1) != 0;
					return val >> (int)shiftAmount;
				case 2:
					if ( shiftAmount == 0 ) { carryOut = (val & 0x80000000) != 0; return carryOut ? 0xFFFFFFFF : 0; }
					carryOut = ((val >> (int)(shiftAmount - 1)) & 1) != 0;
					return (uint)((int)val >> (int)shiftAmount);
				case 3:
					if ( shiftAmount == 0 )
					{
						carryOut = (val & 1) != 0;
						return (FlagC ? 0x80000000u : 0) | (val >> 1);
					}
					uint ror = Ror( val, (int)shiftAmount );
					carryOut = (ror & 0x80000000) != 0;
					return ror;
			}
		}

		carryOut = FlagC;
		return val;
	}

	private void ArmMultiply( uint opcode )
	{
		uint rd = (opcode >> 16) & 0xF;
		uint rn = (opcode >> 12) & 0xF;
		uint rs = (opcode >> 8) & 0xF;
		uint rm = opcode & 0xF;
		bool accumulate = ((opcode >> 21) & 1) != 0;
		bool setFlags = ((opcode >> 20) & 1) != 0;

		uint rsVal = Gprs[rs];
		uint result = Gprs[rm] * rsVal;
		if ( accumulate ) result += Gprs[rn];

		Gprs[rd] = result;

		if ( setFlags )
		{
			FlagN = (result & 0x80000000) != 0;
			FlagZ = result == 0;
		}

		int mulWait = MultiplyExtraCycles( rsVal ) + (accumulate ? 1 : 0);
		Cycles += Memory.MemoryStall( Gprs[15], mulWait );
		int mulCr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq32[mulCr] - Memory.WaitstatesSeq32[mulCr];
	}

	private void ArmMultiplyLong( uint opcode )
	{
		uint rdHi = (opcode >> 16) & 0xF;
		uint rdLo = (opcode >> 12) & 0xF;
		uint rs = (opcode >> 8) & 0xF;
		uint rm = opcode & 0xF;
		bool isSigned = ((opcode >> 22) & 1) != 0;
		bool accumulate = ((opcode >> 21) & 1) != 0;
		bool setFlags = ((opcode >> 20) & 1) != 0;

		long result;
		if ( isSigned )
			result = (long)(int)Gprs[rm] * (int)Gprs[rs];
		else
			result = (long)((ulong)Gprs[rm] * Gprs[rs]);

		if ( accumulate )
			result += (long)(((ulong)Gprs[rdHi] << 32) | Gprs[rdLo]);

		Gprs[rdLo] = (uint)result;
		Gprs[rdHi] = (uint)(result >> 32);

		if ( setFlags )
		{
			FlagN = (Gprs[rdHi] & 0x80000000) != 0;
			FlagZ = result == 0;
		}

		int longWait = accumulate ? 2 : 1;
		if ( isSigned )
			longWait += MultiplyExtraCycles( Gprs[rs] );
		else
			longWait += MultiplyExtraCyclesUnsigned( Gprs[rs] );
		Cycles += Memory.MemoryStall( Gprs[15], longWait );
		int longMulCr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq32[longMulCr] - Memory.WaitstatesSeq32[longMulCr];
	}

	private void ArmSwap( uint opcode )
	{
		uint rn = (opcode >> 16) & 0xF;
		uint rd = (opcode >> 12) & 0xF;
		uint rm = opcode & 0xF;
		bool byteSwap = ((opcode >> 22) & 1) != 0;

		uint addr = Gprs[rn];
		if ( byteSwap )
		{
			byte tmp = Memory.Load8( addr );
			Memory.Store8( addr, (byte)Gprs[rm] );
			Gprs[rd] = tmp;
		}
		else
		{
			uint tmp = ReadWordRotated( addr );
			Memory.Store32( addr, Gprs[rm] );
			Gprs[rd] = tmp;
		}

		int dr = (int)((addr >> 24) & 0xF);
		int swpWait = byteSwap ? Memory.WaitstatesNonseq16[dr] : Memory.WaitstatesNonseq32[dr];
		swpWait = swpWait * 2 + 3;
		if ( dr < 8 )
			swpWait = Memory.MemoryStall( Gprs[15], swpWait );
		Cycles += swpWait;
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq32[cr] - Memory.WaitstatesSeq32[cr];
	}

	private void ArmHalfwordTransfer( uint opcode )
	{
		uint rn = (opcode >> 16) & 0xF;
		uint rd = (opcode >> 12) & 0xF;
		bool preIndex = ((opcode >> 24) & 1) != 0;
		bool addOffset = ((opcode >> 23) & 1) != 0;
		bool immediate = ((opcode >> 22) & 1) != 0;
		bool writeBack = ((opcode >> 21) & 1) != 0;
		bool isLoad = ((opcode >> 20) & 1) != 0;
		uint sh = (opcode >> 5) & 3;

		uint offset;
		if ( immediate )
			offset = ((opcode >> 4) & 0xF0) | (opcode & 0xF);
		else
			offset = Gprs[opcode & 0xF];

		uint addr = Gprs[rn];

		if ( preIndex )
			addr = addOffset ? addr + offset : addr - offset;

		switch ( sh )
		{
			case 1:
				if ( isLoad )
				{
					Gprs[rd] = Memory.Load16( addr );
					if ( (addr & 1) != 0 ) Gprs[rd] = Ror( Gprs[rd], 8 );
				}
				else
				{
					uint val = Gprs[rd];
					if ( rd == 15 ) val += 4;
					Memory.Store16( addr, (ushort)val );
				}
				break;
			case 2:
				if ( isLoad )
					Gprs[rd] = (uint)(sbyte)Memory.Load8( addr );
				break;
			case 3:
				if ( isLoad )
				{
					if ( (addr & 1) != 0 )
						Gprs[rd] = (uint)(sbyte)Memory.Load8( addr );
					else
						Gprs[rd] = (uint)(short)Memory.Load16( addr );
				}
				break;
		}

		if ( !preIndex )
		{
			uint newAddr = addOffset ? Gprs[rn] + offset : Gprs[rn] - offset;
			Gprs[rn] = newAddr;
		}
		else if ( writeBack )
		{
			Gprs[rn] = addr;
		}

		if ( isLoad && rd == 15 ) _prefetchFlushed = true;

		int dataRegion = (int)((addr >> 24) & 0xF);
		{
			int wait = Memory.WaitstatesNonseq16[dataRegion] + (isLoad ? 2 : 1);
			if ( dataRegion < 8 )
				wait = Memory.MemoryStall( Gprs[15], wait );
			Cycles += wait;
		}
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq32[cr] - Memory.WaitstatesSeq32[cr];
	}

	private void ArmSingleTransfer( uint opcode )
	{
		uint rn = (opcode >> 16) & 0xF;
		uint rd = (opcode >> 12) & 0xF;
		bool immediate = ((opcode >> 25) & 1) == 0;
		bool preIndex = ((opcode >> 24) & 1) != 0;
		bool addOffset = ((opcode >> 23) & 1) != 0;
		bool byteTransfer = ((opcode >> 22) & 1) != 0;
		bool writeBack = ((opcode >> 21) & 1) != 0;
		bool isLoad = ((opcode >> 20) & 1) != 0;

		uint offset;
		if ( immediate )
		{
			offset = opcode & 0xFFF;
		}
		else
		{
			uint rm = opcode & 0xF;
			uint shiftType = (opcode >> 5) & 3;
			uint shiftAmount = (opcode >> 7) & 0x1F;
			offset = ApplyShift( Gprs[rm], shiftType, shiftAmount );
		}

		uint addr = Gprs[rn];
		if ( rn == 15 ) addr = Gprs[15];

		if ( preIndex )
			addr = addOffset ? addr + offset : addr - offset;

		if ( isLoad )
		{
			if ( byteTransfer )
				Gprs[rd] = Memory.Load8( addr );
			else
				Gprs[rd] = ReadWordRotated( addr );

			if ( rd == 15 ) _prefetchFlushed = true;
		}
		else
		{
			uint val = Gprs[rd];
			if ( rd == 15 ) val += 4;

			if ( byteTransfer )
				Memory.Store8( addr, (byte)val );
			else
				Memory.Store32( addr, val );
		}

		if ( !preIndex )
		{
			Gprs[rn] = addOffset ? Gprs[rn] + offset : Gprs[rn] - offset;
		}
		else if ( writeBack )
		{
			Gprs[rn] = addr;
		}

		int dataRegion = (int)((addr >> 24) & 0xF);
		{
			int wait = byteTransfer ? Memory.WaitstatesNonseq16[dataRegion] : Memory.WaitstatesNonseq32[dataRegion];
			wait += isLoad ? 2 : 1;
			if ( dataRegion < 8 )
				wait = Memory.MemoryStall( Gprs[15], wait );
			Cycles += wait;
		}
		int cr = (int)((Gprs[15] >> 24) & 0xF);
		Cycles += Memory.WaitstatesNonseq32[cr] - Memory.WaitstatesSeq32[cr];
	}

	private void ArmBlockTransfer( uint opcode )
	{
		uint rn = (opcode >> 16) & 0xF;
		bool preIndex = ((opcode >> 24) & 1) != 0;
		bool addOffset = ((opcode >> 23) & 1) != 0;
		bool psr = ((opcode >> 22) & 1) != 0;
		bool writeBack = ((opcode >> 21) & 1) != 0;
		bool isLoad = ((opcode >> 20) & 1) != 0;
		ushort regList = (ushort)(opcode & 0xFFFF);
		int instructionRegion = (int)((Gprs[15] >> 24) & 0xF);

		if ( regList == 0 )
		{
			uint origAddr = Gprs[rn];
			if ( isLoad )
			{
				Gprs[15] = Memory.Load32( Gprs[rn] );
				_prefetchFlushed = true;
			}
			else
			{
				Memory.Store32( Gprs[rn], Gprs[15] + 4 );
			}
			if ( addOffset )
				Gprs[rn] += 0x40;
			else
				Gprs[rn] -= 0x40;

			int dr0 = (int)((origAddr >> 24) & 0xF);
			int seqWait = Memory.WaitstatesSeq32[dr0];
			int emptyWait = 2 * seqWait - Memory.WaitstatesNonseq32[dr0] + 16 + (isLoad ? 1 : 0);
			if ( dr0 < 8 )
				emptyWait = Memory.MemoryStall( Gprs[15], emptyWait );
			Cycles += emptyWait;
			Cycles += Memory.WaitstatesNonseq32[instructionRegion] - Memory.WaitstatesSeq32[instructionRegion];
			return;
		}

		int count = BitCount( regList );
		uint baseAddr = Gprs[rn];

		uint startAddr;
		if ( addOffset )
			startAddr = preIndex ? baseAddr + 4 : baseAddr;
		else
			startAddr = preIndex ? baseAddr - (uint)(count * 4) : baseAddr - (uint)(count * 4) + 4;

		uint addr = startAddr;

		bool useUserBank = psr && !(isLoad && (regList & (1 << 15)) != 0);

		for ( int i = 0; i < 16; i++ )
		{
			if ( (regList & (1 << i)) == 0 ) continue;

			if ( isLoad )
			{
				uint value = Memory.Load32( addr );
				if ( useUserBank && i >= 8 && i <= 14 )
					SetUserReg( i, value );
				else
					Gprs[i] = value;
				if ( i == 15 )
				{
					_prefetchFlushed = true;
					if ( psr && PrivilegeMode != PrivilegeMode.User && PrivilegeMode != PrivilegeMode.System )
					{
						uint spsr = GetSpsr();
						SetCpsr( spsr );
					}
				}
			}
			else
			{
				uint val;
				if ( useUserBank && i >= 8 && i <= 14 )
					val = GetUserReg( i );
				else
					val = Gprs[i];
				if ( i == 15 ) val += 4;
				Memory.Store32( addr, val );
			}
			addr += 4;
		}

		if ( writeBack && !(isLoad && (regList & (1 << (int)rn)) != 0) )
		{
			if ( addOffset )
				Gprs[rn] = baseAddr + (uint)(count * 4);
			else
				Gprs[rn] = baseAddr - (uint)(count * 4);
		}

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
		Cycles += Memory.WaitstatesNonseq32[instructionRegion] - Memory.WaitstatesSeq32[instructionRegion];
	}

	private void ArmBranch( uint opcode )
	{
		bool link = ((opcode >> 24) & 1) != 0;
		int offset = (int)(opcode & 0x00FFFFFF);
		if ( (offset & 0x800000) != 0 )
			offset |= unchecked((int)0xFF000000);
		offset <<= 2;

		if ( link )
			Gprs[14] = Gprs[15] - 4;

		Gprs[15] = (uint)(Gprs[15] + offset);
		_prefetchFlushed = true;
	}

	private void ArmBx( uint opcode )
	{
		uint rm = opcode & 0xF;
		uint addr = Gprs[rm];
		ThumbMode = (addr & 1) != 0;
		Gprs[15] = addr & ~1u;
		_prefetchFlushed = true;
	}

	private void ArmSwi( uint opcode )
	{
		uint comment = (opcode >> 16) & 0xFF;
		if ( Gba.Bios.HandleSwi( comment ) )
			return;

		uint savedCpsr = GetCpsrRaw();
		SetPrivilegeMode( PrivilegeMode.Supervisor );
		SetSpsr( savedCpsr );
		Gprs[14] = Gprs[15] - 4;
		IrqDisable = true;
		ThumbMode = false;
		Gprs[15] = GbaConstants.BaseSwi;
		_prefetchFlushed = true;
	}

	private void ArmMrs( uint opcode )
	{
		uint rd = (opcode >> 12) & 0xF;
		bool useSPSR = ((opcode >> 22) & 1) != 0;
		Gprs[rd] = useSPSR ? GetSpsr() : GetCpsrRaw();
	}

	private void ArmMsr( uint opcode )
	{
		bool useSPSR = ((opcode >> 22) & 1) != 0;
		uint mask = 0;
		if ( (opcode & 0x00080000u) != 0 ) mask |= 0xFF000000u;
		if ( (opcode & 0x00010000u) != 0 ) mask |= 0x000000FFu;

		uint operand;
		if ( ((opcode >> 25) & 1) != 0 )
		{
			uint imm = opcode & 0xFF;
			uint rotate = ((opcode >> 8) & 0xF) * 2;
			operand = Ror( imm, (int)rotate );
		}
		else
		{
			operand = Gprs[opcode & 0xF];
		}

		if ( useSPSR )
		{
			mask &= PsrUserMask | PsrPrivMask | PsrStateMask;
			uint spsr = GetSpsr();
			SetSpsr( (spsr & ~mask) | (operand & mask) | 0x00000010u );
		}
		else
		{
			if ( (mask & PsrUserMask) != 0 )
			{
				FlagN = (operand & 0x80000000u) != 0;
				FlagZ = (operand & 0x40000000u) != 0;
				FlagC = (operand & 0x20000000u) != 0;
				FlagV = (operand & 0x10000000u) != 0;
			}
			if ( (mask & PsrStateMask) != 0 )
			{
				ThumbMode = (operand & PsrStateMask) != 0;
			}
			if ( PrivilegeMode != PrivilegeMode.User && (mask & PsrPrivMask) != 0 )
			{
				SetPrivilegeMode( (PrivilegeMode)((operand & 0x0Fu) | 0x10u) );
				IrqDisable = (operand & 0x80u) != 0;
				FiqDisable = (operand & 0x40u) != 0;
			}

			Gba.Io.TestIrq();

			if ( ThumbMode )
				_prefetchFlushed = true;
			else
			{
				_prefetch0 = Memory.Load32( Gprs[15] - 4 );
				_prefetch1 = Memory.Load32( Gprs[15] );
				OpenBusPrefetch = _prefetch1;
			}
		}
	}
}
