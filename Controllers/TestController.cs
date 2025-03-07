using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace GrokBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [EnableCors("AllowAll")]
        public IActionResult Get()
        {
            return Ok(new { 
                status = "ok", 
                message = "API is running", 
                time = DateTime.UtcNow,
                version = "1.0.0",
                cors = "Enabled for all origins"
            });
        }

        [HttpPost("echo")]
        [EnableCors("AllowAll")]
        public IActionResult Echo([FromBody] object data)
        {
            return Ok(new { 
                status = "ok", 
                received = data,
                time = DateTime.UtcNow
            });
        }

        [HttpOptions]
        [EnableCors("AllowAll")]
        public IActionResult Options()
        {
            return Ok();
        }
    }
}
