using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebAPI_2.Config;
using WebAPI_2.Core;
using WebAPI_2.Models;
using LoginRequest = WebAPI_2.Models.LoginRequest;
using RegisterRequest = WebAPI_2.Models.RegisterRequest;

namespace WebAPI_2.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController: ControllerBase
    {
        private readonly UsersStore _users;
        private readonly JwtToken _jwt;

        public AuthController(UsersStore users, IConfiguration config)
        {
            _users = users;
            _users.Create("admin", "admin", Roles.Admin);
            _jwt = new JwtToken(config);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and Password are required");
            }

            var ok = _users.Create(request.Email, request.Password, Roles.User);
            return ok 
                ? Ok() 
                : Conflict("User already exists");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and Password are required");
            }

            var user = _users.Find(request.Email);

            if (!_users.CheckPassword(user, request.Password))
            {
                return Unauthorized();
            }

            return Ok(new AuthResponse { AccessToken = _jwt.Create(user)} );

        }

        [HttpPost("createAdmin")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult CreateAdmin(RegisterRequest request)
        {
            var ok = _users.Create(request.Email, request.Password, Roles.Admin);
            return ok
                ? Ok()
                : Conflict("User already exists");
            }
    }
}
