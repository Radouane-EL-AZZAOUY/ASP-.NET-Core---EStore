using System.ComponentModel.DataAnnotations;

namespace TP1.DTO
{
    /// <summary>
    /// DTO for creating new products.
    /// Used internally by administrators and includes sensitive inventory data (Quantity).
    /// </summary>
    public class CreateProductDTO : ProductBaseDTO
    {
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int Quantity { get; set; }
    }
}

