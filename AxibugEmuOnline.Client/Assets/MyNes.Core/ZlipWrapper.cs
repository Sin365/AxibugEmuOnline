using System.IO;
using ComponentAce.Compression.Libs.zlib;

namespace MyNes.Core
{
    internal class ZlipWrapper
    {
    	internal static void CompressData(byte[] inData, out byte[] outData)
    	{
    		using MemoryStream memoryStream = new MemoryStream();
    		using ZOutputStream zOutputStream = new ZOutputStream(memoryStream, -1);
    		using Stream input = new MemoryStream(inData);
    		CopyStream(input, zOutputStream);
    		zOutputStream.finish();
    		outData = memoryStream.ToArray();
    	}

    	internal static void DecompressData(byte[] inData, out byte[] outData)
    	{
    		using MemoryStream memoryStream = new MemoryStream();
    		using ZOutputStream zOutputStream = new ZOutputStream(memoryStream);
    		using Stream input = new MemoryStream(inData);
    		CopyStream(input, zOutputStream);
    		zOutputStream.finish();
    		outData = memoryStream.ToArray();
    	}

    	internal static void CopyStream(Stream input, Stream output)
    	{
    		byte[] buffer = new byte[2000];
    		int count;
    		while ((count = input.Read(buffer, 0, 2000)) > 0)
    		{
    			output.Write(buffer, 0, count);
    		}
    		output.Flush();
    	}
    }
}
