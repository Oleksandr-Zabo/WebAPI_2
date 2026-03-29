using Microsoft.EntityFrameworkCore;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;

namespace WebAPI_2.DAL.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _db;

        public BookRepository(AppDbContext db)
        {
            _db = db;
        }

        public bool AddBook(Book book)
        {
            var res = _db.Books.Add(book) != null;
            _db.SaveChanges();
            return res;
        }

        public Book GetById(Guid id)
        {
            return _db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .FirstOrDefault(x => x.Id == id);
        }

        public List<Book> GetAll()
        {
            return _db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .ToList();
        }

        public bool UpdateBook(Book book)
        {
            var existingBook = _db.Books.FirstOrDefault(x => x.Id == book.Id);
            if (existingBook == null)
                return false;

            existingBook.Title = book.Title;
            // ISBN don't update
            existingBook.PublishYear = book.PublishYear;
            existingBook.Price = book.Price;
            existingBook.AuthorId = book.AuthorId;

            _db.SaveChanges();
            return true;
        }

        public bool DeleteBook(Guid id)
        {
            var book = _db.Books.FirstOrDefault(x => x.Id == id);
            if (book == null)
                return false;

            _db.Books.Remove(book);
            _db.SaveChanges();
            return true;
        }

        public bool BookExistsByISBN(string isbn)
        {
            return _db.Books.Any(b => b.ISBN == isbn);
        }

        public bool BookExistsByISBN(string isbn, Guid excludeBookId)
        {
            return _db.Books.Any(b => b.ISBN == isbn && b.Id != excludeBookId);
        }

        public bool BookExistsByTitle(string title, Guid authorId)
        {
            return _db.Books.Any(b =>
                b.Title.ToLower() == title.ToLower() &&
                b.AuthorId == authorId);
        }

        public bool BookExistsByTitle(string title, Guid authorId, Guid excludeBookId)
        {
            return _db.Books.Any(b =>
                b.Title.ToLower() == title.ToLower() &&
                b.AuthorId == authorId &&
                b.Id != excludeBookId);
        }

        public List<Book> GetFiltered(string searchTitle, Guid? filterAuthorId, string sortBy = "Title", string sortOrder = "ASC")
        {
            //LINQ to SQL 
            IQueryable<Book> query = _db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre);

            // Name 
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                query = query.Where(b => b.Title.Contains(searchTitle));
            }

            // Author
            if (filterAuthorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == filterAuthorId.Value);
            }

            // Sort
            query = sortBy switch
            {
                "Year" => sortOrder == "DESC"
                    ? query.OrderByDescending(b => b.PublishYear)
                    : query.OrderBy(b => b.PublishYear),
                _ => sortOrder == "DESC"
                    ? query.OrderByDescending(b => b.Title)
                    : query.OrderBy(b => b.Title)
            };

            return query.ToList();
        }

        public bool AssignGenresToBook(Guid bookId, List<int> genreIds)
        {
            var book = _db.Books.Include(b => b.BookGenres).FirstOrDefault(b => b.Id == bookId);
            if (book == null)
                return false;

            // Remove existing genres
            _db.BookGenres.RemoveRange(book.BookGenres);

            // Add new genres
            foreach (var genreId in genreIds)
            {
                book.BookGenres.Add(new BookGenre { BookId = bookId, GenreId = genreId });
            }

            _db.SaveChanges();
            return true;
        }

        public List<Book> GetBooksByGenre(int genreId)
        {
            return _db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
                .Where(b => b.BookGenres.Any(bg => bg.GenreId == genreId))
                .ToList();
        }

        public List<Genre> GetBookGenres(Guid bookId)
        {
            return _db.BookGenres
                .Where(bg => bg.BookId == bookId)
                .Select(bg => bg.Genre)
                .ToList();
        }
    }
}