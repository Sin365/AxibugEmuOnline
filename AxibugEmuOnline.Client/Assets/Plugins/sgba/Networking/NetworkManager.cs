using System.Collections.Concurrent;
using Sandbox.Network;

namespace sGBA;

public sealed partial class NetworkManager : Component, IWirelessNetwork, Component.INetworkListener
{
	public enum SessionState
	{
		Solo,
		Hosting,
		Joined,
		InGame
	}

	public enum SessionMode
	{
		WirelessAdapter,
		LinkCable
	}

	public const int MaxWirelessPlayers = 5;
	public const int MaxLinkPlayers = 4;

	private const int BroadcastClientId = 0xFFFF;

	public int PlayerCap => Mode == SessionMode.LinkCable ? MaxLinkPlayers : MaxWirelessPlayers;

	public static NetworkManager Current { get; private set; }

	public SessionState State { get; private set; } = SessionState.Solo;
	public SessionMode Mode { get; private set; } = SessionMode.WirelessAdapter;
	public SessionVisibility Visibility { get; private set; } = SessionVisibility.Public;
	public string RomTitle { get; private set; }
	public string RomSha1 { get; private set; }
	public string RomGameCode { get; private set; }

	public IReadOnlyList<SessionPlayer> Players => _playerView;
	public SessionPlayer LocalPlayer => _localSlot >= 0 ? _playerView.FirstOrDefault( p => p.Slot == _localSlot ) : null;

	public bool IsActive => State != SessionState.Solo;
	public bool IsHost => Networking.IsHost && IsActive;
	public bool IsClient => IsActive && !Networking.IsHost;

	public bool InMultiplayerSession => _playerView.Count > 1;
	public bool AllReady
	{
		get
		{
			var ps = _playerView;
			return ps.Count > 1 && ps.All( p => p.Ready || p.IsHost );
		}
	}
	public bool CanStart => IsHost && AllReady && State == SessionState.Hosting;

	private readonly Dictionary<Guid, bool> _ready = [];

	private readonly Connection[] _slotConns = new Connection[MaxWirelessPlayers];
	private readonly Dictionary<Guid, int> _slotByConn = [];
	private readonly List<SessionPlayer> _playerView = new( MaxWirelessPlayers );
	private int _localSlot = -1;

	private EmulatorComponent _emulator;
	private GbaWirelessAdapter Adapter => _emulator?.Core?.Io?.WirelessAdapter;
	private GbaWirelessAdapter _wiredAdapter;
	private readonly ConcurrentQueue<(int ClientId, byte[] Payload)> _outbox = new();

	protected override void OnAwake()
	{
		Current = this;
	}

	protected override void OnStart()
	{
		_emulator = Scene.GetAllComponents<EmulatorComponent>().FirstOrDefault();
		TryWireAdapter();
	}

	protected override void OnUpdate()
	{
		var adapter = Adapter;
		if ( adapter is not null && !ReferenceEquals( adapter, _wiredAdapter ) )
		{
			adapter.Network = this;
			_wiredAdapter = adapter;
		}

		while ( _outbox.TryDequeue( out var pkt ) )
			DispatchSend( pkt.ClientId, pkt.Payload );

		SendHostHeartbeat();
		DetectSessionLost();
		HostUpdateMode();

		if ( Mode == SessionMode.LinkCable )
		{
			UpdateLinkRoster();
			PumpLinkCable();
		}
	}

	protected override void OnDestroy()
	{
		if ( Current == this )
			Current = null;
	}

	private string _modeGameCode;

