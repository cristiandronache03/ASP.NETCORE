using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FriendrBackend.Entities;
using FriendrBackend.Entities.Models;
using FriendrBackend.Payloads;
using BC = BCrypt.Net.BCrypt;

namespace ViverBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IConfiguration _config { get; }
        private readonly FriendrContext _db;

        public AccountController(FriendrContext db, IConfiguration configuration)
        {
            _config = configuration;
            _db = db;
        }

        public int? ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ThisismySecretKey");
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "Id").Value);

                return accountId;
            }
            catch
            {
                return null;
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterPayload registerPayload)
        {
            try
            {
                var existingUserWithMail = _db.Users
            .Any(u => u.Email == registerPayload.Email);

                if (existingUserWithMail)
                {
                    return BadRequest(new { status = false, message = "Email already exists" });
                }
                var userToCreate = new User
                {

                    Email = registerPayload.Email,
                    FirstName = registerPayload.FirstName,
                    LastName = registerPayload.LastName,
                    Gender = registerPayload.Gender,
                    PasswordHash = BC.HashPassword(registerPayload.Password),
                    Role = "SimpleUser",

                };

                _db.Users.Add(userToCreate);

                _db.SaveChanges();

                return Ok(new { status = true, user = userToCreate });
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginPayload loginPayload)
        {
            var foundUser = _db.Users
                .SingleOrDefault(u => u.Email == loginPayload.Email);

            if (foundUser != null)
            {
                if (BC.Verify(loginPayload.Password, foundUser.PasswordHash))
                {
                    var tokenString = GenerateJSONWebToken(foundUser);
                    return Ok(new { status = true, token = tokenString, user = foundUser });
                }

                return BadRequest(new { status = false, message = "Wrong password or email" });
            }
            else
            {
                return BadRequest(new { status = false, message = "No user with this email found" });
            }

        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("Id", user.Id.ToString()),
                new Claim("Role", user.Role),
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddDays(30),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("updateinfo")]
        public ActionResult SetInfo([FromBody] UserPayload payload)
        {
            int uid = Int32.Parse(payload.Id);
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (uid != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var user = _db.Users.Single(user => uid == user.Id);

                    user.ProfilePic = payload.ProfilePic;
                    user.Age = Int32.Parse(payload.Age);

                    _db.SaveChanges();
                    return Ok(user);
                }
                catch (Exception)
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }

        }
    
    }

}
