using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.Models
{
    public class CreateUpdateGenreRequest
    {
        [Required]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "Name can only contain letters, spaces, and hyphens")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}