namespace sGBA;

public partial class GbaBios
{
	private void BgAffineSet()
	{
		uint src = Gba.Cpu.Registers[0];
		uint dst = Gba.Cpu.Registers[1];
		int count = (int)Gba.Cpu.Registers[2];

		for ( int i = 0; i < count; i++ )
		{
			float ox = (int)Gba.Memory.Load32( src ) / 256.0f;
			float oy = (int)Gba.Memory.Load32( src + 4 ) / 256.0f;
			short cx = (short)Gba.Memory.Load16( src + 8 );
			short cy = (short)Gba.Memory.Load16( src + 10 );
			float sx = (short)Gba.Memory.Load16( src + 12 ) / 256.0f;
			float sy = (short)Gba.Memory.Load16( src + 14 ) / 256.0f;
			float theta = (Gba.Memory.Load16( src + 16 ) >> 8) / 128.0f * MathF.PI;

			float cosA = MathF.Cos( theta );
			float sinA = MathF.Sin( theta );

			float a = cosA * sx;
			float b = -sinA * sx;
			float c = sinA * sy;
			float d = cosA * sy;

			float rx = ox - (a * cx + b * cy);
			float ry = oy - (c * cx + d * cy);

			Gba.Memory.Store16( dst + 0, (ushort)(short)(a * 256) );
			Gba.Memory.Store16( dst + 2, (ushort)(short)(b * 256) );
			Gba.Memory.Store16( dst + 4, (ushort)(short)(c * 256) );
			Gba.Memory.Store16( dst + 6, (ushort)(short)(d * 256) );
			Gba.Memory.Store32( dst + 8, (uint)(int)(rx * 256) );
			Gba.Memory.Store32( dst + 12, (uint)(int)(ry * 256) );

			src += 20;
			dst += 16;
		}
	}

	private void ObjAffineSet()
	{
		uint src = Gba.Cpu.Registers[0];
		uint dst = Gba.Cpu.Registers[1];
		int count = (int)Gba.Cpu.Registers[2];
		int dstStride = (int)Gba.Cpu.Registers[3];

		for ( int i = 0; i < count; i++ )
		{
			float sx = (short)Gba.Memory.Load16( src ) / 256.0f;
			float sy = (short)Gba.Memory.Load16( src + 2 ) / 256.0f;
			float theta = (Gba.Memory.Load16( src + 4 ) >> 8) / 128.0f * MathF.PI;

			float a, b, c, d;
			a = d = MathF.Cos( theta );
			b = c = MathF.Sin( theta );
			a *= sx;
			b *= -sx;
			c *= sy;
			d *= sy;

			Gba.Memory.Store16( dst + (uint)(dstStride * 0), (ushort)(short)(a * 256) );
			Gba.Memory.Store16( dst + (uint)(dstStride * 1), (ushort)(short)(b * 256) );
			Gba.Memory.Store16( dst + (uint)(dstStride * 2), (ushort)(short)(c * 256) );
			Gba.Memory.Store16( dst + (uint)(dstStride * 3), (ushort)(short)(d * 256) );

			src += 8;
			dst += (uint)(dstStride * 4);
		}
	}

	private void BitUnPack()
	{
		uint src = Gba.Cpu.Registers[0];
		uint dst = Gba.Cpu.Registers[1];
		uint info = Gba.Cpu.Registers[2];

		ushort srcLen = Gba.Memory.Load16( info );
		byte srcBpp = Gba.Memory.Load8( info + 2 );
		byte dstBpp = Gba.Memory.Load8( info + 3 );

		switch ( srcBpp )
		{
			case 1: case 2: case 4: case 8: break;
			default: return;
		}
		switch ( dstBpp )
		{
			case 1: case 2: case 4: case 8: case 16: case 32: break;
			default: return;
		}

		uint dataOffset = Gba.Memory.Load32( info + 4 );
		bool zeroFlag = (dataOffset & 0x80000000) != 0;
		dataOffset &= 0x7FFFFFFF;

		int srcMask = (1 << srcBpp) - 1;
		uint buffer = 0;
		int bitsInBuffer = 0;
		int bitsRemaining = 0;
		byte inByte = 0;

		while ( srcLen > 0 || bitsRemaining > 0 )
		{
			if ( bitsRemaining == 0 )
			{
				inByte = Gba.Memory.Load8( src++ );
				bitsRemaining = 8;
				srcLen--;
			}

			int val = inByte & srcMask;
			inByte >>= srcBpp;
			if ( val != 0 || zeroFlag )
				val += (int)dataOffset;
			bitsRemaining -= srcBpp;

			buffer |= (uint)val << bitsInBuffer;
			bitsInBuffer += dstBpp;

			if ( bitsInBuffer == 32 )
			{
				Gba.Memory.Store32( dst, buffer );
				dst += 4;
				buffer = 0;
				bitsInBuffer = 0;
			}
		}

		Gba.Cpu.Registers[0] = src;
		Gba.Cpu.Registers[1] = dst;
	}
}
