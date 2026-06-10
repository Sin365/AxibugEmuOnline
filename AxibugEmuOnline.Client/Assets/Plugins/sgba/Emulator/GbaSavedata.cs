using System.IO;
using System.Text;

namespace sGBA;

public class GbaSavedata
{
	public Gba Gba { get; }
	public SavedataType Type { get; private set; }
	public byte[] Data { get; set; }
	public SavedataCommand Command { get; set; }
	public FlashStateMachine FlashState { get; set; }

	public int ReadBitsRemaining { get; set; }
	public uint ReadAddress { get; set; }
	public uint WriteAddress { get; set; }

	public uint Settling { get; set; }
	public long DustEndCycle { get; set; }

	public int Dirty { get; set; }
	public uint DirtAge { get; set; }

	private int _currentBank;

	private const int FlashEraseCycles = 30000;
	private const int FlashProgramCycles = 650;
	private const int EepromSettleCycles = 115000;
	private const int SavedataCleanupThreshold = 15;

	private const int DirtNone = 0;
	private const int DirtNew = 1;
	private const int DirtSeen = 2;

	private const int FlashBaseHi = 0x5555;
	private const int FlashBaseLo = 0x2AAA;

	private const ushort FlashPanasonicMn63F805Mnp = 0x1B32;
	private const ushort FlashSanyoLe26Fv10N1Ts = 0x1362;

	public GbaSavedata( Gba gba )
	{
		Gba = gba;
		Type = SavedataType.None;
		Data = [];
		Command = SavedataCommand.EepromNull;
		FlashState = FlashStateMachine.Raw;
	}

	public void Reset()
	{
		Command = SavedataCommand.EepromNull;
		FlashState = FlashStateMachine.Raw;
		ReadBitsRemaining = 0;
		ReadAddress = 0;
		WriteAddress = 0;
		_currentBank = 0;
		Settling = 0;
		DustEndCycle = 0;
		Dirty = DirtNone;
		DirtAge = 0;
	}

	public void ForceType( byte[] rom )
	{
		string romText = Encoding.ASCII.GetString( rom );

		if ( romText.Contains( "FLASH1M_V" ) )
		{
			Type = SavedataType.Flash1M;
			Data = new byte[GbaConstants.Flash1MSize];
			Array.Fill( Data, (byte)0xFF );
			_currentBank = 0;
			GbaLog.Write( LogCategory.GBASave, LogLevel.Info, "Detected Flash 128K savegame" );
		}
		else if ( romText.Contains( "FLASH_V" ) || romText.Contains( "FLASH512_V" ) )
		{
			Type = SavedataType.Flash512;
			Data = new byte[GbaConstants.Flash512Size];
			Array.Fill( Data, (byte)0xFF );
			_currentBank = 0;
			GbaLog.Write( LogCategory.GBASave, LogLevel.Info, "Detected Flash 64K savegame" );
		}
		else if ( romText.Contains( "SRAM_V" ) || romText.Contains( "SRAM_F_V" ) )
		{
			Type = SavedataType.Sram;
			Data = new byte[GbaConstants.SramSize];
			Array.Fill( Data, (byte)0xFF );
			GbaLog.Write( LogCategory.GBASave, LogLevel.Info, "Detected SRAM savegame" );
		}
		else if ( romText.Contains( "EEPROM_V" ) )
		{
			Type = SavedataType.Eeprom;
			Data = new byte[GbaConstants.EepromSize];
			Array.Fill( Data, (byte)0xFF );
			GbaLog.Write( LogCategory.GBASave, LogLevel.Info, "Detected EEPROM savegame" );
		}
		else
		{
			Type = SavedataType.None;
			Data = [];
		}
	}

	public void Load( byte[] saveData )
	{
		if ( saveData == null || saveData.Length == 0 || Data.Length == 0 )
			return;

		int copyLen = Math.Min( saveData.Length, Data.Length );
		Array.Copy( saveData, Data, copyLen );
	}

	private bool IsDustScheduled()
	{
		return DustEndCycle > Gba.Cpu.Cycles;
	}

	private void ScheduleDust( int cycles )
	{
		DustEndCycle = Gba.Cpu.Cycles + cycles;
	}

	public byte Read8( uint address )
	{
		uint offset = address & 0xFFFF;

		switch ( Type )
		{
			case SavedataType.Sram:
				return Data[offset & (uint)(Data.Length - 1)];

			case SavedataType.Flash512:
			case SavedataType.Flash1M:
				return ReadFlash( (ushort)offset );

			default:
				return 0;
		}
	}

