using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể User ([User]).
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasKey(u => u.UserName)
            .HasName("PK_User");

        builder.Property(u => u.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(u => u.RoleId)
            .HasColumnName("RoleID")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasColumnName("FullName")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(u => u.Phone)
            .HasColumnName("Phone")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(u => u.Email)
            .HasColumnName("Email")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(u => u.Gender)
            .HasColumnName("Gender")
            .HasColumnType("tinyint")
            .IsRequired(false);

        builder.Property(u => u.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("date")
            .IsRequired(false);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .HasConstraintName("FK_User_Role")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(u => u.RoleID);
        builder.Ignore(u => u.created_at);
    }
}

