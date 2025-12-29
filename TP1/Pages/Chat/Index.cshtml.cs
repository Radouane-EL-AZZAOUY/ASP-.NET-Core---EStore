using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TP1.DTO;
using TP1.Services;

namespace TP1.Pages.Chat;

public class IndexModel : PageModel
{
    private readonly IOllamaService _ollamaService;
    private readonly IRAGService _ragService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IOllamaService ollamaService, 
        IRAGService ragService,
        ILogger<IndexModel> logger)
    {
        _ollamaService = ollamaService;
        _ragService = ragService;
        _logger = logger;
    }

    public bool IsOllamaAvailable { get; set; }

    public async Task OnGetAsync()
    {
        IsOllamaAvailable = await _ollamaService.IsAvailableAsync();
    }

    public async Task<IActionResult> OnPostSendMessageAsync([FromBody] ChatMessageDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new JsonResult(new ChatResponseDTO
            {
                Success = false,
                Error = "Message cannot be empty"
            });
        }

        // Get AI response with RAG context
        var response = await _ollamaService.GetChatResponseAsync(
            request.Message, 
            request.ConversationHistory);

        // Get relevant products to show as cards (show up to 6 for better coverage)
        var relevantProducts = await _ragService.GetRelevantProductsAsync(request.Message, 6);

        return new JsonResult(new ChatResponseDTO
        {
            Response = response,
            Success = true,
            RelevantProducts = relevantProducts.ToList()
        });
    }
}

