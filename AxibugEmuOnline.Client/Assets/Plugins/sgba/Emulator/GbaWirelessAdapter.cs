using System.Collections.Concurrent;

namespace sGBA;

public interface IWirelessNetwork
{
	void Send( int clientId, byte[] data, int length );
}

public sealed class GbaWirelessAdapter
{
	public enum Command : byte
	{
		Init1 = 0x10,
		Init2 = 0x3D,
		SysConfig = 0x17,
		LinkPower = 0x11,
		SysVersion = 0x12,
		SysStatus = 0x13,
		SlotStatus = 0x14,
		ConfigStatus = 0x15,
		BroadcastData = 0x16,
		HostStart = 0x19,
		HostAccept = 0x1A,
		HostStop = 0x1B,
		BroadcastReadStart = 0x1C,
		BroadcastReadFetch = 0x1D,
		BroadcastReadStop = 0x1E,
		Connect = 0x1F,
		IsConnected = 0x20,
		ConnectComplete = 0x21,
		SendData = 0x24,
		SendDataWait = 0x25,
		ReceiveData = 0x26,
		Wait = 0x27,
		RetransmitWait = 0x37,
		Disconnect = 0x30,

		ResponseTimeout = 0x27,
		ResponseData = 0x28,
		ResponseDisconnect = 0x29,
	}

	public enum ComState
	{
		Reset,
		Handshake,
		WaitCommand,
		WaitData,
		ResponseCommand,
		ResponseData,
		ResponseError,
		ResponseError2,
		WaitEvent,
		WaitResponse,
	}

	public enum WifiState
	{
		Idle,
		Host,
		Connecting,
		Client,
	}

	private enum NetType : uint
	{
		Broadcast = 0x00,
		ConnectRequest = 0x01,
		ConnectAck = 0x02,
		ConnectNack = 0x03,
		Disconnect = 0x04,
		HostSend = 0x05,
		ClientSend = 0x06,
		ClientAck = 0x07,
	}

	private const uint ConnInProgress = 0x01000000;
	private const uint ConnFailed = 0x02000000;
	private const uint ConnCompleteFail = 0x01000000;

	private const uint NetHeaderMagic = 0x52465531; // "RFU1"

	private const int BroadcastAnnounceFrames = 30;
	private const int MaxPeers = 32;
	private const int DefaultTimeoutFrames = 32;
	private const int DefaultRetransmitMax = 4;

	private const int ClientQueueSize = 16;
	private const int HostQueueSize = 16;

	private const int MaxClients = 4;
	private const int HostPacketSize = 16;
	private const int ClientPacketSize = 128;

	private struct TxBuffer
	{
		public uint[] Data; // [23]
		public byte Length;
	}

	private struct HostInboxSlot
	{
		public uint Length;
		public byte[] Data; // [HostPacketSize]
	}

	private struct HostClientSlot
	{
		public ushort ClientId;
		public ushort DeviceId;
		public ushort TimeToLive;
		public HostInboxSlot[] Queue;
	}

	private struct HostState
	{
		public ushort DeviceId;
		public byte BroadcastTtl;
		public uint[] BroadcastData; // [6]
		public HostClientSlot[] Clients; // [MaxClients]
	}

	private struct ClientInboxSlot
	{
		public ushort Length;
		public byte[] Data; // [ClientPacketSize]
	}

	private struct ClientState
	{
		public ushort DeviceId;
		public ushort SlotNumber;
		public ushort HostId;
		public ClientInboxSlot[] Queue; // [ClientQueueSize]
	}

	private struct PeerBroadcast
	{
		public bool Valid;
		public byte Ttl;
		public ushort DeviceId;
		public uint[] Data; // [6]
	}

	public Gba Gba { get; }
	public IWirelessNetwork Network { get; set; }

	public ComState SpiState { get; private set; } = ComState.Reset;
	public WifiState WifiMode { get; private set; } = WifiState.Idle;
	public bool IsActive => SpiState != ComState.Reset;

	private uint _prevWord;
	private uint _bufferIndex;
	private readonly uint[] _buffer = new uint[255];
	private Command _command;
	private byte _payloadLength;
	private uint _timeoutCycles;
	private uint _responseTimeoutCycles;
	private byte _timeoutFrames = DefaultTimeoutFrames;
	private byte _retransmitMax = DefaultRetransmitMax;

