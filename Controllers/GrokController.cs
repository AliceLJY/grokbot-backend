using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
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
        [EnableCors("AllowAll")]  // 使用宽松的CORS策略以确保请求能成功
        public async Task<IActionResult> GetChatResponse([FromBody] Chat chat)
        {
            try
            {
                _logger.LogInformation("Received chat request from: {Origin}", Request.Headers["Origin"]);
                
                var response = await _grokService.GetChatResponseAsync(chat);
                
                _logger.LogInformation("Chat response generated successfully");
                
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
        
        // 添加一个简单的测试端点
        [HttpGet("test")]
        [EnableCors("AllowAll")]
        public IActionResult Test()
        {
            return Ok(new { 
                status = "ok", 
                message = "Grok API is running", 
                time = DateTime.UtcNow,
                request_origin = Request.Headers["Origin"].ToString() 
            });
        }
        
        // 处理预检请求
        [HttpOptions("chat")]
        [EnableCors("AllowAll")]
        public IActionResult ChatOptions()
        {
            return Ok();
        }
    }
}
