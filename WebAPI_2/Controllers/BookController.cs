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
        private readonly IBookAPIService _bookAPIService;

        public BookController(IBookService bookService, IBookAPIService bookAPIService)
        {
            _bookService = bookService;
            _bookAPIService = bookAPIService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var books = _bookService.GetAll();
            return Ok(books);
        }

        [HttpGet("full")]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllFull()
        {
            var books = await _bookAPIService.GetAllFullBooksAsync();
            return Ok(books);
        }

        [HttpGet("cover")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookCoverByTitle([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest(new { message = "Title parameter is required." });

            var (coverUrl, downloadCount) = await _bookAPIService.GetBookCoverByTitleAsync(title);

            if (coverUrl == null)
                return NotFound(new { message = $"Cover not found for book '{title}'." });

            return Ok(new { title, coverUrl, downloadCount });
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
        public IActionResult Create([FromBody] CreateBookRequest request)
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
        public IActionResult Update(Guid id, [FromBody] UpdateBookRequest request)
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
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchHybrid([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query parameter is required" });

            var results = await _bookAPIService.SearchBookHybridAsync(query);
            return Ok(results);
        }

        [HttpGet("search/external")]
        [ProducesResponseType(typeof(GutendexResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchExternal(
            [FromQuery] string search = null,
            [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(search))
                return BadRequest(new { message = "Search parameter is required" });

            var searchParams = new GutendexSearchParams
            {
                Search = search,
                Page = page
            };

            var results = await _bookAPIService.SearchBooksAsync(searchParams);
            return Ok(results);
        }

        [HttpPost("import/{gutendexId:int}")]
        [ProducesResponseType(typeof(BookDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportFromGutendex(int gutendexId)
        {
            var (success, errorMessage, bookId) = await _bookAPIService.ImportBookAsync(gutendexId);

            if (!success)
                return BadRequest(new { message = errorMessage });

            var importedBook = _bookService.GetById(bookId.Value);
            return CreatedAtAction(nameof(GetById), new { id = bookId.Value }, importedBook);
        }

        [HttpGet("external/{gutendexId:int}")]
        [ProducesResponseType(typeof(GutendexBook), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExternalBook(int gutendexId)
        {
            var book = await _bookAPIService.GetBookByIdAsync(gutendexId);

            if (book == null)
                return NotFound(new { message = "Book not found in Gutendex API" });

            return Ok(book);
        }
    }
}
