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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserDTO>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById(Guid id)
        {
            var user = _userService.GetById(id);
            
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Register([FromBody] CreateUpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage, id) = _userService.Save(request);

            if (!success)
                return BadRequest(new { message = errorMessage });

            var createdUser = _userService.GetById(id.Value);
            return CreatedAtAction(nameof(GetById), new { id = id.Value }, createdUser);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, [FromBody] CreateUpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, errorMessage) = _userService.Update(id, request);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            var updatedUser = _userService.GetById(id);
            return Ok(updatedUser);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Delete(Guid id)
        {
            var (success, errorMessage) = _userService.Delete(id);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "User deleted successfully." });
        }

        [HttpGet("{userId:guid}/saved-books")]
        [ProducesResponseType(typeof(List<BookDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSavedBooks(Guid userId)
        {
            var user = _userService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var savedBooks = _userService.GetSavedBooks(userId);
            return Ok(savedBooks);
        }

        [HttpPost("{userId:guid}/saved-books/{bookId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddSavedBook(Guid userId, Guid bookId)
        {
            var (success, errorMessage) = _userService.AddSavedBook(userId, bookId);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Book added to saved list successfully." });
        }

        [HttpDelete("{userId:guid}/saved-books/{bookId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveSavedBook(Guid userId, Guid bookId)
        {
            var (success, errorMessage) = _userService.RemoveSavedBook(userId, bookId);

            if (!success)
            {
                if (errorMessage.Contains("not found"))
                    return NotFound(new { message = errorMessage });
                
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Book removed from saved list successfully." });
        }
    }
}
