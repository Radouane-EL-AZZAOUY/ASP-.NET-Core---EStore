namespace TP1.Services;

public interface IOllamaService
{
    Task<string> GetChatResponseAsync(string userMessage, string conversationHistory = "");
    Task<bool> IsAvailableAsync();
}

