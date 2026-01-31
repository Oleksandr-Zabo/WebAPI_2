using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Abstract
{
    public interface IUserService
    {
        (bool Success, string ErrorMessage, Guid? Id) Save(CreateUpdateUserRequest request);
        (bool Success, string ErrorMessage) Update(Guid id, CreateUpdateUserRequest request);
        (bool Success, string ErrorMessage) Delete(Guid id);
        UserDTO GetById(Guid id);
        List<UserDTO> GetAll();
        List<BookDTO> GetSavedBooks(Guid userId);
        (bool Success, string ErrorMessage) AddSavedBook(Guid userId, Guid bookId);
        (bool Success, string ErrorMessage) RemoveSavedBook(Guid userId, Guid bookId);
    }
}
