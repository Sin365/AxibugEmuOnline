namespace sGBA;

public static class GbaConstants
{
	public const int Arm7TdmiFrequency = 16777216;

	public const int ScreenWidth = 240;
	public const int ScreenHeight = 160;

	public const int VideoHDrawLength = 1008;
	public const int VideoHBlankLength = 224;
	public const int VideoHorizontalLength = 1232;
	public const int VisibleLines = 160;
	public const int VideoHBlankPixels = 68;
	public const int VideoVBlankPixels = 68;
	public const int VideoVerticalTotalPixels = 228;
	public const int VideoTotalLength = 280896;

	public const int BiosSize = 0x4000;
	public const int EwramSize = 0x40000;
	public const int IwramSize = 0x8000;
	public const int IoSize = 0x400;
	public const int PaletteSize = 0x400;
	public const int VramSize = 0x18000;
	public const int OamSize = 0x400;
	public const int RomMaxSize = 0x2000000;
	public const int SramSize = 0x8000;
	public const int Flash512Size = 0x10000;
	public const int Flash1MSize = 0x20000;
	public const int EepromSize = 0x2000;
	public const int Eeprom512Size = 0x200;

	public const uint BiosBase = 0x00000000;
	public const uint EwramBase = 0x02000000;
	public const uint IwramBase = 0x03000000;
	public const uint IoBase = 0x04000000;
	public const uint PaletteBase = 0x05000000;
	public const uint VramBase = 0x06000000;
	public const uint OamBase = 0x07000000;
	public const uint RomBase = 0x08000000;
	public const uint RomWs1Base = 0x0A000000;
	public const uint RomWs2Base = 0x0C000000;
	public const uint SramBase = 0x0E000000;

	public const uint SpBaseSystem = 0x03007F00;
	public const uint SpBaseIrq = 0x03007FA0;
	public const uint SpBaseSupervisor = 0x03007FE0;

	public const uint IrqHandlerAddr = 0x03FFFFFC;
	public const uint IrqHandlerAddrAlt = 0x03007FFC;

	public const uint BaseReset = 0x00000000;
	public const uint BaseUndef = 0x00000004;
	public const uint BaseSwi = 0x00000008;
	public const uint BasePabt = 0x0000000C;
	public const uint BaseDabt = 0x00000010;
	public const uint BaseIrq = 0x00000018;
	public const uint BaseFiq = 0x0000001C;

	public const double Fps = (double)Arm7TdmiFrequency / VideoTotalLength;
}
