using Newtonsoft.Json;

namespace ToolsApp.Models
{
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
}
