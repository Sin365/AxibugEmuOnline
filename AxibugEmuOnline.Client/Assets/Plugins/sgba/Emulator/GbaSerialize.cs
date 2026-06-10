using System.IO;

namespace sGBA;

public static class GbaSerialize
{
	private const uint Magic = 0x53474241;
	public const int SlotCount = 4;
	public const int ScreenshotSize = GbaConstants.ScreenWidth * GbaConstants.ScreenHeight * 4;
	private const int HeaderSize = sizeof( uint ) + sizeof( long ); // magic + timestamp

	public static byte[] Save( Gba gba, byte[] screenshot )
	{
		using var ms = new MemoryStream();
		using var w = new BinaryWriter( ms );

		w.Write( Magic );
		w.Write( DateTime.UtcNow.Ticks );

		if ( screenshot != null && screenshot.Length == ScreenshotSize )
			w.Write( screenshot );
		else
			w.Write( new byte[ScreenshotSize] );

		WriteCpu( w, gba.Cpu );
		WriteMemory( w, gba.Memory );
		WriteVideo( w, gba.Video );
		WriteAudio( w, gba.Audio );
		WriteIo( w, gba.Io );
		WriteDma( w, gba.Dma );
		WriteTimers( w, gba.Timers );
		WriteSavedata( w, gba.Savedata );
		WriteHardware( w, gba.Hardware );
		WriteBios( w, gba.Bios );
		WriteSystem( w, gba );

		gba.Cpu.SerializePipeline( w );

		return ms.ToArray();
	}

	public static void Load( Gba gba, byte[] data )
	{
		using var ms = new MemoryStream( data );
		using var r = new BinaryReader( ms );

		ValidateHeader( r );
		r.BaseStream.Position = HeaderSize + ScreenshotSize;

		ReadCpu( r, gba.Cpu );
		ReadMemory( r, gba.Memory );
		ReadVideo( r, gba.Video );
		ReadAudio( r, gba.Audio );
		ReadIo( r, gba.Io );
		ReadDma( r, gba.Dma );
		ReadTimers( r, gba.Timers );
		ReadSavedata( r, gba.Savedata );
		ReadHardware( r, gba.Hardware );
		ReadBios( r, gba.Bios );
		ReadSystem( r, gba );

		gba.Cpu.DeserializePipeline( r );
		gba.Memory.InstallHleBios();

		gba.Audio.SamplesWritten = 0;

		gba.Video._firstAffine = -1;
		gba.Video._lastDrawnY = -1;
		for ( int i = 0; i < 4; i++ )
		{
			bool enabled = (gba.Video.DispCnt & (0x100 << i)) != 0;
			gba.Video._enabledAtY[i] = enabled ? 0 : int.MaxValue;
			gba.Video._wasFullyEnabled[i] = enabled;
		}
		gba.Video._oldCharBase[0] = (uint)((gba.Video.BgCnt[2] >> 2) & 3) * 0x4000u;
		gba.Video._oldCharBase[1] = (uint)((gba.Video.BgCnt[3] >> 2) & 3) * 0x4000u;
		gba.Video._oldCharBaseFirstY[0] = 0;
		gba.Video._oldCharBaseFirstY[1] = 0;
	}

	public static byte[] ReadScreenshot( byte[] data )
	{
		if ( data == null || data.Length < HeaderSize + ScreenshotSize )
			return null;

		using var ms = new MemoryStream( data );
		using var r = new BinaryReader( ms );

		if ( !TryValidateHeader( r ) )
			return null;

		r.BaseStream.Position = HeaderSize;
		return r.ReadBytes( ScreenshotSize );
	}

	public static DateTime? ReadTimestamp( byte[] data )
	{
		if ( data == null || data.Length < HeaderSize )
			return null;

		using var ms = new MemoryStream( data );
		using var r = new BinaryReader( ms );

		if ( !TryValidateHeader( r ) )
			return null;

		return new DateTime( r.ReadInt64(), DateTimeKind.Utc ).ToLocalTime();
	}

	private static void ValidateHeader( BinaryReader r )
	{
		if ( !TryValidateHeader( r ) )
			throw new Exception( "Invalid suspend point." );
	}

	private static bool TryValidateHeader( BinaryReader r )
	{
		return r.ReadUInt32() == Magic;
	}

