using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FriendrBackend.Entities;
using FriendrBackend.Entities.Models;
using FriendrBackend.Payloads;
using System.Text;
using static ViverBackend.Enums;
using Microsoft.IdentityModel.Tokens;

namespace FriendrBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // comm
        private readonly FriendrContext _db;

        public UserController(FriendrContext db)
        {
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

        [HttpGet]
        public ActionResult<List<User>> GetAll(int pageSize, int pageNumber, UsersSortType sortType)
        {
            var currentUser = HttpContext.User;

            if (currentUser.HasClaim(claim => claim.Type == "Role"))
            {
                var role = currentUser.Claims.FirstOrDefault(c => c.Type == "Role").Value;
                if (role == "Admin")
                {
                    // as no tracking for performance improvement when you do not need to track changes
                    var usersQuery = _db.Users.AsNoTracking();

                    if (sortType == UsersSortType.FirstNameAscendent)
                        usersQuery = usersQuery.OrderBy(u => u.FirstName);
                    else if (sortType == UsersSortType.FirstNameDescendent)
                        usersQuery = usersQuery.OrderByDescending(u => u.FirstName);
                    else if (sortType == UsersSortType.LastNameAscendent)
                        usersQuery = usersQuery.OrderBy(u => u.LastName);
                    else if (sortType == UsersSortType.LastNameDescendent)
                        usersQuery = usersQuery.OrderByDescending(u => u.LastName);
                    else
                        usersQuery = usersQuery.OrderBy(u => u.FirstName);

                    usersQuery = usersQuery
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);

                    var users = usersQuery.ToList();

                    return users;
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost]
        public ActionResult<User> Create([FromBody] UserPayload payload)
        {
            try
            {
                var userToAdd = new User
                {
                    FirstName = payload.FirstName,
                    LastName = payload.LastName,
                    Email = payload.Email,
                    Gender = payload.Gender
                };

                _db.Users.Add(userToAdd);
                _db.SaveChanges();

                return Ok(userToAdd);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);

            }
        }

        [HttpDelete]
        public ActionResult Delete(int Id)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (Id != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);

                try
                {
                    var userToDelete = _db.Users.Single(user => Id == user.Id);

                    _db.Users.Remove(userToDelete);
                    _db.SaveChanges();
                    return Ok(new { status = true });
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

        [HttpGet("getuserbyid")]
        public ActionResult<User> GetById(int Id)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                return _db.Users.Where(user => Id == user.Id).Single();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

        [HttpGet("searchuser")]
        public ActionResult<List<User>> Search(String name)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                var nume = name.Replace('+', ' ');
                var usersQuery = _db.Users.AsNoTracking();
                var users = _db.Users.AsNoTracking();

                usersQuery = users.Where(user => (user.FirstName + " " + user.LastName).StartsWith(nume));
                usersQuery = usersQuery.Concat(users.Where(user => (user.LastName + " " + user.FirstName).StartsWith(nume)));

                return usersQuery.ToList();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

    }


}
