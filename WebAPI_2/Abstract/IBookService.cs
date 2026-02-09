using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Abstract
{
    public interface IBookService
    {
        (bool Success, string ErrorMessage, Guid? Id) Save(CreateBookRequest request);
        (bool Success, string ErrorMessage) Update(Guid id, UpdateBookRequest request);
        (bool Success, string ErrorMessage) Delete(Guid id);
        (bool Success, string ErrorMessage) AssignGenres(Guid bookId, List<int> genreIds);
        BookDTO GetById(Guid id);
        List<BookDTO> GetAll();
        List<BookDTO> GetFiltered(string searchTitle, Guid? filterAuthorId, int? filterGenreId, string sortBy = "Title", string sortOrder = "ASC");
        List<BookDTO> GetByGenre(int genreId);
    }
}