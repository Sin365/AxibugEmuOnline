using System.Text;

namespace HtmlTable
{
    public class FileHelper
    {
        public static bool LoadFile(string FilePath, out byte[] data)
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    byte[] buffur = new byte[fs.Length];
                    fs.Read(buffur, 0, (int)fs.Length);
                    fs.Close();
                    data = buffur;
                    return true;
                }
                catch (Exception ex)
                {
                    data = null;
                    return false;
                }
            }
        }

        public static string[] GetDirFile(string Path)
        {
            return Directory.GetFiles(Path);
        }

        public static byte[] String2Byte(string value)
        {
            return Encoding.Default.GetBytes(value);
        }
        public static bool SaveFile(string FilePath, byte[] buffer)
        {
            try
            {
                //创建一个文件流
                using (FileStream wfs = new FileStream(FilePath, FileMode.Create))
                {
                    //将byte数组写入文件中
                    wfs.Write(buffer, 0, buffer.Length);
                    //所有流类型都要关闭流，否则会出现内存泄露问题
                    wfs.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        public static void SaveFile(string FilePath, string[] strArr)
        {
            System.IO.File.WriteAllLines(FilePath, strArr);
        }
    }
}
