namespace sGBA;

public sealed class SessionPlayer
{
	public Connection Connection { get; }
	public int Slot { get; internal set; }
	public bool Ready { get; internal set; }

	private readonly Guid _id;
	private readonly ulong _steamId;
	private readonly string _name;
	private readonly bool _isHost;

	internal SessionPlayer( Connection connection, int slot )
	{
		Connection = connection;
		Slot = slot;
		_id = connection?.Id ?? Guid.Empty;
		_steamId = connection?.SteamId ?? 0UL;
		_name = connection?.DisplayName;
		_isHost = connection?.IsHost ?? false;
	}

	internal SessionPlayer( int slot, Guid id, ulong steamId, string name, bool isHost, bool ready )
	{
		Connection = ResolveConnection( id );
		Slot = slot;
		Ready = ready;
		_id = id;
		_steamId = steamId;
		_name = name;
		_isHost = isHost;
	}

	private static Connection ResolveConnection( Guid id )
	{
		if ( id == Guid.Empty )
			return null;

		var all = Connection.All;
		if ( all is null )
			return null;

		for ( int i = 0; i < all.Count; i++ )
		{
			if ( all[i] is not null && all[i].Id == id )
				return all[i];
		}
		return null;
	}

	public Guid Id => Connection?.Id ?? _id;
	public string DisplayName => Connection?.DisplayName ?? _name ?? string.Empty;
	public ulong SteamId => Connection?.SteamId ?? _steamId;
	public bool IsHost => Connection?.IsHost ?? _isHost;
	public bool IsLocal => Id != Guid.Empty && Id == (Connection.Local?.Id ?? Guid.Empty);
	public int Ping => (int)(Connection?.Ping ?? 0f);
}
