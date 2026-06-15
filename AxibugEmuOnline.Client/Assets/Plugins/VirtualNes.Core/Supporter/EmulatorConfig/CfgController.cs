

namespace VirtualNes.Core
{
    public class CfgController
    {
        public ushort[][] nButton = new ushort[4][]
        {
            new ushort[64],new ushort[64], new ushort[64], new ushort[64],
        };
        public ushort[][] nRapid = new ushort[4][]
        {
            new ushort[2],new ushort[2],new ushort[2],new ushort[2],
        };

        // 0:Crazy Climber
        // 1:Famly Trainer
        // 2:Exciting Boxing
        // 3:Mahjang
        public ushort[][] nExButton = new ushort[4][]
        {
            new ushort[64],new ushort[64], new ushort[64], new ushort[64],
        };

        public ushort[] nVSUnisystem = new ushort[64];
    }
}