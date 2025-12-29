using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TP1.Models;
using TP1.Services;

namespace TP1.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;

        public IndexModel(CartService cartService)
        {
            _cartService = cartService;
        }

        public List<CartItem> CartItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        public void OnGet()
        {
            LoadCart();
        }

        public IActionResult OnPostRemoveItem(int productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateQuantity(int productId, string action)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            
            if (item != null)
            {
                int newQuantity = item.Quantity;
                
                if (action == "increase")
                {
                    newQuantity = item.Quantity + 1;
                }
                else if (action == "decrease")
                {
                    newQuantity = Math.Max(1, item.Quantity - 1);
                }
                
                _cartService.UpdateQuantity(productId, newQuantity);
            }
            
            return RedirectToPage();
        }

        public IActionResult OnPostCheckout()
        {
            // TODO: Implement checkout process
            return RedirectToPage();
        }

        private void LoadCart()
        {
            CartItems = _cartService.GetCart();
            Subtotal = _cartService.GetCartTotal();
            Total = Subtotal; // Add shipping/tax logic here if needed
        }
    }
}

