using System.IO;
namespace MyNes.Core
{

    public interface IExternalSupporter
    {
        string GetWorkingFolderPath();
        Stream OpenDatabaseFile();
        Stream OpenPaletteFile();
        Stream OpenRomFile(string path);
        bool IsKeyPressing(EnumJoyIndex index,EnumKeyKind key);
    }
}