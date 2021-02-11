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

    public class CommentController : ControllerBase
    {
        // comm
        private readonly FriendrContext _db;

        public CommentController(FriendrContext db)
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


        [HttpPost("addcomment")]
        public ActionResult<Comment> AddComment([FromBody] CommentPayload payload)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (payload.UserId != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var commentToAdd = new Comment
                    {
                        UserId = payload.UserId,
                        Text = payload.Text,
                        PostId = payload.PostId,
                        Username = payload.Username,
                    };

                    _db.Comments.Add(commentToAdd);
                    _db.SaveChanges();

                    return Ok(commentToAdd);
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

        
        [HttpPost("deletecomment")]
        public ActionResult DeleteComment(String UserId, String CommentId)
        {
            int id = Int32.Parse(CommentId);
            int uid = Int32.Parse(UserId);

            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                if (uid != validation)
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                try
                {
                    var commentToDelete = _db.Comments.Single(comment => id == comment.Id);

                    _db.Comments.Remove(commentToDelete);
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

        

        [HttpGet("getcomments")]
        public ActionResult<List<Comment>> GetComments(String Id)
        {
            var token = HttpContext.Request.Headers["token"].ToString();
            var validation = ValidateJwtToken(token);
            if (validation != null)
            {
                var id = int.Parse(Id);
                var commentsDb = _db.Comments.AsNoTracking();
                var commentQuery = commentsDb.Where(comment => id == comment.PostId);

                return commentQuery.ToList();
                
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

    }
}

