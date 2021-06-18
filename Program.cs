using System;

namespace ConsoleFileManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application();
            app.Init(args);
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL ERROR:\n{0}", ex.Message);
            }
            app.Stop();
        }
    }
}
