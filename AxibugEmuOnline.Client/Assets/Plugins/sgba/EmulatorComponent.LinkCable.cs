using Sandbox.Rendering;

namespace sGBA;

public sealed partial class EmulatorComponent
{
	private LinkCableSession _linkSession;
	private readonly List<LinkCableInstance> _linkInstances = new();
	private bool _linkedHost;
	private bool _linkedClient;
	private Texture _clientScreenTex;
	private long _clientLastVideoFrame = -1;
	private long _clientInputFrame;
	private bool _clientSaveUploaded;

	private byte[] _clientPendingVideo;
	private long _clientPendingFrame = -1;
	private byte[] _clientLatestState;
	private RealTimeSince _timeSinceClientPacket;
	private const float ClientHostTimeoutSeconds = 2f;
	private const float StateStreamIntervalSeconds = 0.5f;
	private const int StateRewarmFrames = 2;

	public bool IsLinkedHost => _linkedHost;
	public bool IsLinkedClient => _linkedClient;
	public bool IsLinked => _linkedHost || _linkedClient;

	public void BeginLinkedHost( IReadOnlyList<int> activeSlots )
	{
		if ( _linkedHost || _linkedClient )
			return;
		if ( string.IsNullOrEmpty( RomPath ) )
			return;

		byte[] romData = ReadRomBytes();
		if ( romData == null )
			return;

		GbaCoreThread soloThread = _coreThread;
		_coreThread = null;
		soloThread?.End();
		Gba soloCore = soloThread?.Core;

		_linkSession = new LinkCableSession();

		byte[] hostSave = ReadHostSave();

		if ( soloCore?.Savedata != null && soloCore.Savedata.Data.Length > 0 && _savePath != null )
			FileSystem.Data.WriteAllBytes( _savePath, soloCore.Savedata.Data );

		if ( _soundHandle is { IsValid: true } )
			_soundHandle.Volume = 0;
		_audioStream?.Dispose();
		_audioStream = null;

		Gba core0;
		if ( soloCore != null )
		{
			core0 = soloCore;
		}
		else
		{
			core0 = new();
			core0.LoadRom( romData );
			if ( hostSave != null )
				core0.Savedata.Load( hostSave );
			core0.Reset();
			core0.Video.InitGpu( scale: ComputeAutoScale() );
			core0.Video.SetReproduceClassicFeel( GamePreferences.ReproduceClassicFeel );

			if ( _camera.IsValid() && core0.Video.RenderCommandList != null )
				_camera.AddCommandList( core0.Video.RenderCommandList, Stage.AfterOpaque, 0 );
		}

		var inst0 = _linkSession.Attach( core0, 0 );
		_linkInstances.Add( inst0 );

		_appliedReproduceClassicFeel = GamePreferences.ReproduceClassicFeel;

		if ( activeSlots != null )
		{
			for ( int i = 0; i < activeSlots.Count; i++ )
			{
				int slot = activeSlots[i];
				if ( slot == 0 )
					continue;
				CreateHostInstance( slot, romData );
			}
		}

		ScreenTexture = inst0.Core.Video.OutputTexture;
		_stateBasePath = "states/" + System.IO.Path.GetFileNameWithoutExtension( RomPath );

		try { InitAudioStream(); }
		catch ( Exception audioEx ) { GbaLog.Write( LogCategory.GBAAudio, LogLevel.Warn, $"Audio init failed: {audioEx.Message}" ); }

		ResetVideoClock();
		_inputCooldown = 2;
		_linkSession.Start( LogCoreThreadError );
		_linkedHost = true;
		IsReady = true;

		var net = NetworkManager.Current;
		if ( net != null )
		{
			net.AttachLinkSession( _linkSession );
			net.ExternalAvDriver = true;
		}
	}

	public void BeginLinkedClient()
	{
		if ( _linkedHost || _linkedClient )
			return;

		TearDownCore();

		_clientScreenTex = Texture.Create( GbaConstants.ScreenWidth, GbaConstants.ScreenHeight )
			.WithFormat( ImageFormat.RGBA8888 )
			.WithDynamicUsage()
			.WithName( "sgba_client_screen" )
			.Finish();
		ScreenTexture = _clientScreenTex;
		_clientLastVideoFrame = -1;
		_clientInputFrame = 0;
		_clientSaveUploaded = false;
		_clientPendingVideo = null;
		_clientPendingFrame = -1;
		_clientLatestState = null;
		_timeSinceClientPacket = 0;

		try { InitAudioStream(); }
		catch ( Exception audioEx ) { GbaLog.Write( LogCategory.GBAAudio, LogLevel.Warn, $"Audio init failed: {audioEx.Message}" ); }

		_linkedClient = true;
		IsReady = true;

		var net = NetworkManager.Current;
		if ( net != null )
		{
			net.OnRemoteAv = OnRemoteAvPacket;
			net.OnRemoteState = OnRemoteStatePacket;
		}
	}

