namespace sGBA;

public partial class GbaBios
{
	private void Div()
	{
		int num = (int)Gba.Cpu.Registers[0];
		int den = (int)Gba.Cpu.Registers[1];
		if ( den == 0 )
		{
			Gba.Cpu.Registers[0] = (uint)(num < 0 ? -1 : 1);
			Gba.Cpu.Registers[1] = (uint)num;
			Gba.Cpu.Registers[3] = 1;
		}
		else if ( den == -1 && num == int.MinValue )
		{
			Gba.Cpu.Registers[0] = 0x80000000u;
			Gba.Cpu.Registers[1] = 0;
			Gba.Cpu.Registers[3] = 0x80000000u;
		}
		else
		{
			int result = num / den;
			int remainder = num % den;
			Gba.Cpu.Registers[0] = (uint)result;
			Gba.Cpu.Registers[1] = (uint)remainder;
			Gba.Cpu.Registers[3] = result < 0 ? (uint)-result : (uint)result;
		}

		int loops = Clz32( (uint)den ) - Clz32( (uint)num );
		if ( loops < 1 ) loops = 1;
		BiosStall = 4 + 13 * loops + 7;
	}

	private void DivArm()
	{
		(Gba.Cpu.Registers[1], Gba.Cpu.Registers[0]) = (Gba.Cpu.Registers[0], Gba.Cpu.Registers[1]);
		Div();
	}

	private void Sqrt()
	{
		uint val = Gba.Cpu.Registers[0];
		Gba.Cpu.Registers[0] = SqrtWithCycles( val, out int cycles );
		BiosStall = cycles;
	}

	private static uint SqrtWithCycles( uint x, out int cycles )
	{
		if ( x == 0 ) { cycles = 53; return 0; }

		int currentCycles = 15;
		uint upper = x;
		uint bound = 1;

		while ( bound < upper )
		{
			upper >>= 1;
			bound <<= 1;
			currentCycles += 6;
		}

		while ( true )
		{
			currentCycles += 6;
			upper = x;
			uint accum = 0;
			uint lower = bound;

			while ( true )
			{
				currentCycles += 5;
				uint oldLower = lower;
				if ( lower <= upper >> 1 ) lower <<= 1;
				if ( oldLower >= upper >> 1 ) break;
			}

			while ( true )
			{
				currentCycles += 8;
				accum <<= 1;
				if ( upper >= lower )
				{
					++accum;
					upper -= lower;
				}
				if ( lower == bound ) break;
				lower >>= 1;
			}

			uint oldBound = bound;
			bound += accum;
			bound >>= 1;
			if ( bound >= oldBound )
			{
				bound = oldBound;
				break;
			}
		}

		cycles = currentCycles;
		return bound;
	}

	private void ArcTan()
	{
		int i = (int)Gba.Cpu.Registers[0];
		short result = ArcTanCore( i, out int a, out int b, out int cycles );
		Gba.Cpu.Registers[0] = (uint)result;
		Gba.Cpu.Registers[1] = (uint)a;
		Gba.Cpu.Registers[3] = (uint)b;
		BiosStall = cycles;
	}

	private short ArcTanCore( int i, out int a, out int b, out int cycles )
	{
		int currentCycles = 37;
		currentCycles += MulWait( i * i );
		a = -((i * i) >> 14);
		currentCycles += MulWait( 0xA9 * a );
		b = ((0xA9 * a) >> 14) + 0x390;
		currentCycles += MulWait( b * a );
		b = ((b * a) >> 14) + 0x91C;
		currentCycles += MulWait( b * a );
		b = ((b * a) >> 14) + 0xFB6;
		currentCycles += MulWait( b * a );
		b = ((b * a) >> 14) + 0x16AA;
		currentCycles += MulWait( b * a );
		b = ((b * a) >> 14) + 0x2081;
		currentCycles += MulWait( b * a );
		b = ((b * a) >> 14) + 0x3651;
		currentCycles += MulWait( b * a );
		b = ((b * a) >> 14) + 0xA2F9;
		cycles = currentCycles;
		return (short)((i * b) >> 16);
	}

