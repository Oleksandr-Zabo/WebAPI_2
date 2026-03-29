using WebAPI_2.DAL.Entities;

namespace WebAPI_2.DAL.Abstracts
{
    public interface IAuthorRepository
    {
        bool AddAuthor(Author author);
        Author GetById(Guid id);
        List<Author> GetAll();
        List<Book> GetAllBooks();
        List<Book> GetAllAuthorBooks(Guid id);
        bool UpdateAuthor(Author author);
        bool DeleteAuthor(Guid id);
        bool HasBooks(Guid authorId);
    }
}