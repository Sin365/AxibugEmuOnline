using System.Diagnostics;
using System.Threading;
using Sandbox.Rendering;

namespace sGBA;

public sealed partial class EmulatorComponent : Component
{
	[Property, Title( "ROM Path" ), FilePath( Extension = "gba" )]
	public string RomPath { get; set; }

	public string GameCode { get; private set; }

	public static EmulatorComponent Current { get; private set; }
	public Gba Core => _coreThread?.Core;
	public Texture ScreenTexture { get; private set; }
	public bool IsReady { get; private set; }
	public string ErrorMessage { get; private set; }

	private SoundStream _audioStream;
	private SoundHandle _soundHandle;
	private string _savePath;
	private CameraComponent _camera;
	private object _coreLock = new();
	private GbaCoreThread _coreThread;
	private Stopwatch _videoClock;
	private double _nextVideoFrameDue;

	private const int AllKeysReleased = 0x03FF;
	private const int AudioPrefillFrames = 2;
	private const int AudioHighWaterFrames = 3;
	private const int GbaClockRate = 1 << 24;
	private const int GbaCyclesPerFrame = 228 * 1232;
	private const double GbaNativeFps = (double)GbaClockRate / GbaCyclesPerFrame;
	private const double GbaFrameTime = (double)GbaCyclesPerFrame / GbaClockRate;
	private const int MaxPendingFrames = 3;
	private const int SyncWaitMilliseconds = 50;
	private const float StickDeadzone = 0.3f;

	private int _inputKeys = AllKeysReleased;
	private bool _videoFramePending;
	private bool _paused;
	private bool _coreHalted;
	private int _inputCooldown;
	private bool _initCoreOnUpdate;
	private int _initDeferFrames;
	private string _stateBasePath;
	private bool _appliedReproduceClassicFeel;

	protected override void OnStart()
	{
		Current = this;
		GbaLog.SetBackend( LogBackend );
		_camera = Scene.Camera;
		if ( !string.IsNullOrEmpty( RomPath ) )
			_initCoreOnUpdate = true;
	}

	public void Restart( string romPath )
	{
		if ( _linkedHost || _linkedClient )
			EndLinkedMode();

		TearDownCore();
		RomPath = romPath;
		IsReady = false;
		ErrorMessage = null;
		_initCoreOnUpdate = false;
		_initDeferFrames = 0;
		InitCore();
	}

	public void Unload()
	{
		TearDownCore();
		RomPath = null;
		GameCode = null;
		_initCoreOnUpdate = false;
		_initDeferFrames = 0;
	}

	private void TearDownCore()
	{
		GbaCoreThread coreThread = _coreThread;
		_coreThread = null;
		coreThread?.End();

		lock ( CoreLock )
		{
			Gba core = coreThread?.Core;
			if ( core?.Savedata != null && core.Savedata.Data.Length > 0 && _savePath != null )
				FileSystem.Data.WriteAllBytes( _savePath, core.Savedata.Data );

			if ( _camera.IsValid() && core?.Video?.RenderCommandList != null )
				_camera.RemoveCommandList( core.Video.RenderCommandList );

			core?.Video?.DisposeGpu();
			ScreenTexture = null;
		}

		if ( _soundHandle is { IsValid: true } )
			_soundHandle.Volume = 0;
		_audioStream?.Dispose();
		_audioStream = null;
		_videoClock = null;
		_nextVideoFrameDue = 0;
		_videoFramePending = false;
		_inputCooldown = 0;
		_initDeferFrames = 0;
		_paused = false;
		_coreHalted = false;
		Interlocked.Exchange( ref _inputKeys, AllKeysReleased );
		IsReady = false;
	}

	private static string ReadGameCode( byte[] romData )
	{
		if ( romData == null || romData.Length < 0xB0 )
			return null;
		return System.Text.Encoding.ASCII.GetString( romData, 0xAC, 4 ).TrimEnd( '\0' );
	}

