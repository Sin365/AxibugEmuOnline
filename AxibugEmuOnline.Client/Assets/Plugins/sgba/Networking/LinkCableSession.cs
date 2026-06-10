using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace sGBA;

public sealed class LinkCableSession
{
	private const int MaxQueuedAvPackets = 4;
	private const int SyncWaitMilliseconds = 50;
	private const int WakeWaitMilliseconds = 2;
	private const int FrameCreditMax = 2;
	private const int YieldIntervalMilliseconds = 250;

	public LinkCableHost Host { get; } = new();

	private readonly ConcurrentDictionary<LinkCableInstance, ConcurrentQueue<LinkCableAvPacket>> _avQueues = new();
	private readonly ConcurrentDictionary<LinkCableInstance, CancellationTokenSource> _coreThreads = new();
	private readonly object _structLock = new();

	private SemaphoreSlim _frameCredit = new( 0, FrameCreditMax );

	private CancellationTokenSource _cts;
	private Action<string> _logError;

	public bool IsRunning => _cts != null;

	public void Start( Action<string> logError = null )
	{
		if ( _cts != null )
			return;

		_logError = logError;
		_cts = new CancellationTokenSource();
		_frameCredit = new SemaphoreSlim( 0, FrameCreditMax );

		foreach ( var inst in Host.Instances )
			StartCoreThread( inst );
	}

	public void Stop()
	{
		var cts = _cts;
		_cts = null;
		if ( cts == null )
			return;

		Signal( _frameCredit );

		foreach ( var kv in _coreThreads )
		{
			try { kv.Value.Cancel(); }
			catch ( ObjectDisposedException ) { }
			kv.Key.WakeSignal.Set();
		}

		foreach ( var inst in Host.Instances )
			inst.WakeSignal.Set();

		cts.Cancel();
		_coreThreads.Clear();
		cts.Dispose();
	}

	public LinkCableInstance Attach( Gba core, int requestedId )
	{
		LinkCableInstance inst;
		lock ( _structLock )
		{
			inst = Host.Attach( core, requestedId );
			inst.PaceMaster = requestedId == 0;
			_avQueues.TryAdd( inst, new ConcurrentQueue<LinkCableAvPacket>() );
		}

		if ( _cts != null )
			StartCoreThread( inst );

		return inst;
	}

	public void Detach( LinkCableInstance inst )
	{
		if ( inst == null )
			return;

		if ( _coreThreads.TryRemove( inst, out var threadCts ) )
		{
			try { threadCts.Cancel(); }
			catch ( ObjectDisposedException ) { }
			inst.WakeSignal.Set();
		}

		inst.WakeSignal.Set();

		lock ( inst.CoreLock )
		{
			lock ( _structLock )
			{
				_avQueues.TryRemove( inst, out _ );
				Host.Detach( inst );
			}
		}

		threadCts?.Dispose();
	}

	public void SetSlotInput( int slot, ushort keys )
	{
		var inst = Host.FindBySlot( slot );
		if ( inst != null )
			inst.Keys = keys;
	}

	public void LoadSlotSave( int slot, byte[] saveData )
	{
		if ( saveData == null || saveData.Length == 0 )
			return;

		var inst = Host.FindBySlot( slot );
		if ( inst == null )
			return;

		lock ( inst.CoreLock )
			inst.Core.Savedata.Load( saveData );
	}

	public bool TryDequeueAv( LinkCableInstance inst, out LinkCableAvPacket packet )
	{
		packet = null;
		if ( inst == null )
			return false;
		if ( !_avQueues.TryGetValue( inst, out var queue ) )
			return false;
		return queue.TryDequeue( out packet );
	}

	public void GrantFrameCredit()
	{
		Signal( _frameCredit );
	}

	private void StartCoreThread( LinkCableInstance inst )
	{
		var sessionCts = _cts;
		if ( sessionCts == null || inst == null )
			return;

		CancellationTokenSource linked;
		try
		{
			linked = CancellationTokenSource.CreateLinkedTokenSource( sessionCts.Token );
		}
		catch ( ObjectDisposedException )
		{
			return;
		}

		if ( !_coreThreads.TryAdd( inst, linked ) )
		{
			linked.Dispose();
			return;
		}

		var token = linked.Token;
		var task = GameTask.RunInThreadAsync( () => RunCoreThread( inst, token ) );
		_ = ObserveCoreAsync( task );
	}

