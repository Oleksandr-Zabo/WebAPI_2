using Microsoft.AspNetCore.Mvc;
using WebAPI_2.Abstract;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GenreDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var genres = _genreService.GetAll();
            return Ok(genres);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(GenreDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(int id)
        {
            var genre = _genreService.GetById(id);
            
            if (genre == null)
                return NotFound(new { message = "Genre not found." });

            return Ok(genre);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GenreDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] CreateUpdateGenreRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage, id) = _genreService.Save(request);

            if (!success)
                return BadRequest(new { message = errorMessage });

            var createdGenre = _genreService.GetById(id.Value);
            return CreatedAtAction(nameof(GetById), new { id = id.Value }, createdGenre);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(GenreDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(int id, [FromBody] CreateUpdateGenreRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage) = _genreService.Update(id, request);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            var updatedGenre = _genreService.GetById(id);
            return Ok(updatedGenre);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var (success, errorMessage) = _genreService.Delete(id);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Genre deleted successfully." });
        }
    }
}
