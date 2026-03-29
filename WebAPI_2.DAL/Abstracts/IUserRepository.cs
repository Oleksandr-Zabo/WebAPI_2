using WebAPI_2.DAL.Entities;

namespace WebAPI_2.DAL.Abstracts
{
    public interface IUserRepository
    {
        User GetById(Guid id);
        List<User> GetAll();
        bool AddUser(User user);
        bool AddSavedBook(Guid bookId, Guid userId);
        bool RemoveSavedBook(Guid bookId, Guid userId);
        List<Book> GetSavedBooks(Guid userId);
        bool UpdateUser(User user);
        bool DeleteUser(Guid id);
    }
}
