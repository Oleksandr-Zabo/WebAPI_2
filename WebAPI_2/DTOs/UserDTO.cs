using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(2)]
        public string Name { get; set; }
        
        [Required]
        [MinLength(2)]
        public string NickName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }
        
        public List<BookDTO> SavedBooks { get; set; } = new List<BookDTO>();
    }
}

