using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.Models
{
    public class CreateUpdateUserRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; }

        [Required]
        [MinLength(2)]
        public string NickName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6)]
        public string Password { get; set; }
        
        public string Role { get; set; }
    }
}

