using DSDRoute.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DSDRoute.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DSD Entities
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<CreditBill> CreditBills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Shop)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(o => o.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.SalesRep)
                    .WithMany()
                    .HasForeignKey(o => o.SalesRepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.DeliveredByUser)
                    .WithMany()
                    .HasForeignKey(o => o.DeliveredById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(oi => oi.Price)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.RecordedBy)
                    .WithMany()
                    .HasForeignKey(p => p.RecordedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.VerifiedBy)
                    .WithMany()
                    .HasForeignKey(p => p.VerifiedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(p => p.Amount)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Return>(entity =>
            {
                entity.HasOne(r => r.Shop)
                    .WithMany(s => s.Returns)
                    .HasForeignKey(r => r.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Returns)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Order)
                    .WithMany(o => o.Returns)
                    .HasForeignKey(r => r.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.ProcessedBy)
                    .WithMany()
                    .HasForeignKey(r => r.ProcessedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.ApprovedBy)
                    .WithMany()
                    .HasForeignKey(r => r.ApprovedById)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(r => r.RefundAmount)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<CreditBill>(entity =>
            {
                entity.HasOne(cb => cb.Shop)
                    .WithMany(s => s.CreditBills)
                    .HasForeignKey(cb => cb.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cb => cb.SalesRep)
                    .WithMany()
                    .HasForeignKey(cb => cb.SalesRepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(cb => cb.CreditAmount)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(p => p.SKU)
                    .IsUnique();
            });

            modelBuilder.Entity<Shop>(entity =>
            {
                entity.HasIndex(s => s.Name);
            });
        }
    }
}