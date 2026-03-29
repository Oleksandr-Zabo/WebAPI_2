using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI_2.DAL.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly AppDbContext _db;

        public AuthorRepository(AppDbContext db)
        {
            _db = db;
        }

        public bool AddAuthor(Author author)
        {
            var res = _db.Authors.Add(author) != null;
            _db.SaveChanges();
            return res;
        }

        public Author GetById(Guid id)
        {
            return _db.Authors.FirstOrDefault(x => x.Id == id);
        }

        public List<Author> GetAll()
        {
            return _db.Authors.ToList();
        }

        public bool UpdateAuthor(Author author)
        {
            var existingAuthor = _db.Authors.FirstOrDefault(x => x.Id == author.Id);
            if (existingAuthor == null)
                return false;

            existingAuthor.FirstName = author.FirstName;
            existingAuthor.LastName = author.LastName;
            existingAuthor.BirthDate = author.BirthDate;

            _db.SaveChanges();
            return true;
        }

        public bool DeleteAuthor(Guid id)
        {
            var author = _db.Authors.FirstOrDefault(x => x.Id == id);
            if (author == null)
                return false;

            _db.Authors.Remove(author);
            _db.SaveChanges();
            return true;
        }

        public bool HasBooks(Guid authorId)
        {
            return _db.Books.Any(b => b.AuthorId == authorId);
        }

        public List<Book> GetAllBooks()
        {
            return _db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .ToList();
        }

        public List<Book> GetAllAuthorBooks(Guid id)
        {
            return _db.Books
                .Include(b => b.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Where(b => b.AuthorId == id)
                .ToList();
        }
    }
}
