using System.Text;
using System.Text.RegularExpressions;
using TP1.DTO;

namespace TP1.Services;

public class RAGService : IRAGService
{
    private readonly IProductService _productService;
    private readonly ILogger<RAGService> _logger;
    
    // Common product-related keywords to detect product queries
    private static readonly string[] ProductKeywords = 
    {
        "product", "buy", "purchase", "price", "cost", "laptop", "phone", 
        "computer", "airpod", "headphone", "electronic", "available", 
        "stock", "recommend", "suggest", "show", "find", "search",
        "cheap", "expensive", "best", "quality", "specification", "spec"
    };

    public RAGService(IProductService productService, ILogger<RAGService> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<string> GetProductContextAsync(string userQuery, int maxProducts = 5)
    {
        try
        {
            // Check if the query is product-related
            if (!IsProductRelatedQuery(userQuery))
            {
                return string.Empty; // No product context needed
            }

            var relevantProducts = await GetRelevantProductsAsync(userQuery, maxProducts);
            
            if (!relevantProducts.Any())
            {
                return "\nNote: No specific products match your query, but I can still help with general information.";
            }

            return FormatProductContext(relevantProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product context");
            return string.Empty;
        }
    }

    public async Task<IEnumerable<ProductDTO>> GetRelevantProductsAsync(string query, int maxResults = 5)
    {
        try
        {
            _logger.LogInformation("RAG: Searching for products with query: '{Query}'", query);
            
            // First try exact search
            var searchResults = await _productService.SearchProductsAsync(query);
            var products = searchResults.Take(maxResults).ToList();
            
            _logger.LogInformation("RAG: Found {Count} products from search", products.Count);

            // If we don't have enough results, add recommended products
            if (products.Count < maxResults)
            {
                var recommended = await _productService.GetRecommendedProductsAsync(maxResults - products.Count);
                var recommendedList = recommended.Where(r => !products.Any(p => p.Id == r.Id));
                products.AddRange(recommendedList);
                _logger.LogInformation("RAG: Added {Count} recommended products", recommendedList.Count());
            }

            // Score and rank products by relevance
            var scoredProducts = products
                .Select(p => new
                {
                    Product = p,
                    Score = CalculateRelevanceScore(p, query)
                })
                .OrderByDescending(x => x.Score)
                .Take(maxResults)
                .Select(x => x.Product)
                .ToList();

            _logger.LogInformation("RAG: Returning {Count} products: {Products}", 
                scoredProducts.Count, 
                string.Join(", ", scoredProducts.Select(p => $"{p.Title} (${p.Price})")));

            return scoredProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relevant products");
            return Enumerable.Empty<ProductDTO>();
        }
    }

    private bool IsProductRelatedQuery(string query)
    {
        var lowerQuery = query.ToLower();
        return ProductKeywords.Any(keyword => lowerQuery.Contains(keyword));
    }

    private int CalculateRelevanceScore(ProductDTO product, string query)
    {
        var score = 0;
        var lowerQuery = query.ToLower();
        var queryWords = Regex.Split(lowerQuery, @"\W+").Where(w => w.Length > 2);

        foreach (var word in queryWords)
        {
            // Title matches are most important
            if (product.Title?.ToLower().Contains(word) == true)
                score += 10;

            // Description matches
            if (product.Description?.ToLower().Contains(word) == true)
                score += 5;
        }

        // Boost for in-stock items
        if (product.InStock)
            score += 3;

        // Price-related queries
        if (lowerQuery.Contains("cheap") || lowerQuery.Contains("affordable"))
        {
            if (product.Price < 500) score += 5;
        }
        else if (lowerQuery.Contains("expensive") || lowerQuery.Contains("premium") || lowerQuery.Contains("high-end"))
        {
            if (product.Price > 1000) score += 5;
        }

        return score;
    }

    private string FormatProductContext(IEnumerable<ProductDTO> products)
    {
        var context = new StringBuilder();
        context.AppendLine("\n\n=== Available Products in Our Store ===");
        
        foreach (var product in products)
        {
            context.AppendLine($"\nProduct: {product.Title}");
            context.AppendLine($"Description: {product.Description}");
            context.AppendLine($"Price: ${product.Price:F2}");
            context.AppendLine($"Availability: {(product.InStock ? "In Stock" : "Out of Stock")}");
            context.AppendLine($"Product ID: {product.Id}");
        }
        
        context.AppendLine("\n=== End of Product Information ===\n");
        
        return context.ToString();
    }
}

