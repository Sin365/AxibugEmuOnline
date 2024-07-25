using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes
{
    public static class MMU
    {
        // CPU 儊儌儕僶儞僋
        public static byte[][] CPU_MEM_BANK = new byte[8][];            // 8K扨埵

        // NES儊儌儕
        public static byte[] RAM = new byte[8 * 1024];		            // NES撪憻RAM
        public static byte[] WARM = new byte[128 * 1024];               // 儚乕僋/僶僢僋傾僢僾RAM
    }
}
