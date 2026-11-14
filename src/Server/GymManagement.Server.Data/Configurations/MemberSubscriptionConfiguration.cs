using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể MemberSubscription.
/// </summary>
public class MemberSubscriptionConfiguration : IEntityTypeConfiguration<MemberSubscription>
{
    public void Configure(EntityTypeBuilder<MemberSubscription> builder)
    {
        builder.ToTable("MemberSubscription");

        builder.HasKey(s => s.SubscriptionId)
            .HasName("PK_MemberSubscription");

        builder.Property(s => s.SubscriptionId)
            .HasColumnName("SubscriptionId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.QrCode)
            .HasColumnName("QrCode")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(s => s.StartDate)
            .HasColumnName("StartDate")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(s => s.EndDate)
            .HasColumnName("EndDate")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(s => s.RemainingCheckInSessions)
            .HasColumnName("RemainingCheckInSessions")
            .HasColumnType("int");

        builder.Property(s => s.RemainingPtSessions)
            .HasColumnName("RemainingPtSessions")
            .HasColumnType("int");

        builder.Property(s => s.FreezeStartDate)
            .HasColumnName("FreezeStartDate")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(s => s.FreezeEndDate)
            .HasColumnName("FreezeEndDate")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(s => s.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(s => s.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.AssignedPtId)
            .HasColumnName("AssignedPtId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(s => s.PlanId)
            .HasColumnName("PlanId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.PTSessionId)
            .HasColumnName("PTSessionId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        // Unique Constraint (Filtered Index to allow multiple NULLs for pending subscriptions)
        builder.HasIndex(s => s.QrCode)
            .IsUnique()
            .HasFilter("[QrCode] IS NOT NULL")
            .HasDatabaseName("UQ_MemberSubscription_QrCode");

        // Foreign Keys
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserName)
            .HasConstraintName("FK_MemberSubscription_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.AssignedPtId)
            .HasConstraintName("FK_MemberSubscription_AssignedPt")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PackagePlan>()
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .HasConstraintName("FK_MemberSubscription_PackagePlan")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PTTrainingSession>()
            .WithMany()
            .HasForeignKey(s => s.PTSessionId)
            .HasConstraintName("FK_MemberSubscription_PTSession")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.created_at);
    }
}

