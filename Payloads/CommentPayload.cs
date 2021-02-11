using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FriendrBackend.Entities.Models;

namespace FriendrBackend.Payloads
{
    public class CommentPayload
    {
        public String Username { get; set; }

        public int UserId { get; set; }

        public int PostId { get; set; }

        public String Text { get; set; }
    }
}
