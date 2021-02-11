using Backend.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FriendrBackend.Entities.Models
{
    public class Post
    {
        public int Id { get; set; }

        public String Title { get; set; }

        public String Text { get; set; }

        public int UserId { get; set; }

        public String Username { get; set; }

        public int Likes { get; set; }

        public String ImgUrl { get; set; }

       //  public User User { get; set; }

       // public List<Like> Liked { get; set; }

    }
}