	private TxBuffer _txBuffer = new() { Data = new uint[23] };
	private HostState _host;
	private ClientState _client;
	private readonly PeerBroadcast[] _peers = new PeerBroadcast[MaxPeers];

	private readonly ConcurrentQueue<(int FromClientId, byte[] Buffer, int Length)> _inbox = new();
	private readonly Random _rng = new( unchecked((int)DateTime.UtcNow.Ticks) );

	public GbaWirelessAdapter( Gba gba )
	{
		Gba = gba;
		InitHost();
		InitClient();
	}

	public void Reset()
	{
		_prevWord = 0;
		_bufferIndex = 0;
		WifiMode = WifiState.Idle;
		SpiState = ComState.Reset;
		_timeoutCycles = 0;
		_responseTimeoutCycles = 0;
		_timeoutFrames = DefaultTimeoutFrames;
		_retransmitMax = DefaultRetransmitMax;
		InitHost();
		Array.Clear( _peers );
	}

	public void EnqueueReceive( int fromClientId, byte[] buffer, int length )
	{
		_inbox.Enqueue( (fromClientId, buffer, length) );
	}

	private void DrainInbox()
	{
		while ( _inbox.TryDequeue( out var pkt ) )
			ReceiveNetworkPacket( pkt.Buffer, pkt.Length, pkt.FromClientId );
	}

	private void InitHost()
	{
		_host = new HostState
		{
			DeviceId = 0,
			BroadcastTtl = 0,
			BroadcastData = new uint[6],
			Clients = new HostClientSlot[MaxClients],
		};
		for ( int i = 0; i < MaxClients; i++ )
			_host.Clients[i].Queue = NewHostQueue();
	}

	private static HostInboxSlot[] NewHostQueue()
	{
		var q = new HostInboxSlot[HostQueueSize];
		for ( int i = 0; i < q.Length; i++ )
			q[i].Data = new byte[HostPacketSize];
		return q;
	}

	private void InitClient()
	{
		_client = new ClientState
		{
			DeviceId = 0,
			SlotNumber = 0,
			HostId = 0,
			Queue = new ClientInboxSlot[ClientQueueSize],
		};
		for ( int i = 0; i < _client.Queue.Length; i++ )
			_client.Queue[i].Data = new byte[ClientPacketSize];
	}

	public uint Transfer( uint sentValue )
	{
		uint reply = 0x80000000;

		switch ( SpiState )
		{
			case ComState.Reset:
				reply = 0;
				if ( (sentValue & 0xFFFF) == 0x494E ) // "NI"
					SpiState = ComState.Handshake;
				break;

			case ComState.Handshake:
				if ( sentValue == 0xB0BB8001 )
					SpiState = ComState.WaitCommand;
				reply = (sentValue << 16) | ((~_prevWord) & 0xFFFF);
				break;

			case ComState.WaitCommand:
				if ( (sentValue >> 16) == 0x9966 )
				{
					_payloadLength = (byte)(sentValue >> 8);
					_command = (Command)(byte)sentValue;
					_bufferIndex = 0;
					if ( _payloadLength == 0 )
						CompleteCommand();
					else
						SpiState = ComState.WaitData;
				}
				break;

			case ComState.WaitData:
				_buffer[_bufferIndex++] = sentValue;
				if ( _bufferIndex == _payloadLength )
				{
					CompleteCommand();
					_bufferIndex = 0;
				}
				break;

			case ComState.ResponseCommand:
				reply = 0x99660080u | (byte)_command | ((uint)_payloadLength << 8);
				if ( _command is Command.Wait or Command.RetransmitWait or Command.SendDataWait )
				{
					SpiState = ComState.WaitEvent;
					_timeoutCycles = (uint)(_timeoutFrames * (16777216 / 60));
					_responseTimeoutCycles = (uint)(_retransmitMax * (16777216 / 60 / 6));
				}
				else
				{
					SpiState = _payloadLength != 0 ? ComState.ResponseData : ComState.WaitCommand;
				}
				break;

			case ComState.ResponseData:
				reply = _buffer[_bufferIndex++];
				if ( _bufferIndex == _payloadLength )
					SpiState = ComState.WaitCommand;
				break;

			case ComState.WaitEvent:
			case ComState.WaitResponse:
				break;

			case ComState.ResponseError:
				reply = 0x996601EE;
				SpiState = ComState.ResponseError2;
				break;

			case ComState.ResponseError2:
				reply = (byte)_command;
				SpiState = ComState.WaitCommand;
				break;
		}

		_prevWord = sentValue;
		return reply;
	}

