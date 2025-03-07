using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using GrokBot.Api.Models;

namespace GrokBot.Api.Services
{
    public class GrokService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GrokService> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://api.x.ai/v1/chat/completions";
        private readonly string _model = "grok-2-1212";
        private readonly int _timeoutSeconds = 60; // 延长超时时间
        private int _retryCount = 0;
        private const int MAX_RETRIES = 2;

        public GrokService(HttpClient httpClient, IConfiguration configuration, ILogger<GrokService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GrokApi:ApiKey"] ?? throw new ArgumentNullException("GrokApi:ApiKey is missing in configuration");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
            
            _logger.LogInformation("GrokService initialized with API URL: {ApiUrl}", _apiUrl);
        }

        public async Task<string> GetChatResponseAsync(Chat chat)
        {
            _retryCount = 0;
            return await ExecuteWithRetryAsync(chat);
        }
        
        private async Task<string> ExecuteWithRetryAsync(Chat chat)
        {
            try
            {
                _logger.LogInformation("Sending chat request (Attempt: {AttemptCount})", _retryCount + 1);
                
                var messages = chat.Messages.Select(m => new
                {
                    role = m.Role,
                    content = m.Content
                }).ToList();
                
                var requestData = new
                {
                    model = _model,
                    messages = messages,
                    max_tokens = 1024,
                    temperature = 0.7,
                    top_p = 0.9
                };
                
                var jsonString = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                
                _logger.LogDebug("Request payload: {RequestData}", jsonString);
                
                // 设置取消令牌以支持超时
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
                
                var response = await _httpClient.PostAsync(_apiUrl, content, cts.Token);
                
                _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Raw Response: {RawResponse}", responseText);

                    try
                    {
                        var responseData = JsonSerializer.Deserialize<JsonElement>(responseText);
                        if (responseData.TryGetProperty("choices", out var choices) && 
                            choices.GetArrayLength() > 0 && 
                            choices[0].TryGetProperty("message", out var message) &&
                            message.TryGetProperty("content", out var content_text))
                        {
                            return content_text.GetString() ?? "No response content";
                        }
                        
                        _logger.LogWarning("Could not extract content from response: {Response}", responseText);
                        return "Sorry, I couldn't generate a response.";
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "JSON parsing error with response: {Response}", responseText);
                        return $"Error parsing API response: {ex.Message}";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    
                    // 尝试重试请求
                    if (_retryCount < MAX_RETRIES && ((int)response.StatusCode >= 500 || (int)response.StatusCode == 429))
                    {
                        _retryCount++;
                        _logger.LogInformation("Retrying request... Attempt {RetryCount} of {MaxRetries}", _retryCount, MAX_RETRIES);
                        
                        // 增加延迟，避免立即重试，对于429错误增加更长延迟
                        var delayMs = (int)response.StatusCode == 429 ? 2000 * _retryCount : 1000 * _retryCount;
                        await Task.Delay(delayMs);
                        
                        return await ExecuteWithRetryAsync(chat);
                    }
                    
                    // 提供更友好的错误信息
                    return $"Error communicating with Grok API: {response.StatusCode}.\n" +
                           "如果这是首次运行，请注意Render.com免费服务需要一段时间来启动。请稍后再试。";
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request timed out after {TimeoutSeconds} seconds", _timeoutSeconds);
                
                // 尝试重试超时请求
                if (_retryCount < MAX_RETRIES)
                {
                    _retryCount++;
                    _logger.LogInformation("Retrying after timeout... Attempt {RetryCount} of {MaxRetries}", _retryCount, MAX_RETRIES);
                    await Task.Delay(1000 * _retryCount);
                    return await ExecuteWithRetryAsync(chat);
                }
                
                return "请求超时。这可能是因为Grok API响应时间较长或Render.com的免费服务正在启动中。";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error");
                
                // 尝试重试网络错误
                if (_retryCount < MAX_RETRIES)
                {
                    _retryCount++;
                    _logger.LogInformation("Retrying after network error... Attempt {RetryCount} of {MaxRetries}", _retryCount, MAX_RETRIES);
                    await Task.Delay(1000 * _retryCount);
                    return await ExecuteWithRetryAsync(chat);
                }
                
                return "网络连接错误，无法连接到Grok API。请检查您的网络连接并稍后再试。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Grok API");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerExceptionMessage}", ex.InnerException.Message);
                }
                
                return $"发生意外错误: {ex.Message}";
            }
        }
    }
}
