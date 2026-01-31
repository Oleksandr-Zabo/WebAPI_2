using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Abstract
{
    public interface IAuthorService
    {
        (bool Success, string ErrorMessage, Guid? Id) Save(CreateUpdateAuthorRequest request);
        (bool Success, string ErrorMessage) Update(Guid id, CreateUpdateAuthorRequest request);
        (bool Success, string ErrorMessage) Delete(Guid id);
        AuthorDTO GetById(Guid id);
        List<AuthorDTO> GetAll();
        List<AuthorDTO> GetAuthorsWithBookCount();
        List<BookDTO> GetAllAuthorBooks(Guid id);
        List<BookDTO> GetAllBooks();
    }
}