	private void InitCore()
	{
		try
		{
			if ( !FileSystem.Mounted.FileExists( RomPath ) && !FileSystem.Data.FileExists( RomPath ) )
			{
				ErrorMessage = $"ROM not found: {RomPath}";
				GbaLog.Write( LogCategory.GBA, LogLevel.Error, ErrorMessage );
				return;
			}

			BaseFileSystem romFs = FileSystem.Mounted.FileExists( RomPath ) ? FileSystem.Mounted : FileSystem.Data;
			byte[] romData = romFs.ReadAllBytes( RomPath ).ToArray();
			if ( romData.Length < 192 )
			{
				ErrorMessage = "ROM file is too small to be a valid GBA ROM.";
				GbaLog.Write( LogCategory.GBA, LogLevel.Error, ErrorMessage );
				return;
			}

			GameCode = ReadGameCode( romData );

			Gba core = new();
			core.LoadRom( romData );

			_savePath = "saves/" + System.IO.Path.GetFileNameWithoutExtension( RomPath ) + ".sav";
			if ( FileSystem.Data.FileExists( _savePath ) )
			{
				byte[] saveData = FileSystem.Data.ReadAllBytes( _savePath ).ToArray();
				core.Savedata.Load( saveData );
			}

			core.Reset();
			core.Video.InitGpu( scale: ComputeAutoScale() );
			core.Video.SetReproduceClassicFeel( GamePreferences.ReproduceClassicFeel );
			_appliedReproduceClassicFeel = GamePreferences.ReproduceClassicFeel;
			ScreenTexture = core.Video.OutputTexture;

			if ( _camera.IsValid() && core.Video.RenderCommandList != null )
				_camera.AddCommandList( core.Video.RenderCommandList, Stage.AfterOpaque, 0 );

			_stateBasePath = "states/" + System.IO.Path.GetFileNameWithoutExtension( RomPath );

			try { InitAudioStream(); }
			catch ( Exception audioEx ) { GbaLog.Write( LogCategory.GBAAudio, LogLevel.Warn, $"Audio init failed: {audioEx.Message}" ); }

			_coreThread = new GbaCoreThread( core, CoreLock, ReadInputKeysActive, LogCoreThreadError, LogCoreThreadReset );
			_coreThread.Sync.LoadCoreOptions( audioSync: true, videoSync: true, fpsTarget: (float)GbaNativeFps );
			_coreThread.Sync.AudioHighWater = GbaAudio.SamplesPerFrame * AudioHighWaterFrames;
			ResetVideoClock();
			_coreThread.Start();

			IsReady = true;
		}
		catch ( Exception ex )
		{
			ErrorMessage = $"Failed to load ROM: {ex.Message}";
			GbaLog.Write( LogCategory.GBA, LogLevel.Fatal, ErrorMessage );
		}
	}

	private void InitAudioStream()
	{
		if ( _audioStream != null )
		{
			if ( _soundHandle is { IsValid: true } )
				_soundHandle.Volume = 0;
			_audioStream.Dispose();
			_audioStream = null;
		}

		_audioStream = new SoundStream( GbaAudio.SampleRate, 2 );
		_audioStream.WriteData( new short[GbaAudio.SamplesPerFrame * 2 * AudioPrefillFrames] );
		_soundHandle = _audioStream.Play( volume: _paused ? 0f : 1.0f );
		_soundHandle.SpacialBlend = 0f;
		_soundHandle.OcclusionEnabled = false;
		_soundHandle.DistanceAttenuation = false;
		_soundHandle.AirAbsorption = false;
		_soundHandle.Transmission = false;
		_soundHandle.Stop( float.MaxValue );
	}

	private static int ComputeAutoScale()
	{
		int screenWidth = Screen.Width > 0 ? (int)Screen.Width : 1920;
		int screenHeight = Screen.Height > 0 ? (int)Screen.Height : 1080;
		return Math.Clamp( Math.Min( screenWidth / 240, screenHeight / 160 ), 1, 8 );
	}

