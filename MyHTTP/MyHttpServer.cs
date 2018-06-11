using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MyHTTP
{
    public class MyHttpServer : HttpServer
    {
        public MyHttpServer(int port) : base(port)
        {
        }

        public override void HandleGETRequest(HttpProcessor processor)
        {
            Console.WriteLine("request: {0}", processor.httpUrl);
            processor.WriteSuccess();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using (StreamReader htmlReader = new StreamReader("/var/www/html/index.html"))
                {
                    processor.outputStream.Write(htmlReader.ReadToEnd());
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (StreamReader htmlReader = new StreamReader("E://www//html//index.html"))
                {
                    processor.outputStream.Write(htmlReader.ReadToEnd());
                }
            }
        }

        public override void HandlePOSTRequest(HttpProcessor processor, StreamReader inputData)
        {
            Console.WriteLine("POST request: {0}", processor.httpUrl);
            string data = inputData.ReadToEnd();

            processor.outputStream.WriteLine("<html><body><h1>test server</h1>");
            processor.outputStream.WriteLine("post body: <pre>{0}</pre></body></html>");
        }
    }
}
