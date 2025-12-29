namespace TP1.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Movie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        public string? Genre { get; set; }
        public decimal Price { get; set; }
        public string? MainImagePath { get; set; }
        
        // Navigation property for gallery images
        public ICollection<MovieImage> Images { get; set; } = new List<MovieImage>();
    }
}