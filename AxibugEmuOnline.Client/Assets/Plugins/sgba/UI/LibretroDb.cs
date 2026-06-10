using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace sGBA;

public sealed record GameInfo(
	string Name,
	string Region,
	string Serial,
	string Developer,
	string Publisher,
	string ReleaseYear,
	string ReleaseMonth,
	string Genre,
	string Franchise,
	string EsrbRating,
	string MaxUsers
);

internal sealed class LibretroDb
{
	private const string BaseUrl = "https://raw.githubusercontent.com/libretro/libretro-database/master/metadat/";
	private const string PlatformFileName = "Nintendo%20-%20Game%20Boy%20Advance.dat";
	private const string PlatformCacheFileName = "Nintendo - Game Boy Advance.dat";
	private const string CacheRoot = "database";
	private const string CacheDirectory = "database/metadat";

	private static readonly Dictionary<string, Task<FieldIndex>> FieldTasks = [];
	private static readonly Dictionary<string, Task<GameInfo>> LookupTasks = [];
	private static Task<NoIntroIndex> NoIntroTask;

	public static async Task WarmAsync()
	{
		var tasks = new List<Task>
		{
			GetNoIntroAsync(),
			GetFieldIndexAsync( "developer", "developer", true ),
			GetFieldIndexAsync( "publisher", "publisher", true ),
			GetFieldIndexAsync( "releaseyear", "releaseyear", true ),
			GetFieldIndexAsync( "releasemonth", "releasemonth", true ),
			GetFieldIndexAsync( "genre", "genre", true ),
			GetFieldIndexAsync( "franchise", "franchise", true ),
			GetFieldIndexAsync( "esrb", "esrb_rating", true ),
			GetFieldIndexAsync( "maxusers", "users", false )
		};

		foreach ( var task in tasks )
			await task;
	}

	public static Task<GameInfo> FetchAsync( GameEntry game, GameHashes hashes = null )
	{
		if ( game is null )
			return Task.FromResult<GameInfo>( null );

		if ( LookupTasks.TryGetValue( game.Path, out var task ) )
			return task;

		task = FetchInternalAsync( game, hashes );
		LookupTasks[game.Path] = task;
		return task;
	}

	private static async Task<GameInfo> FetchInternalAsync( GameEntry game, GameHashes hashes )
	{
		var crc = NormalizeCrc( hashes?.Crc );

		var noIntro = await GetNoIntroAsync();
		var noIntroEntry = FindNoIntroEntry( noIntro, game, crc );
		var developerTask = GetValueAsync( "developer", "developer", true, game, noIntroEntry, crc );
		var publisherTask = GetValueAsync( "publisher", "publisher", true, game, noIntroEntry, crc );
		var releaseYearTask = GetValueAsync( "releaseyear", "releaseyear", true, game, noIntroEntry, crc );
		var releaseMonthTask = GetValueAsync( "releasemonth", "releasemonth", true, game, noIntroEntry, crc );
		var genreTask = GetValueAsync( "genre", "genre", true, game, noIntroEntry, crc );
		var franchiseTask = GetValueAsync( "franchise", "franchise", true, game, noIntroEntry, crc );
		var esrbTask = GetValueAsync( "esrb", "esrb_rating", true, game, noIntroEntry, crc );
		var maxUsersTask = GetValueAsync( "maxusers", "users", false, game, noIntroEntry, crc );

		await developerTask;
		await publisherTask;
		await releaseYearTask;
		await releaseMonthTask;
		await genreTask;
		await franchiseTask;
		await esrbTask;
		await maxUsersTask;

		return new GameInfo(
			Display( noIntroEntry?.Name, game.NoIntroName, game.DisplayTitle ),
			Display( noIntroEntry?.Region, game.Region ),
			Display( noIntroEntry?.Serial, game.GameCode ),
			developerTask.Result,
			publisherTask.Result,
			releaseYearTask.Result,
			releaseMonthTask.Result,
			genreTask.Result,
			franchiseTask.Result,
			esrbTask.Result,
			maxUsersTask.Result
		);
	}