	private void ArcTan2()
	{
		int x = (int)Gba.Cpu.Registers[0];
		int y = (int)Gba.Cpu.Registers[1];
		short angle;

		if ( y == 0 )
		{
			BiosStall = 11;
			angle = (short)(x >= 0 ? 0 : unchecked((short)0x8000));
		}
		else if ( x == 0 )
		{
			BiosStall = 11;
			angle = (short)(y >= 0 ? 0x4000 : unchecked((short)0xC000));
		}
		else if ( y >= 0 )
		{
			if ( x >= 0 )
			{
				if ( x >= y )
				{
					angle = ArcTanCore( (y << 14) / x, out int a, out _, out int cycles );
					Gba.Cpu.Registers[1] = (uint)a;
					BiosStall = cycles;
				}
				else
				{
					angle = (short)(0x4000 - ArcTanCore( (x << 14) / y, out int a, out _, out int cycles ));
					Gba.Cpu.Registers[1] = (uint)a;
					BiosStall = cycles;
				}
			}
			else if ( -x >= y )
			{
				angle = (short)(ArcTanCore( (y << 14) / x, out int a, out _, out int cycles ) + 0x8000);
				Gba.Cpu.Registers[1] = (uint)a;
				BiosStall = cycles;
			}
			else
			{
				angle = (short)(0x4000 - ArcTanCore( (x << 14) / y, out int a, out _, out int cycles ));
				Gba.Cpu.Registers[1] = (uint)a;
				BiosStall = cycles;
			}
		}
		else
		{
			if ( x <= 0 )
			{
				if ( -x > -y )
				{
					angle = (short)(ArcTanCore( (y << 14) / x, out int a, out _, out int cycles ) + 0x8000);
					Gba.Cpu.Registers[1] = (uint)a;
					BiosStall = cycles;
				}
				else
				{
					angle = (short)(unchecked((short)0xC000) - ArcTanCore( (x << 14) / y, out int a, out _, out int cycles ));
					Gba.Cpu.Registers[1] = (uint)a;
					BiosStall = cycles;
				}
			}
			else if ( x >= -y )
			{
				angle = (short)(ArcTanCore( (y << 14) / x, out int a, out _, out int cycles ) + 0x10000);
				Gba.Cpu.Registers[1] = (uint)a;
				BiosStall = cycles;
			}
			else
			{
				angle = (short)(unchecked((short)0xC000) - ArcTanCore( (x << 14) / y, out int a, out _, out int cycles ));
				Gba.Cpu.Registers[1] = (uint)a;
				BiosStall = cycles;
			}
		}

		Gba.Cpu.Registers[0] = (ushort)angle;
		Gba.Cpu.Registers[3] = 0x170;
	}

	private static int Clz32( uint value )
	{
		if ( value == 0 ) return 32;
		int n = 0;
		if ( (value & 0xFFFF0000) == 0 ) { n += 16; value <<= 16; }
		if ( (value & 0xFF000000) == 0 ) { n += 8; value <<= 8; }
		if ( (value & 0xF0000000) == 0 ) { n += 4; value <<= 4; }
		if ( (value & 0xC0000000) == 0 ) { n += 2; value <<= 2; }
		if ( (value & 0x80000000) == 0 ) { n += 1; }
		return n;
	}

	private static int MulWait( int r )
	{
		if ( (r & unchecked((int)0xFFFFFF00)) == unchecked((int)0xFFFFFF00) || (r & 0xFFFFFF00) == 0 ) return 1;
		if ( (r & unchecked((int)0xFFFF0000)) == unchecked((int)0xFFFF0000) || (r & 0xFFFF0000) == 0 ) return 2;
		if ( (r & unchecked((int)0xFF000000)) == unchecked((int)0xFF000000) || (r & 0xFF000000) == 0 ) return 3;
		return 4;
	}
}
