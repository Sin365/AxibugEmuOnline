using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace sGBA;

public enum ThumbType
{
	BoxArt,
	Logo,
	Snap,
	Title
}

public static class Thumbnails
{
	private const string SystemName = "Nintendo - Game Boy Advance";
	private const string CacheRoot = "thumbnails";
	private const string BaseUrl = "https://thumbnails.libretro.com/";
	private static readonly Regex ImageLinkRegex = new( "href=\"([^\"]+\\.png)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled );
	private static readonly Dictionary<ThumbType, Task<Dictionary<string, string>>> IndexTasks = [];
	private static readonly Dictionary<string, Task<string>> ExactPathTasks = [];
	private static readonly Dictionary<string, Task<string>> FallbackPathTasks = [];
	private static readonly Dictionary<string, Texture> LoadedTextures = [];

	public static async Task<Texture> LoadAsync( GameEntry game, ThumbType type )
	{
		if ( game is null || string.IsNullOrWhiteSpace( game.NoIntroName ) )
			return null;

		var exactCachedTexture = TryLoadCachedTexture( game, type );
		if ( exactCachedTexture != null )
			return exactCachedTexture;

		var exactPath = await GetExactPathAsync( game, type );
		if ( !string.IsNullOrWhiteSpace( exactPath ) )
			return LoadTexture( exactPath, game, type );

		var fallbackPath = await GetFallbackPathAsync( game, type );
		if ( string.IsNullOrWhiteSpace( fallbackPath ) )
			return null;

		return LoadTexture( fallbackPath, game, type );
	}

	public static Texture TryLoadCachedTexture( GameEntry game, ThumbType type )
	{
		if ( game is null || string.IsNullOrWhiteSpace( game.NoIntroName ) )
			return null;

		var cachePath = GetCachePath( type, GetExactFileName( game ) );
		if ( !FileSystem.Data.FileExists( cachePath ) )
			return null;

		return LoadTexture( cachePath, game, type );
	}

	public static void ReleaseTexture( Texture texture )
	{
		if ( texture == null )
			return;

		foreach ( var (cachePath, loadedTexture) in LoadedTextures.ToList() )
		{
			if ( loadedTexture == texture )
				LoadedTextures.Remove( cachePath );
		}

		texture.Dispose();
	}

	private static Task<string> GetExactPathAsync( GameEntry game, ThumbType type )
	{
		var key = $"exact:{type}:{game.NoIntroName}";
		if ( ExactPathTasks.TryGetValue( key, out var task ) )
			return task;

		task = GetExactPathInternalAsync( game, type );
		ExactPathTasks[key] = task;
		return task;
	}

	private static async Task<string> GetExactPathInternalAsync( GameEntry game, ThumbType type )
	{
		var fileName = GetExactFileName( game );
		var cachePath = GetCachePath( type, fileName );
		try
		{
			if ( FileSystem.Data.FileExists( cachePath ) )
				return cachePath;

			var bytes = await Http.RequestBytesAsync( BuildUrl( type, fileName ) );
			if ( !IsPng( bytes ) )
				return null;

			EnsureCacheDirectory( type );
			FileSystem.Data.WriteAllBytes( cachePath, bytes );
			return cachePath;
		}
		catch
		{
			return null;
		}
	}

	private static Task<string> GetFallbackPathAsync( GameEntry game, ThumbType type )
	{
		var key = $"fallback:{type}:{game.NoIntroName}";
		if ( FallbackPathTasks.TryGetValue( key, out var task ) )
			return task;

		task = GetFallbackPathInternalAsync( game, type );
		FallbackPathTasks[key] = task;
		return task;
	}

	private static async Task<string> GetFallbackPathInternalAsync( GameEntry game, ThumbType type )
	{
		try
		{
			var fileName = await FindIndexedFileNameAsync( game, type );
			if ( string.IsNullOrWhiteSpace( fileName ) )
				return null;

			var cachePath = GetCachePath( type, fileName );
			if ( !FileSystem.Data.FileExists( cachePath ) )
			{
				var bytes = await Http.RequestBytesAsync( BuildUrl( type, fileName ) );
				if ( !IsPng( bytes ) )
					return null;

				EnsureCacheDirectory( type );
				FileSystem.Data.WriteAllBytes( cachePath, bytes );
			}

			return cachePath;
		}
		catch ( Exception ex )
		{
			Log.Warning( $"[sGBA] {type} thumbnail failed for {game.DisplayTitle}: {ex.Message}" );
			return null;
		}
	}

	private static Texture LoadTexture( string cachePath, GameEntry game, ThumbType type )
	{
		if ( LoadedTextures.TryGetValue( cachePath, out var cachedTexture ) )
			return cachedTexture;

		try
		{
			var texture = Texture.LoadFromFileSystem( cachePath, FileSystem.Data );
			if ( texture == null )
				return null;

			LoadedTextures[cachePath] = texture;
			return texture;
		}
		catch ( Exception ex )
		{
			Log.Warning( $"[sGBA] {type} texture load failed for {game.DisplayTitle}: {ex.Message}" );
			return null;
		}
	}

	private static async Task<string> FindIndexedFileNameAsync( GameEntry game, ThumbType type )
	{
		var index = await GetIndexAsync( type );
		if ( index.Count == 0 )
			return null;

		var exactKey = NormalizeName( game.NoIntroName );
		if ( index.TryGetValue( exactKey, out var exact ) )
			return exact;

		var titleKey = NormalizeName( game.DisplayTitle );
		return index.Values
			.Where( value => NormalizeName( StripReleaseTags( System.IO.Path.GetFileNameWithoutExtension( value ) ) ) == titleKey )
			.OrderBy( value => ScoreFallbackMatch( value, game ) )
			.FirstOrDefault();
	}

	private static Task<Dictionary<string, string>> GetIndexAsync( ThumbType type )
	{
		if ( IndexTasks.TryGetValue( type, out var task ) )
			return task;

		task = LoadIndexAsync( type );
		IndexTasks[type] = task;
		return task;
	}

	private static async Task<Dictionary<string, string>> LoadIndexAsync( ThumbType type )
	{
		var map = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
		var html = await Http.RequestStringAsync( BuildDirectoryUrl( type ) );

		foreach ( Match match in ImageLinkRegex.Matches( html ) )
		{
			var fileName = DecodeFileName( match.Groups[1].Value );
			if ( string.IsNullOrWhiteSpace( fileName ) || !fileName.EndsWith( ".png", StringComparison.OrdinalIgnoreCase ) )
				continue;

			var key = NormalizeName( System.IO.Path.GetFileNameWithoutExtension( fileName ) );
			if ( string.IsNullOrWhiteSpace( key ) || map.ContainsKey( key ) )
				continue;

			map[key] = fileName;
		}

		return map;
	}

	private static string BuildDirectoryUrl( ThumbType type )
	{
		return BaseUrl + Uri.EscapeDataString( SystemName ) + "/" + GetDirectoryName( type ) + "/";
	}

	private static string BuildUrl( ThumbType type, string fileName )
	{
		return BuildDirectoryUrl( type ) + Uri.EscapeDataString( fileName );
	}

	private static string GetExactFileName( GameEntry game )
	{
		return game.NoIntroName + ".png";
	}

	private static string GetCachePath( ThumbType type, string fileName )
	{
		return $"{CacheRoot}/{SystemName}/{GetDirectoryName( type )}/{fileName}";
	}

	private static void EnsureCacheDirectory( ThumbType type )
	{
		if ( !FileSystem.Data.DirectoryExists( CacheRoot ) )
			FileSystem.Data.CreateDirectory( CacheRoot );

		var systemPath = $"{CacheRoot}/{SystemName}";
		if ( !FileSystem.Data.DirectoryExists( systemPath ) )
			FileSystem.Data.CreateDirectory( systemPath );

		var typePath = $"{systemPath}/{GetDirectoryName( type )}";
		if ( !FileSystem.Data.DirectoryExists( typePath ) )
			FileSystem.Data.CreateDirectory( typePath );
	}

	private static string GetDirectoryName( ThumbType type )
	{
		return type switch
		{
			ThumbType.BoxArt => "Named_Boxarts",
			ThumbType.Logo => "Named_Logos",
			ThumbType.Snap => "Named_Snaps",
			ThumbType.Title => "Named_Titles",
			_ => "Named_Boxarts"
		};
	}

	private static bool IsPng( byte[] bytes )
	{
		return bytes is { Length: >= 8 }
			&& bytes[0] == 0x89
			&& bytes[1] == 0x50
			&& bytes[2] == 0x4E
			&& bytes[3] == 0x47
			&& bytes[4] == 0x0D
			&& bytes[5] == 0x0A
			&& bytes[6] == 0x1A
			&& bytes[7] == 0x0A;
	}

	private static string DecodeFileName( string value )
	{
		try
		{
			var decoded = WebUtility.HtmlDecode( value );
			decoded = Uri.UnescapeDataString( decoded );
			return decoded.Split( '/', StringSplitOptions.RemoveEmptyEntries ).LastOrDefault();
		}
		catch
		{
			return null;
		}
	}

	private static string NormalizeName( string value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return string.Empty;

		var builder = new StringBuilder( value.Length );
		var pendingSpace = false;

		foreach ( var ch in WebUtility.HtmlDecode( value ).Trim() )
		{
			if ( char.IsWhiteSpace( ch ) || ch == '_' || ch == '&' )
			{
				pendingSpace = builder.Length > 0;
				continue;
			}

			if ( pendingSpace )
			{
				builder.Append( ' ' );
				pendingSpace = false;
			}

			builder.Append( char.ToLowerInvariant( ch ) );
		}

		return builder.ToString();
	}

	private static string StripReleaseTags( string value )
	{
		return Regex.Replace( value ?? string.Empty, "\\s*\\([^)]*\\)", string.Empty ).Trim();
	}

	private static int ScoreFallbackMatch( string fileName, GameEntry game )
	{
		var name = System.IO.Path.GetFileNameWithoutExtension( fileName );
		var score = 0;

		if ( !string.IsNullOrWhiteSpace( game.Region ) && name.Contains( $"({game.Region})", StringComparison.OrdinalIgnoreCase ) ) score -= 30;
		if ( name.Contains( "(USA", StringComparison.OrdinalIgnoreCase ) ) score -= 20;
		if ( name.Contains( "(World", StringComparison.OrdinalIgnoreCase ) ) score -= 15;
		if ( name.Contains( "(Europe", StringComparison.OrdinalIgnoreCase ) ) score -= 10;
		if ( name.Contains( "(Virtual Console", StringComparison.OrdinalIgnoreCase ) ) score += 20;
		if ( name.Contains( "(Beta", StringComparison.OrdinalIgnoreCase ) ) score += 30;
		if ( name.Contains( "(Demo", StringComparison.OrdinalIgnoreCase ) ) score += 30;
		if ( name.Contains( "(Proto", StringComparison.OrdinalIgnoreCase ) ) score += 30;

		return score;
	}
}
