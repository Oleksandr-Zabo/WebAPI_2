using Microsoft.AspNetCore.Mvc;
using WebAPI_2.Abstract;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AuthorDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var authors = _authorService.GetAll();
            return Ok(authors);
        }

        [HttpGet("with-book-count")]
        [ProducesResponseType(typeof(List<AuthorDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAuthorsWithBookCount()
        {
            var authors = _authorService.GetAuthorsWithBookCount();
            return Ok(authors);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AuthorDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(Guid id)
        {
            var author = _authorService.GetById(id);
            
            if (author == null)
                return NotFound(new { message = "Author not found." });

            return Ok(author);
        }

        [HttpGet("{id:guid}/books")]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAuthorBooks(Guid id)
        {
            var author = _authorService.GetById(id);
            if (author == null)
                return NotFound(new { message = "Author not found." });

            var books = _authorService.GetAllAuthorBooks(id);
            return Ok(books);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AuthorDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Create([FromBody] CreateUpdateAuthorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage, id) = _authorService.Save(request);

            if (!success)
                return BadRequest(new { message = errorMessage });

            var createdAuthor = _authorService.GetById(id.Value);
            return CreatedAtAction(nameof(GetById), new { id = id.Value }, createdAuthor);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AuthorDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, [FromBody] CreateUpdateAuthorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage) = _authorService.Update(id, request);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            var updatedAuthor = _authorService.GetById(id);
            return Ok(updatedAuthor);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id)
        {
            var (success, errorMessage) = _authorService.Delete(id);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Author deleted successfully." });
        }
    }
}
