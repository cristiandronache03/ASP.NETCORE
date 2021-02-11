using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FriendrBackend.Entities.Models;

namespace FriendrBackend.Payloads
{
    public class PostPayload
    {
        public String Title { get; set; }

        public string Text { get; set; }

        public String Username { get; set; }

        public int UserId { get; set; }

        public String ImgUrl { get; set; }


    }
}
