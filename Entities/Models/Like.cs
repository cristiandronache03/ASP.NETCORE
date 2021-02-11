using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Entities.Models
{
    public class Like
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

    }
}
