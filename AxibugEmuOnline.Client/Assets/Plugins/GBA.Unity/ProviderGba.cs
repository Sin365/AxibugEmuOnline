using System.Text;


namespace OptimeGBA
{
    public interface AxiGbaIO
    {
        public bool SavFileExists();
        public long GetSavFileLength();
    }

    public sealed class ProviderGba : Provider
    {
        public bool BootBios = false;

        public byte[] Bios;
        public byte[] Rom;
        public string RomName;
        public string RomId;
        public AxiGbaIO axiio;

        public ProviderGba(byte[] bios, byte[] rom, string savPath, AudioCallback audioCallback,AxiGbaIO io)
        {
            axiio = io;
            Bios = bios;
            Rom = rom;
            AudioCallback = audioCallback;
            SavPath = savPath;
            if (rom.Length > 0xA0 + 12)
            {
                RomName = Encoding.ASCII.GetString(Rom, 0xA0, 12);
            }

            if (rom.Length >= 0xAC + 4)
            {
                RomId = Encoding.ASCII.GetString(Rom, 0xAC, 4);
            }
        }
    }
}