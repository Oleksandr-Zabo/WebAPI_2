using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Abstract
{
    public interface IGenreService
    {
        (bool Success, string ErrorMessage, int? Id) Save(CreateUpdateGenreRequest request);
        (bool Success, string ErrorMessage) Update(int id, CreateUpdateGenreRequest request);
        (bool Success, string ErrorMessage) Delete(int id);
        GenreDTO GetById(int id);
        List<GenreDTO> GetAll();
    }
}
