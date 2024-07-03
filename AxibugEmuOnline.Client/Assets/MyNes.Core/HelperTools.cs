using System.IO;
using System.Security.Cryptography;

namespace MyNes.Core
{
    public class HelperTools
    {
    	public static string GetFileSize(string FilePath)
    	{
    		if (File.Exists(Path.GetFullPath(FilePath)))
    		{
    			FileInfo fileInfo = new FileInfo(FilePath);
    			string text = " Byte";
    			double num = fileInfo.Length;
    			if (fileInfo.Length >= 1024)
    			{
    				num = (double)fileInfo.Length / 1024.0;
    				text = " KB";
    			}
    			if (num >= 1024.0)
    			{
    				num /= 1024.0;
    				text = " MB";
    			}
    			if (num >= 1024.0)
    			{
    				num /= 1024.0;
    				text = " GB";
    			}
    			return num.ToString("F2") + text;
    		}
    		return "";
    	}

    	public static string GetSize(long size)
    	{
    		string text = " Byte";
    		double num = size;
    		if (size >= 1024)
    		{
    			num = (double)size / 1024.0;
    			text = " KB";
    		}
    		if (num >= 1024.0)
    		{
    			num /= 1024.0;
    			text = " MB";
    		}
    		if (num >= 1024.0)
    		{
    			num /= 1024.0;
    			text = " GB";
    		}
    		if (num < 0.0)
    		{
    			return "???";
    		}
    		return num.ToString("F2") + text;
    	}

    	public static string GetSize(ulong size)
    	{
    		string text = " Byte";
    		double num = size;
    		if (size >= 1024)
    		{
    			num = (double)size / 1024.0;
    			text = " KB";
    		}
    		if (num >= 1024.0)
    		{
    			num /= 1024.0;
    			text = " MB";
    		}
    		if (num >= 1024.0)
    		{
    			num /= 1024.0;
    			text = " GB";
    		}
    		if (num < 0.0)
    		{
    			return "???";
    		}
    		return num.ToString("F2") + text;
    	}

    	public static long GetSizeAsBytes(string FilePath)
    	{
    		if (File.Exists(FilePath))
    		{
    			return new FileInfo(FilePath).Length;
    		}
    		return 0L;
    	}

    	public static bool IsStringContainsNumbers(string text)
    	{
    		char[] array = text.ToCharArray();
    		foreach (char c in array)
    		{
    			int result = 0;
    			if (int.TryParse(c.ToString(), out result))
    			{
    				return true;
    			}
    		}
    		return false;
    	}

    	public static string CalculateCRC(string filePath)
    	{
    		if (File.Exists(filePath))
    		{
    			Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    			byte[] buffer = new byte[stream.Length];
    			stream.Read(buffer, 0, (int)stream.Length);
    			stream.Close();
    			string text = "";
    			byte[] array = new Crc32().ComputeHash(buffer);
    			foreach (byte b in array)
    			{
    				text += b.ToString("x2").ToLower();
    			}
    			return text;
    		}
    		return "";
    	}

    	public static string CalculateCRC(string filePath, int bytesToSkip)
    	{
    		if (File.Exists(filePath))
    		{
    			Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    			stream.Read(new byte[bytesToSkip], 0, bytesToSkip);
    			byte[] buffer = new byte[stream.Length - bytesToSkip];
    			stream.Read(buffer, 0, (int)(stream.Length - bytesToSkip));
    			stream.Close();
    			string text = "";
    			byte[] array = new Crc32().ComputeHash(buffer);
    			foreach (byte b in array)
    			{
    				text += b.ToString("x2").ToLower();
    			}
    			return text;
    		}
    		return "";
    	}

    	public static string CalculateSHA1(string filePath)
    	{
    		if (File.Exists(filePath))
    		{
    			byte[] buffer = GetBuffer(filePath);
    			string text = "";
    			byte[] array = new SHA1Managed().ComputeHash(buffer);
    			foreach (byte b in array)
    			{
    				text += b.ToString("x2").ToLower();
    			}
    			return text;
    		}
    		return "";
    	}

    	public static byte[] GetBuffer(string filePath)
    	{
    		Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    		byte[] array = new byte[stream.Length];
    		stream.Read(array, 0, (int)stream.Length);
    		stream.Close();
    		return array;
    	}
    }
}
