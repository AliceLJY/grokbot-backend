using Microsoft.AspNetCore.Mvc;

namespace GrokBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrokController : ControllerBase
    {
        [HttpPost("chat")]
        public IActionResult GetChatResponse()
        {
            return StatusCode(StatusCodes.Status410Gone, new
            {
                error = "This archived demo no longer provides an AI proxy."
            });
        }
        
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "archived",
                chatEnabled = false,
                time = DateTime.UtcNow,
                version = "1.1.0"
            });
        }
    }
}
