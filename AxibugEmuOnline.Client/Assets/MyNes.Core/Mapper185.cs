using System.IO;

namespace MyNes.Core
{
    [BoardInfo("Unknown", 185)]
    internal class Mapper185 : Board
    {
    	private bool lockchr;

    	internal override void HardReset()
    	{
    		base.HardReset();
    		lockchr = false;
    	}

    	internal override void ReadCHR(ref ushort address, out byte data)
    	{
    		if (!lockchr)
    		{
    			base.ReadCHR(ref address, out data);
    		}
    		else
    		{
    			data = byte.MaxValue;
    		}
    	}

    	internal override void WritePRG(ref ushort address, ref byte data)
    	{
    		lockchr = (data & 0xF) == 0 || data == 19;
    	}

    	internal override void WriteStateData(ref BinaryWriter stream)
    	{
    		base.WriteStateData(ref stream);
    		stream.Write(lockchr);
    	}

    	internal override void ReadStateData(ref BinaryReader stream)
    	{
    		base.ReadStateData(ref stream);
    		lockchr = stream.ReadBoolean();
    	}
    }
}
