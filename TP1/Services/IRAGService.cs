using TP1.DTO;

namespace TP1.Services;

public interface IRAGService
{
    Task<string> GetProductContextAsync(string userQuery, int maxProducts = 5);
    Task<IEnumerable<ProductDTO>> GetRelevantProductsAsync(string query, int maxResults = 5);
}

