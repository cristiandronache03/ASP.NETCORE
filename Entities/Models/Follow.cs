using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FriendrBackend.Entities.Models
{
    public class Follow
    {
        public int Id { get; set; }

        public int User { get; set; }

        public int Follows{ get; set; }
    }
}
