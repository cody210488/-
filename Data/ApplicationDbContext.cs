using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using 打球啊.Models;
namespace 打球啊.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }

        public DbSet<Court> Courts { get; set; }
        public DbSet<Event> Events { get; set; }

        
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<CourtComment> CourtComments { get; set; }
        public DbSet<EventMessage> EventMessages { get; set; }

    }
}
