using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI_2.DAL.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly AppDbContext _db;

        public GenreRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<Genre> GetAll()
        {
            return _db.Genres.OrderBy(g => g.Name).ToList();
        }

        public Genre GetById(int id)
        {
            return _db.Genres
                .Include(g => g.BookGenres)
                .ThenInclude(bg => bg.Book)
                .FirstOrDefault(g => g.Id == id);
        }

        public bool AddGenre(Genre genre)
        {
            var res = _db.Genres.Add(genre) != null;
            _db.SaveChanges();
            return res;
        }

        public bool UpdateGenre(Genre genre)
        {
            var existing = _db.Genres.FirstOrDefault(g => g.Id == genre.Id);
            if (existing == null)
                return false;

            existing.Name = genre.Name;
            existing.Description = genre.Description;
            _db.SaveChanges();
            return true;
        }

        public bool DeleteGenre(int id)
        {
            var genre = _db.Genres.FirstOrDefault(g => g.Id == id);
            if (genre == null)
                return false;

            _db.Genres.Remove(genre);
            _db.SaveChanges();
            return true;
        }
    }
}