	private void HostUpdateMode()
	{
		if ( !Networking.IsHost )
			return;

		var code = EmulatorComponent.Current?.GameCode;
		if ( string.IsNullOrEmpty( code ) || code == _modeGameCode )
			return;

		_modeGameCode = code;
		RomGameCode = code;

		var mode = WirelessAdapterGames.DetectMode( code );
		if ( mode == Mode )
			return;

		Mode = mode;
		try { Networking.SetData( LobbyDataKeys.Mode, ((int)mode).ToString() ); }
		catch { }
		RpcSyncMode( (int)mode );
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcSyncMode( int mode )
	{
		if ( Networking.IsHost )
			return;
		Mode = (SessionMode)mode;
	}

	public bool Host( GameEntry rom, SessionVisibility visibility )
	{
		if ( IsActive )
			return false;
		if ( rom is null )
			return false;

		Visibility = visibility;
		Mode = WirelessAdapterGames.DetectMode( rom.GameCode );
		RomTitle = rom.DisplayTitle;
		RomGameCode = rom.GameCode;
		RomSha1 = ComputeRomSha1( rom );

		var config = new LobbyConfig
		{
			MaxPlayers = PlayerCap,
			Name = $"{rom.DisplayTitle}",
			Privacy = ToLobbyPrivacy( visibility ),
			Hidden = visibility == SessionVisibility.InviteOnly,
			DestroyWhenHostLeaves = true,
			AutoSwitchToBestHost = false
		};

		Networking.CreateLobby( config );
		State = SessionState.Hosting;

		Networking.SetData( LobbyDataKeys.RomTitle, RomTitle ?? string.Empty );
		Networking.SetData( LobbyDataKeys.GameCode, RomGameCode ?? string.Empty );
		Networking.SetData( LobbyDataKeys.RomSha1, RomSha1 ?? string.Empty );
		Networking.SetData( LobbyDataKeys.Visibility, ((int)Visibility).ToString() );
		Networking.SetData( LobbyDataKeys.Mode, ((int)Mode).ToString() );
		Networking.SetData( LobbyDataKeys.HostName, Connection.Local?.DisplayName ?? string.Empty );

		HostCommitRoster();

		return true;
	}

	public bool Join( ulong lobbyId )
	{
		if ( IsActive )
			return false;

		Networking.Connect( lobbyId );
		State = SessionState.Joined;
		return true;
	}

	public void Leave()
	{
		if ( !IsActive )
			return;

		EndLinkedPlay();
		Networking.Disconnect();
		ClearSessionState();
	}

	private void ClearSessionState()
	{
		_ready.Clear();
		Array.Clear( _slotConns, 0, _slotConns.Length );
		_slotByConn.Clear();
		_playerView.Clear();
		_localSlot = -1;
		State = SessionState.Solo;
		Mode = SessionMode.WirelessAdapter;
		RomTitle = null;
		RomSha1 = null;
		RomGameCode = null;
		_joinedRomLoaded = false;
		_hadRemoteHost = false;
	}

	private bool _hadRemoteHost;
	private bool _gotHostHeartbeat;
	private RealTimeSince _timeSinceHostHeartbeat;
	private RealTimeSince _timeSinceHeartbeatSent;
	private const float HostHeartbeatInterval = 0.5f;
	private const float HostHeartbeatTimeout = 2.5f;

	private void SendHostHeartbeat()
	{
		if ( !Networking.IsHost || _playerView.Count < 2 )
			return;
		if ( _timeSinceHeartbeatSent < HostHeartbeatInterval )
			return;

		_timeSinceHeartbeatSent = 0;
		RpcHostHeartbeat();
	}

	[Rpc.Broadcast( NetFlags.UnreliableNoDelay )]
	private void RpcHostHeartbeat()
	{
		if ( Networking.IsHost )
			return;

		_gotHostHeartbeat = true;
		_timeSinceHostHeartbeat = 0;
	}

	private void DetectSessionLost()
	{
		if ( Networking.IsHost )
		{
			_hadRemoteHost = false;
			_gotHostHeartbeat = false;
			return;
		}

		var host = Connection.Host;
		bool hasRemoteHost = host is not null && host != Connection.Local && host.IsActive;

		if ( !_hadRemoteHost )
		{
			if ( hasRemoteHost )
			{
				_hadRemoteHost = true;
				_gotHostHeartbeat = false;
				_timeSinceHostHeartbeat = 0;
			}
			return;
		}

		bool lost = !hasRemoteHost || (_gotHostHeartbeat && _timeSinceHostHeartbeat > HostHeartbeatTimeout);
		if ( !lost )
			return;

		_hadRemoteHost = false;
		_gotHostHeartbeat = false;

		var emu = EmulatorComponent.Current;
		if ( emu?.IsLinked == true )
			emu.ResumeSoloAfterHostLost();
		else
			ClearSessionAfterHostLost();
	}

	public string ResolveSessionRomPath() => FindLocalRomMatch( RomSha1, RomGameCode )?.Path;

	public void ClearSessionAfterHostLost()
	{
		EndLinkedPlay();
		ClearSessionState();
	}

	public void SetReady( bool ready )
	{
		if ( !IsActive )
			return;
		if ( IsHost )
			return;

		RpcSetReady( ready );
	}

	public void StartGame()
	{
		if ( !CanStart )
			return;
		RpcBeginGame();
	}

	public void OnActive( Connection conn )
	{
		if ( !Networking.IsHost && State == SessionState.Solo )
		{
			State = SessionState.Joined;
			LoadSessionFromLobbyData();
		}

		if ( !Networking.IsHost )
			return;

		HostAssignSlot( conn );

		if ( conn is not null && conn != Connection.Local )
		{
			if ( Mode == SessionMode.WirelessAdapter && State == SessionState.Solo )
				State = SessionState.InGame;

			using ( Rpc.FilterInclude( conn ) )
				RpcSyncSession( RomTitle ?? string.Empty, RomSha1 ?? string.Empty, RomGameCode ?? string.Empty, (int)Visibility, (int)Mode );
		}

		HostCommitRoster();
	}

	private void LoadSessionFromLobbyData()
	{
		var title = Networking.GetData( LobbyDataKeys.RomTitle );
		var sha1 = Networking.GetData( LobbyDataKeys.RomSha1 );
		var code = Networking.GetData( LobbyDataKeys.GameCode );

		if ( !string.IsNullOrEmpty( title ) )
			RomTitle = title;
		if ( !string.IsNullOrEmpty( sha1 ) )
			RomSha1 = sha1;
		if ( !string.IsNullOrEmpty( code ) )
			RomGameCode = code;

		var vis = Networking.GetData( LobbyDataKeys.Visibility );
		if ( int.TryParse( vis, out var v ) )
			Visibility = (SessionVisibility)v;

		var modeRaw = Networking.GetData( LobbyDataKeys.Mode );
		if ( int.TryParse( modeRaw, out var m ) )
			Mode = (SessionMode)m;

		TryLoadJoinedRom();
	}

	public void OnDisconnected( Connection conn )
	{
		if ( conn is not null )
			_ready.Remove( conn.Id );

		if ( !Networking.IsHost )
			return;

		HostReleaseSlot( conn );
		HostCommitRoster();
	}

	private int HostAssignSlot( Connection conn )
	{
		if ( conn is null )
			return -1;

		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			if ( _slotConns[i] is not null && _slotConns[i].Id == conn.Id )
				return i;
		}

		if ( conn == Connection.Local )
		{
			if ( _slotConns[0] is null )
			{
				_slotConns[0] = conn;
				return 0;
			}
		}

		int start = conn.IsHost ? 0 : 1;
		int cap = PlayerCap;
		for ( int i = start; i < cap; i++ )
		{
			if ( _slotConns[i] is null )
			{
				_slotConns[i] = conn;
				return i;
			}
		}
		return -1;
	}