	protected override void OnUpdate()
	{
		if ( _linkedClient )
		{
			if ( _timeSinceClientPacket > ClientHostTimeoutSeconds )
			{
				ResumeSoloAfterHostLost();
				return;
			}

			PollInput();
			TickLinkedClient();
			RestoreAudioStreamIfNeeded();
			return;
		}

		if ( _linkedHost )
		{
			RescaleGpuIfNeeded();
			if ( _appliedReproduceClassicFeel != GamePreferences.ReproduceClassicFeel )
				ApplyDisplaySettings();
			PollInput();
			TickLinkedHost();
			RestoreAudioStreamIfNeeded();
			return;
		}

		if ( !StartCoreWhenReady() )
			return;

		ReconcileCorePause();
		RescaleGpuIfNeeded();

		if ( _appliedReproduceClassicFeel != GamePreferences.ReproduceClassicFeel )
			ApplyDisplaySettings();

		PollInput();
		RestoreAudioStreamIfNeeded();
	}

	private bool StartCoreWhenReady()
	{
		if ( _initCoreOnUpdate )
		{
			if ( ShouldDeferInitialCore() )
				return false;

			_initCoreOnUpdate = false;
			_initDeferFrames = 0;
			InitCore();
		}

		return IsReady && _coreThread?.Core != null;
	}

	private void RestoreAudioStreamIfNeeded()
	{
		if ( _audioStream == null || _soundHandle is { IsValid: true } )
			return;

		try { InitAudioStream(); }
		catch { _audioStream = null; }
	}

	private bool ShouldDeferInitialCore()
	{
		if ( (int)Screen.Width != 1024 || (int)Screen.Height != 1024 )
			return false;

		if ( _initDeferFrames++ >= 5 )
			return false;

		return true;
	}

	private object CoreLock => _coreLock ??= new object();

	private ushort ReadInputKeysActive()
	{
		return (ushort)(AllKeysReleased ^ Interlocked.CompareExchange( ref _inputKeys, 0, 0 ));
	}

	private void ResetVideoClock()
	{
		_videoClock ??= new Stopwatch();
		_videoClock.Restart();
		_nextVideoFrameDue = 0;
	}

	private bool IsVideoFrameDue( GbaCoreSync sync )
	{
		if ( _videoClock == null )
			ResetVideoClock();

		if ( sync == null || _nextVideoFrameDue <= 0 )
			return true;

		return _videoClock.Elapsed.TotalSeconds >= _nextVideoFrameDue;
	}

	private void AdvanceVideoClock( GbaCoreSync sync )
	{
		double frameTime = sync?.FpsTarget > 0 ? 1.0 / sync.FpsTarget : GbaFrameTime;
		double now = _videoClock?.Elapsed.TotalSeconds ?? 0;

		if ( _nextVideoFrameDue <= 0 || now - _nextVideoFrameDue > frameTime * MaxPendingFrames )
			_nextVideoFrameDue = now + frameTime;
		else
			_nextVideoFrameDue += frameTime;
	}

	private void RunOnCoreThread( Action<Gba> action )
	{
		_coreThread?.RunFunction( action );
	}

	private void RescaleGpuIfNeeded()
	{
		if ( _paused )
			return;

		Gba core = Core;
		if ( core?.Video == null )
			return;

		int desiredScale = ComputeAutoScale();
		if ( core.Video.GpuScale == desiredScale )
			return;

		lock ( CoreLock )
		{
			if ( Core != core || core.Video == null )
				return;

			if ( core.Video.GpuScale == desiredScale )
				return;

			if ( _camera.IsValid() && core.Video.RenderCommandList != null )
				_camera.RemoveCommandList( core.Video.RenderCommandList );

			core.Video.InitGpu( desiredScale );
			core.Video.SetReproduceClassicFeel( GamePreferences.ReproduceClassicFeel );
			_appliedReproduceClassicFeel = GamePreferences.ReproduceClassicFeel;
			ScreenTexture = core.Video.OutputTexture;

			if ( _camera.IsValid() && core.Video.RenderCommandList != null )
				_camera.AddCommandList( core.Video.RenderCommandList, Stage.AfterOpaque, 0 );

			_videoFramePending = true;
			_coreThread?.Sync.ForceFrame();
		}
	}

