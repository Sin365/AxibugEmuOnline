using MAME.Core;

public class UniIO : IMAMEIOSupport
{
    public bool File_Exists(string path)
    {
        return AxiIO.File.Exists(path);
    }

    public byte[] File_ReadAllBytes(string path)
    {
        return AxiIO.File.ReadAllBytes(path);
    }

    public void File_WriteAllBytesFromStre(string path, System.IO.MemoryStream ms)
    {
        AxiIO.File.WriteAllBytesFromStream(path, ms);
    }
}