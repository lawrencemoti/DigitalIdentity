using ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class IdentityDBContext: DbContext
    {
        public IdentityDBContext(DbContextOptions<IdentityDBContext> options) 
            : base(options)
        {
        }

        public DbSet<Identity> Identities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configuration if needed

            modelBuilder.Entity<Identity>(entity =>
            {
                entity.HasKey(e => e.IdentityNumber);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(13);
            });
        }
    }
}
