using Newtonsoft.Json;

namespace ToolsApp.Services.LogSyncService
{
    public class LogSyncQueueService
    {
        public static Queue<LogItem> LogQueue = new();

        public void PushLog(LogItem logItem)
        {
            LogQueue.Enqueue(logItem);
        }

        public LogItem? PopLog()
        {
            if (LogQueue.Count > 0)
            {
                return LogQueue.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public int Count => LogQueue.Count;
    }

    public class LogItem
    {
        [JsonProperty("level")]
        public LogLevel LogLevel { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public LogItem(LogLevel level, string message)
        {
            LogLevel = level;
            Message = message;
        }
    }

    public enum LogLevel
    {
        Success = 0,
        Processing = 1,
        Failed = 2
    }
}
