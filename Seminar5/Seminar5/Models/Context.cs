using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // Добавьте эту директиву

namespace Seminar5.Models
{
    public partial class Context : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(LoggerFactory.Create(builder =>
                {
                    builder
                        .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                        .AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning)
                        .AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.Warning)
                        .AddConsole(); // Теперь будет работать
                }))
                .UseLazyLoadingProxies()
                .UseNpgsql("Host=localhost;Port=5436;Username=postgres;Password=Example;Database=NetAppSem5");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.FromMessages)
                    .HasForeignKey(d => d.FromUserId);

                entity.HasOne(d => d.ToUser)
                    .WithMany(p => p.ToMessages)
                    .HasForeignKey(d => d.ToUserId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}