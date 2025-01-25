using HtmlAgilityPack;
using System.Text;
using System.Xml;

namespace HtmlTable
{
    internal class Program
    {
        static string loc = Path.GetDirectoryName(AppContext.BaseDirectory) + "\\";
        const string InDir = "Input";
        const string OutDir = "Out";
        static void Main(string[] args)
        {
            if (!Directory.Exists(loc + InDir))
            {
                Console.WriteLine("Input文件不存在");
                Console.ReadLine();
                return;
            }

            if (!Directory.Exists(loc + OutDir))
            {
                Console.WriteLine("Out文件不存在");
                Console.ReadLine();
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string[] files = FileHelper.GetDirFile(loc + InDir);
            Console.WriteLine($"共{files.Length}个文件，是否处理? (y/n)");

            string yn = Console.ReadLine();
            if (yn.ToLower() != "y")
                return;

            int index = 0;
            int errcount = 0;
            List<string> outline = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                string FileName = files[i].Substring(files[i].LastIndexOf("\\"));

                if (!FileName.ToLower().Contains(".htm"))
                {
                    continue;
                }
                index++;

                Console.WriteLine($">>>>>>>>>>>>>>开始处理 第{index}个文件  {FileName}<<<<<<<<<<<<<<<<<<<");
                outline.AddRange(GetToData(File.ReadAllText(files[i],System.Text.Encoding.GetEncoding("gb2312"))));
                Console.WriteLine($">>>>>>>>>>>>>>成功处理 第{index}个");
            }

            string newfileName = "out.csv";
            string outstring = loc + OutDir + "\\" + newfileName;
            FileHelper.SaveFile(outstring, outline.ToArray());


            Console.WriteLine($"已处理{files.Length}个文件，其中{errcount}个失败");

            Console.ReadLine();
        }

        static List<string> GetToData(string html)
        {
            List<string> result = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 假设table的XPath已经给出，但这里我们直接使用根table（因为示例中只有一个）  
            HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@width='100%' and @border='1']");

            if (table != null)
            {
                var all = table.SelectNodes("tr");

                int Idx = 0;
                // 遍历除了标题行之外的所有行  
                foreach (HtmlNode row in all) // 跳过标题行  
                {
                    Idx++;
                    if (Idx == 1)
                        continue;
                    // 提取游戏名称和游戏链接  
                    HtmlNode gameNameNode = row.SelectSingleNode("td");



                    if (gameNameNode != null)
                    {

                        try
                        {
                            HtmlNode gameNode1 = row.SelectSingleNode("td[1]/div");
                            if(gameNode1 == null) gameNode1 = row.SelectSingleNode("td[1]");
                            string gameName = gameNode1.InnerText.Trim();
                            string gameUrl = gameNode1.SelectSingleNode("a").GetAttributeValue("href", null);

                            HtmlNode gameNode2 = row.SelectSingleNode("td[2]/div");
                            if (gameNode2 == null) gameNode2 = row.SelectSingleNode("td[2]");
                            string imgUrl = gameNode2.SelectSingleNode("a").GetAttributeValue("href", null);

                            HtmlNode gameNode3 = row.SelectSingleNode("td[3]/div");
                            if (gameNode3 == null) gameNode3 = row.SelectSingleNode("td[3]");
                            string gameType = gameNode3.InnerText.Trim();

                            HtmlNode gameNode4 = row.SelectSingleNode("td[4]/div");
                            if (gameNode4 == null) gameNode4 = row.SelectSingleNode("td[4]");
                            string description = gameNode4.InnerText.Trim();

                            //// 假设图片URL、游戏类型和说明分别在第二个、第三个和第四个<td>中  
                            //HtmlNode imgNode = row.SelectSingleNode("td:nth-child(2) img");
                            //string imgUrl = imgNode?.GetAttributeValue("src", null);

                            //HtmlNode gameTypeNode = row.SelectSingleNode("td:nth-child(3)");
                            //string gameType = gameTypeNode?.InnerText.Trim();

                            //HtmlNode descriptionNode = row.SelectSingleNode("td:nth-child(4)");
                            //string description = descriptionNode?.InnerText.Trim();

                            string outline = $"\"{gameName}\",\"{gameUrl}\",\"{imgUrl}\",\"{gameType}\",\"{description}\"";
                            // 输出信息  
                            Console.WriteLine(outline);

                            result.Add(outline);
                        }
                        catch
                        {
                            
                        }

                    }
                }
            }
            else
            {
                Console.WriteLine("未找到指定的table元素");
            }
            return result;
        }
    }
}
