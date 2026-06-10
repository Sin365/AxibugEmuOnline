using System.Security.Cryptography;
using Sandbox.Utility;

namespace sGBA;

public sealed record GameHashes( string Md5, string Sha1, long Size, string Crc = "" );

internal static class GameHasher
{
	private static readonly Dictionary<string, GameHashes> Hashes = new( StringComparer.OrdinalIgnoreCase );

	public static GameHashes Compute( GameEntry game )
	{
		if ( game is null )
			return new GameHashes( string.Empty, string.Empty, 0 );

		if ( Hashes.TryGetValue( game.Path, out var cached ) )
			return cached;

		try
		{
			var bytes = game.FileSystem.ReadAllBytes( game.Path ).ToArray();
			var hashes = new GameHashes(
				Convert.ToHexString( MD5.HashData( bytes ) ).ToLowerInvariant(),
				Convert.ToHexString( SHA1.HashData( bytes ) ).ToLowerInvariant(),
				bytes.LongLength,
				Crc32.FromBytes( bytes ).ToString( "X8" )
			);

			Hashes[game.Path] = hashes;
			return hashes;
		}
		catch ( Exception ex )
		{
			Log.Warning( $"[sGBA] Failed to hash {game.DisplayTitle}: {ex.Message}" );
			return new GameHashes( string.Empty, string.Empty, 0 );
		}
	}
}
