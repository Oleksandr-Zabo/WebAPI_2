using System.Security.Cryptography;

namespace WebAPI_2.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password ?? ""));
                return Convert.ToBase64String(bytes);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var computedHash = HashPassword(password);
            return computedHash == hashedPassword;
        }
    }
}