	private void CompleteCommand()
	{
		int ret = ProcessCommand();
		if ( ret < 0 )
		{
			SpiState = ComState.ResponseError;
			_command = (Command)(byte)(-ret);
			_payloadLength = 1;
		}
		else
		{
			SpiState = ComState.ResponseCommand;
			_payloadLength = (byte)ret;
		}
	}

	private bool HasInboundData()
	{
		if ( WifiMode == WifiState.Client )
			return _client.Queue[0].Length != 0;

		if ( WifiMode == WifiState.Host )
		{
			for ( int i = 0; i < MaxClients; i++ )
				if ( _host.Clients[i].DeviceId != 0 && _host.Clients[i].Queue[0].Length != 0 )
					return true;
		}
		return false;
	}

	private ushort NewDeviceId()
	{
		while ( true )
		{
			ushort n = (ushort)(_rng.Next() ^ DateTime.UtcNow.Ticks);
			if ( n != 0 )
				return n;
		}
	}

	private int ProcessCommand()
	{
		switch ( _command )
		{
			case Command.Init1:
			case Command.Init2:
				return 0;

			case Command.SysConfig:
				_timeoutFrames = (byte)_buffer[0];
				_retransmitMax = (byte)(_buffer[0] >> 8);
				return 0;

			case Command.SysVersion:
				_buffer[0] = 0x00830117;
				return 1;

			case Command.SysStatus:
				_buffer[0] = WifiMode switch
				{
					WifiState.Host => (1u << 24) | _host.DeviceId,
					WifiState.Client => (5u << 24) | (1u << _client.SlotNumber << 16) | _client.DeviceId,
					_ => 0,
				};
				return 1;

			case Command.SlotStatus:
				if ( WifiMode == WifiState.Host )
				{
					uint cnt = 1;
					_buffer[0] = 0;
					for ( int i = 0; i < MaxClients; i++ )
					{
						if ( _host.Clients[i].DeviceId != 0 )
						{
							_buffer[0]++;
							_buffer[cnt++] = (uint)(_host.Clients[i].DeviceId | (i << 16));
						}
					}
					return (int)cnt;
				}
				return 0;

			case Command.LinkPower:
				_buffer[0] = WifiMode switch
				{
					WifiState.Host =>
						(_host.Clients[0].DeviceId != 0 ? 0x000000FFu : 0u) |
						(_host.Clients[1].DeviceId != 0 ? 0x0000FF00u : 0u) |
						(_host.Clients[2].DeviceId != 0 ? 0x00FF0000u : 0u) |
						(_host.Clients[3].DeviceId != 0 ? 0xFF000000u : 0u),
					WifiState.Client => 0xFFFFFFFFu,
					_ => 0u,
				};
				return 1;

			case Command.BroadcastReadStart:
				return 0;

			case Command.BroadcastReadStop:
			case Command.BroadcastReadFetch:
				{
					uint start = (uint)(_rng.Next() % MaxPeers);
					uint cnt = 0;
					for ( uint j = 0; cnt < 4 * 7 && j < MaxPeers; j++ )
					{
						uint entry = (start + j) % MaxPeers;
						if ( _peers[entry].Valid )
						{
							_buffer[cnt++] = _peers[entry].DeviceId;
							for ( int k = 0; k < 6; k++ )
								_buffer[cnt++] = _peers[entry].Data[k];
						}
					}
					return (int)cnt;
				}

			case Command.BroadcastData:
				if ( _payloadLength == 6 )
					Array.Copy( _buffer, 0, _host.BroadcastData, 0, 6 );
				return 0;

			case Command.HostStart:
				if ( WifiMode == WifiState.Client )
					return -1;
				if ( WifiMode == WifiState.Idle )
				{
					_host.DeviceId = NewDeviceId();
					for ( int k = 0; k < MaxClients; k++ )
						ClearClientSlot( k );
					WifiMode = WifiState.Host;
				}
				_host.BroadcastTtl = 0xFF;
				return 0;

			case Command.HostStop:
				if ( WifiMode == WifiState.Idle )
					return -1;
				if ( WifiMode == WifiState.Host )
				{
					for ( int i = 0; i < MaxClients; i++ )
						if ( _host.Clients[i].DeviceId != 0 )
							return 0;
					WifiMode = WifiState.Idle;
				}
				return 0;

			case Command.HostAccept:
				{
					if ( WifiMode == WifiState.Idle )
						return -1;
					uint cnt = 0;
					for ( int i = 0; i < MaxClients; i++ )
						if ( _host.Clients[i].DeviceId != 0 )
							_buffer[cnt++] = (uint)(_host.Clients[i].DeviceId | (i << 16));
					return (int)cnt;
				}

			case Command.Connect:
				{
					if ( WifiMode == WifiState.Host )
						return -1;
					ushort reqId = (ushort)(_buffer[0] & 0xFFFF);
					for ( int i = 0; i < MaxPeers; i++ )
					{
						if ( _peers[i].Valid && _peers[i].DeviceId == reqId )
						{
							SendNetCommand( i, NetType.ConnectRequest, reqId );
							WifiMode = WifiState.Connecting;
							return 0;
						}
					}
					return 0;
				}

			case Command.IsConnected:
				if ( WifiMode == WifiState.Host )
					return -1;
				_buffer[0] = WifiMode switch
				{
					WifiState.Connecting => ConnInProgress,
					WifiState.Idle => ConnFailed,
					_ => (uint)(_client.DeviceId | (_client.SlotNumber << 16)),
				};
				return 1;

			case Command.ConnectComplete:
				if ( WifiMode == WifiState.Host )
					return -1;
				if ( WifiMode == WifiState.Client )
				{
					_buffer[0] = (uint)(_client.DeviceId | (_client.SlotNumber << 16));
				}
				else
				{
					_buffer[0] = ConnCompleteFail;
					WifiMode = WifiState.Idle;
				}
				return 1;

			case Command.SendDataWait:
			case Command.SendData:
				if ( _payloadLength == 0 )
					return 0;
				if ( WifiMode == WifiState.Host )
				{
					_txBuffer.Length = (byte)(_buffer[0] & 0x7F);
					CopyWords( _buffer, 1, _txBuffer.Data, 0, _payloadLength - 1 );
				}
				else if ( WifiMode == WifiState.Client )
				{
					_txBuffer.Length = (byte)((_buffer[0] >> (8 + _client.SlotNumber * 5)) & 0x1F);
					CopyWords( _buffer, 1, _txBuffer.Data, 0, _payloadLength - 1 );
				}
				goto case Command.RetransmitWait;

			case Command.RetransmitWait:
				if ( WifiMode == WifiState.Host )
				{
					if ( _txBuffer.Length <= 90 )
					{
						for ( int i = 0; i < MaxClients; i++ )
							if ( _host.Clients[i].DeviceId != 0 )
								SendNetData( _host.Clients[i].ClientId, NetType.HostSend,
									_txBuffer.Length, _txBuffer.Data, _txBuffer.Length );
					}
				}
				else if ( WifiMode == WifiState.Client )
				{
					if ( _txBuffer.Length <= 16 )
					{
						uint hdr = ((uint)_txBuffer.Length << 24)
							| ((uint)_client.SlotNumber << 16)
							| _client.DeviceId;
						SendNetData( _client.HostId, NetType.ClientSend, hdr, _txBuffer.Data, _txBuffer.Length );
					}
				}
				else
				{
					return -1;
				}
				break;

			case Command.ReceiveData:
				return ReadInbound();

			case Command.Wait:
				return 0;

			case Command.Disconnect:
				if ( WifiMode == WifiState.Client )
				{
					SendNetCommand( _client.HostId, NetType.Disconnect,
						(uint)(_client.DeviceId | (_client.SlotNumber << 16)) );
					WifiMode = WifiState.Idle;
				}
				else if ( WifiMode == WifiState.Host )
				{
					for ( int i = 0; i < MaxClients; i++ )
					{
						if ( (_buffer[0] & (1u << i)) != 0 )
						{
							SendNetCommand( _host.Clients[i].ClientId, NetType.Disconnect,
								(uint)(_host.Clients[i].DeviceId | (i << 16)) );
							ClearClientSlot( i );
						}
					}
				}
				return 0;
		}

		return 0;
	}

