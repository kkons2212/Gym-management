using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể CheckInLog.
/// </summary>
public class CheckInLogConfiguration : IEntityTypeConfiguration<CheckInLog>
{
    public void Configure(EntityTypeBuilder<CheckInLog> builder)
    {
        builder.ToTable("CheckInLog");

        builder.HasKey(c => c.CheckInId)
            .HasName("PK_CheckInLog");

        builder.Property(c => c.CheckInId)
            .HasColumnName("CheckInId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.CheckInAt)
            .HasColumnName("check_in_at")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(c => c.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(c => c.DenialReason)
            .HasColumnName("DenialReason")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.SubscriptionId)
            .HasColumnName("SubscriptionId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.VerifiedByAdminId)
            .HasColumnName("VerifiedByAdminId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(c => c.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        // Foreign Keys
        builder.HasOne<MemberSubscription>()
            .WithMany()
            .HasForeignKey(c => c.SubscriptionId)
            .HasConstraintName("FK_CheckInLog_Subscription")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserName)
            .HasConstraintName("FK_CheckInLog_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.VerifiedByAdminId)
            .HasConstraintName("FK_CheckInLog_Admin")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.check_in_at);
        builder.Ignore(c => c.VerifiedByAdminID);
    }
}