	public void OnRemoteStatePacket( LinkCableStatePacket packet )
	{
		if ( _linkedClient && packet?.State != null )
			_clientLatestState = packet.State;
	}

	public void ResumeSoloAfterHostLost()
	{
		string rom = RomPath;
		byte[] state = _clientLatestState;
		bool wasPaused = _paused;

		var net = NetworkManager.Current;
		if ( string.IsNullOrEmpty( rom ) )
			rom = net?.ResolveSessionRomPath();
		net?.ClearSessionAfterHostLost();

		if ( !string.IsNullOrEmpty( rom ) )
		{
			RestartSoloWithState( rom, state, wasPaused );
		}
		else
		{
			EndLinkedMode();
			HomeScreen.Current?.Show();
		}
	}

	public void ContinueSoloFromLinkedHost()
	{
		if ( !_linkedHost )
			return;

		byte[] live = CaptureHostSlot0State();
		string rom = RomPath;
		bool wasPaused = _paused;

		EndLinkedMode();

		if ( !string.IsNullOrEmpty( rom ) )
			RestartSoloWithState( rom, live, wasPaused );
	}

	private void RestartSoloWithState( string rom, byte[] state, bool wasPaused )
	{
		Restart( rom );
		if ( state != null )
			LoadStateBytes( state, wasPaused );
		if ( wasPaused )
			SetPaused( true );
	}

	private byte[] CaptureHostSlot0State()
	{
		for ( int i = 0; i < _linkInstances.Count; i++ )
		{
			var inst = _linkInstances[i];
			if ( inst.Slot != 0 )
				continue;
			try
			{
				lock ( inst.CoreLock )
					return GbaSerialize.Save( inst.Core, null );
			}
			catch ( Exception e )
			{
				GbaLog.Write( LogCategory.GBAState, LogLevel.Warn, $"Failed to capture host slot0 state: {e.Message}" );
				return null;
			}
		}
		return null;
	}

	private void LoadStateBytes( byte[] data, bool renderFrame = false )
	{
		if ( data == null || data.Length == 0 )
			return;

		RunOnCoreThread( core =>
		{
			try
			{
				GbaSerialize.Load( core, data );
				if ( renderFrame )
				{
					for ( int i = 0; i < StateRewarmFrames; i++ )
						core.RunFrame();
					_videoFramePending = true;
				}
			}
			catch ( Exception e ) { GbaLog.Write( LogCategory.GBAState, LogLevel.Error, $"Failed to load streamed state: {e.Message}" ); }
		} );
	}

	public void EndLinkedMode()
	{
		if ( !_linkedHost && !_linkedClient )
			return;

		var net = NetworkManager.Current;
		if ( net != null )
		{
			net.DetachLinkSession();
			net.ExternalAvDriver = false;
			net.OnRemoteAv = null;
			net.OnRemoteState = null;
		}

		if ( _linkedHost )
		{
			var session = _linkSession;
			_linkSession = null;
			_linkedHost = false;

			session?.Stop();

			foreach ( var inst in _linkInstances )
			{
				var core = inst.Core;
				if ( _camera.IsValid() && core?.Video?.RenderCommandList != null )
					_camera.RemoveCommandList( core.Video.RenderCommandList );
				core?.Video?.DisposeGpu();
			}
			_linkInstances.Clear();
			ScreenTexture = null;
		}

		if ( _linkedClient )
		{
			_linkedClient = false;
			_clientPendingVideo = null;
			_clientPendingFrame = -1;
			_clientLatestState = null;
			_clientScreenTex?.Dispose();
			_clientScreenTex = null;
			ScreenTexture = null;
		}

		if ( _soundHandle is { IsValid: true } )
			_soundHandle.Volume = 0;
		_audioStream?.Dispose();
		_audioStream = null;
		IsReady = false;
	}