	private int ReadInbound()
	{
		if ( WifiMode == WifiState.Host )
		{
			uint cnt = 1;
			uint bufBytes = 0;
			byte[] tmp = new byte[HostPacketSize * MaxClients];
			_buffer[0] = 0;
			for ( int i = 0; i < MaxClients; i++ )
			{
				ref var client = ref _host.Clients[i];
				uint dlen = Math.Min( (uint)HostPacketSize, client.Queue[0].Length );
				if ( client.DeviceId != 0 && dlen != 0 )
				{
					Buffer.BlockCopy( client.Queue[0].Data, 0, tmp, (int)bufBytes, (int)dlen );
					bufBytes += dlen;
					_buffer[0] |= dlen << (8 + i * 5);
					PopHostQueue( ref client );
				}
			}
			for ( int i = 0; i < (bufBytes + 3) / 4; i++ )
				_buffer[cnt++] = ReadLe32( tmp, i * 4 );
			return (int)cnt;
		}

		if ( WifiMode == WifiState.Client )
		{
			uint cnt = 1;
			uint dlen = _client.Queue[0].Length;
			_buffer[0] = dlen;
			for ( uint j = 0; j < (dlen + 3) / 4; j++ )
				_buffer[cnt++] = ReadLe32( _client.Queue[0].Data, (int)(j * 4) );
			PopClientQueue();
			return (int)cnt;
		}

		return 0;
	}

