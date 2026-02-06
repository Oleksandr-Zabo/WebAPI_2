namespace WebAPI_2.Core
{
    /// <summary>
    /// DTO for JWT token generation
    /// </summary>
    public sealed class UserRecord
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
