using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TP1.DTO;
using TP1.Services;

namespace TP1.Pages.Promotions
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

        public IList<ProductDTO> RecommendedProducts { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Get recommended products using the service
            RecommendedProducts = (await _productService.GetRecommendedProductsAsync(8)).ToList();
        }

        public async Task<IActionResult> OnPostAddToCart(int productId)
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

            return RedirectToPage();
        }
    }
}

