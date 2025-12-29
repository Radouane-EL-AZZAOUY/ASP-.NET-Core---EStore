namespace TP1.Models
{
    public class MovieImage
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        
        // Navigation property
        public Movie? Movie { get; set; }
    }
}

