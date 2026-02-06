using WebAPI_2.Abstract;
using WebAPI_2.Core;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(
            IUserRepository userRepository, 
            IBookRepository bookRepository, 
            IAuthorRepository authorRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _passwordHasher = passwordHasher;
        }

        public (bool Success, string ErrorMessage, Guid? Id) Save(CreateUpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                return (false, "Password must be at least 6 characters long.", null);
            }

            // Check if user with same email already exists
            if (_userRepository.EmailExists(request.Email))
            {
                return (false, $"User with email '{request.Email}' already exists!", null);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                NickName = request.NickName,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Role = request.Role ?? Roles.User,
                CreatedAt = DateTime.UtcNow
            };

            var result = _userRepository.AddUser(user);
            return result 
                ? (true, null, user.Id) 
                : (false, "Failed to create user.", null);
        }

        public (bool Success, string ErrorMessage) Update(Guid id, CreateUpdateUserRequest request)
        {
            // Check if user exists
            var existingUser = _userRepository.GetById(id);
            if (existingUser == null)
            {
                return (false, "User not found.");
            }

            existingUser.Name = request.Name;
            existingUser.NickName = request.NickName;
            existingUser.Email = request.Email;
            existingUser.Role = request.Role ?? existingUser.Role;
            
            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length >= 6)
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(request.Password);
            }

            var result = _userRepository.UpdateUser(existingUser);
            
            if (!result)
            {
                return (false, "Failed to update user.");
            }

            return (true, null);
        }

        public (bool Success, string ErrorMessage) Delete(Guid id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                return (false, "User not found.");
            }

            
            var result = _userRepository.DeleteUser(id);
            
            if (!result)
            {
                return (false, "Failed to delete user.");
            }

            return (true, "User deleted successfully.");
        }

        public UserDTO GetById(Guid id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
                return null;

            return MapToDTO(user);
        }

        public List<UserDTO> GetAll()
        {
            var users = _userRepository.GetAll();
            return users.Select(MapToDTO).ToList();
        }

        public (bool Success, string ErrorMessage) AddSavedBook(Guid userId, Guid bookId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            var book = _bookRepository.GetById(bookId);
            if (book == null)
            {
                return (false, "Book not found.");
            }

            var result = _userRepository.AddSavedBook(bookId, userId);
            
            if (!result)
            {
                return (false, "Book is already in your saved list.");
            }

            return (true, "Book added to saved list successfully.");
        }

        public (bool Success, string ErrorMessage) RemoveSavedBook(Guid userId, Guid bookId)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            var result = _userRepository.RemoveSavedBook(bookId, userId);
            
            if (!result)
            {
                return (false, "Book not found in your saved list.");
            }

            return (true, "Book removed from saved list successfully.");
        }

        public List<BookDTO> GetSavedBooks(Guid userId)
        {
            var books = _userRepository.GetSavedBooks(userId);
            var allAuthors = _authorRepository.GetAll();

            return books.Select(b => new BookDTO
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                PublishYear = b.PublishYear,
                Price = b.Price,
                AuthorId = b.AuthorId,
                AuthorFullName = b.Author != null 
                    ? $"{b.Author.FirstName} {b.Author.LastName}" 
                    : allAuthors.FirstOrDefault(a => a.Id == b.AuthorId)?.FirstName + " " + allAuthors.FirstOrDefault(a => a.Id == b.AuthorId)?.LastName,
                GenreIds = b.BookGenres?.Select(bg => bg.GenreId).ToList() ?? new List<int>(),
                GenreNames = b.BookGenres?.Select(bg => bg.Genre?.Name ?? "Unknown").ToList() ?? new List<string>()
            }).ToList();
        }

        private UserDTO MapToDTO(User user)
        {
            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                NickName = user.NickName,
                Email = user.Email,
                Role = user.Role,
                SavedBooks = user.SavedBooks?.Select(b => new BookDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    PublishYear = b.PublishYear,
                    Price = b.Price,
                    AuthorId = b.AuthorId,
                    AuthorFullName = b.Author != null ? $"{b.Author.FirstName} {b.Author.LastName}" : "Unknown",
                    GenreIds = b.BookGenres?.Select(bg => bg.GenreId).ToList() ?? new List<int>(),
                    GenreNames = b.BookGenres?.Select(bg => bg.Genre?.Name ?? "Unknown").ToList() ?? new List<string>()
                }).ToList() ?? new List<BookDTO>()
            };
        }
    }
}
