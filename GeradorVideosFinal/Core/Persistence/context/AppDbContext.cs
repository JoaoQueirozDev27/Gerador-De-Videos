using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Internal;

namespace Persistence.context
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=VideoGenerator.db");
        }

        public DbSet<Model> Models { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<ChannelSegment> ChannelSegments { get; set; } 
    }
}
