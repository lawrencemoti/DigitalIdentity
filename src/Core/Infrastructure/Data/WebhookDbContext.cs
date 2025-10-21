using ApplicationCore.Models.Webhook;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class WebhookDbContext: DbContext
    {
        public WebhookDbContext(DbContextOptions<WebhookDbContext> options) : 
            base(options) { }

        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
        public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Vendor>().Property(p => p.CreatedAt).HasColumnType("datetime(6)");
            b.Entity<Vendor>().Property(p => p.UpdatedAt).HasColumnType("datetime(6)");


            b.Entity<Subscription>()
            .HasIndex(s => new { s.VendorId, s.EventType })
            .IsUnique();
            b.Entity<Subscription>().Property(p => p.CreatedAt).HasColumnType("datetime(6)");


            b.Entity<WebhookEvent>().Property(p => p.CreatedAt).HasColumnType("datetime(6)");
            b.Entity<WebhookEvent>().Property(p => p.UpdatedAt).HasColumnType("datetime(6)");
            b.Entity<WebhookEvent>().Property(p => p.AvailableAt).HasColumnType("datetime(6)");
            b.Entity<WebhookEvent>().Property(p => p.Status).HasConversion<string>();
            b.Entity<WebhookEvent>().HasIndex(p => new { p.Status, p.AvailableAt });


            b.Entity<DeliveryAttempt>().Property(p => p.CreatedAt).HasColumnType("datetime(6)");
        }
    }
}
