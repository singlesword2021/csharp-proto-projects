using Microsoft.AspNetCore.Mvc;
using MultiThreadsDemo;
using ToolsApp.Services.LogSyncService;
using WebSocketMessageServer;
using LogLevel = ToolsApp.Services.LogSyncService.LogLevel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ToolsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            MainThread mh = new MainThread();
            mh.Start();
            return "done";
        }

        // GET api/<ValuesController>/5
        [HttpPost("send")]
        public string SendToWS([FromBody] string message, [FromServices] WebSocketMessageServer.MessageWebsocketServer<LogItem> _MessageWebsocketServer)
        {
            LogItem log = new LogItem(LogLevel.Success, message);
            _MessageWebsocketServer.SendMessage(log);
            return "sent";
        }

    }
}
