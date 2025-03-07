using Microsoft.AspNetCore.Mvc;
using GrokBot.Api.Models;
using GrokBot.Api.Services;

namespace GrokBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrokController : ControllerBase
    {
        private readonly GrokService _grokService;
        private readonly ILogger<GrokController> _logger;

        public GrokController(GrokService grokService, ILogger<GrokController> logger)
        {
            _grokService = grokService;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> GetChatResponse([FromBody] Chat chat)
        {
            try
            {
                var response = await _grokService.GetChatResponseAsync(chat);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}