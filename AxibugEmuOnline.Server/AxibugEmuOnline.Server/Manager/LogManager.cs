namespace AxibugEmuOnline.Server.Manager
{
    public class LogManager
    {
        public void Info(string str)
        {
            Console.WriteLine(str);
        }
        public void Debug(string str)
        {
            Console.WriteLine(str);
        }

        public void Warning(string str)
        {
            Console.WriteLine(str);
        }

        public void Error(string str)
        {
            Console.WriteLine(str);
        }

        public void Log(int logtype, string str)
        {
            Console.WriteLine(str);
        }
    }
}