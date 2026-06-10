namespace sGBA;

public sealed partial class NetworkManager
{
	private const int HostClientId = 0;

	public LinkCableSession LinkSession { get; private set; }
	public Action<LinkCableAvPacket> OnRemoteAv;
	public Action<LinkCableStatePacket> OnRemoteState;

	public bool ExternalAvDriver { get; set; }

	public void AttachLinkSession( LinkCableSession session )
	{
		LinkSession = session;
	}

	public void DetachLinkSession()
	{
		LinkSession = null;
	}

	public void BeginLinkedPlay()
	{
		var emu = EmulatorComponent.Current;
		if ( emu == null )
			return;

		if ( IsHost )
			emu.BeginLinkedHost( HostActiveSlots() );
		else if ( IsClient )
			emu.BeginLinkedClient();
	}

	private bool _linkActive;
	private int _linkedMask;
	private int _pendingRosterMask = -1;
	private RealTimeSince _timeSinceRosterChange;
	private const float LinkRosterDebounceSeconds = 1.5f;

	public void UpdateLinkRoster()
	{
		if ( !Networking.IsHost )
			return;

		var emu = EmulatorComponent.Current;
		if ( emu == null )
			return;

		int mask = ActiveSlotMask();

		if ( mask != _pendingRosterMask )
		{
			_pendingRosterMask = mask;
			_timeSinceRosterChange = 0;
			return;
		}
		if ( _timeSinceRosterChange < LinkRosterDebounceSeconds )
			return;
		if ( mask == _linkedMask )
			return;

		int count = CountBits( mask );

		if ( !_linkActive )
		{
			if ( count < 2 )
				return;

			_linkActive = true;
			_linkedMask = mask;
			State = SessionState.InGame;
			RpcBeginGame();
			return;
		}

		if ( count < 2 )
		{
			_linkActive = false;
			_linkedMask = 0;
			ResetHostToSolo();
			emu.ContinueSoloFromLinkedHost();
			return;
		}

		int additions = mask & ~_linkedMask;
		_linkedMask = mask;
		emu.SyncLinkedHostSlots( HostActiveSlots() );

		for ( int slot = 0; slot < _slotConns.Length; slot++ )
		{
			if ( (additions & (1 << slot)) == 0 )
				continue;
			var conn = _slotConns[slot];
			if ( conn is not null && conn != Connection.Local )
			{
				using ( Rpc.FilterInclude( conn ) )
					RpcJoinLinkedGame();
			}
		}
	}

	private void ResetHostToSolo()
	{
		var local = Connection.Local;
		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			if ( _slotConns[i] is not null && _slotConns[i] != local )
				_slotConns[i] = null;
		}
		HostCommitRoster();
		State = SessionState.Solo;
	}

	private int ActiveSlotMask()
	{
		int mask = 0;
		for ( int i = 0; i < _slotConns.Length; i++ )
		{
			if ( _slotConns[i] is not null && _slotConns[i].IsActive )
				mask |= 1 << i;
		}
		return mask;
	}

	private static int CountBits( int mask )
	{
		int n = 0;
		while ( mask != 0 )
		{
			n += mask & 1;
			mask >>= 1;
		}
		return n;
	}

	private void RestartLinkedPlay()
	{
		var emu = EmulatorComponent.Current;
		if ( emu == null )
			return;
		if ( emu.IsLinked )
			emu.EndLinkedMode();
		BeginLinkedPlay();
	}

	public void EndLinkedPlay()
	{
		_linkActive = false;
		_linkedMask = 0;
		_pendingRosterMask = -1;
		EmulatorComponent.Current?.EndLinkedMode();
	}

	public void SendLocalInput( long frame, ushort keys )
	{
		if ( !IsClient )
			return;

		var packet = new LinkCableInputPacket( frame, keys ).Serialize();
		SendLinkPacket( HostClientId, packet );
	}

	public void SendLocalSave( byte[] saveData )
	{
		if ( !IsClient || saveData == null || saveData.Length == 0 )
			return;

		var packet = new LinkCableSaveUploadPacket
		{
			PlayerId = LocalPlayer?.Slot ?? -1,
			SaveData = saveData,
		}.Serialize();
		SendLinkPacket( HostClientId, packet );
	}

	public void PumpLinkCable()
	{
		var session = LinkSession;
		if ( session is null || !IsHost )
			return;

		if ( ExternalAvDriver )
			return;

		var instances = session.Host.Instances;
		for ( int i = 0; i < instances.Count; i++ )
		{
			var inst = instances[i];
			int slot = inst.Slot;
			if ( slot < 0 )
				continue;

			while ( session.TryDequeueAv( inst, out var packet ) )
			{
				var data = packet.Serialize();
				SendLinkPacket( slot, data );
			}
		}
	}

	public void HostSendAv( int slot, LinkCableAvPacket packet )
	{
		if ( !IsHost || packet is null )
			return;

		var data = packet.Serialize();
		SendLinkPacket( slot, data );
	}

	public void HostSendState( int slot, byte[] state )
	{
		if ( !IsHost || state is null || state.Length == 0 )
			return;

		var data = new LinkCableStatePacket { PlayerId = slot, State = state }.Serialize();
		SendLinkPacket( slot, data );
	}

	private void RouteLinkCablePacket( int senderIndex, byte[] payload )
	{
		if ( payload.Length < 1 )
			return;

		switch ( (LinkCablePacketType)payload[0] )
		{
			case LinkCablePacketType.Input:
				RouteInput( senderIndex, payload );
				break;
			case LinkCablePacketType.SaveUpload:
				RouteSaveUpload( senderIndex, payload );
				break;
			case LinkCablePacketType.Av:
				RouteAv( payload );
				break;
			case LinkCablePacketType.State:
				RouteState( payload );
				break;
		}
	}

	private void RouteState( byte[] payload )
	{
		if ( !IsClient )
			return;

		if ( LinkCableStatePacket.TryParse( payload, payload.Length, out var packet ) )
		{
			try { OnRemoteState?.Invoke( packet ); }
			catch ( Exception e ) { Log.Warning( $"[sGBA] OnRemoteState error: {e.Message}" ); }
		}
	}

	private void RouteInput( int senderIndex, byte[] payload )
	{
		if ( !IsHost )
			return;

		var session = LinkSession;
		if ( session is null )
			return;

		if ( LinkCableInputPacket.TryParse( payload, payload.Length, out var packet ) )
			session.SetSlotInput( senderIndex, packet.Keys );
	}

	private void RouteSaveUpload( int senderIndex, byte[] payload )
	{
		if ( !IsHost )
			return;

		var session = LinkSession;
		if ( session is null )
			return;

		if ( LinkCableSaveUploadPacket.TryParse( payload, payload.Length, out var packet ) )
			session.LoadSlotSave( senderIndex, packet.SaveData );
	}

	private void RouteAv( byte[] payload )
	{
		if ( !IsClient )
			return;

		if ( LinkCableAvPacket.TryParse( payload, payload.Length, out var packet ) )
		{
			try { OnRemoteAv?.Invoke( packet ); }
			catch ( Exception e ) { Log.Warning( $"[sGBA] OnRemoteAv error: {e.Message}" ); }
		}
	}

	private void SendLinkPacket( int clientId, byte[] data )
	{
		((IWirelessNetwork)this).Send( clientId, data, data.Length );
	}
}
