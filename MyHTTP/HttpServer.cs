using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyHTTP
{
    public abstract class HttpServer
    {
        protected int port; //端口号
        private TcpListener listener; //监听器
        private bool isActive = true; //判断是否启用监听
        private IPAddress computerIp; //本机IP，为什么要创建这个对象呢？因为TcpListener（端口号）已经不被提倡

        public HttpServer(int port) //构造方法
        {
            this.port = port;
        }

        public void Listen() //监听方法
        {
            computerIp = IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(computerIp, port); //初始化TCP端口监听
            listener.Start();

            while (isActive)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                HttpProcessor httpProcessor = new HttpProcessor(tcpClient, this);
                Thread tcpThread = new Thread(new ThreadStart(httpProcessor.Process));
                tcpThread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void HandleGETRequest(HttpProcessor processor); //GET句柄

        public abstract void HandlePOSTRequest(HttpProcessor processor, StreamReader inputData); //POST句柄
    }
}