	private void HostReleaseSlot( Connection conn )
	{
		if ( conn is null )
			return;

		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			if ( _slotConns[i] is not null && _slotConns[i].Id == conn.Id )
				_slotConns[i] = null;
		}
	}

	private void HostCommitRoster()
	{
		if ( !Networking.IsHost )
			return;

		var local = Connection.Local;
		if ( local is not null )
		{
			bool present = false;
			for ( int i = 0; i < _slotConns.Length; i++ )
			{
				if ( _slotConns[i] is not null && _slotConns[i].Id == local.Id )
				{
					present = true;
					break;
				}
			}
			if ( !present )
				HostAssignSlot( local );
		}

		RebuildLocalSlotState();

		var payload = SerializeRoster();
		RpcSyncRoster( payload );
	}

	private List<int> HostActiveSlots()
	{
		var slots = new List<int>( _slotConns.Length );
		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			if ( _slotConns[i] is not null && _slotConns[i].IsActive )
				slots.Add( i );
		}
		return slots;
	}

	private void RebuildLocalSlotState()
	{
		_slotByConn.Clear();
		_playerView.Clear();
		_localSlot = -1;

		var localId = Connection.Local?.Id ?? Guid.Empty;
		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			var c = _slotConns[i];
			if ( c is null )
				continue;

			_slotByConn[c.Id] = i;
			var ready = c.IsHost || _ready.GetValueOrDefault( c.Id );
			_playerView.Add( new SessionPlayer( c, i ) { Ready = ready } );
			if ( c.Id == localId )
				_localSlot = i;
		}
	}

	private byte[] SerializeRoster()
	{
		int count = 0;
		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			if ( _slotConns[i] is not null )
				count++;
		}

		using var ms = new System.IO.MemoryStream();
		using var w = new System.IO.BinaryWriter( ms );
		w.Write( count );
		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			var c = _slotConns[i];
			if ( c is null )
				continue;

			w.Write( i );
			w.Write( c.Id.ToByteArray() );
			w.Write( c.SteamId );
			w.Write( c.IsHost );
			w.Write( c.IsHost || _ready.GetValueOrDefault( c.Id ) );
			w.Write( c.DisplayName ?? string.Empty );
		}
		return ms.ToArray();
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcSyncRoster( byte[] payload )
	{
		if ( Networking.IsHost || payload is null || payload.Length < 4 )
			return;

		Array.Clear( _slotConns, 0, _slotConns.Length );
		_slotByConn.Clear();
		_playerView.Clear();
		_localSlot = -1;

		var localId = Connection.Local?.Id ?? Guid.Empty;

		try
		{
			using var ms = new System.IO.MemoryStream( payload );
			using var r = new System.IO.BinaryReader( ms );
			int count = r.ReadInt32();
			for ( int n = 0; n < count; n++ )
			{
				int slot = r.ReadInt32();
				var id = new Guid( r.ReadBytes( 16 ) );
				ulong steamId = r.ReadUInt64();
				bool isHost = r.ReadBoolean();
				bool ready = r.ReadBoolean();
				string name = r.ReadString();

				if ( slot < 0 || slot >= _slotConns.Length )
					continue;

				var player = new SessionPlayer( slot, id, steamId, name, isHost, ready );
				_slotConns[slot] = player.Connection;
				_slotByConn[id] = slot;
				_playerView.Add( player );
				if ( id == localId )
					_localSlot = slot;
			}
		}
		catch ( Exception e )
		{
			Log.Warning( $"[sGBA] RpcSyncRoster parse failed: {e.Message}" );
		}
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcSetReady( bool ready )
	{
		var caller = Rpc.Caller;
		if ( caller is null )
			return;

		_ready[caller.Id] = ready;
		HostCommitRoster();
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcSyncSession( string romTitle, string romSha1, string gameCode, int visibility, int mode )
	{
		if ( Networking.IsHost )
			return;

		if ( !string.IsNullOrEmpty( romTitle ) )
			RomTitle = romTitle;
		if ( !string.IsNullOrEmpty( romSha1 ) )
			RomSha1 = romSha1;
		if ( !string.IsNullOrEmpty( gameCode ) )
			RomGameCode = gameCode;
		Visibility = (SessionVisibility)visibility;
		Mode = (SessionMode)mode;
		State = SessionState.Joined;

		TryLoadJoinedRom();
	}

	private bool _joinedRomLoaded;

	private void TryLoadJoinedRom()
	{
		if ( _joinedRomLoaded )
			return;

		if ( string.IsNullOrEmpty( RomSha1 ) && string.IsNullOrEmpty( RomGameCode ) )
			return;

		var match = FindLocalRomMatch( RomSha1, RomGameCode );
		if ( match is null )
		{
			Log.Warning( $"[sGBA] Join: no matching ROM for '{RomTitle}' (code {RomGameCode}); returning to home." );
			Leave();
			HomeScreen.Current?.Show();
			HomeScreen.Current?.ShowToast( "#toast.missingrom.title", "#toast.missingrom.message", "warning", "orange" );
			return;
		}

		_joinedRomLoaded = true;
		EmulatorComponent.Current?.Restart( match.Path );
		HomeScreen.Current?.Hide();
	}

	private static GameEntry FindLocalRomMatch( string sha1, string gameCode )
	{
		List<GameEntry> roms;
		try { roms = GameEntry.Discover(); }
		catch ( Exception e ) { Log.Warning( $"[sGBA] GameEntry.Discover failed: {e.Message}" ); return null; }

		if ( !string.IsNullOrEmpty( sha1 ) )
		{
			foreach ( var r in roms )
			{
				if ( string.Equals( ComputeRomSha1( r ), sha1, StringComparison.OrdinalIgnoreCase ) )
					return r;
			}
		}

		if ( !string.IsNullOrEmpty( gameCode ) )
		{
			return roms.FirstOrDefault( r => string.Equals( r.GameCode, gameCode, StringComparison.OrdinalIgnoreCase ) );
		}

		return null;
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcBeginGame()
	{
		State = SessionState.InGame;

		if ( Mode == SessionMode.LinkCable )
			RestartLinkedPlay();
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcJoinLinkedGame()
	{
		if ( Networking.IsHost )
			return;
		State = SessionState.InGame;
		EmulatorComponent.Current?.BeginLinkedClient();
	}


	private static LobbyPrivacy ToLobbyPrivacy( SessionVisibility v ) => v switch
	{
		SessionVisibility.Public => LobbyPrivacy.Public,
		SessionVisibility.FriendsOnly => LobbyPrivacy.FriendsOnly,
		SessionVisibility.InviteOnly => LobbyPrivacy.Private,
		_ => LobbyPrivacy.Public
	};

	private static string ComputeRomSha1( GameEntry rom )
	{
		try
		{
			var bytes = rom.FileSystem.ReadAllBytes( rom.Path ).ToArray();
			return Convert.ToHexString( System.Security.Cryptography.SHA1.HashData( bytes ) );
		}
		catch
		{
			return string.Empty;
		}
	}

	private void TryWireAdapter()
	{
		var adapter = Adapter;
		if ( adapter is null )
			return;
		adapter.Network = this;
		_wiredAdapter = adapter;
	}

	void IWirelessNetwork.Send( int clientId, byte[] data, int length )
	{
		if ( data is null || length <= 0 )
			return;

		var payload = new byte[length];
		Buffer.BlockCopy( data, 0, payload, 0, length );
		_outbox.Enqueue( (clientId, payload) );
	}

	private void DispatchSend( int clientId, byte[] payload )
	{
		bool isAv = Mode == SessionMode.LinkCable && payload is not null && payload.Length > 0
			&& payload[0] == (byte)LinkCablePacketType.Av;

		if ( clientId == BroadcastClientId )
		{
			if ( isAv )
				RpcReceiveAv( payload );
			else
				RpcReceive( payload );
			return;
		}

		var target = ResolveSlotConnection( clientId );
		if ( target is null || target == Connection.Local )
			return;

		using ( Rpc.FilterInclude( target ) )
		{
			if ( isAv )
				RpcReceiveAv( payload );
			else
				RpcReceive( payload );
		}
	}

	private Connection ResolveSlotConnection( int slot )
	{
		if ( slot < 0 || slot >= _slotConns.Length )
			return null;

		var c = _slotConns[slot];
		if ( c is not null )
			return c;

		if ( slot == 0 && !Networking.IsHost )
		{
			var all = Connection.All;
			if ( all is not null )
			{
				for ( int i = 0; i < all.Count; i++ )
				{
					if ( all[i] is not null && all[i].IsHost )
						return all[i];
				}
			}
		}
		return null;
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	private void RpcReceive( byte[] payload )
	{
		HandleReceive( payload );
	}

	[Rpc.Broadcast( NetFlags.UnreliableNoDelay )]
	private void RpcReceiveAv( byte[] payload )
	{
		HandleReceive( payload );
	}

	private void HandleReceive( byte[] payload )
	{
		if ( Rpc.Caller == Connection.Local )
			return;

		if ( payload is null || payload.Length < 12 )
			return;

		if ( !_slotByConn.TryGetValue( Rpc.Caller.Id, out var senderIndex ) )
			return;

		if ( Mode == SessionMode.LinkCable )
			RouteLinkCablePacket( senderIndex, payload );
		else
			Adapter?.EnqueueReceive( senderIndex, payload, payload.Length );
	}
}
