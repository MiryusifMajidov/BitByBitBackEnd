using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace BitByBit.DataAccess.Context
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Custom DbSets (gələcək üçün)
        // public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity base configuration
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                // Custom properties configuration
                entity.Property(e => e.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.LastName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedDate)
                      .IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.UpdatedDate)
                      .IsRequired(false);

                entity.Property(e => e.LastLoginDate)
                      .IsRequired(false);

                entity.Property(e => e.IsDeleted)
                      .IsRequired()
                      .HasDefaultValue(false);

                // Enum conversions
                entity.Property(e => e.Role)
                      .HasConversion<int>()
                      .IsRequired(); // UserRole.User = 1

                entity.Property(e => e.Status)
                      .HasConversion<int>()
                      .IsRequired(); // UserStatus.Active = 1

                // Soft Delete Query Filter
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Custom indexes
                entity.HasIndex(e => e.CreatedDate)
                      .HasDatabaseName("IX_Users_CreatedDate");

                entity.HasIndex(e => e.Role)
                      .HasDatabaseName("IX_Users_Role");
            });

            // Identity table names (opsional)
            modelBuilder.Entity<User>().ToTable("Users");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto UpdatedDate
            var entries = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedDate = DateTime.Now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}