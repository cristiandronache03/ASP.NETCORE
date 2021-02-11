using System;
using System.Collections.Generic;
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
using static ViverBackend.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FriendrBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        // comm
        private readonly FriendrContext _db;

        public FollowController(FriendrContext db)
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


        [HttpPost("followuser")]
        public ActionResult FollowUser(int id1, int id2)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (id1 != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var followQuery = _db.Follows.Where(follow => id1 == follow.User && id2 == follow.Follows);

                    if (!followQuery.Any())
                    {

                        var followToAdd = new Follow
                        {
                            User = id1,
                            Follows = id2
                        };

                        _db.Follows.Add(followToAdd);
                        _db.SaveChanges();

                        return Ok(new { message = "isFollowed" });
                    }
                    else
                    {
                        _db.Follows.Remove(followQuery.Single());
                        _db.SaveChanges();

                        return Ok(new { message = "isNotFollowed" });
                    }
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

        [HttpGet("getfollowstatus")]
        public ActionResult GetFollowStatus(int id1, int id2)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (id1 != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var followQuery = _db.Follows.Where(follow => id1 == follow.User && id2 == follow.Follows);

                    if (!followQuery.Any())
                    {
                        return Ok(new { message = "isFollowed" });
                    }
                    else
                    {
                        return Ok(new { message = "isNotFollowed" });
                    }
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

        [HttpGet("getfollowers")]
        public ActionResult<List<User>> GetFollowers(string id)
        {
            int uid = Int32.Parse(id);
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (uid != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);

                var followQuery = _db.Follows.AsNoTracking();
                var follows = followQuery.Where(follow => uid == follow.Follows);
                if (!follows.Any())
                {
                    return null;
                }
                var usersDb = _db.Users.AsNoTracking();
                var usersQuery = _db.Users.AsNoTracking();
                Boolean first = true;
                foreach (Follow f in follows)
                {
                    int n = f.User;
                    if (first)
                    {
                        usersQuery = usersQuery.Where(user => n == user.Id);
                        first = false;
                    }
                    else
                        usersQuery = usersQuery.Concat(usersDb.Where(user => n == user.Id));
                }
                return usersQuery.ToList();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }
    }


}
