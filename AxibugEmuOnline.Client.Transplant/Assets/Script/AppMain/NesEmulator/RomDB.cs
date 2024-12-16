using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client.ClientCore
{
    public class RomDB : ScriptableObject
    {
        [SerializeField]
        private List<RomInfo> romInfos = new List<RomInfo>();

        private Dictionary<uint, RomInfo> crc_Info_mapper;

        public void AddInfo(RomInfo romInfo)
        {
            romInfos.Add(romInfo);
        }

        public void Clear()
        {
            romInfos.Clear();
        }

        public bool GetMapperNo(uint crc, out int mapperNo)
        {
            if (crc_Info_mapper == null)
            {
                crc_Info_mapper = new Dictionary<uint, RomInfo>();
                foreach (var info in romInfos)
                {
                    crc_Info_mapper[info.CRC] = info;
                }
            }
            RomInfo romInfo;
            if (crc_Info_mapper.TryGetValue(crc, out romInfo))
            {
                mapperNo = romInfo.Mapper;
                return true;
            }
            else
            {
                mapperNo = -1;
                return false;
            }
        }

        [Serializable]
        public class RomInfo
        {
            public uint CRC;
            public int Mapper;
        }
    }

}
