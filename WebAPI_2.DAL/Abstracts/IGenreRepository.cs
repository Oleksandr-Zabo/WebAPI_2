using WebAPI_2.DAL.Entities;

namespace WebAPI_2.DAL.Abstracts
{
    public interface IGenreRepository
    {
        List<Genre> GetAll();
        Genre GetById(int id);
        bool AddGenre(Genre genre);
        bool UpdateGenre(Genre genre);
        bool DeleteGenre(int id);
    }
}
