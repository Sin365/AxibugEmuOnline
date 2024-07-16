namespace AxibugEmuOnline.Server
{
    internal class Program
    {
        static string Title = "AxibugEmuOnline.Server";
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = Title;
            AppSrv.InitServer(10492);
            while (true) 
            {
                Console.ReadLine();
            }
        }
    }
}