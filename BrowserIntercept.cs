﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using static DownloadManager.Logging;

namespace DownloadManager
{
    internal class BrowserIntercept
    {
        public BrowserIntercept _instance;
        public Socket httpServer;
        private int serverPort = Settings1.Default.serverPort;
        public Thread thread;

        public void StartServer()
        {
            _instance = this;
            Log("Starting internal server...", Color.White);
            try
            {
                thread = new Thread(new ThreadStart(ConnectionThreadMethod));
                thread.Start();
            }
            catch (Exception ex)
            {
                Log("Error while starting internal server:" + Environment.NewLine + ex.Message + ex.StackTrace, Color.Red);
            }
        }

        private void ConnectionThreadMethod()
        {
            try
            {
                httpServer = new Socket(SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                httpServer.Bind(endPoint);
                httpServer.Listen(1);
                ListenForConnections();
            }
            catch (Exception ex)
            {
                Log("Error while starting internal server:" + Environment.NewLine + ex.Message + ex.StackTrace, Color.Red);
                ConnectionThreadMethod();
            }
        }

        private void ListenForConnections()
        {
            while (true)
            {
                String data = "";
                byte[] bytes = new byte[2048];

                Log("Internal server started. Listening for connections on port " + serverPort + ".", Color.Green);
                Socket client = httpServer.Accept();

                // Read inbound connection data
                while (true)
                {
                    int numBytes = client.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, numBytes);

                    if (data.IndexOf("\r\n") > -1)
                    {
                        break;
                    }
                }

                string[] splittedData = data.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                String url = splittedData[0].ToString().Replace("%22 HTTP/1.1", "");
                url = url.Replace("GET /?url=%22", "");

                if (!url.Contains("favicon.ico"))
                {
                    Logging._instance.Invoke((MethodInvoker)delegate
                    {
                        Log("Request received for URL: " + url, Color.White);
                        if (Settings1.Default.downloadHistory.Contains(url) == false)
                        {
                            DownloadForm._instance.textBox1.Items.Add(url);
                            Settings1.Default.downloadHistory.Add(url);
                            Settings1.Default.Save();
                        }
                        DownloadProgress downloadProgress = new DownloadProgress(url, Settings1.Default.defaultDownload, "", 0);
                        downloadProgress.Show();
                        //Log("--- Start Request ---", Color.White);
                        //Log(data, Color.White);
                        //Log("--- End Request ---", Color.White);
                    });
                }

                String resHeader = "HTTP/1.1 200 OK\nServer: DownloadManager_Internal\nContent-Type: text/html; charset: UTF-8\n\n";
                String resBody = "<!DOCTYPE html><html><head><title>Download Manager</title><style>body{background-color: rgb(18, 22, 58);color: white;font-family: Arial, Helvetica, sans-serif;}h1{font-size: 30px;}</style></head><body><h1>Download Manager</h1><hr><p>Please do not manually send requests to the internal server.</p><p>If a request is sent incorrectly the internal server may crash.If this occurs please restart the Download Manager application.</p></body></html>";
                String resStr = resHeader + resBody;
                byte[] resData = Encoding.ASCII.GetBytes(resStr);
                client.SendTo(resData, client.RemoteEndPoint);
                client.Close();
                bytes = null;
                httpServer.Close();
                ConnectionThreadMethod();
            }
        }
    }
}