	protected override void OnPreRender()
	{
		if ( _linkedHost )
		{
			CaptureAndSendHostVideo();
			return;
		}

		if ( _linkedClient )
		{
			PresentClientVideo();
			return;
		}

		WaitForPostedCoreFrame();
		UploadPendingVideoFrame();
	}

	private void WaitForPostedCoreFrame()
	{
		GbaCoreThread coreThread = _coreThread;
		GbaCoreSync sync = coreThread?.Sync;
		if ( sync == null || !IsVideoFrameDue( sync ) )
			return;

		if ( !sync.WaitFrameStart() )
			return;

		DrainPostedCoreFrames( coreThread );
		sync.WaitFrameEnd();
		AdvanceVideoClock( sync );
	}

	private void DrainPostedCoreFrames( GbaCoreThread coreThread )
	{
		bool hasFrame = false;

		while ( coreThread.PostedFrames.TryDequeue( out FramePacket frame ) )
		{
			if ( _audioStream != null && frame.AudioSamples > 0 && _audioStream.QueuedSampleCount <= GbaAudio.SamplesPerFrame * AudioHighWaterFrames )
				_audioStream.WriteData( frame.Audio.AsSpan( 0, frame.AudioSamples * 2 ) );

			coreThread.Sync.ConsumeAudio( frame.AudioSamples );

			if ( frame.SaveData != null )
				FileSystem.Data.WriteAllBytes( _savePath, frame.SaveData );

			hasFrame = true;
		}

		if ( hasFrame )
			_videoFramePending = true;
	}

	private void UploadPendingVideoFrame()
	{
		Gba core = Core;
		if ( core?.Video?.RenderCommandList == null )
			return;

		if ( !_videoFramePending )
			return;

		if ( core.Video.UploadAndBuildCommandList() )
			_videoFramePending = false;
	}

	private void PollInput()
	{
		if ( _paused )
			return;

		if ( _inputCooldown > 0 )
		{
			bool anyHeld = Input.Down( "GBA_A" ) || Input.Down( "GBA_B" ) ||
				Input.Down( "GBA_Start" ) || Input.Down( "GBA_Select" ) ||
				Input.Down( "GBA_L" ) || Input.Down( "GBA_R" ) ||
				Input.Down( "GBA_Up" ) || Input.Down( "GBA_Down" ) ||
				Input.Down( "GBA_Left" ) || Input.Down( "GBA_Right" ) ||
				MathF.Abs( Input.GetAnalog( InputAnalog.LeftStickX ) ) > StickDeadzone ||
				MathF.Abs( Input.GetAnalog( InputAnalog.LeftStickY ) ) > StickDeadzone;

			if ( anyHeld )
				return;

			_inputCooldown = 0;
		}

		int keys = AllKeysReleased;
		if ( Input.Down( "GBA_A" ) ) keys &= ~(int)GbaKey.A;
		if ( Input.Down( "GBA_B" ) ) keys &= ~(int)GbaKey.B;
		if ( Input.Down( "GBA_Start" ) ) keys &= ~(int)GbaKey.Start;
		if ( Input.Down( "GBA_Select" ) ) keys &= ~(int)GbaKey.Select;
		if ( Input.Down( "GBA_L" ) ) keys &= ~(int)GbaKey.L;
		if ( Input.Down( "GBA_R" ) ) keys &= ~(int)GbaKey.R;

		float stickX = Input.GetAnalog( InputAnalog.LeftStickX );
		float stickY = Input.GetAnalog( InputAnalog.LeftStickY );
		if ( Input.Down( "GBA_Up" ) || stickY < -StickDeadzone ) keys &= ~(int)GbaKey.Up;
		if ( Input.Down( "GBA_Down" ) || stickY > StickDeadzone ) keys &= ~(int)GbaKey.Down;
		if ( Input.Down( "GBA_Left" ) || stickX < -StickDeadzone ) keys &= ~(int)GbaKey.Left;
		if ( Input.Down( "GBA_Right" ) || stickX > StickDeadzone ) keys &= ~(int)GbaKey.Right;

		Interlocked.Exchange( ref _inputKeys, keys );
	}

