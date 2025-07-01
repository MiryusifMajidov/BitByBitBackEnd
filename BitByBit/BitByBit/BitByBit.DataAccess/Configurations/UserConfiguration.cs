using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.DataAccess.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Identity User artıq Id, Email, UserName, PasswordHash var
            // Yalnız custom field-lər üçün configuration

            // Custom Properties
            builder.Property(x => x.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.CreatedDate)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedDate)
                   .IsRequired(false);

            builder.Property(x => x.LastLoginDate)
                   .IsRequired(false);

            builder.Property(x => x.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            // Enum conversions
            builder.Property(x => x.Role)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(1); // UserRole.User = 1

            builder.Property(x => x.Status)
                   .HasConversion<int>()
                   .IsRequired()
                   .HasDefaultValue(1); // UserStatus.Active = 1

            // Custom Indexes
            builder.HasIndex(x => x.CreatedDate)
                   .HasDatabaseName("IX_Users_CreatedDate");

            builder.HasIndex(x => x.Role)
                   .HasDatabaseName("IX_Users_Role");

            builder.HasIndex(x => x.Status)
                   .HasDatabaseName("IX_Users_Status");

            // Soft Delete Query Filter
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Identity field-lər üçün əlavə konfigurasiya (opsional)
            builder.Property(x => x.UserName)
                   .HasMaxLength(256);

            builder.Property(x => x.Email)
                   .HasMaxLength(256);
        }
    }
}
