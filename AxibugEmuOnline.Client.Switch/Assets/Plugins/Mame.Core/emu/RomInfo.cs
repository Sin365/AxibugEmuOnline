using System.Collections.Generic;

namespace MAME.Core
{
    public class RomInfo
    {
        public static List<RomInfo> romList;
        public static Dictionary<string, RomInfo> dictName2Rom;
        public static RomInfo Rom;
        public string Name, Board;
        public string Parent;
        public string Direction;
        public string Description;
        public string Year;
        public string Manufacturer;
        public string M1Default, M1Stop, M1Min, M1Max, M1Subtype;
        public static ushort IStop;
        public RomInfo()
        {

        }
        public static RomInfo GetRomByName(string s1)
        {
            if (!dictName2Rom.TryGetValue(s1, out RomInfo info))
                return null;
            return info;
        }
        public static string GetParent(string s1)
        {
            string sParent = "";
            foreach (RomInfo ri in romList)
            {
                if (s1 == ri.Name)
                {
                    sParent = ri.Parent;
                    break;
                }
            }
            return sParent;
        }
        public static List<string> GetParents(string s1)
        {
            string sChild, sParent;
            List<string> ls1 = new List<string>();
            sChild = s1;
            while (sChild != "")
            {
                ls1.Add(sChild);
                sParent = GetParent(sChild);
                sChild = sParent;
            }
            return ls1;
        }

    }
}