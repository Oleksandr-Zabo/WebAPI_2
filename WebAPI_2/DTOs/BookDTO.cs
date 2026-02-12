using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.DTOs
{
    public class BookDTO
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(2)]
        public string Title { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        [Range(1450, 2100, ErrorMessage = "Publish year must be between 1450 and 2100")]
        public int PublishYear { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal? Price { get; set; }

        [Required]
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public List<int> GenreIds { get; set; } = new List<int>();
        public List<string> GenreNames { get; set; } = new List<string>();
        public string CoverImageUrl { get; set; }
        public int? GutendexId { get; set; }
        public string Summary { get; set; }
        public int? DownloadCount { get; set; }
        public bool IsExternal => Id == Guid.Empty;
    }
}