	private static async Task<string> GetValueAsync( string folder, string propertyName, bool quoted, GameEntry game, NoIntroEntry noIntroEntry, string crc )
	{
		var index = await GetFieldIndexAsync( folder, propertyName, quoted );

		if ( !string.IsNullOrWhiteSpace( crc ) && index.ByCrc.TryGetValue( crc, out var value ) )
			return value;

		foreach ( var title in CandidateTitles( game, noIntroEntry ) )
		{
			var key = NormalizeTitle( title );
			if ( !string.IsNullOrWhiteSpace( key ) && index.ByTitle.TryGetValue( key, out value ) )
				return value;
		}

		return string.Empty;
	}

	private static Task<FieldIndex> GetFieldIndexAsync( string folder, string propertyName, bool quoted )
	{
		var key = $"{folder}:{propertyName}";
		if ( FieldTasks.TryGetValue( key, out var task ) )
			return task;

		task = LoadFieldIndexAsync( folder, propertyName, quoted );
		FieldTasks[key] = task;
		return task;
	}

	private static async Task<FieldIndex> LoadFieldIndexAsync( string folder, string propertyName, bool quoted )
	{
		try
		{
			var text = await LoadDatAsync( folder );
			return ParseFieldIndex( text, propertyName, quoted );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"[sGBA] Libretro metadata {folder} failed: {ex.Message}" );
			return new FieldIndex();
		}
	}

	private static Task<NoIntroIndex> GetNoIntroAsync()
	{
		NoIntroTask ??= LoadNoIntroAsync();
		return NoIntroTask;
	}

	private static async Task<NoIntroIndex> LoadNoIntroAsync()
	{
		try
		{
			var text = await LoadDatAsync( "no-intro" );
			return ParseNoIntroIndex( text );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"[sGBA] Libretro no-intro metadata failed: {ex.Message}" );
			return new NoIntroIndex();
		}
	}

	private static async Task<string> LoadDatAsync( string folder )
	{
		var cachePath = $"{CacheDirectory}/{folder}/{PlatformCacheFileName}";
		try
		{
			if ( FileSystem.Data.FileExists( cachePath ) )
				return FileSystem.Data.ReadAllText( cachePath );
		}
		catch { }

		var text = await Http.RequestStringAsync( BaseUrl + folder + "/" + PlatformFileName );
		try
		{
			if ( !FileSystem.Data.DirectoryExists( CacheRoot ) )
				FileSystem.Data.CreateDirectory( CacheRoot );

			if ( !FileSystem.Data.DirectoryExists( CacheDirectory ) )
				FileSystem.Data.CreateDirectory( CacheDirectory );

			var folderPath = $"{CacheDirectory}/{folder}";
			if ( !FileSystem.Data.DirectoryExists( folderPath ) )
				FileSystem.Data.CreateDirectory( folderPath );

			FileSystem.Data.WriteAllText( cachePath, text );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"[sGBA] Failed to cache Libretro metadata {folder}: {ex.Message}" );
		}

		return text;
	}

	private static FieldIndex ParseFieldIndex( string text, string propertyName, bool quoted )
	{
		var index = new FieldIndex();
		foreach ( var block in EnumerateGameBlocks( text ) )
		{
			var value = quoted ? ExtractQuotedValue( block, propertyName ) : ExtractBareValue( block, propertyName );
			if ( string.IsNullOrWhiteSpace( value ) )
				continue;

			var crc = ExtractCrc( block );
			if ( !string.IsNullOrWhiteSpace( crc ) )
				index.ByCrc[crc] = value;

			var title = Display( ExtractQuotedValue( block, "comment" ), ExtractQuotedValue( block, "name" ) );
			var titleKey = NormalizeTitle( title );
			if ( !string.IsNullOrWhiteSpace( titleKey ) && !index.ByTitle.ContainsKey( titleKey ) )
				index.ByTitle[titleKey] = value;
		}

		return index;
	}

	private static NoIntroIndex ParseNoIntroIndex( string text )
	{
		var index = new NoIntroIndex();
		foreach ( var block in EnumerateGameBlocks( text ) )
		{
			var name = ExtractQuotedValue( block, "name" );
			if ( string.IsNullOrWhiteSpace( name ) )
				continue;

			var entry = new NoIntroEntry(
				name,
				ExtractQuotedValue( block, "region" ),
				ExtractQuotedValue( block, "serial" )
			);

			var crc = ExtractCrc( block );
			if ( !string.IsNullOrWhiteSpace( crc ) )
				index.ByCrc[crc] = entry;

			var titleKey = NormalizeTitle( name );
			if ( !string.IsNullOrWhiteSpace( titleKey ) && !index.ByTitle.ContainsKey( titleKey ) )
				index.ByTitle[titleKey] = entry;
		}

		return index;
	}

	private static NoIntroEntry FindNoIntroEntry( NoIntroIndex index, GameEntry game, string crc )
	{
		if ( !string.IsNullOrWhiteSpace( crc ) && index.ByCrc.TryGetValue( crc, out var entry ) )
			return entry;

		foreach ( var title in CandidateTitles( game, null ) )
		{
			var key = NormalizeTitle( title );
			if ( !string.IsNullOrWhiteSpace( key ) && index.ByTitle.TryGetValue( key, out entry ) )
				return entry;
		}

		return null;
	}

	private static IEnumerable<string> EnumerateGameBlocks( string text )
	{
		if ( string.IsNullOrWhiteSpace( text ) )
			yield break;

		var cursor = 0;
		while ( cursor < text.Length )
		{
			var gameStart = text.IndexOf( "game", cursor, StringComparison.Ordinal );
			if ( gameStart < 0 )
				yield break;

			var openParen = text.IndexOf( '(', gameStart );
			if ( openParen < 0 )
				yield break;

			var depth = 0;
			var inQuote = false;
			for ( var scan = openParen; scan < text.Length; scan++ )
			{
				if ( text[scan] == '"' )
				{
					inQuote = !inQuote;
					continue;
				}

				if ( inQuote )
					continue;

				if ( text[scan] == '(' ) depth++;
				else if ( text[scan] == ')' ) depth--;

				if ( depth == 0 )
				{
					yield return text.Substring( openParen + 1, scan - openParen - 1 );
					cursor = scan + 1;
					break;
				}
			}

			if ( cursor <= openParen )
				yield break;
		}
	}

	private static string ExtractQuotedValue( string block, string propertyName )
	{
		var match = Regex.Match( block, $"\\b{Regex.Escape( propertyName )}\\s+\"([^\"]*)\"", RegexOptions.IgnoreCase );
		return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
	}

	private static string ExtractBareValue( string block, string propertyName )
	{
		var match = Regex.Match( block, $"\\b{Regex.Escape( propertyName )}\\s+([^\\s\\)]+)", RegexOptions.IgnoreCase );
		return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
	}

	private static string ExtractCrc( string block )
	{
		var match = Regex.Match( block, "\\bcrc\\s+([0-9A-Fa-f]{8})", RegexOptions.IgnoreCase );
		return match.Success ? NormalizeCrc( match.Groups[1].Value ) : string.Empty;
	}

	private static IEnumerable<string> CandidateTitles( GameEntry game, NoIntroEntry noIntroEntry )
	{
		if ( !string.IsNullOrWhiteSpace( noIntroEntry?.Name ) ) yield return noIntroEntry.Name;
		if ( !string.IsNullOrWhiteSpace( game?.NoIntroName ) ) yield return game.NoIntroName;
		if ( !string.IsNullOrWhiteSpace( game?.DisplayTitle ) ) yield return game.DisplayTitle;
	}

	private static string NormalizeTitle( string value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return string.Empty;

		return Regex.Replace( value.Trim().ToLowerInvariant(), "\\s+", " " );
	}

	private static string NormalizeCrc( string value )
	{
		return string.IsNullOrWhiteSpace( value ) ? string.Empty : value.Trim().ToUpperInvariant();
	}

	private static string Display( params string[] values )
	{
		foreach ( var value in values )
		{
			if ( !string.IsNullOrWhiteSpace( value ) )
				return value;
		}

		return string.Empty;
	}

	private sealed class FieldIndex
	{
		public Dictionary<string, string> ByCrc { get; } = new( StringComparer.OrdinalIgnoreCase );
		public Dictionary<string, string> ByTitle { get; } = new( StringComparer.OrdinalIgnoreCase );
	}

	private sealed class NoIntroIndex
	{
		public Dictionary<string, NoIntroEntry> ByCrc { get; } = new( StringComparer.OrdinalIgnoreCase );
		public Dictionary<string, NoIntroEntry> ByTitle { get; } = new( StringComparer.OrdinalIgnoreCase );
	}

	private sealed record NoIntroEntry( string Name, string Region, string Serial );
}
