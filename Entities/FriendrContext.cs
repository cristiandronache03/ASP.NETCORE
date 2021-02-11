using Backend.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FriendrBackend.Entities.Models;

namespace FriendrBackend.Entities
{
    public class FriendrContext : DbContext
    {
        public FriendrContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Follow> Follows { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<Comment> Comments { get; set; }

    }


}
