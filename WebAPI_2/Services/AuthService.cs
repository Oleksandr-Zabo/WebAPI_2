using WebAPI_2.Core;
using WebAPI_2.DAL.Abstracts;

namespace WebAPI_2.Services
{
    public interface IAuthService
    {
        UserRecord Authenticate(string email, string password);
        (bool success, string errorMessage, Guid? userId) Register(string email, string password, string name, string nickName, string role);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public UserRecord Authenticate(string email, string password)
        {
            var user = _userRepository.GetByEmail(email);

            if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return new UserRecord 
            { 
                Id = user.Id.ToString(), 
                Email = user.Email, 
                Role = user.Role,
                PasswordHash = user.PasswordHash
            };
        }

        public (bool success, string errorMessage, Guid? userId) Register(string email, string password, string name, string nickName, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Email and password are required", null);
            }

            if (password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long", null);
            }

            if (_userRepository.EmailExists(email))
            {
                return (false, "User with this email already exists", null);
            }

            var user = new DAL.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                Name = name ?? email.Split('@')[0],
                NickName = nickName ?? email.Split('@')[0],
                Role = role ?? Roles.User,
                CreatedAt = DateTime.UtcNow
            };

            var success = _userRepository.AddUser(user);

            return success 
                ? (true, null, user.Id) 
                : (false, "Failed to create user", null);
        }
    }
}
