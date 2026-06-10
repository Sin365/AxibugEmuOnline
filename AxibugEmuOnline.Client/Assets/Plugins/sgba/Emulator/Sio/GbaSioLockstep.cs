namespace sGBA;

public interface ILockstepUser
{
	void Sleep();
	void Wake();
	int RequestedId();
	void PlayerIdChanged( int id );
}

public enum GbaSioLockstepEventType
{
	Attach,
	Detach,
	HardSync,
	ModeSet,
	TransferStart,
}

public sealed class GbaSioLockstepEvent
{
	public GbaSioLockstepEventType Type;
	public int Timestamp;
	public int PlayerId;
	public GbaSioMode Mode;
	public int FinishCycle;
}

public sealed class GbaSioLockstepPlayer
{
	public GbaSioLockstepDriver Driver;
	public int PlayerId;
	public GbaSioMode Mode;
	public readonly GbaSioMode[] OtherModes = new GbaSioMode[GbaSioLockstepCoordinator.MaxGbas];
	public bool Asleep;
	public int CycleOffset;
	public readonly List<GbaSioLockstepEvent> Queue = new();
	public bool DataReceived;

	public int LockstepTime => Driver.CurrentTime - CycleOffset;
}

public sealed class GbaSioLockstepCoordinator
{
	public const int MaxGbas = 4;
	public const int LockstepInterval = 4096;
	public const int UnlockedInterval = 4096;
	public const int HardSyncInterval = 0x80000;

	internal readonly object Sync = new();

	private readonly Dictionary<uint, GbaSioLockstepPlayer> _players = new();
	private uint _nextId;

	public readonly uint[] AttachedPlayers = new uint[MaxGbas];
	public int NAttached;
	public uint Waiting;

	public bool TransferActive;
	public GbaSioMode TransferMode;

	public int Cycle;
	public int NextHardSync;

	public readonly ushort[] MultiData = new ushort[4];
	public readonly uint[] NormalData = new uint[4];

	public int PlayerCount
	{
		get { lock ( Sync ) return _players.Count; }
	}

	public GbaSioLockstepPlayer Lookup( uint id )
	{
		return _players.TryGetValue( id, out var p ) ? p : null;
	}

	internal Dictionary<uint, GbaSioLockstepPlayer> Players => _players;

	public void Attach( GbaSioLockstepDriver driver )
	{
		if ( driver.Coordinator != null && driver.Coordinator != this )
			throw new InvalidOperationException( "Driver already attached to a different coordinator" );
		driver.Coordinator = this;
	}

	public void Detach( GbaSioLockstepDriver driver )
	{
		if ( driver.Coordinator != this )
			return;
		lock ( Sync )
		{
			var player = Lookup( driver.LockstepId );
			if ( player != null )
				RemovePlayer( player );
		}
		driver.Coordinator = null;
	}

	internal uint RegisterPlayer( GbaSioLockstepPlayer player )
	{
		while ( true )
		{
			if ( _nextId == uint.MaxValue )
				_nextId = 0;
			++_nextId;
			uint id = _nextId;
			if ( !_players.ContainsKey( id ) )
			{
				_players[id] = player;
				return id;
			}
		}
	}

	internal int UntilNextSync( GbaSioLockstepPlayer player )
	{
		int cycle = Cycle - player.LockstepTime;
		if ( player.PlayerId == 0 )
			cycle += NAttached < 2 ? UnlockedInterval : LockstepInterval;
		return cycle;
	}

	internal void AdvanceCycle( GbaSioLockstepPlayer player )
	{
		int newCycle = player.LockstepTime;
		NextHardSync -= newCycle - Cycle;
		Cycle = newCycle;
	}

	internal void RemovePlayer( GbaSioLockstepPlayer player )
	{
		var detach = new GbaSioLockstepEvent
		{
			Type = GbaSioLockstepEventType.Detach,
			PlayerId = player.PlayerId,
			Timestamp = player.LockstepTime,
		};
		EnqueueEvent( detach, TargetAll & ~Target( player.PlayerId ) );

		Waiting = 0;
		TransferActive = false;

		_players.Remove( player.Driver.LockstepId );
		ReconfigPlayers();

		var runner = Lookup( AttachedPlayers[0] );
		runner?.Wake();
	}