	public void Write8( uint address, byte value )
	{
		uint offset = address & 0xFFFF;

		switch ( Type )
		{
			case SavedataType.Sram:
				Data[offset & (uint)(Data.Length - 1)] = value;
				Dirty |= DirtNew;
				break;

			case SavedataType.Flash512:
			case SavedataType.Flash1M:
				WriteFlash( (ushort)offset, value );
				break;
		}
	}

	private byte ReadFlash( ushort address )
	{
		if ( Command == SavedataCommand.FlashId )
		{
			if ( Type == SavedataType.Flash512 )
			{
				if ( address < 2 )
					return (byte)(FlashPanasonicMn63F805Mnp >> (address * 8));
			}
			else if ( Type == SavedataType.Flash1M )
			{
				if ( address < 2 )
					return (byte)(FlashSanyoLe26Fv10N1Ts >> (address * 8));
			}
		}

		if ( IsDustScheduled() && (address >> 12) == Settling )
			return (byte)((Data[_currentBank + address] ^ 0x80) & 0x80);

		return Data[_currentBank + address];
	}

	private void WriteFlash( ushort address, byte value )
	{
		switch ( FlashState )
		{
			case FlashStateMachine.Raw:
				switch ( Command )
				{
					case SavedataCommand.FlashProgram:
						Dirty |= DirtNew;
						Data[_currentBank + address] = value;
						Command = SavedataCommand.FlashNone;
						ScheduleDust( FlashProgramCycles );
						break;

					case SavedataCommand.FlashSwitchBank:
						if ( address == 0 && value < 2 )
							FlashSwitchBank( value );
						Command = SavedataCommand.FlashNone;
						break;

					default:
						if ( address == FlashBaseHi && value == (byte)SavedataCommand.FlashStart )
							FlashState = FlashStateMachine.Start;
						break;
				}
				break;

			case FlashStateMachine.Start:
				if ( address == FlashBaseLo && value == (byte)SavedataCommand.FlashContinue )
					FlashState = FlashStateMachine.Continue;
				else
					FlashState = FlashStateMachine.Raw;
				break;

			case FlashStateMachine.Continue:
				FlashState = FlashStateMachine.Raw;
				if ( address == FlashBaseHi )
				{
					switch ( Command )
					{
						case SavedataCommand.FlashNone:
							switch ( (SavedataCommand)value )
							{
								case SavedataCommand.FlashErase:
								case SavedataCommand.FlashId:
								case SavedataCommand.FlashProgram:
								case SavedataCommand.FlashSwitchBank:
									Command = (SavedataCommand)value;
									break;
							}
							break;

						case SavedataCommand.FlashErase:
							if ( value == (byte)SavedataCommand.FlashEraseChip )
								FlashErase();
							Command = SavedataCommand.FlashNone;
							break;

						case SavedataCommand.FlashId:
							if ( value == (byte)SavedataCommand.FlashTerminate )
								Command = SavedataCommand.FlashNone;
							break;

						default:
							Command = SavedataCommand.FlashNone;
							break;
					}
				}
				else if ( Command == SavedataCommand.FlashErase )
				{
					if ( value == (byte)SavedataCommand.FlashEraseSector )
					{
						FlashEraseSector( address );
						Command = SavedataCommand.FlashNone;
					}
				}
				break;
		}
	}

	private void FlashSwitchBank( int bank )
	{
		if ( bank > 0 && Type == SavedataType.Flash512 )
		{
			GbaLog.Write( LogCategory.GBASave, LogLevel.Info, "Upgrading flash chip from 512kb to 1Mb" );
			Type = SavedataType.Flash1M;
			byte[] newData = new byte[GbaConstants.Flash1MSize];
			Array.Fill( newData, (byte)0xFF );
			Array.Copy( Data, newData, Data.Length );
			Data = newData;
		}
		_currentBank = bank << 16;
	}

	private void FlashErase()
	{
		Dirty |= DirtNew;
		int size = Type == SavedataType.Flash1M ? GbaConstants.Flash1MSize : GbaConstants.Flash512Size;
		Array.Fill( Data, (byte)0xFF, 0, size );
	}

	private void FlashEraseSector( ushort sectorStart )
	{
		Dirty |= DirtNew;
		Settling = (uint)(sectorStart >> 12);
		ScheduleDust( FlashEraseCycles );
		Array.Fill( Data, (byte)0xFF, _currentBank + (sectorStart & 0xF000), 0x1000 );
	}

