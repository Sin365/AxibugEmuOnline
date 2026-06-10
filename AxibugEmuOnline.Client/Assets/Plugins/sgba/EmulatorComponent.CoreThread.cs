using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace sGBA;

public sealed partial class EmulatorComponent
{
	private readonly struct FramePacket( short[] audio, int audioSamples, byte[] saveData )
	{
		public readonly short[] Audio = audio;
		public readonly int AudioSamples = audioSamples;
		public readonly byte[] SaveData = saveData;
	}

	private enum GbaCoreThreadState
	{
		Initialized = -1,
		Running = 0,
		Request,
		Interrupted,
		Paused,
		Crashed,
		Interrupting,
		Exiting,
		Shutdown
	}

	[Flags]
	private enum GbaCoreThreadRequest
	{
		None = 0,
		Pause = 1,
		Wait = 2,
		Reset = 4,
		RunOn = 8,
		Crashed = 16,
		RewindEmpty = 32
	}

	private sealed class GbaCoreThread
	{
		public Gba Core { get; }
		public GbaCoreSync Sync { get; } = new();
		public ConcurrentQueue<FramePacket> PostedFrames { get; } = new();

		private readonly ConcurrentQueue<Action<Gba>> _runQueue = new();
		private readonly SemaphoreSlim _stateOnThreadSignal = new( 0, 1 );
		private readonly object _stateLock = new();
		private readonly object _coreLock;
		private readonly Func<ushort> _readInputKeysActive;
		private readonly Action<string> _logError;
		private readonly Action _resetCallback;
		private readonly short[][] _audioBuffers;
		private CancellationTokenSource _cts;
		private Task _workerTask;
		private GbaCoreThreadState _state = GbaCoreThreadState.Initialized;
		private GbaCoreThreadRequest _requested;
		private int _audioBufferIndex;
		private bool _waitPrologueActive;
		private bool _waitPrologueVideoFrameWait;
		private bool _waitPrologueAudioWait;

		public GbaCoreThread( Gba core, object coreLock, Func<ushort> readInputKeysActive, Action<string> logError, Action resetCallback )
		{
			Core = core;
			_coreLock = coreLock;
			_readInputKeysActive = readInputKeysActive;
			_logError = logError;
			_resetCallback = resetCallback;
			_audioBuffers = new short[4][];

			int audioBufferSize = GbaAudio.SamplesPerFrame * 2;
			for ( int i = 0; i < _audioBuffers.Length; i++ )
				_audioBuffers[i] = new short[audioBufferSize];
		}

		public void Start()
		{
			if ( _cts != null )
				return;

			_cts = new CancellationTokenSource();
			ChangeState( GbaCoreThreadState.Running );
			_workerTask = GameTask.RunInThreadAsync( Run );
			_ = ObserveWorkerTaskAsync( _workerTask );
		}

		public void End()
		{
			ChangeState( GbaCoreThreadState.Exiting );
			_cts?.Cancel();
			Sync.SetAudioSync( false );
			Sync.SetVideoSync( false );
			Sync.ForceFrame();
			SignalStateOnThread();
			_cts = null;
		}

		public void Reset()
		{
			SendRequest( GbaCoreThreadRequest.Reset );
			SignalStateOnThread();
			Sync.ForceFrame();
		}

		public void Pause()
		{
			SendRequest( GbaCoreThreadRequest.Pause );
			WaitPrologue();
			SignalStateOnThread();
			Sync.ForceFrame();
		}

		public void Unpause()
		{
			CancelRequest( GbaCoreThreadRequest.Pause );
			WaitEpilogue();
			SignalStateOnThread();
			Sync.ForceFrame();
		}

		public void SetPaused( bool paused )
		{
			if ( paused )
				Pause();
			else
				Unpause();
		}

		public void RunFunction( Action<Gba> run )
		{
			if ( run == null || _cts == null )
				return;

			_runQueue.Enqueue( run );
			SendRequest( GbaCoreThreadRequest.RunOn );
			SignalStateOnThread();
			Sync.ForceFrame();
		}

		private async Task ObserveWorkerTaskAsync( Task workerTask )
		{
			try
			{
				await workerTask;
			}
			catch ( OperationCanceledException ) { }
			catch ( ObjectDisposedException ) { }
			catch ( Exception ex )
			{
				try
				{
					_logError?.Invoke( $"Emulation worker task error: {ex.Message}\n{ex.StackTrace}" );
				}
				catch { }
			}
			finally
			{
				if ( _workerTask == workerTask )
					_workerTask = null;
			}
		}