	private static void PopHostQueue( ref HostClientSlot client )
	{
		for ( int q = 0; q < HostQueueSize - 1; q++ )
		{
			client.Queue[q].Length = client.Queue[q + 1].Length;
			Buffer.BlockCopy( client.Queue[q + 1].Data, 0, client.Queue[q].Data, 0, HostPacketSize );
		}
		client.Queue[HostQueueSize - 1].Length = 0;
	}

	private void PopClientQueue()
	{
		for ( int q = 0; q < ClientQueueSize - 1; q++ )
		{
			_client.Queue[q].Length = _client.Queue[q + 1].Length;
			Buffer.BlockCopy( _client.Queue[q + 1].Data, 0, _client.Queue[q].Data, 0, ClientPacketSize );
		}
		_client.Queue[ClientQueueSize - 1].Length = 0;
	}

	private void ClearClientSlot( int slot )
	{
		ref var c = ref _host.Clients[slot];
		c.ClientId = 0;
		c.DeviceId = 0;
		c.TimeToLive = 0;
		for ( int p = 0; p < HostQueueSize; p++ )
		{
			c.Queue[p].Length = 0;
			Array.Clear( c.Queue[p].Data, 0, HostPacketSize );
		}
	}

	public void FrameUpdate()
	{
		if ( SpiState == ComState.Reset )
			return;

		DrainInbox();

		for ( int i = 0; i < MaxPeers; i++ )
		{
			if ( _peers[i].Valid && --_peers[i].Ttl == 0 )
				_peers[i].Valid = false;
		}

		if ( WifiMode != WifiState.Host )
			return;

		if ( _host.BroadcastTtl++ >= BroadcastAnnounceFrames )
		{
			_host.BroadcastTtl = 0;
			SendNetBroadcast( NetType.Broadcast, _host.DeviceId, _host.BroadcastData );
		}

		for ( int i = 0; i < MaxClients; i++ )
		{
			if ( _host.Clients[i].DeviceId == 0 )
				continue;
			if ( ++_host.Clients[i].TimeToLive >= 240 )
				ClearClientSlot( i );
		}
	}

	private void ReceiveNetworkPacket( byte[] buf, int len, int clientId )
	{
		if ( len < 12 || ReadBe32( buf, 0 ) != NetHeaderMagic )
			return;

		var ptype = (NetType)ReadBe32( buf, 4 );
		uint hdata = ReadBe32( buf, 8 );
		const int payOff = 12;

		switch ( ptype )
		{
			case NetType.Broadcast:
				if ( clientId < MaxPeers )
				{
					ref var peer = ref _peers[clientId];
					peer.DeviceId = (ushort)hdata;
					peer.Valid = true;
					peer.Ttl = 0xFF;
					peer.Data ??= new uint[6];
					for ( int j = 0; j < 6; j++ )
						peer.Data[j] = ReadBe32( buf, payOff + j * 4 );
				}
				break;

			case NetType.ConnectRequest:
				HandleConnectRequest( clientId );
				break;

			case NetType.ConnectAck:
				if ( WifiMode == WifiState.Connecting )
				{
					InitClient();
					_client.DeviceId = (ushort)(hdata & 0xFFFF);
					_client.SlotNumber = (ushort)(hdata >> 16);
					_client.HostId = (ushort)clientId;
					WifiMode = WifiState.Client;
				}
				break;

			case NetType.ConnectNack:
				if ( WifiMode == WifiState.Connecting )
					WifiMode = WifiState.Idle;
				break;

			case NetType.Disconnect:
				HandleDisconnect( hdata );
				break;

			case NetType.HostSend:
				if ( WifiMode == WifiState.Client )
					HandleHostSend( buf, len, clientId, hdata, payOff );
				break;

			case NetType.ClientSend:
				if ( WifiMode == WifiState.Host )
					HandleClientSend( buf, hdata, payOff );
				goto case NetType.ClientAck;

			case NetType.ClientAck:
				if ( WifiMode == WifiState.Host )
				{
					ushort devId = (ushort)(hdata & 0xFFFF);
					int slot = (int)((hdata >> 16) & 0x3);
					if ( _host.Clients[slot].DeviceId == devId )
						_host.Clients[slot].TimeToLive = 0;
				}
				break;
		}
	}

