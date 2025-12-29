namespace TP1.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string? ProductTitle { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
        
        public decimal Total => Price * Quantity;
    }
}

