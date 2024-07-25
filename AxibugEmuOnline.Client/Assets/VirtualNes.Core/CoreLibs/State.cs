using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualNes.Core
{
    public class DISKFILEHDR
    {
        public byte[] ID = new byte[12];        // "VirtuaNES DI"
        public ushort BlockVersion;  // 0x0200:0.30	0x0210:0.31
        public ushort Reserved;
        public ulong ProgID;       // 僾儘僌儔儉ID
        public ushort MakerID;   // 儊乕僇乕ID
        public ushort DiskNo;        // 僨傿僗僋悢
        public ulong DifferentSize; // 憡堘悢


        public byte[] ToBytes()
        {
            byte[] res = new byte[36];
            Array.Copy(ID, res, ID.Length);
            var temp = BitConverter.GetBytes(BlockVersion);
            res[12] = temp[0];
            res[13] = temp[1];
            temp = BitConverter.GetBytes(Reserved);
            res[14] = temp[0];
            res[15] = temp[1];
            temp = BitConverter.GetBytes(ProgID);
            res[16] = temp[0];
            res[17] = temp[1];
            res[18] = temp[2];
            res[19] = temp[3];
            res[20] = temp[4];
            res[21] = temp[5];
            res[22] = temp[6];
            res[23] = temp[7];
            temp = BitConverter.GetBytes(MakerID);
            res[24] = temp[0];
            res[25] = temp[1];
            temp = BitConverter.GetBytes(DiskNo);
            res[26] = temp[0];
            res[27] = temp[1];
            temp = BitConverter.GetBytes(ProgID);
            res[28] = temp[0];
            res[29] = temp[1];
            res[30] = temp[2];
            res[31] = temp[3];
            res[32] = temp[4];
            res[33] = temp[5];
            res[34] = temp[6];
            res[35] = temp[7];

            return res;
        }
    }
}