	public ushort ReadEEPROM()
	{
		if ( Command != SavedataCommand.EepromRead )
		{
			if ( !IsDustScheduled() )
				return 1;
			return 0;
		}

		--ReadBitsRemaining;
		if ( ReadBitsRemaining < 64 )
		{
			int step = 63 - ReadBitsRemaining;
			uint address = (ReadAddress + (uint)step) >> 3;
			if ( address >= GbaConstants.EepromSize )
			{
				if ( ReadBitsRemaining == 0 )
					Command = SavedataCommand.EepromNull;
				return 0xFF;
			}
			byte data = Data[address];
			if ( ReadBitsRemaining == 0 )
				Command = SavedataCommand.EepromNull;
			return (ushort)((data >> (0x7 - (step & 0x7))) & 0x1);
		}
		return 0;
	}

	public void WriteEEPROM( ushort value, int writeSize )
	{
		switch ( Command )
		{
			case SavedataCommand.EepromNull:
			default:
				Command = (SavedataCommand)(value & 0x1);
				break;

			case SavedataCommand.EepromPending:
				Command = (SavedataCommand)(((int)Command << 1) | (value & 0x1));
				if ( Command == SavedataCommand.EepromWrite )
					WriteAddress = 0;
				else
					ReadAddress = 0;
				break;

			case SavedataCommand.EepromWrite:
				if ( writeSize > 65 )
				{
					WriteAddress <<= 1;
					WriteAddress |= (uint)(value & 0x1) << 6;
				}
				else if ( writeSize == 1 )
				{
					Command = SavedataCommand.EepromNull;
				}
				else
				{
					uint byteAddr = WriteAddress >> 3;
					if ( byteAddr < GbaConstants.EepromSize )
					{
						Dirty |= DirtNew;
						byte current = Data[byteAddr];
						current &= (byte)~(1 << (0x7 - (int)(WriteAddress & 0x7)));
						current |= (byte)((value & 0x1) << (0x7 - (int)(WriteAddress & 0x7)));
						Data[byteAddr] = current;
						ScheduleDust( EepromSettleCycles );
					}
					++WriteAddress;
				}
				break;

			case SavedataCommand.EepromReadPending:
				if ( writeSize > 1 )
				{
					ReadAddress <<= 1;
					if ( (value & 0x1) != 0 )
						ReadAddress |= 0x40;
				}
				else
				{
					ReadBitsRemaining = 68;
					Command = SavedataCommand.EepromRead;
				}
				break;
		}
	}

	public bool Clean()
	{
		if ( (Dirty & DirtNew) != 0 )
		{
			DirtAge = 0;
			Dirty &= ~DirtNew;
			Dirty |= DirtSeen;
		}
		else if ( (Dirty & DirtSeen) != 0 )
		{
			++DirtAge;
			if ( DirtAge > SavedataCleanupThreshold )
			{
				Dirty = DirtNone;
				return true;
			}
		}
		return false;
	}

	public void Serialize( BinaryWriter w )
	{
		w.Write( (int)Command );
		w.Write( (int)FlashState );
		w.Write( _currentBank );
		w.Write( ReadBitsRemaining );
		w.Write( ReadAddress );
		w.Write( WriteAddress );
		w.Write( Settling );
		w.Write( DustEndCycle );
		w.Write( Dirty );
		w.Write( DirtAge );
	}

	public void Deserialize( BinaryReader r )
	{
		Command = (SavedataCommand)r.ReadInt32();
		FlashState = (FlashStateMachine)r.ReadInt32();
		_currentBank = r.ReadInt32();
		ReadBitsRemaining = r.ReadInt32();
		ReadAddress = r.ReadUInt32();
		WriteAddress = r.ReadUInt32();
		Settling = r.ReadUInt32();
		DustEndCycle = r.ReadInt64();
		Dirty = r.ReadInt32();
		DirtAge = r.ReadUInt32();
	}
}

public enum SavedataType
{
	None = 0,
	Sram = 1,
	Flash512 = 2,
	Flash1M = 3,
	Eeprom = 4,
}

public enum SavedataCommand
{
	EepromNull = 0,
	EepromPending = 1,
	EepromWrite = 2,
	EepromReadPending = 3,
	EepromRead = 4,

	FlashStart = 0xAA,
	FlashContinue = 0x55,

	FlashEraseChip = 0x10,
	FlashEraseSector = 0x30,

	FlashNone = 0,
	FlashErase = 0x80,
	FlashId = 0x90,
	FlashProgram = 0xA0,
	FlashSwitchBank = 0xB0,
	FlashTerminate = 0xF0,
}

public enum FlashStateMachine
{
	Raw = 0,
	Start = 1,
	Continue = 2,
}
