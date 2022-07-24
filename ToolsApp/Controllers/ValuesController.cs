using Microsoft.AspNetCore.Mvc;
using MultiThreadsDemo;

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

    }
}