	public void SyncLinkedHostSlots( IReadOnlyList<int> activeSlots )
	{
		if ( !_linkedHost || _linkSession == null || activeSlots == null )
			return;

		for ( int i = _linkInstances.Count - 1; i >= 0; i-- )
		{
			var inst = _linkInstances[i];
			if ( inst.Slot <= 0 )
				continue;
			if ( !ContainsSlot( activeSlots, inst.Slot ) )
				DestroyHostInstance( inst );
		}

		byte[] romData = null;
		for ( int i = 0; i < activeSlots.Count; i++ )
		{
			int slot = activeSlots[i];
			if ( slot <= 0 || slot >= NetworkManager.MaxLinkPlayers )
				continue;
			if ( HasHostInstance( slot ) )
				continue;

			romData ??= ReadRomBytes();
			if ( romData == null )
				return;

			CreateHostInstance( slot, romData );
		}
	}

	private static bool ContainsSlot( IReadOnlyList<int> slots, int slot )
	{
		for ( int i = 0; i < slots.Count; i++ )
		{
			if ( slots[i] == slot )
				return true;
		}
		return false;
	}

	private bool HasHostInstance( int slot )
	{
		for ( int i = 0; i < _linkInstances.Count; i++ )
		{
			if ( _linkInstances[i].Slot == slot )
				return true;
		}
		return false;
	}

	private void CreateHostInstance( int slot, byte[] romData )
	{
		var core = new Gba();
		core.LoadRom( romData );
		core.Reset();
		core.Video.InitGpu( scale: 1 );
		core.Video.SetReproduceClassicFeel( GamePreferences.ReproduceClassicFeel );

		if ( _camera.IsValid() && core.Video.RenderCommandList != null )
			_camera.AddCommandList( core.Video.RenderCommandList, Stage.AfterOpaque, 0 );

		var inst = _linkSession.Attach( core, slot );
		_linkInstances.Add( inst );
	}

	private void DestroyHostInstance( LinkCableInstance inst )
	{
		_linkSession.Detach( inst );

		var core = inst.Core;
		if ( _camera.IsValid() && core?.Video?.RenderCommandList != null )
			_camera.RemoveCommandList( core.Video.RenderCommandList );
		core?.Video?.DisposeGpu();

		_linkInstances.Remove( inst );
	}

	public void OnRemoteAvPacket( LinkCableAvPacket packet )
	{
		if ( !_linkedClient || packet == null )
			return;

		_timeSinceClientPacket = 0;
		EnqueuePacketAudio( packet );

		if ( packet.Video != null && packet.Frame > _clientPendingFrame )
		{
			_clientPendingVideo = packet.Video;
			_clientPendingFrame = packet.Frame;
		}

		PersistSave( packet.SaveData );
	}

	private void PresentClientVideo()
	{
		if ( _clientScreenTex == null || _clientPendingFrame <= _clientLastVideoFrame )
			return;

		byte[] pixels = _clientPendingVideo;
		int expected = GbaConstants.ScreenWidth * GbaConstants.ScreenHeight * 4;
		if ( pixels != null && pixels.Length >= expected )
			_clientScreenTex.Update( pixels.AsSpan( 0, expected ) );

		_clientLastVideoFrame = _clientPendingFrame;
	}

	private void PersistSave( byte[] saveData )
	{
		if ( saveData == null || _savePath == null )
			return;

		try { FileSystem.Data.WriteAllBytes( _savePath, saveData ); }
		catch ( Exception e ) { GbaLog.Write( LogCategory.GBA, LogLevel.Warn, $"Save write failed: {e.Message}" ); }
	}

	private void EnqueuePacketAudio( LinkCableAvPacket packet )
	{
		if ( packet.AudioSamples <= 0 || packet.Audio == null || _audioStream == null )
			return;
		if ( _audioStream.QueuedSampleCount > GbaAudio.SamplesPerFrame * AudioHighWaterFrames )
			return;

		int shorts = Math.Min( packet.Audio.Length, packet.AudioSamples * 2 );
		if ( shorts > 0 )
			_audioStream.WriteData( packet.Audio.AsSpan( 0, shorts ) );
	}

	private void TickLinkedClient()
	{
		var net = NetworkManager.Current;
		if ( net == null )
			return;

		if ( !_clientSaveUploaded )
		{
			byte[] save = ReadHostSave();
			if ( save != null )
				net.SendLocalSave( save );
			_clientSaveUploaded = true;
		}

		ushort keys = ReadInputKeysActive();
		net.SendLocalInput( _clientInputFrame++, keys );
	}

