using CommonTools;
using Fleck;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ToolsApp.Services.LogSyncService
{
    public class ConsoleLogSyncServer : BackgroundService
    {
        private List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        private int MaxCount = int.MaxValue;
        private LogSyncQueueService _LogSyncQueueService;

        public ConsoleLogSyncServer()
        {
            _LogSyncQueueService = new LogSyncQueueService();
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
                if (_LogSyncQueueService.Count > 0)
                {
                    foreach (var socket in allSockets.ToList())
                    {
                        var logItem = _LogSyncQueueService.PopLog();
                        if (logItem is not null)
                        {
                            await socket.Send(JsonConvert.SerializeObject(logItem));
                            Console.WriteLine("Send from pop!");
                        }
                    }
                }
                
                await Task.Delay(100);
            }
        }

    }
}
