using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.DTOs
{
    public class AuthorDTO
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(2)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        public string LastName { get; set; }

        public DateTime BirthDate { get; set; }

        public int BookCount { get; set; }
    }
}