  namespace TP1.Models
  {
    using System.ComponentModel.DataAnnotations;
    
    public class Product
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
       
        public string? MainImagePath { get; set; }
        [DataType(DataType.Date)]
        public DateTime AddedAt { get; set; }
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property for gallery images
        // public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}