using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_2.Abstract;
using WebAPI_2.Core;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var books = _bookService.GetAll();
            return Ok(books);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(Guid id)
        {
            var book = _bookService.GetById(id);

            if (book == null)
                return NotFound(new { message = "Book not found." });

            return Ok(book);
        }

        [HttpGet("filter")]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        public IActionResult GetFiltered(
            [FromQuery] string? searchTitle = null,
            [FromQuery] Guid? filterAuthorId = null,
            [FromQuery] int? filterGenreId = null,
            [FromQuery] string sortBy = "Title",
            [FromQuery] string sortOrder = "ASC")
        {
            var books = _bookService.GetFiltered(searchTitle, filterAuthorId, filterGenreId, sortBy, sortOrder);
            return Ok(books);
        }

        [HttpGet("by-genre/{genreId:int}")]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        public IActionResult GetByGenre(int genreId)
        {
            var books = _bookService.GetByGenre(genreId);
            return Ok(books);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BookDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Create([FromBody] CreateUpdateBookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage, id) = _bookService.Save(request);

            if (!success)
                return BadRequest(new { message = errorMessage });

            var createdBook = _bookService.GetById(id.Value);
            return CreatedAtAction(nameof(GetById), new { id = id.Value }, createdBook);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BookDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Update(Guid id, [FromBody] CreateUpdateBookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage) = _bookService.Update(id, request);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });

                return BadRequest(new { message = errorMessage });
            }

            var updatedBook = _bookService.GetById(id);
            return Ok(updatedBook);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Delete(Guid id)
        {
            var (success, errorMessage) = _bookService.Delete(id);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });

                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Book deleted successfully." });
        }

        [HttpPost("{id:guid}/genres")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AssignGenres(Guid id, [FromBody] List<int> genreIds)
        {
            var (success, errorMessage) = _bookService.AssignGenres(id, genreIds);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });

                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Genres assigned successfully." });
        }
    }
}
