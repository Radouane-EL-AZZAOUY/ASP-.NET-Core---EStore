using System.ComponentModel.DataAnnotations;

namespace TP1.DTO
{
    /// <summary>
    /// Base class for product DTOs containing common properties and validation.
    /// Follows DRY principle by centralizing shared attributes.
    /// </summary>
    public abstract class ProductBaseDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal Price { get; set; }
        
        public string? MainImagePath { get; set; }
    }
}

