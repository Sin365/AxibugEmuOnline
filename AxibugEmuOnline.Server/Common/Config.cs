using System.Data;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace AxibugEmuOnline.Server.Common
{

    public class ConfigDataModel
    {
        public string DBIp{get;set;}
        public ushort DBPort{get;set; }
        public string DBName { get; set; }
        public string DBUname{get;set;}
        public string DBPwd{get;set;}
        public string RomDir{get;set;}
        public string ImageDir { get; set; }
        public string ServerIp { get; set; }
        public ushort ServerPort { get; set; }
        public string ClientVersion { get; set; }
        public string AesKey { get; set; }
        public string AesIv { get; set; }
        public string savDataPath { get; set; }
    }


    public static class Config
    {
        public static ConfigDataModel cfg;
        public static bool LoadConfig()
        {
            try
            {
                string path = System.Environment.CurrentDirectory + "//config.cfg";
                if (!File.Exists(path))
                {
                    ConfigDataModel sampleCfg = new ConfigDataModel
                    {
                        DBIp = "127.0.0.1",
                        DBPort = 3306,
                        DBUname = "user",
                        DBPwd = "password",
                        DBName = "DBName",
                        RomDir = "./Rom",
                        ImageDir = "./Img",
                        ServerIp = "127.0.0.1",
                        ServerPort = 10001,
                        ClientVersion = "0.0.0.1"
                    };

                    string jsonString = JsonSerializer.Serialize(sampleCfg, new JsonSerializerOptions()
                    {
                        // 整齐打印
                        WriteIndented = true,
                        //重新编码，解决中文乱码问题
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });
                    System.IO.File.WriteAllText(path, jsonString, Encoding.UTF8);

                    Console.WriteLine("未找到配置，已生成模板，请浏览" + path);
                    return false;
                }
                StreamReader sr = new StreamReader(path, Encoding.Default);
                String jsonstr = sr.ReadToEnd();
                cfg = JsonSerializer.Deserialize<ConfigDataModel>(jsonstr);
                sr.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("配置文件异常：" + ex.ToString());
                return false;
            }
        }
    }
}
