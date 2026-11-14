using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể Role.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");

        builder.HasKey(r => r.RoleID)
            .HasName("PK_Role");

        builder.Property(r => r.RoleID)
            .HasColumnName("RoleID")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(r => r.RoleName)
            .HasColumnName("RoleName")
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        // NOTE: Do NOT ignore RoleId - it's needed for EF Core relationships
        // EF Core automatically maps RoleId to RoleID column via the property naming convention
    }
}

