using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TP1.DTO;
using TP1.Services;

namespace TP1.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly CartService _cartService;

        public IndexModel(IProductService productService, CartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public IList<ProductDTO> Product { get; set; } = default!;
        public string? SearchQuery { get; set; }
        public int TotalResults { get; set; }

        public async Task OnGetAsync(string? search)
        {
            SearchQuery = search;

            if (!string.IsNullOrWhiteSpace(search))
            {
                Product = (await _productService.SearchProductsAsync(search)).ToList();
            }
            else
            {
                Product = (await _productService.GetAllProductsAsync()).ToList();
            }

            TotalResults = Product.Count;
        }

        public async Task<IActionResult> OnPostAddToCart(int productId, string? search)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product != null)
            {
                _cartService.AddToCart(
                    product.Id,
                    product.Title ?? "Unknown Product",
                    product.Price,
                    product.MainImagePath,
                    quantity: 1
                );

                TempData["CartMessage"] = $"{product.Title} added to cart!";
            }

            // Preserve search query when redirecting
            if (!string.IsNullOrWhiteSpace(search))
            {
                return RedirectToPage(new { search });
            }

            return RedirectToPage();
        }
    }
}
