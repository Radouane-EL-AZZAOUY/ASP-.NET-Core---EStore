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
        prompt.AppendLine("You are an intelligent shopping assistant for an E-Store application.");
        prompt.AppendLine("Your role is to help customers find products, answer questions, and provide recommendations.");
        prompt.AppendLine();
        prompt.AppendLine("Guidelines:");
        prompt.AppendLine("- Be helpful, friendly, and concise");
        prompt.AppendLine("- When recommending products, mention their names, prices, and key features");
        prompt.AppendLine("- If a product is out of stock, mention it but suggest alternatives");
        prompt.AppendLine("- Always base your product recommendations on the actual product data provided");
        prompt.AppendLine("- If asked about products not in the data, politely say they're not currently available");
        prompt.AppendLine("- Include product IDs when mentioning specific products so users can find them easily");
        
        if (!string.IsNullOrEmpty(productContext))
        {
            prompt.AppendLine();
            prompt.AppendLine("=== IMPORTANT: Use this product information to answer the user's question ===");
            prompt.AppendLine(productContext);
        }
        
        return prompt.ToString();
    }
}

