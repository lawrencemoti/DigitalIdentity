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
        public DbSet<IdentityVerification> Verifications => Set<IdentityVerification>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            // Additional configuration if needed

            b.Entity<Identity>(e => {
                e.HasKey(x => x.Id);
                e.HasIndex(e => e.IdentityNumber).IsUnique();
                e.Property(x => x.Status).HasConversion<string>();
                e.HasMany(x => x.Verifications).WithOne(v => v.Identity).HasForeignKey(v => v.IdentityId);
            });

            b.Entity<IdentityVerification>(e => {
                e.HasKey(x => x.Id);
                e.Property(x => x.Status).HasConversion<string>();
            });
        }
    }
}
