using System;
using System.IO;

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
            using (StreamReader htmlRead = new StreamReader("/www/html/index.html")) //应该使用绝对地址
            {
                processor.outputStream.Write(htmlRead.ReadToEnd());
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