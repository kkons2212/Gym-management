using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể Notification (Hệ thống thông báo trong ứng dụng).
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notification");

        builder.HasKey(n => n.NotificationId)
            .HasName("PK_Notification");

        builder.Property(n => n.NotificationId)
            .HasColumnName("NotificationId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("Title")
            .HasColumnType("nvarchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnName("Message")
            .HasColumnType("nvarchar(2000)")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasColumnName("IsRead")
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(n => n.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserName)
            .HasConstraintName("FK_Notification_User")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserName, n.IsRead })
            .HasDatabaseName("IX_Notification_UserName_IsRead");

        builder.Ignore(n => n.created_at);
    }
}
