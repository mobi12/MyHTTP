using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MyHTTP
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer server;

        private Stream inputStream;
        public StreamWriter outputStream;

        public string httpMethod;
        public string httpUrl;
        public string httpProtocolVersionString;
        public Hashtable httpHeaders = new Hashtable();

        private const int MAX_POST_SIZE = 10 * 1024 * 1024; //10MB?
        private const int BUF_SIZE = 4096;

        public HttpProcessor(TcpClient tcpClient, HttpServer httpServer)
        {
            this.server = httpServer;
            this.socket = tcpClient;
        }

        private string StreamReadLine(Stream inputStream)
        {
            int nextChar;
            string data = "";

            while (true)
            {
                nextChar = inputStream.ReadByte();

                if (nextChar == '\n')
                {
                    break;
                }
                if (nextChar == '\r')
                {
                    continue;
                }
                if (nextChar == -1)
                {
                    Thread.Sleep(1);
                    continue;
                }

                data += Convert.ToChar(nextChar);
            }

            return data;
        }

        public void Process()
        {
            inputStream = new BufferedStream(socket.GetStream());
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));

            try
            {
                ParseRequest();
                ReadHeaders();

                if (httpMethod.Equals("GET"))
                {
                    HandleGETRequest();
                }
                else if (httpMethod.Equals("POST"))
                {
                    HandlePOSTRequest();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                WriteFailure();
            }

            outputStream.Flush();
            inputStream = null;
            outputStream = null;
            socket.Close();
        }

        public void WriteFailure()
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void HandlePOSTRequest()
        {
            Console.WriteLine("get post data start");
            int contentLenth = 0;
            MemoryStream memoryStream = new MemoryStream();

            if (this.httpHeaders.ContainsKey("Content-Length"))
            {
                contentLenth = Convert.ToInt32(this.httpHeaders["Content-Length"]);

                if (contentLenth > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server", contentLenth));
                }
                byte[] buffer = new byte[BUF_SIZE];
                int toRead = contentLenth;

                while (toRead > 0)
                {
                    Console.WriteLine("starting Read, toRead = {0}", toRead);

                    int numberRead = this.inputStream.Read(buffer, 0, Math.Min(BUF_SIZE, toRead));
                    Console.WriteLine("read finished, number read = {0}", numberRead);

                    if (numberRead == 0)
                    {
                        if (toRead == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }

                    toRead -= numberRead;
                    memoryStream.Write(buffer, 0, numberRead);
                }

                if (memoryStream.CanSeek)
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                }
            }

            Console.WriteLine("get post data end");
            server.HandlePOSTRequest(this, new StreamReader(memoryStream));
        }

        public void HandleGETRequest()
        {
            server.HandleGETRequest(this);
        }

        private void ReadHeaders()
        {
            Console.WriteLine("ReadHeaders");
            string line = String.Empty;
            string name = String.Empty;
            string value = String.Empty;

            while ((line = StreamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }

                name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }

        public void ParseRequest() //分析请求
        {
            string request = StreamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }

            httpMethod = tokens[0].ToUpper();
            httpUrl = tokens[1];
            httpProtocolVersionString = tokens[2];
        }

        public void WriteSuccess()
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: text/html");
            outputStream.WriteLine("");
        }
    }
}