
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using scopic_test_server.Helper;
using scopic_test_server.Interface;

namespace scopic_test_server
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _repository;
        private readonly AppSettings _appSettings;
        public UserController(IUserRepository repository, IOptions<AppSettings> appSettings)
        {
            _repository = repository;
            _appSettings = appSettings.Value;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public IActionResult Authenticate([FromBody] LoginDto Login)
        {
            var user = _repository.Authenticate(Login.Username, Login.Password);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            string tokenString = GenerateToken(user);

            // return basic user info and authentication token
            return Ok(new
            {
                Id = user.UserId,
                Username = user.Username,
                Token = tokenString,
                Role = user.Role
            });
        }

        private string GenerateToken(Data.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role,user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet]
        [Route("{UserId}")]
        public IActionResult GetUser(Guid UserId)
        {
            var user = _repository.GetUser(UserId);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [Authorize(Roles = Role.User)]
        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
            var userId = HttpContext.User.Identity.Name;
            var user = _repository.GetUserProfile(Guid.Parse(userId));
            if (user == null)
                return NotFound();
            return Ok(user);
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] LoginDto Register)
        {
            var user = _repository.Register(Register.Username, Register.Password);
            if (user == null)
                return BadRequest(new { error = "Registration Failed" });
            string tokenString = GenerateToken(user);
            return Ok(new
            {
                Id = user.UserId,
                Username = user.Username,
                Token = tokenString,
                Role = user.Role
            });
        }

    }

}