	internal void ReconfigPlayers()
	{
		int players = _players.Count;
		Array.Clear( AttachedPlayers, 0, AttachedPlayers.Length );

		if ( players == 0 )
		{
			NAttached = 0;
			return;
		}

		if ( players == 1 )
		{
			using var e = _players.GetEnumerator();
			e.MoveNext();
			uint p0 = e.Current.Key;
			AttachedPlayers[0] = p0;

			var player = e.Current.Value;
			Cycle = player.Driver.CurrentTime;
			NextHardSync = HardSyncInterval;

			if ( player.PlayerId != 0 )
			{
				player.PlayerId = 0;
				player.Driver.User?.PlayerIdChanged( 0 );
			}

			if ( !TransferActive )
				TransferMode = player.Mode;
		}
		else
		{
			var preferences = new List<uint>[MaxGbas];
			for ( int i = 0; i < MaxGbas; ++i )
				preferences[i] = [];

			int seen = 0;
			foreach ( var kv in _players )
			{
				if ( seen >= MaxGbas )
					break;
				int requested = kv.Value.Driver.User?.RequestedId() ?? (MaxGbas - 1);
				if ( requested < 0 )
					continue;
				if ( requested >= MaxGbas )
					requested = MaxGbas - 1;
				preferences[requested].Add( kv.Key );
				++seen;
			}

			seen = 0;
			for ( int i = 0; i < MaxGbas; ++i )
			{
				for ( int j = 0; j <= i; ++j )
				{
					while ( preferences[j].Count > 0 && seen < MaxGbas )
					{
						uint pid = preferences[j][0];
						preferences[j].RemoveAt( 0 );
						var player = Lookup( pid );
						if ( player == null )
							continue;
						AttachedPlayers[seen] = pid;
						if ( player.PlayerId != seen )
						{
							player.PlayerId = seen;
							player.Driver.User?.PlayerIdChanged( seen );
						}
						++seen;
					}
				}
			}
		}

		int nAttached = 0;
		for ( int i = 0; i < MaxGbas; ++i )
		{
			uint pid = AttachedPlayers[i];
			if ( pid == 0 )
				continue;
			if ( Lookup( pid ) == null )
				AttachedPlayers[i] = 0;
			else
				++nAttached;
		}
		NAttached = nAttached;
	}

	internal void SetData( int id, GbaIo sio )
	{
		switch ( TransferMode )
		{
			case GbaSioMode.Multi:
				MultiData[id] = sio.GetSioReg( 5 );
				break;
			case GbaSioMode.Normal8:
				NormalData[id] = sio.GetSioReg( 5 );
				break;
			case GbaSioMode.Normal32:
				NormalData[id] = (uint)(sio.GetSioReg( 0 ) | (sio.GetSioReg( 1 ) << 16));
				break;
		}
	}

	internal void SetReady( GbaSioLockstepPlayer activePlayer, int playerId, GbaSioMode mode )
	{
		activePlayer.OtherModes[playerId] = mode;
		bool ready = true;
		for ( int i = 0; ready && i < NAttached; ++i )
			ready = activePlayer.OtherModes[i] == activePlayer.Mode;

		if ( activePlayer.Mode == GbaSioMode.Multi )
		{
			var sio = activePlayer.Driver.Sio;
			if ( ready )
				sio.SioCnt |= 0x0008;
			else
				sio.SioCnt &= unchecked((ushort)~0x0008);
			if ( ready )
				sio.Rcnt |= 0x0002;
			else
				sio.Rcnt &= unchecked((ushort)~0x0002);
		}
	}

	internal void HardSync( GbaSioLockstepPlayer player )
	{
		var ev = new GbaSioLockstepEvent
		{
			Type = GbaSioLockstepEventType.HardSync,
			PlayerId = 0,
			Timestamp = player.LockstepTime,
		};
		EnqueueEvent( ev, TargetSecondary );
		WaitOnPlayers( player );
	}

	internal void EnqueueEvent( GbaSioLockstepEvent template, uint target )
	{
		for ( int i = 0; i < NAttached; ++i )
		{
			if ( (target & Target( i )) == 0 )
				continue;
			var player = Lookup( AttachedPlayers[i] );
			if ( player == null )
				continue;

			var newEvent = new GbaSioLockstepEvent
			{
				Type = template.Type,
				Timestamp = template.Timestamp,
				PlayerId = template.PlayerId,
				Mode = template.Mode,
				FinishCycle = template.FinishCycle,
			};

			int idx = 0;
			while ( idx < player.Queue.Count && newEvent.Timestamp - player.Queue[idx].Timestamp >= 0 )
				++idx;
			player.Queue.Insert( idx, newEvent );
		}
	}

	internal void WaitOnPlayers( GbaSioLockstepPlayer player )
	{
		if ( NAttached < 2 )
			return;

		AdvanceCycle( player );
		Waiting = (uint)((1 << NAttached) - 1) & ~Target( player.PlayerId );
		player.Sleep();
		WakePlayers();
	}