	private void TickLinkedHost()
	{
		var session = _linkSession;
		if ( session == null )
			return;

		ushort hostKeys = ReadInputKeysActive();
		if ( _linkInstances.Count > 0 )
			session.SetSlotInput( 0, hostKeys );

		if ( IsLinkVideoFrameDue() )
		{
			AdvanceLinkVideoClock();
			session.GrantFrameCredit();
		}
	}

	private bool IsLinkVideoFrameDue()
	{
		if ( _videoClock == null )
			ResetVideoClock();

		if ( _nextVideoFrameDue <= 0 )
			return true;

		return _videoClock.Elapsed.TotalSeconds >= _nextVideoFrameDue;
	}

	private void AdvanceLinkVideoClock()
	{
		double now = _videoClock?.Elapsed.TotalSeconds ?? 0;

		if ( _nextVideoFrameDue <= 0 || now - _nextVideoFrameDue > GbaFrameTime * MaxPendingFrames )
			_nextVideoFrameDue = now + GbaFrameTime;
		else
			_nextVideoFrameDue += GbaFrameTime;
	}

	private void CaptureAndSendHostVideo()
	{
		var session = _linkSession;
		var net = NetworkManager.Current;
		if ( session == null || net == null )
			return;

		for ( int i = 0; i < _linkInstances.Count; i++ )
		{
			var inst = _linkInstances[i];
			int slot = inst.Slot;
			if ( slot < 0 )
				continue;

			if ( slot == 0 )
			{
				long h0 = inst.LastHarvestedFrame;
				if ( h0 != inst.LastUploadedFrame )
				{
					inst.Core.Video.UploadAndBuildCommandList();
					inst.LastUploadedFrame = h0;
				}
				while ( session.TryDequeueAv( inst, out var packet ) )
				{
					EnqueuePacketAudio( packet );
					PersistSave( packet.SaveData );
				}
				continue;
			}

			while ( session.TryDequeueAv( inst, out var packet ) )
			{
				packet.Video = null;
				net.HostSendAv( slot, packet );
			}

			if ( inst.TimeSinceStateSent > StateStreamIntervalSeconds )
			{
				inst.TimeSinceStateSent = 0;
				byte[] state;
				lock ( inst.CoreLock )
					state = GbaSerialize.Save( inst.Core, null );
				net.HostSendState( slot, state );
			}

			if ( inst.PendingVideoFrame >= 0 )
			{
				long frame = inst.PendingVideoFrame;
				inst.PendingVideoFrame = -1;
				int capturedSlot = slot;
				int playerId = inst.PlayerId;
				inst.Core.Video.CaptureScreenshotAsync( video =>
				{
					net.HostSendAv( capturedSlot, new LinkCableAvPacket { PlayerId = playerId, Frame = frame, Video = video } );
				} );
			}

			long harvested = inst.LastHarvestedFrame;
			if ( harvested != inst.LastUploadedFrame )
			{
				inst.Core.Video.UploadAndBuildCommandList();
				inst.LastUploadedFrame = harvested;
				inst.PendingVideoFrame = harvested;
			}
		}
	}

	private byte[] ReadRomBytes()
	{
		try
		{
			if ( FileSystem.Mounted.FileExists( RomPath ) )
				return FileSystem.Mounted.ReadAllBytes( RomPath ).ToArray();
			if ( FileSystem.Data.FileExists( RomPath ) )
				return FileSystem.Data.ReadAllBytes( RomPath ).ToArray();
		}
		catch ( Exception e )
		{
			GbaLog.Write( LogCategory.GBA, LogLevel.Error, $"ReadRomBytes failed: {e.Message}" );
		}
		return null;
	}

	private byte[] ReadHostSave()
	{
		_savePath = "saves/" + System.IO.Path.GetFileNameWithoutExtension( RomPath ) + ".sav";
		try
		{
			if ( FileSystem.Data.FileExists( _savePath ) )
				return FileSystem.Data.ReadAllBytes( _savePath ).ToArray();
		}
		catch ( Exception e )
		{
			GbaLog.Write( LogCategory.GBA, LogLevel.Warn, $"ReadHostSave failed: {e.Message}" );
		}
		return null;
	}
}
