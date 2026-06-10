using System.Threading;

namespace sGBA;

public sealed class WakeLatch
{
	private readonly SemaphoreSlim _sem = new( 0, int.MaxValue );

	public void Set()
	{
		if ( _sem.CurrentCount == 0 )
		{
			try { _sem.Release(); }
			catch ( SemaphoreFullException ) { }
		}
	}

	public void Wait( int timeoutMs )
	{
		_sem.Wait( timeoutMs );
	}
}

public sealed class LinkCablePlayerUser : ILockstepUser
{
	private readonly LinkCableInstance _instance;

	public LinkCablePlayerUser( LinkCableInstance instance )
	{
		_instance = instance;
	}

	public void Sleep() { }

	public void Wake()
	{
		_instance.WakeSignal.Set();
	}

	public int RequestedId()
	{
		return _instance.RequestedId;
	}

	public void PlayerIdChanged( int id )
	{
		_instance.PlayerId = id;
	}
}

public sealed class LinkCableInstance
{
	public Gba Core { get; }
	public GbaSioLockstepDriver Driver { get; }
	public LinkCablePlayerUser User { get; }

	public int RequestedId { get; }
	public int Slot { get; }
	public int PlayerId { get; set; } = -1;

	public ushort Keys { get; set; } = 0x03FF;
	public long LastHarvestedFrame { get; set; }
	public long LastUploadedFrame { get; set; } = -1;
	public long PendingVideoFrame { get; set; } = -1;
	public RealTimeSince TimeSinceStateSent { get; set; }

	public object CoreLock { get; } = new();
	public WakeLatch WakeSignal { get; } = new();
	public bool PaceMaster { get; set; }

	public LinkCableInstance( Gba core, int requestedId )
	{
		Core = core;
		RequestedId = requestedId;
		Slot = requestedId;
		User = new LinkCablePlayerUser( this );
		Driver = new GbaSioLockstepDriver( User );
	}
}

public sealed class LinkCableHost
{
	public GbaSioLockstepCoordinator Coordinator { get; } = new();

	private readonly List<LinkCableInstance> _instances = [];
	private readonly object _listLock = new();

	public IReadOnlyList<LinkCableInstance> Instances => _instances;

	public LinkCableInstance Attach( Gba core, int requestedId )
	{
		var inst = new LinkCableInstance( core, requestedId );
		Coordinator.Attach( inst.Driver );
		core.Io.SetSioDriver( inst.Driver );
		lock ( _listLock )
			_instances.Add( inst );
		return inst;
	}

	public void Detach( LinkCableInstance inst )
	{
		if ( inst == null )
			return;
		lock ( _listLock )
		{
			if ( !_instances.Remove( inst ) )
				return;
		}
		if ( ReferenceEquals( inst.Core.Io.SioDriver, inst.Driver ) )
			inst.Core.Io.SetSioDriver( null );
		Coordinator.Detach( inst.Driver );
	}

	public LinkCableInstance FindBySlot( int slot )
	{
		lock ( _listLock )
		{
			for ( int i = 0; i < _instances.Count; i++ )
			{
				if ( _instances[i].Slot == slot )
					return _instances[i];
			}
		}
		return null;
	}
}
