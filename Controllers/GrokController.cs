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
                var origin = Request.Headers["Origin"].ToString();
                _logger.LogInformation("Received chat request from: {Origin}", origin);
                
                if (chat == null || chat.Messages == null || !chat.Messages.Any())
                {
                    _logger.LogWarning("Received invalid chat request: chat is null or has no messages");
                    return BadRequest(new { error = "Invalid chat data", message = "Chat must contain at least one message" });
                }
                
                // 记录请求详情以便调试
                _logger.LogInformation("Chat request - ID: {ChatId}, Messages: {MessageCount}", 
                    chat.Id, chat.Messages.Count);
                
                var response = await _grokService.GetChatResponseAsync(chat);
                
                _logger.LogInformation("Chat response generated successfully");
                
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                
                // 提供更详细的错误信息
                var errorDetails = new
                {
                    error = "Internal server error",
                    message = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerError = ex.InnerException?.Message
                };
                
                return StatusCode(500, errorDetails);
            }
        }
        
        // 添加一个健康检查端点
        [HttpGet("health")]
        [EnableCors("AllowAll")]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "healthy", 
                message = "Grok API is running", 
                time = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                version = "1.0.1"
            });
        }
        
        // 测试端点
        [HttpGet("test")]
        [EnableCors("AllowAll")]
        public IActionResult Test()
        {
            return Ok(new { 
                status = "ok", 
                message = "Grok API is running", 
                time = DateTime.UtcNow,
                request_origin = Request.Headers["Origin"].ToString(),
                cors_enabled = true
            });
        }
        
        // 处理预检请求
        [HttpOptions("chat")]
        [EnableCors("AllowAll")]
        public IActionResult ChatOptions()
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return Ok();
        }
    }
}
