using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

namespace MyNes.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class GameGenie
    {
    	public string[] LettersTable = new string[16]
    	{
    		"A", "P", "Z", "L", "G", "I", "T", "Y", "E", "O",
    		"X", "U", "K", "S", "V", "N"
    	};

    	public byte[] HEXTable = new byte[16]
    	{
    		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
    		10, 11, 12, 13, 14, 15
    	};

    	private List<string> lettersTable = new List<string>();

    	public GameGenie()
    	{
    		lettersTable = new List<string>(LettersTable);
    	}

    	public int GetCodeAsHEX(string code)
    	{
    		int num = 0;
    		int num2 = code.ToCharArray().Length - 1;
    		char[] array = code.ToCharArray();
    		foreach (char c in array)
    		{
    			num |= HEXTable[lettersTable.IndexOf(c.ToString())] << num2 * 4;
    			num2--;
    		}
    		return num;
    	}

    	public byte GetGGValue(int code, int length)
    	{
    		int num = 0;
    		int num2 = 0;
    		int num3 = 0;
    		int num4 = 0;
    		int num5 = 0;
    		int num6 = 0;
    		int num7 = 0;
    		int num8 = 0;
    		switch (length)
    		{
    		case 6:
    			num8 = (code & 0x800000) >> 23;
    			num7 = (code & 0x40000) >> 18;
    			num6 = (code & 0x20000) >> 17;
    			num5 = (code & 0x10000) >> 16;
    			num4 = (code & 8) >> 3;
    			num3 = (code & 0x400000) >> 22;
    			num2 = (code & 0x200000) >> 21;
    			num = (code & 0x100000) >> 20;
    			break;
    		case 8:
    			num8 = (code >> 31) & 1;
    			num7 = (code >> 27) & 1;
    			num6 = (code >> 26) & 1;
    			num5 = (code >> 25) & 1;
    			num4 = (code >> 3) & 1;
    			num3 = (code >> 30) & 1;
    			num2 = (code >> 29) & 1;
    			num = (code >> 28) & 1;
    			break;
    		}
    		return (byte)((num8 << 7) | (num7 << 6) | (num6 << 5) | (num5 << 4) | (num4 << 3) | (num3 << 2) | (num2 << 1) | num);
    	}

    	public int GetGGAddress(int code, int length)
    	{
    		int num = 0;
    		int num2 = 0;
    		int num3 = 0;
    		int num4 = 0;
    		int num5 = 0;
    		int num6 = 0;
    		int num7 = 0;
    		int num8 = 0;
    		int num9 = 0;
    		int num10 = 0;
    		int num11 = 0;
    		int num12 = 0;
    		int num13 = 0;
    		int num14 = 0;
    		int num15 = 0;
    		switch (length)
    		{
    		case 6:
    			num15 = (code >> 10) & 1;
    			num14 = (code >> 9) & 1;
    			num13 = (code >> 8) & 1;
    			num12 = (code >> 7) & 1;
    			num11 = (code >> 2) & 1;
    			num10 = (code >> 1) & 1;
    			num9 = code & 1;
    			num8 = (code >> 19) & 1;
    			num7 = (code >> 14) & 1;
    			num6 = (code >> 13) & 1;
    			num5 = (code >> 12) & 1;
    			num4 = (code >> 11) & 1;
    			num3 = (code >> 6) & 1;
    			num2 = (code >> 5) & 1;
    			num = (code >> 4) & 1;
    			break;
    		case 8:
    			num15 = (code >> 18) & 1;
    			num14 = (code >> 17) & 1;
    			num13 = (code >> 16) & 1;
    			num12 = (code >> 15) & 1;
    			num11 = (code >> 10) & 1;
    			num10 = (code >> 9) & 1;
    			num9 = (code >> 8) & 1;
    			num8 = (code >> 25) & 1;
    			num7 = (code >> 22) & 1;
    			num6 = (code >> 21) & 1;
    			num5 = (code >> 20) & 1;
    			num4 = (code >> 19) & 1;
    			num3 = (code >> 14) & 1;
    			num2 = (code >> 13) & 1;
    			num = (code >> 12) & 1;
    			break;
    		}
    		return (num15 << 14) | (num14 << 13) | (num13 << 12) | (num12 << 11) | (num11 << 10) | (num10 << 9) | (num9 << 8) | (num8 << 7) | (num7 << 6) | (num6 << 5) | (num5 << 4) | (num4 << 3) | (num3 << 2) | (num2 << 1) | num;
    	}

    	public byte GetGGCompareValue(int code)
    	{
    		int num = 0;
    		int num2 = 0;
    		int num3 = 0;
    		int num4 = 0;
    		int num5 = 0;
    		int num6 = 0;
    		int num7 = 0;
    		int num8 = (code >> 7) & 1;
    		num7 = (code >> 2) & 1;
    		num6 = (code >> 1) & 1;
    		num5 = code & 1;
    		num4 = (code >> 11) & 1;
    		num3 = (code >> 6) & 1;
    		num2 = (code >> 5) & 1;
    		num = (code >> 4) & 1;
    		return (byte)((num8 << 7) | (num7 << 6) | (num6 << 5) | (num5 << 4) | (num4 << 3) | (num3 << 2) | (num2 << 1) | num);
    	}
    }
}
