using WebAPI_2.Abstract;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;

        private const int MIN_PUBLISH_YEAR = 1450;

        public BookService(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        public (bool Success, string ErrorMessage, Guid? Id) Save(CreateBookRequest request)
        {
            // Validate publish year
            var yearValidation = ValidatePublishYear(request.PublishYear);
            if (!yearValidation.Success)
            {
                return (false, yearValidation.ErrorMessage, null);
            }

            // Check if book with same ISBN already exists
            if (_bookRepository.BookExistsByISBN(request.ISBN))
            {
                return (false, $"Book with ISBN '{request.ISBN}' already exists!", null);
            }

            // Check if book with same title and author already exists
            if (_bookRepository.BookExistsByTitle(request.Title, request.AuthorId))
            {
                var author = _authorRepository.GetById(request.AuthorId);
                if (author != null)
                {
                    return (false, $"Book '{request.Title}' by {author.FirstName} {author.LastName} already exists!", null);
                }
                else
                {
                    return (false, $"Book '{request.Title}' already exists!", null);
                }
            }

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                ISBN = request.ISBN,
                PublishYear = request.PublishYear,
                Price = request.Price,
                AuthorId = request.AuthorId
            };

            _bookRepository.AddBook(book);
            
            // Assign genres if provided
            if (request.GenreIds != null && request.GenreIds.Any())
            {
                _bookRepository.AssignGenresToBook(book.Id, request.GenreIds);
            }
            else
            {
                // Assign "Unknown" genre by default
                _bookRepository.AssignGenresToBook(book.Id, new List<int> { 1 });
            }

            return (true, null, book.Id);
        }

        public (bool Success, string ErrorMessage) Update(Guid id, UpdateBookRequest request)
        {
            // Check if book exists
            var existingBook = _bookRepository.GetById(id);
            if (existingBook == null)
            {
                return (false, "Book not found.");
            }

            // Validate publish year
            var yearValidation = ValidatePublishYear(request.PublishYear);
            if (!yearValidation.Success)
            {
                return yearValidation;
            }

            // Check if another book with same title and author exists
            if (_bookRepository.BookExistsByTitle(request.Title, request.AuthorId, id))
            {
                var author = _authorRepository.GetById(request.AuthorId);
                if (author != null)
                {
                    return (false, $"Another book with title '{request.Title}' by {author.FirstName} {author.LastName} already exists!");
                }
            }

            var book = new Book
            {
                Id = id,
                Title = request.Title,
                ISBN = existingBook.ISBN, // ISBN doesn't change
                PublishYear = request.PublishYear,
                Price = request.Price,
                AuthorId = request.AuthorId
            };

            var result = _bookRepository.UpdateBook(book);

            if (!result)
            {
                return (false, "Failed to update book.");
            }

            // Update genres
            if (request.GenreIds != null && request.GenreIds.Any())
            {
                _bookRepository.AssignGenresToBook(id, request.GenreIds);
            }

            return (true, null);
        }

        public (bool Success, string ErrorMessage) Delete(Guid id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return (false, "Book not found.");
            }

            var result = _bookRepository.DeleteBook(id);

            if (!result)
            {
                return (false, "Failed to delete book.");
            }

            return (true, null);
        }

        public (bool Success, string ErrorMessage) AssignGenres(Guid bookId, List<int> genreIds)
        {
            var book = _bookRepository.GetById(bookId);
            if (book == null)
            {
                return (false, "Book not found.");
            }

            if (genreIds == null || !genreIds.Any())
            {
                genreIds = new List<int> { 1 }; // Unknown genre
            }

            var result = _bookRepository.AssignGenresToBook(bookId, genreIds);
            return result
                ? (true, null)
                : (false, "Failed to assign genres to book.");
        }

        public BookDTO GetById(Guid id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return null;
            }

            var author = _authorRepository.GetById(book.AuthorId);
            return MapToDTO(book, author);
        }

        public List<BookDTO> GetAll()
        {
            var allBooks = _bookRepository.GetAll();
            var allAuthors = _authorRepository.GetAll();

            return allBooks.Select(b => MapToDTO(b, allAuthors.FirstOrDefault(a => a.Id == b.AuthorId))).ToList();
        }

        public List<BookDTO> GetFiltered(string searchTitle, Guid? filterAuthorId, int? filterGenreId, string sortBy = "Title", string sortOrder = "ASC")
        {
            var books = _bookRepository.GetFiltered(searchTitle, filterAuthorId, sortBy, sortOrder);

            if (filterGenreId.HasValue)
            {
                books = books.Where(b => b.BookGenres.Any(bg => bg.GenreId == filterGenreId.Value)).ToList();
            }

            var allAuthors = _authorRepository.GetAll();
            return books.Select(b => MapToDTO(b, allAuthors.FirstOrDefault(a => a.Id == b.AuthorId))).ToList();
        }

        public List<BookDTO> GetByGenre(int genreId)
        {
            var books = _bookRepository.GetBooksByGenre(genreId);
            var allAuthors = _authorRepository.GetAll();

            return books.Select(b => MapToDTO(b, allAuthors.FirstOrDefault(a => a.Id == b.AuthorId))).ToList();
        }

        private (bool Success, string ErrorMessage) ValidatePublishYear(int publishYear)
        {
            if (publishYear < MIN_PUBLISH_YEAR)
            {
                return (false, $"Publish year must be at least {MIN_PUBLISH_YEAR}.");
            }

            var currentYear = DateTime.Now.Year;
            if (publishYear > currentYear)
            {
                return (false, "Publish year cannot be in the future.");
            }

            return (true, null);
        }

        private BookDTO MapToDTO(Book book, Author author)
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
                AuthorFullName = author != null ? $"{author.FirstName} {author.LastName}" : "Unknown",
                GenreIds = book.BookGenres?.Select(bg => bg.GenreId).ToList() ?? new List<int>(),
                GenreNames = book.BookGenres?.Select(bg => bg.Genre?.Name ?? "Unknown").ToList() ?? new List<string>()
            };
        }
    }
}
