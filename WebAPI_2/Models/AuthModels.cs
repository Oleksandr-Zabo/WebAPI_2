using System.ComponentModel.DataAnnotations;

namespace WebAPI_2.Models
{
    public sealed class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        public string Name { get; set; }
        
        public string NickName { get; set; }
    }

    public sealed class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public sealed class AuthResponse
    {
        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}

