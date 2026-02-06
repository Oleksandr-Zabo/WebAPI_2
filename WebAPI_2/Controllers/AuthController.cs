using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI_2.Abstract;
using WebAPI_2.Core;
using WebAPI_2.Models;
using WebAPI_2.Services;

namespace WebAPI_2.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtToken _jwtToken;

        public AuthController(IAuthService authService, IJwtToken jwtToken)
        {
            _authService = authService;
            _jwtToken = jwtToken;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and Password are required");
            }

            var (success, errorMessage, userId) = _authService.Register(
                request.Email, 
                request.Password,
                request.Name ?? request.Email.Split('@')[0],
                request.NickName ?? request.Email.Split('@')[0],
                Roles.User);

            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "User registered successfully", userId });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and Password are required");
            }

            var user = _authService.Authenticate(request.Email, request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new AuthResponse { AccessToken = _jwtToken.Create(user) });
        }

        [HttpPost("createAdmin")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult CreateAdmin([FromBody] RegisterRequest request)
        {
            var (success, errorMessage, userId) = _authService.Register(
                request.Email, 
                request.Password,
                request.Name ?? request.Email.Split('@')[0],
                request.NickName ?? request.Email.Split('@')[0],
                Roles.Admin);

            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Admin user created successfully", userId });
        }
    }
}