		private async Task Run()
		{
			CancellationTokenSource cts = _cts;
			if ( cts == null )
				return;

			CancellationToken token = cts.Token;

			try
			{
				while ( !token.IsCancellationRequested )
				{
					GbaCoreThreadRequest pendingRequests = TakePendingRequests();
					RunPendingRequests( pendingRequests );

					if ( IsRequested( GbaCoreThreadRequest.Pause | GbaCoreThreadRequest.Wait | GbaCoreThreadRequest.Crashed | GbaCoreThreadRequest.RewindEmpty ) )
					{
						ChangeState( GbaCoreThreadState.Paused );
						await _stateOnThreadSignal.WaitAsync( SyncWaitMilliseconds, token );
						continue;
					}

					ChangeState( GbaCoreThreadState.Running );
					FramePacket frame = RunFrame( token );
					PostedFrames.Enqueue( frame );
					await Sync.ProduceAudioAsync( frame.AudioSamples, token );
					await Sync.PostFrameAsync( token );
					await GameTask.Yield();
				}
			}
			catch ( OperationCanceledException ) { }
			catch ( ObjectDisposedException ) { }
			catch ( Exception ex )
			{
				SendRequest( GbaCoreThreadRequest.Crashed );
				ChangeState( GbaCoreThreadState.Crashed );
				try
				{
					_logError?.Invoke( $"Emulation worker error: {ex.Message}\n{ex.StackTrace}" );
				}
				catch { }
			}
			finally
			{
				ChangeState( GbaCoreThreadState.Shutdown );
			}
		}

		private FramePacket RunFrame( CancellationToken token )
		{
			short[] audio;
			int audioSamples;
			byte[] saveData = null;

			lock ( _coreLock )
			{
				Core.KeysActive = _readInputKeysActive();
				Core.RunFrame();

				if ( token.IsCancellationRequested )
					return new FramePacket( null, 0, null );

				int bufferIndex = _audioBufferIndex;
				_audioBufferIndex = (bufferIndex + 1) & 3;
				audio = _audioBuffers[bufferIndex];

				audioSamples = Core.Audio.SamplesWritten;
				if ( audioSamples > 0 )
					Buffer.BlockCopy( Core.Audio.OutputBuffer, 0, audio, 0, audioSamples * 2 * sizeof( short ) );

				if ( Core.Savedata.Clean() && Core.Savedata.Data.Length > 0 )
					saveData = Core.Savedata.Data.ToArray();
			}

			return new FramePacket( audio, audioSamples, saveData );
		}

		private GbaCoreThreadRequest TakePendingRequests()
		{
			lock ( _stateLock )
			{
				GbaCoreThreadRequest pendingRequests = _requested;
				_requested &= GbaCoreThreadRequest.Pause | GbaCoreThreadRequest.Wait | GbaCoreThreadRequest.Crashed | GbaCoreThreadRequest.RewindEmpty;
				return pendingRequests;
			}
		}

		private void RunPendingRequests( GbaCoreThreadRequest pendingRequests )
		{
			if ( (pendingRequests & GbaCoreThreadRequest.Reset) != 0 )
			{
				lock ( _coreLock )
				{
					Core.Reset();
				}
				_resetCallback?.Invoke();
			}

			if ( (pendingRequests & GbaCoreThreadRequest.RunOn) != 0 )
				RunPendingFunctions();
		}

		private void RunPendingFunctions()
		{
			while ( _runQueue.TryDequeue( out Action<Gba> run ) )
			{
				try
				{
					lock ( _coreLock )
					{
						run( Core );
					}
				}
				catch ( Exception ex )
				{
					GbaLog.Write( LogCategory.GBA, LogLevel.Error, ex.Message );
				}
			}
		}

		private void WaitPrologue()
		{
			if ( _waitPrologueActive )
				return;

			Sync.WaitPrologue( out _waitPrologueVideoFrameWait, out _waitPrologueAudioWait );
			_waitPrologueActive = true;
		}

		private void WaitEpilogue()
		{
			if ( !_waitPrologueActive )
				return;

			Sync.WaitEpilogue( _waitPrologueVideoFrameWait, _waitPrologueAudioWait );
			_waitPrologueActive = false;
		}

		private bool IsRequested( GbaCoreThreadRequest request )
		{
			lock ( _stateLock )
			{
				return (_requested & request) != 0;
			}
		}

		private void SendRequest( GbaCoreThreadRequest request )
		{
			lock ( _stateLock )
			{
				_requested |= request;
				if ( _state is GbaCoreThreadState.Running or GbaCoreThreadState.Paused or GbaCoreThreadState.Crashed )
					_state = GbaCoreThreadState.Request;
			}
		}

		private void CancelRequest( GbaCoreThreadRequest request )
		{
			lock ( _stateLock )
			{
				_requested &= ~request;
				if ( _state == GbaCoreThreadState.Request && _requested == GbaCoreThreadRequest.None )
					_state = GbaCoreThreadState.Running;
			}
		}

		private void ChangeState( GbaCoreThreadState state )
		{
			lock ( _stateLock )
			{
				_state = state;
			}
		}

		private void SignalStateOnThread()
		{
			try { _stateOnThreadSignal.Release(); }
			catch ( SemaphoreFullException ) { }
		}
	}
}
