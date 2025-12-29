namespace TP1.DTO;

public class ChatMessageDTO
{
    public string Message { get; set; } = string.Empty;
    public string ConversationHistory { get; set; } = string.Empty;
}

public class ChatResponseDTO
{
    public string Response { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<ProductDTO>? RelevantProducts { get; set; }
}

