using Microsoft.EntityFrameworkCore;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;

namespace WebAPI_2.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public bool AddUser(User user)
        {
            var res = _db.Users.Add(user) != null;
            _db.SaveChanges();
            return res;
        }

        public User GetById(Guid id)
        {
            return _db.Users
                .Include(u => u.SavedBooks)
                    .ThenInclude(b => b.Author)
                .Include(u => u.SavedBooks)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .FirstOrDefault(x => x.Id == id);
        }

        public List<User> GetAll() { 
            return _db.Users.ToList();
        }

        public bool AddSavedBook(Guid bookId, Guid userId)
        {
            var user = _db.Users.Include(u => u.SavedBooks).FirstOrDefault(u => u.Id == userId);
            var book = _db.Books.FirstOrDefault(b => b.Id == bookId);

            if (user == null || book == null)
                return false;

            if (user.SavedBooks == null)
                user.SavedBooks = new List<Book>();

            if (!user.SavedBooks.Any(b => b.Id == bookId))
            {
                user.SavedBooks.Add(book);
                _db.SaveChanges();
                return true;
            }

            return false;
        }

        public bool RemoveSavedBook(Guid bookId, Guid userId)
        {
            var user = _db.Users.Include(u => u.SavedBooks).FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return false;

            var bookToRemove = user.SavedBooks?.FirstOrDefault(b => b.Id == bookId);

            if (bookToRemove == null)
                return false;

            user.SavedBooks.Remove(bookToRemove);
            _db.SaveChanges();
            return true;
        }

        public List<Book> GetSavedBooks(Guid userId)
        {
            var user = _db.Users
                .Include(u => u.SavedBooks)
                    .ThenInclude(b => b.Author)
                .Include(u => u.SavedBooks)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .FirstOrDefault(u => u.Id == userId);
            
            return user?.SavedBooks?.ToList() ?? new List<Book>();
        }

        public bool UpdateUser(User user)
        {
            var existingUser = _db.Users.FirstOrDefault(u => u.Id == user.Id);
            
            if (existingUser == null)
                return false;

            existingUser.Name = user.Name;
            existingUser.NickName = user.NickName;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            existingUser.IsAdmin = user.IsAdmin;

            _db.SaveChanges();
            return true;
        }

        public bool DeleteUser(Guid id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
                return false;

            _db.Users.Remove(user);
            _db.SaveChanges();
            return true;
        }
    }
}
