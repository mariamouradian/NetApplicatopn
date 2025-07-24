using Microsoft.EntityFrameworkCore;

namespace Seminar5.Models
{
    public class Context : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;

        public Context(DbContextOptions<Context> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasOne(m => m.FromUser)
                .WithMany(u => u.FromMessages)
                .HasForeignKey(m => m.FromUserId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ToUser)
                .WithMany(u => u.ToMessages)
                .HasForeignKey(m => m.ToUserId);
        }
    }
}