using BitByBit.Entities.Models;
using BitByBit.Entities.Enums;
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

        // Custom DbSets
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

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

                // Identity table name
                entity.ToTable("Users");
            });

            // Room entity configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(e => e.RoomName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Description)
                      .HasMaxLength(500);
                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
                entity.Property(e => e.Capacity)
                      .IsRequired()
                      .HasDefaultValue(2);
                entity.Property(e => e.RoomCount)
                      .IsRequired()
                      .HasDefaultValue(1);
                entity.Property(e => e.BathCount)
                      .IsRequired()
                      .HasDefaultValue(1);
                entity.Property(e => e.Wifi)
                      .IsRequired()
                      .HasDefaultValue(false);

                // Enum conversion - Default value enum type ilə
                entity.Property(e => e.Role)
                      .HasConversion<int>()
                      .IsRequired()
                      .HasDefaultValue(RoomType.Standart); // ✅ Enum value istifadə et

                // Soft Delete Query Filter
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Indexes
                entity.HasIndex(e => e.RoomName)
                      .HasDatabaseName("IX_Rooms_RoomName");
                entity.HasIndex(e => e.Role)
                      .HasDatabaseName("IX_Rooms_RoomType");
                entity.HasIndex(e => e.Price)
                      .HasDatabaseName("IX_Rooms_Price");
            });

            // Services entity configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.ServiceName)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Description)
                      .HasMaxLength(500);
                entity.Property(e => e.IconUrl)
                      .HasMaxLength(255);

                // Enum conversion - Default value enum type ilə
                entity.Property(e => e.Role)
                      .HasConversion<int>()
                      .IsRequired()
                      .HasDefaultValue(RoomType.Standart); // ✅ Enum value istifadə et

                // Soft Delete Query Filter
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Index
                entity.HasIndex(e => e.ServiceName)
                      .HasDatabaseName("IX_Services_ServiceName");
            });

            // Images entity configuration
            modelBuilder.Entity<Images>(entity =>
            {
                entity.Property(e => e.ImageUrl)
                      .IsRequired()
                      .HasMaxLength(255);
                entity.Property(e => e.AltText)
                      .HasMaxLength(100);
                entity.Property(e => e.DisplayOrder)
                      .IsRequired()
                      .HasDefaultValue(1);
                entity.Property(e => e.IsMain)
                      .IsRequired()
                      .HasDefaultValue(false);

                // Foreign Key
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.Images)
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Soft Delete Query Filter
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Indexes
                entity.HasIndex(e => e.RoomId)
                      .HasDatabaseName("IX_Images_RoomId");
                entity.HasIndex(e => e.DisplayOrder)
                      .HasDatabaseName("IX_Images_DisplayOrder");
            });

            // Reservation entity configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.Property(e => e.StartDate)
                      .IsRequired();
                entity.Property(e => e.EndDate)
                      .IsRequired();
                entity.Property(e => e.TotalNights)
                      .IsRequired();
                entity.Property(e => e.UserId)
                      .IsRequired()
                      .HasMaxLength(450); // Identity User ID length

                // Foreign Keys
                entity.HasOne(e => e.Room)
                      .WithMany()
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Soft Delete Query Filter
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Indexes
                entity.HasIndex(e => e.RoomId)
                      .HasDatabaseName("IX_Reservations_RoomId");
                entity.HasIndex(e => e.UserId)
                      .HasDatabaseName("IX_Reservations_UserId");
                entity.HasIndex(e => e.StartDate)
                      .HasDatabaseName("IX_Reservations_StartDate");
                entity.HasIndex(e => e.EndDate)
                      .HasDatabaseName("IX_Reservations_EndDate");

                // Unique constraint to prevent double booking
                entity.HasIndex(e => new { e.RoomId, e.StartDate, e.EndDate })
                      .HasDatabaseName("IX_Reservations_Room_DateRange")
                      .IsUnique();
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto UpdatedDate for all BaseEntity descendants
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                ((BaseEntity)entry.Entity).UpdatedDate = DateTime.Now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}