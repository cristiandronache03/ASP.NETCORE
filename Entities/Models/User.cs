using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FriendrBackend.Entities.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Gender { get; set; }

        public string ProfilePic { get; set; }

        public int Age { get; set; }

        public string Role { get; set; }

       // public List<Post> Posts { get; set; }

       // public List<Follow> Follows { get; set; }

       // public List<Follow> Followed { get; set; }

    }
}
