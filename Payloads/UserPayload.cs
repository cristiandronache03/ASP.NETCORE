using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FriendrBackend.Payloads
{
    public class UserPayload
    {
        public string? Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Gender { get; set; }

        public string? Email { get; set; }

        public string? Age { get; set; }
        
        public string? ProfilePic { get; set; }

    }
}
