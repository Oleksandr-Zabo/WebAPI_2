using WebAPI_2.Abstract;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreService(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public (bool Success, string ErrorMessage, int? Id) Save(CreateUpdateGenreRequest request)
        {
            // Check if genre with same name already exists
            var existingGenres = _genreRepository.GetAll();
            if (existingGenres.Any(g => g.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, $"Genre '{request.Name}' already exists!", null);
            }

            var genre = new Genre
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            };

            var result = _genreRepository.AddGenre(genre);
            return result 
                ? (true, null, genre.Id) 
                : (false, "Failed to add genre.", null);
        }

        public (bool Success, string ErrorMessage) Update(int id, CreateUpdateGenreRequest request)
        {
            // Check if genre exists
            var existingGenre = _genreRepository.GetById(id);
            if (existingGenre == null)
            {
                return (false, "Genre not found.");
            }

            // Check if another genre with same name exists
            var allGenres = _genreRepository.GetAll();
            if (allGenres.Any(g => g.Id != id && 
                g.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, $"Another genre with name '{request.Name}' already exists!");
            }

            var genre = new Genre
            {
                Id = id,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            };

            var result = _genreRepository.UpdateGenre(genre);
            return result 
                ? (true, null) 
                : (false, "Failed to update genre.");
        }

        public (bool Success, string ErrorMessage) Delete(int id)
        {
            // Check if genre exists
            var genre = _genreRepository.GetById(id);
            if (genre == null)
            {
                return (false, "Genre not found.");
            }

            // Check if genre is assigned to any books
            if (genre.BookGenres != null && genre.BookGenres.Any())
            {
                return (false, $"Cannot delete genre '{genre.Name}' because it is assigned to {genre.BookGenres.Count} book(s). Please remove the genre from all books first.");
            }

            // Prevent deletion of "Unknown" genre (Id = 1)
            if (genre.Id == 1)
            {
                return (false, "Cannot delete the 'Unknown' genre as it is a system genre.");
            }

            var result = _genreRepository.DeleteGenre(id);
            return result 
                ? (true, null) 
                : (false, "Failed to delete genre.");
        }

        public GenreDTO GetById(int id)
        {
            var genre = _genreRepository.GetById(id);
            if (genre == null)
                return null;

            return new GenreDTO
            {
                Id = genre.Id,
                Name = genre.Name,
                Description = genre.Description,
                BooksCount = genre.BookGenres?.Count ?? 0
            };
        }

        public List<GenreDTO> GetAll()
        {
            var genres = _genreRepository.GetAll();
            return genres.Select(g => new GenreDTO
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                BooksCount = g.BookGenres?.Count ?? 0
            }).ToList();
        }

    }
}
