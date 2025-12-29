namespace TP1.DTO
{
    /// <summary>
    /// Customer-facing DTO that hides sensitive business data (Quantity).
    /// Exposes only InStock status instead of actual inventory quantity.
    /// </summary>
    public class ProductDTO : ProductBaseDTO
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Indicates if the product is available in stock.
        /// This replaces the sensitive Quantity field for customer-facing operations.
        /// </summary>
        public bool InStock { get; set; }
        
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