	private static void WriteCpu( BinaryWriter w, ArmCore cpu )
	{
		for ( int i = 0; i < 16; i++ ) w.Write( cpu.Gprs[i] );
		w.Write( cpu.FlagN ); w.Write( cpu.FlagZ ); w.Write( cpu.FlagC ); w.Write( cpu.FlagV );
		w.Write( cpu.IrqDisable ); w.Write( cpu.FiqDisable );
		w.Write( cpu.ThumbMode );
		w.Write( (int)cpu.PrivilegeMode );

		for ( int i = 0; i < 6; i++ )
		{
			w.Write( cpu.BankedSPSRs[i] );
		}

		WriteBankedRegs( w, cpu );

		w.Write( cpu.Cycles );
		w.Write( cpu.Halted ); w.Write( cpu.IrqPending );
		w.Write( cpu.OpenBusPrefetch );
	}

	private static void WriteBankedRegs( BinaryWriter w, ArmCore cpu )
	{
		var origMode = cpu.PrivilegeMode;
		var origR13 = cpu.Gprs[13];
		var origR14 = cpu.Gprs[14];

		PrivilegeMode[] modes = [PrivilegeMode.User, PrivilegeMode.FIQ, PrivilegeMode.IRQ, PrivilegeMode.Supervisor, PrivilegeMode.Abort, PrivilegeMode.Undefined];

		foreach ( var mode in modes )
		{
			cpu.SetPrivilegeMode( mode );
			w.Write( cpu.Gprs[13] );
			w.Write( cpu.Gprs[14] );
		}

		cpu.SetPrivilegeMode( PrivilegeMode.FIQ );
		for ( int i = 8; i <= 12; i++ ) w.Write( cpu.Gprs[i] );

		cpu.SetPrivilegeMode( PrivilegeMode.System );
		for ( int i = 8; i <= 12; i++ ) w.Write( cpu.Gprs[i] );

		cpu.SetPrivilegeMode( origMode );
		cpu.Gprs[13] = origR13;
		cpu.Gprs[14] = origR14;
	}

	private static void ReadCpu( BinaryReader r, ArmCore cpu )
	{
		for ( int i = 0; i < 16; i++ ) cpu.Gprs[i] = r.ReadUInt32();
		cpu.FlagN = r.ReadBoolean(); cpu.FlagZ = r.ReadBoolean();
		cpu.FlagC = r.ReadBoolean(); cpu.FlagV = r.ReadBoolean();
		cpu.IrqDisable = r.ReadBoolean(); cpu.FiqDisable = r.ReadBoolean();
		cpu.ThumbMode = r.ReadBoolean();
		cpu.PrivilegeMode = (PrivilegeMode)r.ReadInt32();

		for ( int i = 0; i < 6; i++ )
			cpu.BankedSPSRs[i] = r.ReadUInt32();

		ReadBankedRegs( r, cpu );

		cpu.Cycles = r.ReadInt64();
		cpu.Halted = r.ReadBoolean(); cpu.IrqPending = r.ReadBoolean();
		cpu.OpenBusPrefetch = r.ReadUInt32();
	}

	private static void ReadBankedRegs( BinaryReader r, ArmCore cpu )
	{
		PrivilegeMode[] modes = [PrivilegeMode.User, PrivilegeMode.FIQ, PrivilegeMode.IRQ, PrivilegeMode.Supervisor, PrivilegeMode.Abort, PrivilegeMode.Undefined];
		var targetMode = cpu.PrivilegeMode;
		var savedR13 = cpu.Gprs[13];
		var savedR14 = cpu.Gprs[14];

		foreach ( var mode in modes )
		{
			cpu.SetPrivilegeMode( mode );
			cpu.Gprs[13] = r.ReadUInt32();
			cpu.Gprs[14] = r.ReadUInt32();
		}

		cpu.SetPrivilegeMode( PrivilegeMode.FIQ );
		for ( int i = 8; i <= 12; i++ ) cpu.Gprs[i] = r.ReadUInt32();

		cpu.SetPrivilegeMode( PrivilegeMode.System );
		for ( int i = 8; i <= 12; i++ ) cpu.Gprs[i] = r.ReadUInt32();

		cpu.SetPrivilegeMode( targetMode );
		cpu.Gprs[13] = savedR13;
		cpu.Gprs[14] = savedR14;
	}