	internal void WakePlayers()
	{
		for ( int i = 1; i < NAttached; ++i )
		{
			if ( AttachedPlayers[i] == 0 )
				continue;
			var player = Lookup( AttachedPlayers[i] );
			player?.Wake();
		}
	}

	internal void AckPlayer( GbaSioLockstepPlayer player )
	{
		if ( player.PlayerId == 0 )
			return;
		Waiting &= ~Target( player.PlayerId );
		if ( Waiting == 0 )
		{
			if ( TransferActive )
			{
				for ( int i = 0; i < NAttached; ++i )
				{
					if ( AttachedPlayers[i] == 0 )
						continue;
					var p = Lookup( AttachedPlayers[i] );
					p?.DataReceived = true;
				}
				TransferActive = false;
			}

			var runner = Lookup( AttachedPlayers[0] );
			runner?.Wake();
		}
		player.Sleep();
	}

	internal void AbortTransfer( GbaSioLockstepPlayer player )
	{
		TransferActive = false;
		Waiting = 0;

		if ( player.PlayerId != 0 )
		{
			var runner = Lookup( AttachedPlayers[0] );
			runner?.Wake();
		}
		else
		{
			WakePlayers();
		}
	}

	internal static uint Target( int p ) => (uint)(1 << p);
	internal const uint TargetAll = 0xF;
	internal const uint TargetPrimary = 0x1;
	internal const uint TargetSecondary = TargetAll & ~TargetPrimary;
}

public sealed class GbaSioLockstepDriver : IGbaSioDriver
{
	public GbaIo Sio { get; set; }
	public GbaSioLockstepCoordinator Coordinator;
	public uint LockstepId;
	public ILockstepUser User;

	public GbaSioLockstepDriver( ILockstepUser user )
	{
		User = user;
	}

	public int CurrentTime => unchecked((int)Sio.Gba.Cpu.Cycles);

	public bool Init()
	{
		Sio.DriverEventCallback = LockstepEvent;
		Reset();
		return true;
	}

	public void Deinit()
	{
		var coordinator = Coordinator;
		if ( coordinator != null )
		{
			lock ( coordinator.Sync )
			{
				var player = coordinator.Lookup( LockstepId );
				if ( player != null )
					coordinator.RemovePlayer( player );
			}
		}
		Sio.DescheduleDriverEvent();
		LockstepId = 0;
	}

	public void Reset()
	{
		var coordinator = Coordinator;
		if ( coordinator == null )
			return;

		lock ( coordinator.Sync )
		{
			GbaSioLockstepPlayer player;
			if ( LockstepId == 0 )
			{
				player = new GbaSioLockstepPlayer
				{
					Driver = this,
					Mode = Sio.SioMode,
					PlayerId = -1,
				};

				LockstepId = coordinator.RegisterPlayer( player );
				coordinator.ReconfigPlayers();
				player.CycleOffset = CurrentTime - coordinator.Cycle;
				if ( player.PlayerId != 0 )
				{
					var ev = new GbaSioLockstepEvent
					{
						Type = GbaSioLockstepEventType.Attach,
						PlayerId = player.PlayerId,
						Timestamp = player.LockstepTime,
					};
					coordinator.EnqueueEvent( ev, GbaSioLockstepCoordinator.TargetAll & ~GbaSioLockstepCoordinator.Target( player.PlayerId ) );
				}
			}
			else
			{
				player = coordinator.Lookup( LockstepId );
				player.CycleOffset = CurrentTime - coordinator.Cycle;
			}

			if ( coordinator.TransferActive )
			{
				coordinator.AbortTransfer( player );
				player.Asleep = false;
				Sio.LockstepBlocked = false;
			}
			if ( player.PlayerId == 0 && coordinator.NAttached > 1 )
			{
				coordinator.Waiting = 0;
				player.Asleep = false;
				Sio.LockstepBlocked = false;
				coordinator.WakePlayers();
			}

			if ( Sio.IsDriverEventScheduled )
				return;

			int nextEvent;
			coordinator.SetReady( player, player.PlayerId, player.Mode );
			if ( coordinator.Players.Count == 1 )
			{
				coordinator.Cycle = CurrentTime;
				nextEvent = GbaSioLockstepCoordinator.LockstepInterval;
			}
			else
			{
				coordinator.SetReady( player, 0, coordinator.TransferMode );
				nextEvent = coordinator.UntilNextSync( player );
			}
			Sio.ScheduleDriverEventIn( nextEvent );
		}
	}

