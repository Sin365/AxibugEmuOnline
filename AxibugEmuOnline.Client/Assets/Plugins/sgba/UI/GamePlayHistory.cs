namespace sGBA;

public static class GamePlayHistory
{
	private const string CookieKey = "sgba.game_play_history";
	private static readonly Dictionary<string, long> LastPlayedByPath = new( StringComparer.OrdinalIgnoreCase );
	private static bool _loaded;

	public static long LastPlayedAt( string path )
	{
		EnsureLoaded();
		return !string.IsNullOrWhiteSpace( path ) && LastPlayedByPath.TryGetValue( path, out long value ) ? value : 0L;
	}

	public static void MarkPlayed( string path )
	{
		if ( string.IsNullOrWhiteSpace( path ) )
			return;

		EnsureLoaded();
		LastPlayedByPath[path] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		Save();
	}

	private static void EnsureLoaded()
	{
		if ( _loaded )
			return;

		_loaded = true;
		LastPlayedByPath.Clear();

		var saved = Game.Cookies?.Get<Dictionary<string, long>>( CookieKey, null );
		if ( saved is null )
			return;

		foreach ( var (path, value) in saved )
		{
			if ( !string.IsNullOrWhiteSpace( path ) )
				LastPlayedByPath[path] = value;
		}
	}

	private static void Save()
	{
		Game.Cookies?.Set( CookieKey, LastPlayedByPath );
	}
}
