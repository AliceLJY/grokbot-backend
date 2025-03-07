using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using GrokBot.Api.Models;
using GrokBot.Api.Services;

namespace GrokBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // 在控制器级别应用CORS，确保所有动作都已启用
    [EnableCors("AllowAll")]
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
        public IActionResult ChatOptions()
        {
            // 实际上我们不需要手动添加这些头部，因为CORS中间件会处理它们
            // 保留这些代码以确保所有跨域预检请求都能得到正确响应
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            return Ok();
        }
    }
}
