using System;
using System.Threading;

namespace MyHTTP
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            HttpServer httpServer;

            if (args.GetLength(0) > 0)
            {
                httpServer = new MyHttpServer(Convert.ToInt16(args[0]));
            }
            else
            {
                httpServer = new MyHttpServer(8080);
            }

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
            return 0;
        }
    }
}