	private async Task RunCoreThread( LinkCableInstance inst, CancellationToken token )
	{
		try
		{
			var core = inst.Core;
			var sinceYield = System.Diagnostics.Stopwatch.StartNew();

			while ( !token.IsCancellationRequested )
			{
				if ( sinceYield.ElapsedMilliseconds >= YieldIntervalMilliseconds )
				{
					await GameTask.Yield();
					sinceYield.Restart();
				}

				if ( !core.IsRunning )
				{
					inst.WakeSignal.Wait( SyncWaitMilliseconds );
					continue;
				}

				if ( core.Io.LockstepBlocked )
				{
					inst.WakeSignal.Wait( WakeWaitMilliseconds );
					if ( core.Io.LockstepBlocked )
						PumpBlockedCore( inst );
					continue;
				}

				if ( inst.PaceMaster && !core.FrameInProgress )
				{
					bool haveCredit;
					try
					{
						haveCredit = _frameCredit.Wait( SyncWaitMilliseconds, token );
					}
					catch ( OperationCanceledException )
					{
						break;
					}

					if ( !haveCredit )
						continue;
				}

				lock ( inst.CoreLock )
				{
					core.KeysActive = inst.Keys;
					if ( core.StepFrame() )
					{
						inst.LastHarvestedFrame = core.FrameCounter;
						HarvestFrame( inst );
					}
				}
			}
		}
		catch ( OperationCanceledException )
		{
		}
		catch ( ObjectDisposedException )
		{
		}
		catch ( Exception ex )
		{
			LogError( $"core thread error: {ex.Message}\n{ex.StackTrace}" );
		}
	}

	private async Task ObserveCoreAsync( Task task )
	{
		try
		{
			await task;
		}
		catch ( OperationCanceledException ) { }
		catch ( ObjectDisposedException ) { }
		catch ( Exception ex )
		{
			LogError( $"core task error: {ex.Message}\n{ex.StackTrace}" );
		}
	}

	private static void PumpBlockedCore( LinkCableInstance inst )
	{
		var core = inst.Core;
		lock ( inst.CoreLock )
		{
			if ( !core.Io.LockstepBlocked || core.FrameInProgress || !core.Io.IsDriverEventScheduled )
				return;

			long when = core.Io.NextDriverEvent;
			if ( core.Cpu.Cycles < when )
				core.Cpu.Cycles = when;

			core.Io.ProcessDriverEvent();
		}
	}

	private void HarvestFrame( LinkCableInstance inst )
	{
		if ( inst.PlayerId < 0 )
			return;

		var core = inst.Core;

		int audioSamples = core.Audio.SamplesWritten;
		short[] audio = null;
		if ( audioSamples > 0 )
		{
			audio = new short[audioSamples * 2];
			Buffer.BlockCopy( core.Audio.OutputBuffer, 0, audio, 0, audioSamples * 2 * sizeof( short ) );
		}

		byte[] saveData = null;
		if ( core.Savedata.Clean() && core.Savedata.Data.Length > 0 )
			saveData = core.Savedata.Data.ToArray();

		var packet = new LinkCableAvPacket
		{
			PlayerId = inst.PlayerId,
			Frame = core.FrameCounter,
			Audio = audio,
			AudioSamples = audioSamples,
			SaveData = saveData,
		};

		if ( _avQueues.TryGetValue( inst, out var queue ) )
		{
			queue.Enqueue( packet );
			while ( queue.Count > MaxQueuedAvPackets && queue.TryDequeue( out _ ) )
			{
			}
		}
	}

	private void LogError( string message )
	{
		try { _logError?.Invoke( $"LinkCableSession {message}" ); }
		catch { }
	}

	private static void Signal( SemaphoreSlim semaphore )
	{
		try { semaphore.Release(); }
		catch ( SemaphoreFullException ) { }
	}
}
