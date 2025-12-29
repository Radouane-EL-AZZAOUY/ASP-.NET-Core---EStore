using System.Text.Json;
using TP1.Models;

namespace TP1.Services
{
    public class CartService
    {
        private const string CartCookieName = "ShoppingCart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CartItem> GetCart()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return new List<CartItem>();

            var cartJson = httpContext.Request.Cookies[CartCookieName];
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        public void SaveCart(List<CartItem> cart)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var cartJson = JsonSerializer.Serialize(cart);
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
            };

            httpContext.Response.Cookies.Append(CartCookieName, cartJson, options);
        }

        public void AddToCart(int productId, string productTitle, decimal price, string? imagePath, int quantity = 1)
        {
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductTitle = productTitle,
                    Price = price,
                    Quantity = quantity,
                    ImagePath = imagePath
                });
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveFromCart(productId);
                }
                else
                {
                    item.Quantity = quantity;
                    SaveCart(cart);
                }
            }
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Delete(CartCookieName);
        }

        public int GetCartItemCount()
        {
            return GetCart().Sum(x => x.Quantity);
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(x => x.Total);
        }
    }
}