	private void HandleConnectRequest( int clientId )
	{
		if ( WifiMode != WifiState.Host )
		{
			SendNetCommand( clientId, NetType.ConnectNack, 0 );
			return;
		}

		for ( int i = 0; i < MaxClients; i++ )
			if ( _host.Clients[i].DeviceId != 0 && _host.Clients[i].ClientId == clientId )
				return;

		for ( int i = 0; i < MaxClients; i++ )
		{
			if ( _host.Clients[i].DeviceId == 0 )
			{
				ushort newId = NewDeviceId();
				_host.Clients[i].DeviceId = newId;
				_host.Clients[i].ClientId = (ushort)clientId;
				SendNetCommand( clientId, NetType.ConnectAck, (uint)(newId | (i << 16)) );
				return;
			}
		}

		SendNetCommand( clientId, NetType.ConnectNack, 0 );
	}

	private void HandleDisconnect( uint hdata )
	{
		if ( WifiMode == WifiState.Host )
		{
			int slot = (int)((hdata >> 16) & 0x3);
			ushort devId = (ushort)(hdata & 0xFFFF);
			if ( _host.Clients[slot].DeviceId == devId )
				ClearClientSlot( slot );
		}
		else if ( WifiMode == WifiState.Client )
		{
			InitClient();
			WifiMode = WifiState.Idle;
		}
	}

	private void HandleHostSend( byte[] buf, int len, int clientId, uint hdata, int payOff )
	{
		uint blen = hdata & 0x7F;
		if ( len < blen + 12 )
			return;

		SendNetCommand( clientId, NetType.ClientAck,
			(uint)(_client.DeviceId | (_client.SlotNumber << 16)) );

		for ( int i = 0; i < ClientQueueSize; i++ )
		{
			if ( _client.Queue[i].Length == 0 )
			{
				Buffer.BlockCopy( buf, payOff, _client.Queue[i].Data, 0, (int)blen );
				_client.Queue[i].Length = (ushort)blen;
				return;
			}
		}
	}

	private void HandleClientSend( byte[] buf, uint hdata, int payOff )
	{
		ushort devId = (ushort)(hdata & 0xFFFF);
		int slot = (int)((hdata >> 16) & 0x3);
		uint blen = hdata >> 24;
		if ( _host.Clients[slot].DeviceId != devId )
			return;

		_host.Clients[slot].TimeToLive = 0;
		for ( int i = 0; i < HostQueueSize; i++ )
		{
			if ( _host.Clients[slot].Queue[i].Length == 0 )
			{
				Buffer.BlockCopy( buf, payOff, _host.Clients[slot].Queue[i].Data, 0, (int)blen );
				_host.Clients[slot].Queue[i].Length = blen;
				return;
			}
		}
	}

	public bool Update( uint cycles, ushort sioCnt, out bool wroteSio, out ushort newSioCnt, out ushort newSioDataLo, out ushort newSioDataHi )
	{
		wroteSio = false;
		newSioCnt = sioCnt;
		newSioDataLo = 0;
		newSioDataHi = 0;

		if ( SpiState == ComState.WaitEvent )
		{
			DrainInbox();
			_timeoutCycles -= Math.Min( cycles, _timeoutCycles );
			_responseTimeoutCycles -= Math.Min( cycles, _responseTimeoutCycles );

			if ( (sioCnt & 0x1) == 0 )
				EvaluateWaitEvent();
		}

		if ( SpiState == ComState.WaitResponse )
		{
			if ( (sioCnt & 0xC) == 0 && (sioCnt & 0x80) != 0 )
			{
				uint w = _buffer[_bufferIndex];
				newSioDataLo = (ushort)(w & 0xFFFF);
				newSioDataHi = (ushort)(w >> 16);
				newSioCnt = (ushort)(sioCnt & ~0x80);
				wroteSio = true;
				_bufferIndex++;
				if ( _bufferIndex == _payloadLength )
					SpiState = ComState.WaitCommand;
				return (newSioCnt & 0x4000) != 0;
			}
		}

		return false;
	}

