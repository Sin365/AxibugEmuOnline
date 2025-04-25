using System.IO;

namespace MAME.Core
{
    public interface IMAMEIOSupport
    {
        bool File_Exists(string path);
        byte[] File_ReadAllBytes(string path);
        void File_WriteAllBytesFromStre(string path, MemoryStream ms);
    }
}
