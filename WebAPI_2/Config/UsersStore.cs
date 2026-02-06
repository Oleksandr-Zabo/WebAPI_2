using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace WebAPI_2.Core
{
    public static class Roles {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public sealed class UserRecord { 
        public string Id { get; set; }
        //public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        //public string Password { get; set; }
        public string Role { get; set; }

    }

    public sealed class UsersStore
    {
        private readonly ConcurrentDictionary<string, UserRecord> _users = new();
        public UserRecord Find(string email)
        {
            if (string.IsNullOrEmpty(email)) {
                return null;
            }

            _users.TryGetValue(email, out var user);
            return user;
        }

        public bool Create(string email, string passwordHash, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passwordHash) || string.IsNullOrEmpty(role))
            {
                return false;
            }

            if (_users.ContainsKey(email))//email.Normalize()
            {
                return false;
            }

            var user = new UserRecord 
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                PasswordHash = Hash(passwordHash),
                Role = role
            };

            return _users.TryAdd(email, user);
        }

        public bool CheckPassword(UserRecord user, string password)
        { 
            return user != null && user.PasswordHash == password;
        }

        private static string Hash(string value)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value ?? ""));

                return Convert.ToBase64String(bytes);
            }
        }
    }
}