	public void SetMode( GbaSioMode mode )
	{
		var coordinator = Coordinator;
		if ( coordinator == null )
			return;
		lock ( coordinator.Sync )
		{
			var player = coordinator.Lookup( LockstepId );
			if ( player == null || mode == player.Mode )
				return;
			player.Mode = mode;
			var ev = new GbaSioLockstepEvent
			{
				Type = GbaSioLockstepEventType.ModeSet,
				PlayerId = player.PlayerId,
				Timestamp = player.LockstepTime,
				Mode = mode,
			};
			if ( player.PlayerId == 0 )
			{
				coordinator.TransferMode = mode;
				coordinator.WaitOnPlayers( player );
			}
			coordinator.SetReady( player, player.PlayerId, mode );
			coordinator.EnqueueEvent( ev, GbaSioLockstepCoordinator.TargetAll & ~GbaSioLockstepCoordinator.Target( player.PlayerId ) );
		}
	}

	public bool HandlesMode( GbaSioMode mode ) => true;

	public int ConnectedDevices()
	{
		var coordinator = Coordinator;
		if ( coordinator == null || LockstepId == 0 )
			return 0;
		lock ( coordinator.Sync )
		{
			return coordinator.NAttached - 1;
		}
	}

	public int DeviceId()
	{
		var coordinator = Coordinator;
		if ( coordinator == null )
			return 0;
		lock ( coordinator.Sync )
		{
			var player = coordinator.Lookup( LockstepId );
			if ( player != null && player.PlayerId >= 0 )
				return player.PlayerId;
			return 0;
		}
	}

	public ushort WriteSioCnt( ushort value ) => value;

	public ushort WriteRcnt( ushort value ) => value;

	public bool Start()
	{
		var coordinator = Coordinator;
		if ( coordinator == null )
			return false;
		lock ( coordinator.Sync )
		{
			if ( coordinator.TransferActive )
				return false;
			if ( coordinator.NAttached < 2 )
				return false;
			var player = coordinator.Lookup( LockstepId );
			if ( player.PlayerId != 0 )
				return false;

			for ( int i = 0; i < coordinator.MultiData.Length; ++i )
				coordinator.MultiData[i] = 0xFFFF;
			coordinator.SetData( 0, Sio );

			int timestamp = player.LockstepTime;
			var ev = new GbaSioLockstepEvent
			{
				Type = GbaSioLockstepEventType.TransferStart,
				Timestamp = timestamp,
				FinishCycle = timestamp + Sio.SioTransferCyclesForLockstep( coordinator.NAttached - 1 ),
			};
			coordinator.EnqueueEvent( ev, GbaSioLockstepCoordinator.TargetSecondary );
			coordinator.WaitOnPlayers( player );
			coordinator.TransferActive = true;
			return true;
		}
	}

	public void FinishMultiplayer( ushort[] data )
	{
		var coordinator = Coordinator;
		lock ( coordinator.Sync )
		{
			if ( coordinator.TransferMode == GbaSioMode.Multi )
			{
				var player = coordinator.Lookup( LockstepId );
				if ( !player.DataReceived )
				{
					for ( int i = 0; i < 4; ++i )
						data[i] = 0xFFFF;
				}
				else
				{
					for ( int i = 0; i < 4; ++i )
						data[i] = coordinator.MultiData[i];
				}
				player.DataReceived = false;
				if ( player.PlayerId == 0 )
					coordinator.HardSync( player );
			}
		}
	}

	public byte FinishNormal8()
	{
		var coordinator = Coordinator;
		byte data = 0xFF;
		lock ( coordinator.Sync )
		{
			if ( coordinator.TransferMode == GbaSioMode.Normal8 )
			{
				var player = coordinator.Lookup( LockstepId );
				if ( player.PlayerId > 0 && player.DataReceived )
					data = (byte)coordinator.NormalData[player.PlayerId - 1];
				player.DataReceived = false;
				if ( player.PlayerId == 0 )
					coordinator.HardSync( player );
			}
		}
		return data;
	}

	public uint FinishNormal32()
	{
		var coordinator = Coordinator;
		uint data = 0xFFFFFFFF;
		lock ( coordinator.Sync )
		{
			if ( coordinator.TransferMode == GbaSioMode.Normal32 )
			{
				var player = coordinator.Lookup( LockstepId );
				if ( player.PlayerId > 0 && player.DataReceived )
					data = coordinator.NormalData[player.PlayerId - 1];
				player.DataReceived = false;
				if ( player.PlayerId == 0 )
					coordinator.HardSync( player );
			}
		}
		return data;
	}