	private void EvaluateWaitEvent()
	{
		if ( WifiMode == WifiState.Idle )
		{
			_buffer[0] = 0x99660000u | (1u << 8) | (byte)Command.ResponseDisconnect;
			_buffer[1] = 0xF;
			_buffer[2] = 0x80000000;
			_bufferIndex = 0;
			_payloadLength = 3;
			SpiState = ComState.WaitResponse;
		}
		else if ( HasInboundData() )
		{
			_buffer[0] = 0x99660000u | (byte)Command.ResponseData;
			_buffer[1] = 0x80000000;
			_bufferIndex = 0;
			_payloadLength = 2;
			SpiState = ComState.WaitResponse;
		}
		else if ( WifiMode == WifiState.Host && _responseTimeoutCycles == 0 )
		{
			_buffer[0] = 0x99660000u | (byte)Command.ResponseData | (1u << 8);
			_buffer[1] = 0x00000F0F;
			_buffer[2] = 0x80000000;
			_bufferIndex = 0;
			_payloadLength = 3;
			SpiState = ComState.WaitResponse;
		}
		else if ( _timeoutCycles == 0 )
		{
			_buffer[0] = 0x99660000u | (byte)Command.ResponseTimeout;
			_buffer[1] = 0x80000000;
			_bufferIndex = 0;
			_payloadLength = 2;
			SpiState = ComState.WaitResponse;
		}
	}

	private void SendNetCommand( int clientId, NetType type, uint hdata )
	{
		if ( Network is null )
			return;
		var pkt = new byte[16];
		WriteBe32( pkt, 0, NetHeaderMagic );
		WriteBe32( pkt, 4, (uint)type );
		WriteBe32( pkt, 8, hdata );
		Network.Send( clientId, pkt, 16 );
	}

	private void SendNetBroadcast( NetType type, uint hdata, uint[] payload )
	{
		if ( Network is null )
			return;
		var pkt = new byte[36];
		WriteBe32( pkt, 0, NetHeaderMagic );
		WriteBe32( pkt, 4, (uint)type );
		WriteBe32( pkt, 8, hdata );
		for ( int i = 0; i < 6; i++ )
			WriteBe32( pkt, 12 + i * 4, payload[i] );
		Network.Send( 0xFFFF, pkt, 36 );
	}

	private void SendNetData( int clientId, NetType type, uint hdata, uint[] payload, uint length )
	{
		if ( Network is null )
			return;
		var pkt = new byte[12 + 92];
		WriteBe32( pkt, 0, NetHeaderMagic );
		WriteBe32( pkt, 4, (uint)type );
		WriteBe32( pkt, 8, hdata );
		for ( int i = 0; i < length; i++ )
			pkt[12 + i] = (byte)(payload[i / 4] >> (8 * (i & 3)));
		Network.Send( clientId, pkt, 12 + 92 );
	}

	private static uint ReadBe32( byte[] b, int o )
		=> (uint)((b[o] << 24) | (b[o + 1] << 16) | (b[o + 2] << 8) | b[o + 3]);

	private static uint ReadLe32( byte[] b, int o )
	{
		uint v = 0;
		int n = Math.Min( 4, b.Length - o );
		for ( int i = 0; i < n; i++ )
			v |= (uint)b[o + i] << (8 * i);
		return v;
	}

	private static void WriteBe32( byte[] b, int o, uint v )
	{
		b[o + 0] = (byte)(v >> 24);
		b[o + 1] = (byte)(v >> 16);
		b[o + 2] = (byte)(v >> 8);
		b[o + 3] = (byte)v;
	}

	private static void CopyWords( uint[] src, int srcOff, uint[] dst, int dstOff, int count )
	{
		if ( count <= 0 )
			return;
		int n = Math.Min( count, Math.Min( src.Length - srcOff, dst.Length - dstOff ) );
		Array.Copy( src, srcOff, dst, dstOff, n );
	}
}
