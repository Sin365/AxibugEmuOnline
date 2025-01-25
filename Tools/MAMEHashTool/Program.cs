using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace MAMEHashTool
{

    class roomHashInfo
    {
        public int RomID { get; set; }
        public string RomDirName { get; set; }
        public string RomAllFileHash { get; set; }
        public string ParentName { get; set; }
        public int ParentID { get; set; }
        public string desc { get; set; }
        public string board { get; set; }
        public string year { get; set; }
        public string manufacturer { get; set; }
    }

    internal class Program
    {
        static Dictionary<int, roomHashInfo> dict = new Dictionary<int, roomHashInfo>();
        static Dictionary<string, int> dictName2RomID = new Dictionary<string, int>();
        static string romRootDir = "G:\\MAME.Core\\roms";
        static HashSet<string> NoInDB = new HashSet<string>();
        static HashSet<string> NotfindParent = new HashSet<string>();
        static void Main(string[] args)
        {
            int RoomSeed = 10000;
            MAMEDBHelper.LoadROMXML(File.ReadAllText("./mame.xml"));
            // 获取当前程序所在目录
            string currentDirectory = romRootDir;

            // 列出当前目录下的所有文件夹
            string[] directories = Directory.GetDirectories(currentDirectory);

            // 文件夹名称
            foreach (string directory in directories)
            {
                // 使用Path.GetFileName来获取文件夹名称（不包含路径）
                string directoryName = Path.GetFileName(directory);
                Console.WriteLine($"正在处理，{RoomSeed}|{directoryName}");


                string[] files = Directory.GetFiles(directory);
                List<byte[]> filedatas = new List<byte[]>();
                foreach (string file in files)
                {
                    filedatas.Add(File.ReadAllBytes(file));
                }

                if (!RomInfo.dictName2Rom.ContainsKey(directoryName))
                {
                    if (!NoInDB.Contains(directoryName))
                        NoInDB.Add(directoryName);

                    Console.WriteLine($"数据库中不存在|{directoryName}");
                    continue;
                }
                RomInfo xmlinfo = RomInfo.dictName2Rom[directoryName];
                roomHashInfo hashrom = new roomHashInfo()
                {
                    RomID = RoomSeed,
                    ParentName = xmlinfo.Parent,
                    RomDirName = directoryName,
                    RomAllFileHash = FileMD5Hash(filedatas),
                    board = xmlinfo.Board,
                    desc = xmlinfo.Description,
                    manufacturer = xmlinfo.Manufacturer,
                    year = xmlinfo.Year,
                };
                dict[RoomSeed] = hashrom;
                dictName2RomID[hashrom.RomDirName] = RoomSeed;
                //StartZip(romRootDir, directory);
                RoomSeed++;
            }

            foreach (var rom in dict.Values)
            {
                if (string.IsNullOrEmpty(rom.ParentName))
                    continue;
                if (!dictName2RomID.ContainsKey(rom.ParentName))
                {
                    Console.WriteLine($"父级{rom.ParentName}无法找到");
                    if (!NotfindParent.Contains(rom.ParentName))
                        NotfindParent.Add(rom.ParentName);
                    continue;
                }
                rom.ParentID = dictName2RomID[rom.ParentName];
            }

            List<string> tempOutInfoList = new List<string>();
            foreach (var rom in dict.Values)
            {
                string ParentID = rom.ParentID == 0 ? "" : rom.ParentID.ToString();
                tempOutInfoList.Add($"{rom.RomID}|{rom.RomDirName}|{rom.board}|{rom.year}|{rom.manufacturer}|{rom.desc}|{rom.RomAllFileHash}|{ParentID}");
            }

            Console.WriteLine($"共{NoInDB.Count}个数据库中不存在");
            tempOutInfoList.Add($"共{NoInDB.Count}个数据库中不存在");
            foreach (var name in NoInDB)
            {
                tempOutInfoList.Add(name);
            }
            Console.WriteLine($"共{NotfindParent.Count}个父级没找到");
            tempOutInfoList.Add($"共{NotfindParent.Count}个父级没找到");
            foreach (var name in NotfindParent)
            {
                tempOutInfoList.Add(name);
            }

            try
            {
                File.WriteAllLines(romRootDir + "//axiromInfolist.txt", tempOutInfoList);
                Console.WriteLine($"写入文件完毕");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入文件失败：{ex}");
            }

            Console.ReadLine();
        }

        static void StartZip(string rootPath, string subDirectory)
        {
            Console.WriteLine($"压缩{subDirectory}");
            string subDirectoryName = Path.GetFileName(subDirectory); // 获取子文件夹名称
            string zipFileName = Path.Combine(rootPath, subDirectoryName + ".zip"); // 构建ZIP文件名

            // 创建ZIP文件
            using (FileStream zipStream = new FileStream(zipFileName, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                // 遍历子文件夹中的所有文件，并将它们添加到ZIP文件中
                foreach (string filePath in Directory.GetFiles(subDirectory, "*.*", SearchOption.AllDirectories))
                {
                    // 获取相对于子文件夹的路径，以便在ZIP文件中保持正确的目录结构
                    string relativePath = filePath.Substring(subDirectory.Length + 1); // +1 是为了排除子文件夹路径末尾的反斜杠

                    // 创建ZIP条目，使用相对于子文件夹的路径作为条目名称
                    ZipArchiveEntry entry = archive.CreateEntry(relativePath, CompressionLevel.SmallestSize);

                    // 打开ZIP条目以进行写入
                    using (Stream entryStream = entry.Open())
                    {
                        // 读取文件内容并写入ZIP条目
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
            }

            Console.WriteLine($"Compressed {subDirectoryName} to {zipFileName}");
        }

        static byte[] FileMD5HashByte(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(data))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        /// <summary>
        /// 单个文件hash
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FileMD5Hash(byte[] data)
        {
            byte[] hash = FileMD5HashByte(data);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        /// <summary>
        /// 多文件总hash (顺序不影响结果)
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static string FileMD5Hash(List<byte[]> dataList)
        {
            string allhash = string.Empty;
            List<string> temp = new List<string>();
            for (int i = 0; i < dataList.Count; i++)
            {
                if (dataList[i] == null)
                    continue;
                temp.Add(FileMD5Hash(dataList[i]));
            }
            temp.Sort();
            var sb = new StringBuilder();
            for (int i = 0; i < temp.Count; i++)
            {
                sb.AppendLine(temp[i].ToString());
            }

            //这里用Ascll
            return FileMD5Hash(Encoding.ASCII.GetBytes(sb.ToString()));
        }
    }
    public class MAMEDBHelper
    {
        public static void LoadROMXML(string xmbString)
        {
            XElement xe = XElement.Parse(xmbString);
            IEnumerable<XElement> elements = from ele in xe.Elements("game") select ele;
            showInfoByElements(elements);
        }

        static void showInfoByElements(IEnumerable<XElement> elements)
        {
            RomInfo.romList = new List<RomInfo>();
            RomInfo.dictName2Rom = new Dictionary<string, RomInfo>();
            foreach (var ele in elements)
            {
                RomInfo rom = new RomInfo();
                rom.Name = ele.Attribute("name").Value;
                rom.Board = ele.Attribute("board").Value;
                rom.Parent = ele.Element("parent").Value;
                rom.Direction = ele.Element("direction").Value;
                rom.Description = ele.Element("description").Value;
                rom.Year = ele.Element("year").Value;
                rom.Manufacturer = ele.Element("manufacturer").Value;
                RomInfo.romList.Add(rom);
                RomInfo.dictName2Rom[rom.Name] = rom;
                //loadform.listView1.Items.Add(new ListViewItem(new string[] { rom.Description, rom.Year, rom.Name, rom.Parent, rom.Direction, rom.Manufacturer, rom.Board }));
            }
        }


    }


    public class RomInfo
    {
        public static List<RomInfo> romList = new List<RomInfo>();
        public static Dictionary<string, RomInfo> dictName2Rom = new Dictionary<string, RomInfo>();
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