	private void LockstepEvent( int cyclesLate )
	{
		var coordinator = Coordinator;
		if ( coordinator == null )
			return;

		lock ( coordinator.Sync )
		{
			var player = coordinator.Lookup( LockstepId );
			if ( player == null )
				return;
			var sio = player.Driver.Sio;

			bool wasDetach = player.Queue.Count > 0 && player.Queue[0].Type == GbaSioLockstepEventType.Detach;

			if ( player.PlayerId == 0 && player.LockstepTime - coordinator.Cycle >= 0 )
			{
				coordinator.AdvanceCycle( player );
				if ( !coordinator.TransferActive )
					coordinator.WakePlayers();
				if ( coordinator.NextHardSync < 0 )
				{
					if ( coordinator.Waiting == 0 )
						coordinator.HardSync( player );
					coordinator.NextHardSync += GbaSioLockstepCoordinator.HardSyncInterval;
				}
			}

			int nextEvent = coordinator.UntilNextSync( player );
			while ( player.Queue.Count > 0 )
			{
				var ev = player.Queue[0];
				if ( ev.Timestamp > player.LockstepTime )
					break;
				player.Queue.RemoveAt( 0 );

				var reply = new GbaSioLockstepEvent
				{
					PlayerId = player.PlayerId,
					Timestamp = player.LockstepTime,
				};

				switch ( ev.Type )
				{
					case GbaSioLockstepEventType.Attach:
						coordinator.SetReady( player, ev.PlayerId, (GbaSioMode)(-1) );
						if ( player.PlayerId == 0 )
							sio.SioCnt &= unchecked((ushort)~0x0004);
						reply.Mode = player.Mode;
						reply.Type = GbaSioLockstepEventType.ModeSet;
						coordinator.EnqueueEvent( reply, GbaSioLockstepCoordinator.Target( ev.PlayerId ) );
						break;
					case GbaSioLockstepEventType.HardSync:
						coordinator.AckPlayer( player );
						break;
					case GbaSioLockstepEventType.TransferStart:
						coordinator.SetData( player.PlayerId, sio );
						nextEvent = ev.FinishCycle - player.LockstepTime - cyclesLate;
						sio.SioCnt |= 0x80;
						sio.ScheduleSioCompletion( sio.Gba.Cpu.Cycles + nextEvent );
						coordinator.AckPlayer( player );
						break;
					case GbaSioLockstepEventType.ModeSet:
						if ( coordinator.TransferActive && player.Mode != ev.Mode )
							coordinator.AbortTransfer( player );
						coordinator.SetReady( player, ev.PlayerId, ev.Mode );
						if ( ev.PlayerId == 0 )
							coordinator.AckPlayer( player );
						break;
					case GbaSioLockstepEventType.Detach:
						coordinator.SetReady( player, ev.PlayerId, (GbaSioMode)(-1) );
						coordinator.SetReady( player, player.PlayerId, player.Mode );
						reply.Mode = player.Mode;
						reply.Type = GbaSioLockstepEventType.ModeSet;
						coordinator.EnqueueEvent( reply, ~GbaSioLockstepCoordinator.Target( ev.PlayerId ) );
						if ( player.Mode == GbaSioMode.Multi )
						{
							sio.SioCnt = (ushort)((sio.SioCnt & ~0x0030) | ((player.PlayerId & 3) << 4));
							bool slave = player.PlayerId != 0 || coordinator.NAttached < 2;
							if ( slave )
								sio.SioCnt |= 0x0004;
							else
								sio.SioCnt &= unchecked((ushort)~0x0004);
						}
						wasDetach = true;
						break;
				}
			}

			if ( player.Queue.Count > 0 && player.Queue[0].Timestamp - player.LockstepTime < nextEvent )
				nextEvent = player.Queue[0].Timestamp - player.LockstepTime;

			if ( player.PlayerId != 0 && nextEvent <= GbaSioLockstepCoordinator.LockstepInterval )
			{
				if ( player.Queue.Count == 0 || wasDetach )
				{
					player.Sleep();
					if ( nextEvent < 4 )
						nextEvent = 4;
				}
			}

			if ( nextEvent <= 0 )
				nextEvent = 4;
			sio.ScheduleDriverEventIn( nextEvent );
		}
	}
}

public static class GbaSioLockstepPlayerExtensions
{
	public static void Wake( this GbaSioLockstepPlayer player )
	{
		if ( !player.Asleep )
			return;
		player.Asleep = false;
		player.Driver.Sio.LockstepBlocked = false;
		player.Driver.User?.Wake();
	}

	public static void Sleep( this GbaSioLockstepPlayer player )
	{
		if ( player.Asleep )
			return;
		player.Asleep = true;
		player.Driver.Sio.LockstepBlocked = true;
		player.Driver.User?.Sleep();
	}
}
