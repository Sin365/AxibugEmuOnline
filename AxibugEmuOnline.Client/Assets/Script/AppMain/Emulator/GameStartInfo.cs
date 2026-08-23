namespace AxibugEmuOnline.Client
{
    public class GameStartInfo
    {
        public RomFile romFile;
        public GameStartInfo(RomFile romFile)
        {
            this.romFile = romFile;
        }
    }

    public class GBAGameStartInfo : GameStartInfo
    {
        public byte MineIdx;
        public byte PlayerCount;

        public GBAGameStartInfo (RomFile romFile) : base (romFile)
        {
            MineIdx = 0;
            PlayerCount = 1;
        }

        public GBAGameStartInfo(RomFile romFile,byte mineIdx, byte playerCount) : base(romFile)
        {
            MineIdx = mineIdx;
            PlayerCount = playerCount;
        }
    }
}