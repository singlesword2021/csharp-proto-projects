using CommonTools;
using Fleck;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace WebSocketMessageServer
{
    /// <summary>
    /// A simple websocket message server
    /// When "SendMessage" if called, the message will be broadcasted to all connected clients.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageWebsocketServer<T> : BackgroundService
    {
        private readonly ILogger _Logger;

        // save all connected sockets
        private readonly List<IWebSocketConnection> allSockets = new();  

        // save the messages
        private static MessagesQueue<T> _MessagesQueue = new();  

        /// <summary>
        /// One LogWebsocketServer corresponds to a message source, can be connectted with many Websock clients.
        /// All client will receive the message when the message is send to the server
        /// </summary>
        public MessageWebsocketServer(string serverIP, int serverPort, ILogger logger, bool useHttps = false)
        {
            _MessagesQueue = new MessagesQueue<T>();
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("Server Starting...");
            string serveIP = IPTools.GetLocalIP(); //获得字符串形式的IP值
            _Logger.LogInformation("Server IP:" + serveIP);

            FleckLog.Level = Fleck.LogLevel.Debug;
            var server = new WebSocketServer("ws://" + serveIP + ":50000");
            //server.Certificate = new X509Certificate2(@"E:\\temp\\cert.pfx", "singlesword");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _Logger.LogInformation("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    _Logger.LogInformation("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    _Logger.LogDebug(message);                    
                    allSockets.ToList().ForEach(s => {
                        var response = "Message received from the client:" + message;
                        s.Send(JsonConvert.SerializeObject(response));
                    });
                };
            });
            _Logger.LogDebug("Server Started!");
            await Task.Delay(1000);
            await HostedForBroadcast();
        }

        public void SendMessage(T message)
        {
            _MessagesQueue.PushMessage(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HostedForBroadcast()
        {
            while (true)
            {
                if (!_MessagesQueue.IsEmpty)
                {
                    var message = _MessagesQueue.PopMessage();
                    if (message is not null)
                    {
                        foreach (var socket in allSockets.ToList())
                        {
                            var jsonMessage = JsonConvert.SerializeObject(message);
                            await socket.Send(jsonMessage);
                            _Logger.LogDebug(jsonMessage);
                        }
                    }
                }
                await Task.Delay(100);
            }
        }
    }

    /// <summary>
    /// 
    /// A encapsulation of a queue to handle messages
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessagesQueue<T>
    {
        // Elements limit of the queue, if exceed, will be droped before new is added
        private int _MaxCount { get; set; }
        private readonly ConcurrentQueue<T> _Queue = new();

        public MessagesQueue(int maxCount = int.MaxValue)
        {
            _MaxCount = maxCount;
        }

        public void PushMessage(T message)
        {
            if (_Queue.Count == _MaxCount)
            {
                _Queue.TryDequeue(out _);
            }
            _Queue.Enqueue(message);
        }

        public T? PopMessage()
        {
            if (!_Queue.IsEmpty)
            {
                _Queue.TryDequeue(out var message);
                return message;
            }
            else
            {
                return default;
            }
        }

        public bool IsEmpty => _Queue.IsEmpty;
    }
}
