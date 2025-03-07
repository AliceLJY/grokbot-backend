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
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://api.x.ai/v1/chat/completions";
        private readonly string _model = "grok-2-1212";

        public GrokService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GrokApi:ApiKey"] ?? throw new ArgumentNullException("GrokApi:ApiKey is missing in configuration");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GetChatResponseAsync(Chat chat)
        {
            try
            {
                var grokRequest = new
                {
                    model = _model,
                    messages = chat.Messages.Select(m => new
                    {
                        role = m.Role,
                        content = m.Content
                    }).ToList(),
                    max_tokens = 1024,
                    temperature = 0.7,
                    top_p = 0.9
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(grokRequest),
                    Encoding.UTF8,
                    "application/json");

                Console.WriteLine($"Sending request to: {_apiUrl}");
                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode}, Content: {errorContent}");
                    return $"Error communicating with Grok API: {response.StatusCode} - {errorContent}";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var grokResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (grokResponse.TryGetProperty("choices", out var choices) && 
                    choices.GetArrayLength() > 0 && 
                    choices[0].TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content_text))
                {
                    return content_text.GetString() ?? "No response content";
                }
                
                return "Sorry, I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Grok API: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}