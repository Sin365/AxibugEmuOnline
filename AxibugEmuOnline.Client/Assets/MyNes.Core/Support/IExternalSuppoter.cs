using System.IO;
namespace MyNes.Core
{

    public interface IExternalSupporter
    {
        string GetWorkingFolderPath();
        public Stream OpenDatabaseFile();
        public Stream OpenPaletteFile();
        public Stream OpenRomFile(string path);
        public bool IsKeyPressing(EnumJoyIndex index,EnumKeyKind key);
    }
}