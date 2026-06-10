using System.IO;
using System.IO.Compression;

namespace sGBA;

public enum LinkCablePacketType : byte
{
	Input = 1,
	Av = 2,
	SaveUpload = 3,
	State = 4,
}

internal static class LinkCableCompression
{
	public static byte[] Deflate( byte[] data )
	{
		using var output = new MemoryStream();
		using ( var deflate = new DeflateStream( output, CompressionLevel.Fastest, leaveOpen: true ) )
			deflate.Write( data, 0, data.Length );
		return output.ToArray();
	}

	public static byte[] Inflate( byte[] compressed, int rawLength )
	{
		var result = new byte[rawLength];
		using var input = new MemoryStream( compressed, false );
		using var deflate = new DeflateStream( input, CompressionMode.Decompress );
		int read = 0;
		while ( read < rawLength )
		{
			int n = deflate.Read( result, read, rawLength - read );
			if ( n <= 0 )
				break;
			read += n;
		}
		return result;
	}
}

public sealed class LinkCableStatePacket
{
	public int PlayerId;
	public byte[] State;

	public byte[] Serialize()
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter( stream );
		writer.Write( (byte)LinkCablePacketType.State );
		writer.Write( PlayerId );

		int rawLen = State?.Length ?? 0;
		writer.Write( rawLen );
		if ( rawLen > 0 )
		{
			byte[] compressed = LinkCableCompression.Deflate( State );
			writer.Write( compressed.Length );
			writer.Write( compressed );
		}

		writer.Flush();
		return stream.ToArray();
	}

	public static bool TryParse( byte[] payload, int length, out LinkCableStatePacket packet )
	{
		packet = null;
		if ( payload == null || length < 1 || payload[0] != (byte)LinkCablePacketType.State )
			return false;

		using var stream = new MemoryStream( payload, 0, length, false );
		using var reader = new BinaryReader( stream );
		reader.ReadByte();

		var result = new LinkCableStatePacket { PlayerId = reader.ReadInt32() };

		int rawLen = reader.ReadInt32();
		if ( rawLen < 0 )
			return false;
		if ( rawLen > 0 )
		{
			int compressedLen = reader.ReadInt32();
			if ( compressedLen < 0 )
				return false;
			byte[] compressed = reader.ReadBytes( compressedLen );
			result.State = LinkCableCompression.Inflate( compressed, rawLen );
		}

		packet = result;
		return true;
	}
}

public sealed class LinkCableSaveUploadPacket
{
	public int PlayerId;
	public byte[] SaveData;

	public byte[] Serialize()
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter( stream );
		writer.Write( (byte)LinkCablePacketType.SaveUpload );
		writer.Write( PlayerId );

		int saveLen = SaveData?.Length ?? 0;
		writer.Write( saveLen );
		if ( saveLen > 0 )
			writer.Write( SaveData );

		writer.Flush();
		return stream.ToArray();
	}

	public static bool TryParse( byte[] payload, int length, out LinkCableSaveUploadPacket packet )
	{
		packet = null;
		if ( payload == null || length < 1 )
			return false;
		if ( payload[0] != (byte)LinkCablePacketType.SaveUpload )
			return false;

		using var stream = new MemoryStream( payload, 0, length, false );
		using var reader = new BinaryReader( stream );
		reader.ReadByte();

		var result = new LinkCableSaveUploadPacket
		{
			PlayerId = reader.ReadInt32(),
		};

		int saveLen = reader.ReadInt32();
		if ( saveLen < 0 )
			return false;
		if ( saveLen > 0 )
			result.SaveData = reader.ReadBytes( saveLen );

		packet = result;
		return true;
	}
}

public readonly struct LinkCableInputPacket
{
	public readonly long Frame;
	public readonly ushort Keys;

	public LinkCableInputPacket( long frame, ushort keys )
	{
		Frame = frame;
		Keys = keys;
	}

	public byte[] Serialize()
	{
		var buffer = new byte[12];
		buffer[0] = (byte)LinkCablePacketType.Input;
		WriteInt64( buffer, 1, Frame );
		buffer[9] = (byte)(Keys & 0xFF);
		buffer[10] = (byte)(Keys >> 8);
		buffer[11] = 0;
		return buffer;
	}

	public static bool TryParse( byte[] payload, int length, out LinkCableInputPacket packet )
	{
		packet = default;
		if ( payload == null || length < 12 )
			return false;
		if ( payload[0] != (byte)LinkCablePacketType.Input )
			return false;
		long frame = ReadInt64( payload, 1 );
		ushort keys = (ushort)(payload[9] | (payload[10] << 8));
		packet = new LinkCableInputPacket( frame, keys );
		return true;
	}

	internal static void WriteInt64( byte[] buffer, int offset, long value )
	{
		for ( int i = 0; i < 8; i++ )
			buffer[offset + i] = (byte)(value >> (i * 8));
	}

	internal static long ReadInt64( byte[] buffer, int offset )
	{
		long value = 0;
		for ( int i = 0; i < 8; i++ )
			value |= (long)buffer[offset + i] << (i * 8);
		return value;
	}
}

public sealed class LinkCableAvPacket
{
	public int PlayerId;
	public long Frame;
	public short[] Audio;
	public int AudioSamples;
	public byte[] SaveData;
	public byte[] Video;

	public byte[] Serialize()
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter( stream );
		writer.Write( (byte)LinkCablePacketType.Av );
		writer.Write( PlayerId );
		writer.Write( Frame );

		int samples = AudioSamples < 0 ? 0 : AudioSamples;
		int shorts = samples * 2;
		if ( Audio == null || shorts > Audio.Length )
			shorts = Audio == null ? 0 : Audio.Length;
		writer.Write( shorts );
		for ( int i = 0; i < shorts; i++ )
			writer.Write( Audio[i] );

		int saveLen = SaveData?.Length ?? 0;
		writer.Write( saveLen );
		if ( saveLen > 0 )
			writer.Write( SaveData );

		int videoLen = Video?.Length ?? 0;
		writer.Write( videoLen );
		if ( videoLen > 0 )
		{
			byte[] compressed = LinkCableCompression.Deflate( Video );
			writer.Write( compressed.Length );
			writer.Write( compressed );
		}

		writer.Flush();
		return stream.ToArray();
	}

	public static bool TryParse( byte[] payload, int length, out LinkCableAvPacket packet )
	{
		packet = null;
		if ( payload == null || length < 1 )
			return false;
		if ( payload[0] != (byte)LinkCablePacketType.Av )
			return false;

		using var stream = new MemoryStream( payload, 0, length, false );
		using var reader = new BinaryReader( stream );
		reader.ReadByte();

		var result = new LinkCableAvPacket
		{
			PlayerId = reader.ReadInt32(),
			Frame = reader.ReadInt64(),
		};

		int shorts = reader.ReadInt32();
		if ( shorts < 0 )
			return false;
		result.AudioSamples = shorts / 2;
		result.Audio = new short[shorts];
		for ( int i = 0; i < shorts; i++ )
			result.Audio[i] = reader.ReadInt16();

		int saveLen = reader.ReadInt32();
		if ( saveLen > 0 )
			result.SaveData = reader.ReadBytes( saveLen );

		int videoLen = reader.ReadInt32();
		if ( videoLen > 0 )
		{
			int compressedLen = reader.ReadInt32();
			if ( compressedLen < 0 )
				return false;
			byte[] compressed = reader.ReadBytes( compressedLen );
			result.Video = LinkCableCompression.Inflate( compressed, videoLen );
		}

		packet = result;
		return true;
	}
}
