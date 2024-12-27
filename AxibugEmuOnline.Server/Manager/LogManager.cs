using AxibugProtobuf;
using static Mysqlx.Expect.Open.Types;

namespace AxibugEmuOnline.Server.Manager
{
    public class LogManager
    {
        public void Info(string str)
        {
            Console.WriteLine(str);
        }
        public void DebugCmd(string str)
        {
            ConsoleColor srcColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss.fff")}][{str}]");
            Console.ForegroundColor = srcColor;
        }
        public void Debug(string str)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss.fff")}][{str}]");
            //Console.WriteLine(str);
        }

        public void Assert(bool conditional, string message)
        {
            if (!conditional)
            {
                Debug(message);
            }
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