using Microsoft.EntityFrameworkCore;
using poll_api.Domain.Entities;

namespace poll_api.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Poll> Polls { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Vote> Votes { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraint for voting
            modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.PollId, v.UserId })
                .IsUnique();

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );

            // Seed Admin User
            // Note: Password is 'Admin@123' hashed with BCrypt. 
            // We'll generate a dummy hash here for seeding, but in a real scenario, use a secure initial hash.
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@pollapp.com",
                    PasswordHash = adminPasswordHash,
                    RoleId = 1
                }
            );
        }
    }
}
