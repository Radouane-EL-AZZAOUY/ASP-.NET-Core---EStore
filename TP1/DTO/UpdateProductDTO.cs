using System.ComponentModel.DataAnnotations;

namespace TP1.DTO
{
    /// <summary>
    /// DTO for updating existing products.
    /// Used internally by administrators and includes sensitive inventory data (Quantity).
    /// </summary>
    public class UpdateProductDTO : ProductBaseDTO
    {
        public int Id { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int Quantity { get; set; }
    }
}

