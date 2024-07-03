using System.IO;
using System.Security.Cryptography;

namespace MyNes.Core
{
    public class INes : IRom
    {
    	public bool HasBattery { get; private set; }

    	public bool IsPlaychoice10 { get; private set; }

    	public bool IsVSUnisystem { get; private set; }

    	public override void Load(string fileName, bool loadDumps)
    	{
    		FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
    		if (fileStream.Length < 16)
    		{
    			fileStream.Close();
    			base.IsValid = false;
    			return;
    		}
    		byte[] array = new byte[16];
    		fileStream.Read(array, 0, 16);
    		byte[] buffer = new byte[fileStream.Length - 16];
    		fileStream.Read(buffer, 0, (int)(fileStream.Length - 16));
    		base.SHA1 = "";
    		SHA1Managed sHA1Managed = new SHA1Managed();
    		byte[] array2 = sHA1Managed.ComputeHash(buffer);
    		byte[] array3 = array2;
    		foreach (byte b in array3)
    		{
    			base.SHA1 += b.ToString("x2").ToLower();
    		}
    		if (array[0] != 78 || array[1] != 69 || array[2] != 83 || array[3] != 26)
    		{
    			fileStream.Close();
    			base.IsValid = false;
    			return;
    		}
    		base.PRGCount = array[4];
    		base.CHRCount = array[5];
    		switch (array[6] & 9)
    		{
    		case 0:
    			base.Mirroring = Mirroring.Horz;
    			break;
    		case 1:
    			base.Mirroring = Mirroring.Vert;
    			break;
    		case 8:
    		case 9:
    			base.Mirroring = Mirroring.Full;
    			break;
    		}
    		HasBattery = (array[6] & 2) != 0;
    		base.HasTrainer = (array[6] & 4) != 0;
    		if ((array[7] & 0xF) == 0)
    		{
    			base.MapperNumber = (byte)((array[7] & 0xF0) | (array[6] >> 4));
    		}
    		else
    		{
    			base.MapperNumber = (byte)(array[6] >> 4);
    		}
    		IsVSUnisystem = (array[7] & 1) != 0;
    		IsPlaychoice10 = (array[7] & 2) != 0;
    		if (loadDumps)
    		{
    			fileStream.Seek(16L, SeekOrigin.Begin);
    			if (base.HasTrainer)
    			{
    				base.Trainer = new byte[512];
    				fileStream.Read(base.Trainer, 0, 512);
    			}
    			else
    			{
    				base.Trainer = new byte[0];
    			}
    			base.PRG = new byte[base.PRGCount * 16384];
    			fileStream.Read(base.PRG, 0, base.PRGCount * 16384);
    			if (base.CHRCount > 0)
    			{
    				base.CHR = new byte[base.CHRCount * 8192];
    				fileStream.Read(base.CHR, 0, base.CHRCount * 8192);
    			}
    			else
    			{
    				base.CHR = new byte[0];
    			}
    		}
    		base.IsValid = true;
    		fileStream.Dispose();
    		fileStream.Close();
    	}
    }
}
