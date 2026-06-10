using System.Threading;
using System.Threading.Tasks;

namespace sGBA;

public sealed partial class EmulatorComponent
{
	private sealed class GbaCoreSync
	{
		public int VideoFramePending { get; private set; }
		public bool VideoFrameWait { get; private set; }
		public bool AudioWait { get; private set; }
		public int AudioHighWater { get; set; }
		public float FpsTarget { get; private set; }

		private readonly object _videoFrameLock = new();
		private readonly object _audioBufferLock = new();
		private readonly SemaphoreSlim _videoFrameAvailable = new( 0, 1 );
		private readonly SemaphoreSlim _videoFrameRequired = new( 0, 1 );
		private readonly SemaphoreSlim _audioRequired = new( 0, 1 );
		private int _audioSamplesPending;

		public void LoadCoreOptions( bool audioSync, bool videoSync, float fpsTarget )
		{
			AudioWait = audioSync;
			VideoFrameWait = videoSync;
			FpsTarget = fpsTarget > 0 ? fpsTarget : 60f;
			AudioHighWater = 512;
		}

		public async Task PostFrameAsync( CancellationToken token )
		{
			lock ( _videoFrameLock )
			{
				VideoFramePending++;
				Signal( _videoFrameAvailable );
			}

			while ( true )
			{
				lock ( _videoFrameLock )
				{
					if ( !VideoFrameWait || VideoFramePending <= 0 )
						return;
				}

				await _videoFrameRequired.WaitAsync( SyncWaitMilliseconds, token );
			}
		}

		public void ForceFrame()
		{
			Signal( _videoFrameAvailable );
			Signal( _videoFrameRequired );
		}

		public void WaitPrologue( out bool videoFrameWait, out bool audioWait )
		{
			lock ( _videoFrameLock )
			{
				videoFrameWait = VideoFrameWait;
				VideoFrameWait = false;
				Signal( _videoFrameAvailable );
				Signal( _videoFrameRequired );
			}

			lock ( _audioBufferLock )
			{
				audioWait = AudioWait;
				AudioWait = false;
				Signal( _audioRequired );
			}
		}

		public void WaitEpilogue( bool videoFrameWait, bool audioWait )
		{
			lock ( _audioBufferLock )
			{
				AudioWait = audioWait;
			}

			lock ( _videoFrameLock )
			{
				VideoFrameWait = videoFrameWait;
				Signal( _videoFrameAvailable );
			}
		}

		public bool WaitFrameStart()
		{
			Monitor.Enter( _videoFrameLock );
			try
			{
				if ( !VideoFrameWait && VideoFramePending <= 0 )
					return false;

				Signal( _videoFrameRequired );

				if ( VideoFrameWait && VideoFramePending <= 0 )
				{
					Drain( _videoFrameAvailable );
					Monitor.Exit( _videoFrameLock );
					try
					{
						if ( !_videoFrameAvailable.Wait( SyncWaitMilliseconds ) )
							return false;
					}
					finally
					{
						Monitor.Enter( _videoFrameLock );
					}
				}

				if ( VideoFramePending <= 0 )
					return false;

				Drain( _videoFrameAvailable );
				VideoFramePending = 0;
				return true;
			}
			finally
			{
				Monitor.Exit( _videoFrameLock );
			}
		}

		public void WaitFrameEnd()
		{
			Signal( _videoFrameRequired );
		}

		public void SetVideoSync( bool wait )
		{
			lock ( _videoFrameLock )
			{
				if ( wait == VideoFrameWait )
					return;

				VideoFrameWait = wait;
				Signal( _videoFrameAvailable );
			}
		}

		public void SetAudioSync( bool wait )
		{
			lock ( _audioBufferLock )
			{
				AudioWait = wait;
				Signal( _audioRequired );
			}
		}

		public async Task ProduceAudioAsync( int audioSamples, CancellationToken token )
		{
			if ( audioSamples <= 0 )
				return;

			lock ( _audioBufferLock )
			{
				_audioSamplesPending += audioSamples;
			}

			while ( true )
			{
				lock ( _audioBufferLock )
				{
					if ( !AudioWait || AudioHighWater <= 0 || _audioSamplesPending < AudioHighWater )
						return;
				}

				await _audioRequired.WaitAsync( SyncWaitMilliseconds, token );
			}
		}

		public void ConsumeAudio( int audioSamples )
		{
			if ( audioSamples > 0 )
			{
				lock ( _audioBufferLock )
				{
					_audioSamplesPending = Math.Max( 0, _audioSamplesPending - audioSamples );
				}
			}

			Signal( _audioRequired );
		}

		private static void Signal( SemaphoreSlim signal )
		{
			try { signal.Release(); }
			catch ( SemaphoreFullException ) { }
		}

		private static void Drain( SemaphoreSlim signal )
		{
			while ( signal.Wait( 0 ) ) { }
		}
	}
}
