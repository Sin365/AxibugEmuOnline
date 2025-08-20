namespace VirtualNes.Core
{
    public class CfgPath
    {
        public bool bRomPath = true;
        public bool bSavePath = true;
        public bool bStatePath = true;
        public bool bSnapshotPath = true;
        public bool bMoviePath = true;
        public bool bWavePath = true;
        public bool bCheatPath = true;
        public bool bIpsPath = true;

        public string szRomPath = "roms";
        public string szSavePath = "sav";
        public string szStatePath = "state";
        public string szSnapshotPath = "snapshot";
        public string szMoviePath = "movie";
        public string szWavePath = "wave";
        public string szCheatPath = "cheatcode";
        public string szIpsPath = "ips";
    }
}