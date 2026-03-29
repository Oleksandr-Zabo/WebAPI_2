using WebAPI_2.DAL.Entities;

namespace WebAPI_2.DAL.Abstracts
{
    public interface IBookRepository
    {
        Book GetById(Guid id);
        List<Book> GetAll();
        List<Book> GetFiltered(string searchTitle, Guid? filterAuthorId, string sortBy = "Title", string sortOrder = "ASC");
        bool AddBook(Book book);
        bool UpdateBook(Book book);
        bool DeleteBook(Guid id);
        bool BookExistsByISBN(string isbn);
        bool BookExistsByISBN(string isbn, Guid excludeBookId);
        bool BookExistsByTitle(string title, Guid authorId);
        bool BookExistsByTitle(string title, Guid authorId, Guid excludeBookId);
        bool AssignGenresToBook(Guid bookId, List<int> genreIds);
        List<Book> GetBooksByGenre(int genreId);
        List<Genre> GetBookGenres(Guid bookId);
    }
}
