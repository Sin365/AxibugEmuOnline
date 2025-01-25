using System;
using System.Collections.Generic;
using System.IO;

namespace CheckFilesExist
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] checklist = File.ReadAllLines("checkfiles.txt");

            List<string> dontExist = new List<string>();
            Console.WriteLine("不存在列表：");
            foreach (var item in checklist)
            {
                var path = System.Net.WebUtility.UrlDecode(item);
                if (!File.Exists(path))
                {
                    Console.WriteLine(path);
                    dontExist.Add(path);
                }
            }
            Console.WriteLine($"完毕,共{dontExist.Count}个不存在");
            Console.ReadLine();
        }
    }
}
