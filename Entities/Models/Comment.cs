using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FriendrBackend.Entities.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public String Username { get; set; }

        public int UserId { get; set; }

        public int PostId { get; set; }

        public String Text { get; set; }

    }
}
