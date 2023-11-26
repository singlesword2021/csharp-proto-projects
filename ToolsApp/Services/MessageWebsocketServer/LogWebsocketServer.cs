using CommonTools;
using Fleck;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace ToolsApp.Services.LogSyncService
{
    public class MessageWebsocketServer<T> : BackgroundService
    {
        private readonly ILogger _Logger;
        // save all connected sockets
        private List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();  
        // cache limit of the queue, if exceed, will be droped 
        private int MaxCount = int.MaxValue;
        // save the messages
        private ConcurrentQueue<T> _LogQueue = new();  

        public delegate bool BroadCastMethod(T message);

        /// <summary>
        /// One LogWebsocketServer corresponds to a message source, can be connectted with many Websock clients.
        /// All client will receive the message when the message is send to the server
        /// </summary>
        public MessageWebsocketServer(BroadCastMethod broadCastMethod,string serverIP, int serverPort, ILogger logger, bool useHttps = false)
        {
            _LogQueue = new ConcurrentQueue<T>();
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Server Starting...");
            string serveIP = IPTools.GetLocalIP(); //获得字符串形式的IP值
            Console.WriteLine("Server IP:" + serveIP);

            FleckLog.Level = Fleck.LogLevel.Debug;
            var server = new WebSocketServer("ws://" + serveIP + ":50000");
            //server.Certificate = new X509Certificate2(@"E:\\temp\\cert.pfx", "singlesword");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                    
                    allSockets.ToList().ForEach(s => {
                        var logItem = new LogItem(LogLevel.Success, "Echo:" + message);
                        s.Send(JsonConvert.SerializeObject(logItem));
                    });
                };
            });
            Console.WriteLine("Server Started!");
            await Task.Delay(3000);
            await SendToWebPage();
        }

        private async Task SendToWebPage()
        {
            Console.WriteLine("Start to Send...");

            while (--MaxCount > 0)
            {
                if (!_LogQueue.IsEmpty)
                {
                    foreach (var socket in allSockets.ToList())
                    {
                        _LogQueue.TryDequeue(out var message);
                        if (message is not null)
                        {
                            await socket.Send(JsonConvert.SerializeObject(message));
                            Console.WriteLine("Send from pop!");
                        }
                    }
                }
                await Task.Delay(100);
            }
        }

    }
}
