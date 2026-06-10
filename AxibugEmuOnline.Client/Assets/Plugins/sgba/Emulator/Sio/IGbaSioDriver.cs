namespace sGBA;

public interface IGbaSioDriver
{
	GbaIo Sio { get; set; }

	bool Init();
	void Deinit();
	void Reset();
	void SetMode( GbaSioMode mode );
	bool HandlesMode( GbaSioMode mode );
	int ConnectedDevices();
	int DeviceId();
	ushort WriteSioCnt( ushort value );
	ushort WriteRcnt( ushort value );
	bool Start();
	void FinishMultiplayer( ushort[] data );
	byte FinishNormal8();
	uint FinishNormal32();
}