	private static void WriteMemory( BinaryWriter w, GbaMemory memory )
	{
		w.Write( memory.Wram );
		w.Write( memory.Iwram );
		w.Write( memory.PaletteRam );
		w.Write( memory.Vram );
		w.Write( memory.Oam );

		for ( int i = 0; i < memory.Io.Length; i++ )
			w.Write( memory.Io[i] );

		w.Write( memory.BiosPrefetch );
		w.Write( memory.Prefetch );
		w.Write( memory.LastPrefetchedPc );

		for ( int i = 0; i < 16; i++ ) w.Write( memory.WaitstatesNonseq16[i] );
		for ( int i = 0; i < 16; i++ ) w.Write( memory.WaitstatesNonseq32[i] );
		for ( int i = 0; i < 16; i++ ) w.Write( memory.WaitstatesSeq16[i] );
		for ( int i = 0; i < 16; i++ ) w.Write( memory.WaitstatesSeq32[i] );

		w.Write( memory.Debug );
		w.Write( memory.DebugString );
		w.Write( memory.DebugFlags );
	}

	private static void ReadMemory( BinaryReader r, GbaMemory memory )
	{
		r.Read( memory.Wram );
		r.Read( memory.Iwram );
		r.Read( memory.PaletteRam );
		r.Read( memory.Vram );
		r.Read( memory.Oam );

		for ( int i = 0; i < memory.Io.Length; i++ )
			memory.Io[i] = r.ReadUInt16();

		memory.BiosPrefetch = r.ReadUInt32();
		memory.Prefetch = r.ReadBoolean();
		memory.LastPrefetchedPc = r.ReadUInt32();

		for ( int i = 0; i < 16; i++ ) memory.WaitstatesNonseq16[i] = r.ReadInt32();
		for ( int i = 0; i < 16; i++ ) memory.WaitstatesNonseq32[i] = r.ReadInt32();
		for ( int i = 0; i < 16; i++ ) memory.WaitstatesSeq16[i] = r.ReadInt32();
		for ( int i = 0; i < 16; i++ ) memory.WaitstatesSeq32[i] = r.ReadInt32();

		memory.Debug = r.ReadBoolean();
		r.Read( memory.DebugString );
		memory.DebugFlags = r.ReadUInt16();
		memory.ClearAgbPrint();
	}

