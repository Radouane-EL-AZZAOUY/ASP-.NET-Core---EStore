using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TP1.DataLayer;
using TP1.Models;
using TP1.Services;

namespace TP1.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly TP1.DataLayer.DBContext _context;
        private readonly IProductService _productService;
        private readonly CartService _cartService;

        public DetailsModel(TP1.DataLayer.DBContext context, IProductService productService, CartService cartService)
        {
            _context = context;
            _productService = productService;
            _cartService = cartService;
        }

        public Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FirstOrDefaultAsync(m => m.Id == id);

            if (product is not null)
            {
                Product = product;

                return Page();
            }

            return NotFound();
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

            return RedirectToPage(new { id = productId });
        }
    }
}
