using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Backend.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FriendrBackend.Entities;
using FriendrBackend.Entities.Models;
using FriendrBackend.Payloads;
using Microsoft.IdentityModel.Tokens;

namespace FriendrBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PostController : ControllerBase
    {
        // comm
        private readonly FriendrContext _db;

        public PostController(FriendrContext db)
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


        [HttpPost("addpost")]
        public ActionResult<Post> AddPost([FromBody] PostPayload payload)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (payload.UserId != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var postToAdd = new Post
                    {
                        Title = payload.Title,
                        Text = payload.Text,
                        Username = payload.Username,
                        UserId = payload.UserId,
                        ImgUrl = payload.ImgUrl,
                    };

                    _db.Posts.Add(postToAdd);
                    _db.SaveChanges();

                    return Ok(postToAdd);
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

        [HttpPost("deletepost")]
        public ActionResult DeletePost(String UserId, String PostId)
        {
            int id = Int32.Parse(PostId);
            int uid = Int32.Parse(UserId);

            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (uid != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var postToDelete = _db.Posts.Single(post => id == post.Id);

                    _db.Posts.Remove(postToDelete);
                    _db.SaveChanges();

                    return Ok(postToDelete);
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

        [HttpPost("likepost")]
        public ActionResult LikePost(String UserId, String PostId)
        {
            int uid = Int32.Parse(UserId);
            int pid = Int32.Parse(PostId);

            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (uid != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);

                var likeQuery = _db.Likes.Where(like => pid == like.PostId && uid == like.UserId);

                try
                {

                    var postToLike = _db.Posts.Single(post => pid == post.Id);

                    if (!likeQuery.Any())
                    {
                        postToLike.Likes++;

                        var likeToAdd = new Like
                        {
                            UserId = uid,
                            PostId = pid
                        };

                        _db.Likes.Add(likeToAdd);

                        _db.SaveChanges();

                        return Ok(new { status = true });
                    }
                    else
                    {
                        postToLike.Likes--;

                        _db.Likes.Remove(likeQuery.Single());

                        _db.SaveChanges();

                        return Ok(new { status = true });

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

        [HttpGet("getposts")]
        public ActionResult<List<Post>> GetPosts(String Id, int pageSize, int pageNumber)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                int id = Int32.Parse(Id);
                if(id!=validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                var followQuery = _db.Follows.AsNoTracking();
                var follows = followQuery.Where(follow => id == follow.User);
                var postsDb = _db.Posts.AsNoTracking();
                var postQuery = _db.Posts.AsNoTracking();
                if (!follows.Any())
                {
                    postQuery = postQuery.Where(post => id == post.UserId);
                    postQuery = postQuery
                       .Skip((pageNumber - 1) * pageSize)
                       .Take(pageSize);
                    return postQuery.ToList();
                }
                Boolean first = true;
                foreach (Follow f in follows)
                {
                    int n = f.Follows;
                    if (first)
                    {
                        postQuery = postQuery.Where(post => n == post.UserId);
                        first = false;
                    }
                    else
                        postQuery = postQuery.Concat(postsDb.Where(post => n == post.UserId));
                }

                postQuery = postQuery.Concat(postsDb.Where(post => id == post.UserId));
                postQuery = postQuery
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);

                return postQuery.ToList();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

        [HttpGet("getpostsbyid")]
        public ActionResult<List<Post>> GetPostsById(String Id)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                int id = Int32.Parse(Id);
                var postQuery = _db.Posts.AsNoTracking();
                postQuery = postQuery.Where(post => id == post.UserId);
                return postQuery.ToList();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

        [HttpGet("getpost")]
        public ActionResult<Post> GetPostd(String Id)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                int id = Int32.Parse(Id);
                var postQuery = _db.Posts.AsNoTracking();
                postQuery = postQuery.Where(post => id == post.Id);
                return postQuery.Single();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

    }
}

