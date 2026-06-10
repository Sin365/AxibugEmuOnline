namespace sGBA;

public sealed class GameEntry( string path, string displayTitle, string region,
	string gameCode, string noIntroName, BaseFileSystem fileSystem )
{
	public string Path { get; } = path;
	public string DisplayTitle { get; } = displayTitle;
	public string Region { get; } = region;
	public string GameCode { get; } = gameCode;
	public string NoIntroName { get; } = noIntroName;
	public BaseFileSystem FileSystem { get; } = fileSystem;

	public static List<GameEntry> Discover()
	{
		List<GameEntry> entries = [];
		CollectFrom( Sandbox.FileSystem.Mounted, entries );
		CollectFrom( Sandbox.FileSystem.Data, entries );
		entries.Sort( ( a, b ) => string.Compare( a.DisplayTitle, b.DisplayTitle, StringComparison.OrdinalIgnoreCase ) );
		return entries;
	}

	public static HashSet<string> GetInstalledPaths()
	{
		var paths = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
		CollectPathsFrom( Sandbox.FileSystem.Mounted, paths );
		CollectPathsFrom( Sandbox.FileSystem.Data, paths );
		return paths;
	}

	private static void CollectFrom( BaseFileSystem fileSystem, List<GameEntry> entries )
	{
		IEnumerable<string> found;
		try { found = fileSystem.FindFile( "roms", "*.gba" ) ?? []; }
		catch { return; }

		foreach ( string fileName in found )
		{
			GameEntry entry = BuildEntry( fileSystem, $"roms/{fileName}", fileName );
			if ( entry is not null )
				entries.Add( entry );
		}
	}

	private static void CollectPathsFrom( BaseFileSystem fileSystem, HashSet<string> paths )
	{
		try
		{
			foreach ( var fileName in fileSystem.FindFile( "roms", "*.gba" ) ?? [] )
				paths.Add( $"roms/{fileName}" );
		}
		catch
		{
		}
	}

	private static GameEntry BuildEntry( BaseFileSystem fileSystem, string fullPath, string fileName )
	{
		string baseName = System.IO.Path.GetFileNameWithoutExtension( fileName );
		(string displayTitle, string region) = ParseNoIntroName( baseName );
		string gameCode = ReadGameCode( fileSystem, fullPath );

		return new GameEntry( fullPath, displayTitle, region, gameCode, baseName, fileSystem );
	}

	private static (string DisplayTitle, string Region) ParseNoIntroName( string baseName )
	{
		int parenOpen = baseName.IndexOf( '(' );
		if ( parenOpen <= 0 )
			return (baseName, string.Empty);

		string displayTitle = baseName[..parenOpen].TrimEnd();
		int parenClose = baseName.IndexOf( ')', parenOpen );
		string region = parenClose > parenOpen ? baseName[(parenOpen + 1)..parenClose] : string.Empty;
		return (displayTitle, region);
	}

	private static string ReadGameCode( BaseFileSystem fileSystem, string path )
	{
		try
		{
			using System.IO.Stream stream = fileSystem.OpenRead( path );
			if ( stream.Length < 0xB0 )
				return string.Empty;

			byte[] header = new byte[0xB0];
			stream.ReadExactly( header, 0, 0xB0 );

			return System.Text.Encoding.ASCII.GetString( header, 0xAC, 4 ).TrimEnd( '\0' );
		}
		catch
		{
			return string.Empty;
		}
	}
}