	private static void WriteVideo( BinaryWriter w, GbaVideo video )
	{
		w.Write( video.VCount ); w.Write( video.Dot );
		w.Write( video.DispCnt ); w.Write( video.DispStat );

		for ( int i = 0; i < 4; i++ ) w.Write( video.BgCnt[i] );
		for ( int i = 0; i < 4; i++ ) w.Write( video.BgHOfs[i] );
		for ( int i = 0; i < 4; i++ ) w.Write( video.BgVOfs[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgPA[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgPB[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgPC[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgPD[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgX[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgY[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgRefX[i] );
		for ( int i = 0; i < 2; i++ ) w.Write( video.BgRefY[i] );

		w.Write( video.BldCnt ); w.Write( video.BldAlpha ); w.Write( video.BldY );
		w.Write( video.Win0H ); w.Write( video.Win0V );
		w.Write( video.Win1H ); w.Write( video.Win1V );
		w.Write( video.WinIn ); w.Write( video.WinOut );
		w.Write( video.Mosaic );
	}

	private static void ReadVideo( BinaryReader r, GbaVideo video )
	{
		video.VCount = r.ReadInt32(); video.Dot = r.ReadInt32();
		video.DispCnt = r.ReadUInt16(); video.DispStat = r.ReadUInt16();

		for ( int i = 0; i < 4; i++ ) video.BgCnt[i] = r.ReadUInt16();
		for ( int i = 0; i < 4; i++ ) video.BgHOfs[i] = r.ReadInt16();
		for ( int i = 0; i < 4; i++ ) video.BgVOfs[i] = r.ReadInt16();
		for ( int i = 0; i < 2; i++ ) video.BgPA[i] = r.ReadInt16();
		for ( int i = 0; i < 2; i++ ) video.BgPB[i] = r.ReadInt16();
		for ( int i = 0; i < 2; i++ ) video.BgPC[i] = r.ReadInt16();
		for ( int i = 0; i < 2; i++ ) video.BgPD[i] = r.ReadInt16();
		for ( int i = 0; i < 2; i++ ) video.BgX[i] = r.ReadInt32();
		for ( int i = 0; i < 2; i++ ) video.BgY[i] = r.ReadInt32();
		for ( int i = 0; i < 2; i++ ) video.BgRefX[i] = r.ReadInt32();
		for ( int i = 0; i < 2; i++ ) video.BgRefY[i] = r.ReadInt32();

		video.BldCnt = r.ReadUInt16(); video.BldAlpha = r.ReadUInt16(); video.BldY = r.ReadUInt16();
		video.Win0H = r.ReadUInt16(); video.Win0V = r.ReadUInt16();
		video.Win1H = r.ReadUInt16(); video.Win1V = r.ReadUInt16();
		video.WinIn = r.ReadUInt16(); video.WinOut = r.ReadUInt16();
		video.Mosaic = r.ReadUInt16();
	}

	private static void WriteAudio( BinaryWriter w, GbaAudio audio )
	{
		w.Write( audio.Enable );
		w.Write( audio.SoundBias );

		w.Write( audio.Sound1CntL ); w.Write( audio.Sound1CntH ); w.Write( audio.Sound1CntX );
		w.Write( audio.Sound2CntL ); w.Write( audio.Sound2CntH );
		w.Write( audio.Sound3CntL ); w.Write( audio.Sound3CntH ); w.Write( audio.Sound3CntX );
		w.Write( audio.Sound4CntL ); w.Write( audio.Sound4CntH );
		w.Write( audio.SoundCntL ); w.Write( audio.SoundCntH ); w.Write( audio.SoundCntX );

		w.Write( audio.WaveRam );

		audio.Serialize( w );
	}

	private static void ReadAudio( BinaryReader r, GbaAudio audio )
	{
		audio.Enable = r.ReadBoolean();
		audio.SoundBias = r.ReadUInt16();

		audio.Sound1CntL = r.ReadUInt16(); audio.Sound1CntH = r.ReadUInt16(); audio.Sound1CntX = r.ReadUInt16();
		audio.Sound2CntL = r.ReadUInt16(); audio.Sound2CntH = r.ReadUInt16();
		audio.Sound3CntL = r.ReadUInt16(); audio.Sound3CntH = r.ReadUInt16(); audio.Sound3CntX = r.ReadUInt16();
		audio.Sound4CntL = r.ReadUInt16(); audio.Sound4CntH = r.ReadUInt16();
		audio.SoundCntL = r.ReadUInt16(); audio.SoundCntH = r.ReadUInt16(); audio.SoundCntX = r.ReadUInt16();

		r.Read( audio.WaveRam );

		audio.Deserialize( r );
	}

	private static void WriteIo( BinaryWriter w, GbaIo io )
	{
		w.Write( io.IE ); w.Write( io.IF ); w.Write( io.IME );
		w.Write( io.WaitCnt );
		w.Write( io.Read16( 0x130 ) ); w.Write( io.KeyCnt );
		w.Write( io.Rcnt );
		w.Write( io.PostFlg );
		w.Write( io.Gba.HaltPending );
		io.Serialize( w );
	}

	private static void ReadIo( BinaryReader r, GbaIo io )
	{
		io.IE = r.ReadUInt16(); io.IF = r.ReadUInt16(); io.IME = r.ReadUInt16();
		io.WaitCnt = r.ReadUInt16();
		ushort keyInput = r.ReadUInt16(); io.KeyCnt = r.ReadUInt16();
		io.Gba.KeysActive = (ushort)(0x03FF ^ (keyInput & 0x03FF));
		io.Gba.Memory.Io[0x130 >> 1] = keyInput;
		io.Rcnt = r.ReadUInt16();
		io.PostFlg = r.ReadByte();
		io.Gba.HaltPending = r.ReadBoolean();
		io.Deserialize( r );
	}

	private static void WriteDma( BinaryWriter w, GbaDmaController dma )
	{
		w.Write( dma.ActiveDma );
		w.Write( dma.CpuBlocked );
		w.Write( dma.PerformingDma );

		for ( int i = 0; i < 4; i++ )
		{
			var c = dma.Channels[i];
			w.Write( c.SrcLow ); w.Write( c.SrcHigh );
			w.Write( c.DstLow ); w.Write( c.DstHigh );
			w.Write( c.CntLo ); w.Write( c.Reg );
			w.Write( c.NextSource ); w.Write( c.NextDest );
			w.Write( c.NextCount ); w.Write( c.Count ); w.Write( c.Latch );
			w.Write( c.When ); w.Write( c.Cycles );
			w.Write( c.IsFirstUnit );
			w.Write( c.SourceOffset ); w.Write( c.DestOffset );
			w.Write( c.DestInvalid );
		}
	}

	private static void ReadDma( BinaryReader r, GbaDmaController dma )
	{
		dma.ActiveDma = r.ReadInt32();
		dma.CpuBlocked = r.ReadBoolean();
		dma.PerformingDma = r.ReadInt32();

		for ( int i = 0; i < 4; i++ )
		{
			var c = dma.Channels[i];
			c.SrcLow = r.ReadUInt16(); c.SrcHigh = r.ReadUInt16();
			c.DstLow = r.ReadUInt16(); c.DstHigh = r.ReadUInt16();
			c.CntLo = r.ReadUInt16(); c.Reg = r.ReadUInt16();
			c.NextSource = r.ReadUInt32(); c.NextDest = r.ReadUInt32();
			c.NextCount = r.ReadInt32(); c.Count = r.ReadInt32(); c.Latch = r.ReadUInt32();
			c.When = r.ReadInt64(); c.Cycles = r.ReadInt32();
			c.IsFirstUnit = r.ReadBoolean();
			c.SourceOffset = r.ReadInt32(); c.DestOffset = r.ReadInt32();
			c.DestInvalid = r.ReadBoolean();
		}
	}

	private static void WriteTimers( BinaryWriter w, GbaTimerController timers )
	{
		w.Write( timers.NextGlobalEvent );

		for ( int i = 0; i < 4; i++ )
		{
			var c = timers.Channels[i];
			w.Write( c.Reload ); w.Write( c.Counter ); w.Write( c.Control );
			w.Write( c.Enabled ); w.Write( c.CountUp ); w.Write( c.DoIrq );
			w.Write( c.PrescaleBits );
			w.Write( c.LastEvent ); w.Write( c.NextOverflowCycle );
		}
	}

	private static void ReadTimers( BinaryReader r, GbaTimerController timers )
	{
		timers.NextGlobalEvent = r.ReadInt64();

		for ( int i = 0; i < 4; i++ )
		{
			var c = timers.Channels[i];
			c.Reload = r.ReadUInt16(); c.Counter = r.ReadUInt16(); c.Control = r.ReadUInt16();
			c.Enabled = r.ReadBoolean(); c.CountUp = r.ReadBoolean(); c.DoIrq = r.ReadBoolean();
			c.PrescaleBits = r.ReadInt32();
			c.LastEvent = r.ReadInt64(); c.NextOverflowCycle = r.ReadInt64();
		}
	}

	private static void WriteSavedata( BinaryWriter w, GbaSavedata savedata )
	{
		w.Write( (int)savedata.Type );
		w.Write( savedata.Data.Length );
		w.Write( savedata.Data );
		savedata.Serialize( w );
	}

	private static void ReadSavedata( BinaryReader r, GbaSavedata savedata )
	{
		var type = (SavedataType)r.ReadInt32();
		int len = r.ReadInt32();
		var data = r.ReadBytes( len );

		if ( type == savedata.Type && data.Length == savedata.Data.Length )
			Array.Copy( data, savedata.Data, data.Length );
		else if ( type == savedata.Type )
			r.BaseStream.Position -= len;

		savedata.Deserialize( r );
	}

	private static void WriteHardware( BinaryWriter w, GbaCartridgeHardware hardware )
	{
		w.Write( hardware.HasRtc );
		hardware.Serialize( w );
	}

	private static void ReadHardware( BinaryReader r, GbaCartridgeHardware hardware )
	{
		hardware.HasRtc = r.ReadBoolean();
		hardware.Deserialize( r );
	}

	private static void WriteBios( BinaryWriter w, GbaBios bios )
	{
		w.Write( bios.HleActive );
		w.Write( bios.BiosStall );
	}

	private static void ReadBios( BinaryReader r, GbaBios bios )
	{
		bios.HleActive = r.ReadBoolean();
		bios.BiosStall = r.ReadInt32();
	}

	private static void WriteSystem( BinaryWriter w, Gba gba )
	{
		w.Write( gba.CyclesThisFrame );
		w.Write( gba.FrameCounter );
		w.Write( gba.TotalCycles );
	}

	private static void ReadSystem( BinaryReader r, Gba gba )
	{
		gba.CyclesThisFrame = r.ReadInt32();
		gba.FrameCounter = r.ReadInt64();
		gba.TotalCycles = r.ReadInt64();
		gba.IsRunning = true;
	}
}