	public void SetPaused( bool paused )
	{
		_paused = paused;

		if ( paused )
			Interlocked.Exchange( ref _inputKeys, AllKeysReleased );
		else
			_inputCooldown = 2;

		if ( _soundHandle is { IsValid: true } )
			_soundHandle.Volume = paused ? 0 : 1.0f;

		ReconcileCorePause();
	}

	private void ReconcileCorePause()
	{
		bool networked = IsLinked || NetworkManager.Current?.InMultiplayerSession == true;
		bool shouldHalt = _paused && !networked;
		if ( shouldHalt == _coreHalted )
			return;

		_coreHalted = shouldHalt;
		_coreThread?.SetPaused( shouldHalt );
		if ( !shouldHalt )
			ResetVideoClock();
	}

	public string GetStatePath( int slot ) => $"{_stateBasePath}.ss{slot}";

	public void CreateSuspendPoint( int slot )
	{
		Gba core = Core;
		if ( core == null )
			return;

		string path = GetStatePath( slot );
		try
		{
			byte[] data;
			lock ( CoreLock )
			{
				if ( Core != core )
					return;

				byte[] screenshot = core.Video.CaptureScreenshot();
				data = GbaSerialize.Save( core, screenshot );
			}

			FileSystem.Data.WriteAllBytes( path, data );
			GbaLog.Write( LogCategory.GBAState, LogLevel.Info, $"Suspend point created in slot {slot}" );
		}
		catch ( Exception ex )
		{
			GbaLog.Write( LogCategory.GBAState, LogLevel.Error, $"Failed to create suspend point {slot}: {ex.Message}" );
		}
	}

	public void LoadSuspendPoint( int slot )
	{
		string path = GetStatePath( slot );
		RunOnCoreThread( core =>
		{
			if ( !FileSystem.Data.FileExists( path ) )
			{
				GbaLog.Write( LogCategory.GBAState, LogLevel.Warn, $"No suspend point in slot {slot}" );
				return;
			}

			byte[] data = FileSystem.Data.ReadAllBytes( path ).ToArray();
			GbaSerialize.Load( core, data );
			GbaLog.Write( LogCategory.GBAState, LogLevel.Info, $"Suspend point loaded from slot {slot}" );
		} );
	}

	public void ResetEmulator()
	{
		_coreThread?.Reset();
	}

	public void ApplyDisplaySettings()
	{
		bool reproduceClassicFeel = GamePreferences.ReproduceClassicFeel;
		Gba core = Core;
		if ( core?.Video != null )
		{
			lock ( CoreLock )
			{
				if ( Core != core || core.Video == null )
					return;

				core.Video.SetReproduceClassicFeel( reproduceClassicFeel );
			}
		}
		_appliedReproduceClassicFeel = reproduceClassicFeel;
	}

	protected override void OnDestroy()
	{
		if ( _linkedHost || _linkedClient )
			EndLinkedMode();
		TearDownCore();
		_camera = null;
		if ( Current == this )
			Current = null;
	}

	private void LogCoreThreadError( string message )
	{
		GbaLog.Write( LogCategory.GBA, LogLevel.Fatal, message );
	}

	private void LogCoreThreadReset()
	{
		GbaLog.Write( LogCategory.GBA, LogLevel.Info, "Emulator reset" );
	}

	private static void LogBackend( LogCategory category, LogLevel level, string message )
	{
		string formatted = $"{GbaLog.GetCategoryName( category )}: {message}";

		if ( (level & (LogLevel.Fatal | LogLevel.Error)) != 0 )
			Log.Error( formatted );
		else if ( (level & (LogLevel.Warn | LogLevel.GameError)) != 0 )
			Log.Warning( formatted );
		else
			Log.Info( formatted );
	}
}
