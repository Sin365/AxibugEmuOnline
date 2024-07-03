using System.IO;

namespace MyNes.Core;

[BoardInfo("Unknown", 226)]
internal class Mapper226 : Board
{
	private int prg_reg;

	private bool prg_mode;

	internal override void HardReset()
	{
		base.HardReset();
		prg_reg = 0;
		prg_mode = false;
	}

	internal override void WritePRG(ref ushort address, ref byte data)
	{
		switch (address & 0x8001)
		{
		case 32768:
			prg_reg = (data & 0x1F) | ((data & 0x80) >> 2) | (prg_reg & 0xC0);
			prg_mode = (data & 0x20) == 32;
			Switch01KNMTFromMirroring(((data & 0x40) == 64) ? Mirroring.Vert : Mirroring.Horz);
			if (prg_mode)
			{
				Switch16KPRG(prg_reg, PRGArea.Area8000);
				Switch16KPRG(prg_reg, PRGArea.AreaC000);
			}
			else
			{
				Switch32KPRG(prg_reg >> 1, PRGArea.Area8000);
			}
			break;
		case 32769:
			prg_reg = ((data & 1) << 6) | (prg_reg & 0x3F);
			if (prg_mode)
			{
				Switch16KPRG(prg_reg, PRGArea.Area8000);
				Switch16KPRG(prg_reg, PRGArea.AreaC000);
			}
			else
			{
				Switch32KPRG(prg_reg >> 1, PRGArea.Area8000);
			}
			break;
		}
	}

	internal override void WriteStateData(ref BinaryWriter stream)
	{
		base.WriteStateData(ref stream);
		stream.Write(prg_reg);
		stream.Write(prg_mode);
	}

	internal override void ReadStateData(ref BinaryReader stream)
	{
		base.ReadStateData(ref stream);
		prg_reg = stream.ReadInt32();
		prg_mode = stream.ReadBoolean();
	}
}
