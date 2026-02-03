using WebAPI_2.Abstract;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;

        public AuthorService(IAuthorRepository authorRepository, IBookRepository bookRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
        }

        public (bool Success, string ErrorMessage, Guid? Id) Save(CreateUpdateAuthorRequest request)
        {
            // Validate birth date
            var dateValidation = ValidateBirthDate(request.BirthDate);
            if (!dateValidation.Success)
            {
                return (false, dateValidation.ErrorMessage, null);
            }

            // Check if author already exists
            if (AuthorExists(request.FirstName, request.LastName, request.BirthDate))
            {
                return (false, $"Author '{request.FirstName} {request.LastName}' with birth date {request.BirthDate:yyyy-MM-dd} already exists!", null);
            }

            var author = new Author
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate
            };

            _authorRepository.AddAuthor(author);
            return (true, null, author.Id);
        }

        public (bool Success, string ErrorMessage) Update(Guid id, CreateUpdateAuthorRequest request)
        {
            // Check if author exists
            var existingAuthor = _authorRepository.GetById(id);
            if (existingAuthor == null)
            {
                return (false, "Author not found.");
            }

            // Validate birth date
            var dateValidation = ValidateBirthDate(request.BirthDate);
            if (!dateValidation.Success)
            {
                return dateValidation;
            }

            var author = new Author
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate
            };

            var result = _authorRepository.UpdateAuthor(author);

            if (!result)
            {
                return (false, "Failed to update author.");
            }

            return (true, null);
        }

        public (bool Success, string ErrorMessage) Delete(Guid id)
        {
            // Check if author exists
            var author = _authorRepository.GetById(id);
            if (author == null)
            {
                return (false, "Author not found.");
            }

            // Check if author has books
            if (_authorRepository.HasBooks(id))
            {
                return (false, $"Cannot delete author '{author.FirstName} {author.LastName}' because they have associated books. Please remove all books by this author first.");
            }

            var result = _authorRepository.DeleteAuthor(id);

            if (!result)
            {
                return (false, "Failed to delete author.");
            }

            return (true, null);
        }

        public AuthorDTO GetById(Guid id)
        {
            var author = _authorRepository.GetById(id);
            return MapToDTO(author);
        }

        public List<AuthorDTO> GetAll()
        {
            return _authorRepository.GetAll()
                .Select(MapToDTO)
                .ToList();
        }

        public List<AuthorDTO> GetAuthorsWithBookCount()
        {
            var allAuthors = _authorRepository.GetAll();
            var allBooks = _bookRepository.GetAll();

            return allAuthors.Select(a => new AuthorDTO
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                BirthDate = a.BirthDate,
                BookCount = allBooks.Count(b => b.AuthorId == a.Id)
            }).ToList();
        }

        public List<BookDTO> GetAllBooks()
        {
            var books = _authorRepository.GetAllBooks();
            return books.Select(MapBookToDTO).ToList();
        }

        public List<BookDTO> GetAllAuthorBooks(Guid id)
        {
            var books = _authorRepository.GetAllAuthorBooks(id);
            return books.Select(MapBookToDTO).ToList();
        }

        private (bool Success, string ErrorMessage) ValidateBirthDate(DateTime birthDate)
        {
            var today = DateTime.Today;

            if (birthDate > today)
            {
                return (false, "Birth date cannot be in the future.");
            }

            var minBirthDate = today.AddYears(-350);
            if (birthDate < minBirthDate)
            {
                return (false, "Birth date is too far in the past (max 350 years ago).");
            }

            return (true, null);
        }

        private bool AuthorExists(string firstName, string lastName, DateTime birthDate)
        {
            var allAuthors = _authorRepository.GetAll();

            return allAuthors.Any(a =>
                a.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                a.BirthDate.Date == birthDate.Date);
        }

        private AuthorDTO MapToDTO(Author author)
        {
            if (author == null)
                return null;

            return new AuthorDTO
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                BirthDate = author.BirthDate,
                BookCount = 0
            };
        }

        private BookDTO MapBookToDTO(Book book)
        {
            if (book == null)
                return null;

            return new BookDTO
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                PublishYear = book.PublishYear,
                Price = book.Price,
                AuthorId = book.AuthorId,
                AuthorFullName = book.Author != null ? $"{book.Author.FirstName} {book.Author.LastName}" : "Unknown",
                GenreIds = book.BookGenres?.Select(bg => bg.GenreId).ToList() ?? new List<int>(),
                GenreNames = book.BookGenres?.Select(bg => bg.Genre?.Name ?? "Unknown").ToList() ?? new List<string>()
            };
        }
    }
}
