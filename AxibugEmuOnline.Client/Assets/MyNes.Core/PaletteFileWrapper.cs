using System.Collections.Generic;
using System.IO;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class PaletteFileWrapper
    {
    	public static bool LoadFile(Stream fileStream, out int[] palette)
    	{
            Stream stream = fileStream;
    		if (stream.Length == 192 || stream.Length == 1536)
    		{
    			int[] array = new int[512];
    			byte[] array2 = new byte[stream.Length];
    			stream.Read(array2, 0, array2.Length);
    			int num = 0;
    			for (int i = 0; i < 512; i++)
    			{
    				byte b = array2[num];
    				num++;
    				if (num == array2.Length)
    				{
    					num = 0;
    				}
    				byte b2 = array2[num];
    				num++;
    				if (num == array2.Length)
    				{
    					num = 0;
    				}
    				byte b3 = array2[num];
    				num++;
    				if (num == array2.Length)
    				{
    					num = 0;
    				}
    				array[i] = -16777216 | (b << 16) | (b2 << 8) | b3;
    			}
    			stream.Close();
    			palette = array;
    			return true;
    		}
    		palette = null;
    		return false;
    	}

    	public static void SaveFile(string file, int[] palette)
    	{
    		Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write);
    		List<byte> list = new List<byte>();
    		foreach (int num in palette)
    		{
    			list.Add((byte)((uint)(num >> 16) & 0xFFu));
    			list.Add((byte)((uint)(num >> 8) & 0xFFu));
    			list.Add((byte)((uint)num & 0xFFu));
    		}
    		stream.Write(list.ToArray(), 0, list.Count);
    		stream.Close();
    	}
    }
}
