using System.Text;
using System.Text.Json;

namespace TP1.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly IRAGService _ragService;
    private readonly string _model;
    private readonly string _baseUrl;

    public OllamaService(
        HttpClient httpClient, 
        ILogger<OllamaService> logger, 
        IRAGService ragService,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _ragService = ragService;
        _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "llama2";
        
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }

    public async Task<string> GetChatResponseAsync(string userMessage, string conversationHistory = "")
    {
        try
        {
            _logger.LogInformation("Processing chat request for user message: {Message}", userMessage);
            
            // Get relevant product context using RAG
            var productContext = await _ragService.GetProductContextAsync(userMessage);

            var systemPrompt = BuildSystemPrompt(productContext);
            
            var fullPrompt = string.IsNullOrEmpty(conversationHistory) 
                ? $"{systemPrompt}\n\nUser: {userMessage}\nAssistant:"
                : $"{systemPrompt}\n\n{conversationHistory}\nUser: {userMessage}\nAssistant:";

            var requestBody = new
            {
                model = _model,
                prompt = fullPrompt,
                stream = false,
                options = new
                {
                    temperature = 0.7,
                    top_p = 0.9,
                    top_k = 40
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/generate", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama API returned status code: {StatusCode}. Error: {Error}", 
                    response.StatusCode, errorBody);
                return $"I'm having trouble connecting to the AI service. Error: {response.StatusCode}. Please try again later.";
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Ollama response received: {Response}", responseBody.Substring(0, Math.Min(200, responseBody.Length)));
            var jsonResponse = JsonDocument.Parse(responseBody);
            
            return jsonResponse.RootElement.GetProperty("response").GetString() 
                   ?? "I couldn't generate a response. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API");
            return "I'm currently unavailable. Please try again later.";
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string BuildSystemPrompt(string productContext)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("You are a friendly shopping assistant for an E-Store.");
        prompt.AppendLine();
        prompt.AppendLine("Communication Style:");
        prompt.AppendLine("- Be warm, conversational, and helpful");
        prompt.AppendLine("- Keep responses SHORT and natural (2-3 sentences max)");
        prompt.AppendLine("- Don't use formatting like ** or bullet points");
        prompt.AppendLine("- Speak like a helpful friend, not a formal assistant");
        prompt.AppendLine("- Don't mention 'Product ID' - users will see product cards below");
        prompt.AppendLine();
        prompt.AppendLine("Product Recommendations:");
        prompt.AppendLine("- Focus on 1-2 key points about each product");
        prompt.AppendLine("- Mention price and availability naturally");
        prompt.AppendLine("- If out of stock, suggest alternatives casually");
        prompt.AppendLine("- Only recommend products from the data provided below");
        
        if (!string.IsNullOrEmpty(productContext))
        {
            prompt.AppendLine();
            prompt.AppendLine("=== Available Products (use this data) ===");
            prompt.AppendLine(productContext);
        }
        
        return prompt.ToString();
    }
}

