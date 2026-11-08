using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể PTTrainingSession.
/// </summary>
public class PTTrainingSessionConfiguration : IEntityTypeConfiguration<PTTrainingSession>
{
    public void Configure(EntityTypeBuilder<PTTrainingSession> builder)
    {
        builder.ToTable("PTTrainingSession");

        builder.HasKey(s => s.PTSessionId)
            .HasName("PK_PTTrainingSession");

        builder.Property(s => s.PTSessionId)
            .HasColumnName("PTSessionId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.SessionStartTime)
            .HasColumnName("SessionStartTime")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.SessionEndTime)
            .HasColumnName("SessionEndTime")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(s => s.WorkoutNotes)
            .HasColumnName("WorkoutNotes")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(s => s.RatingStars)
            .HasColumnName("RatingStars")
            .HasColumnType("tinyint")
            .IsRequired(false);

        builder.Property(s => s.ReviewComment)
            .HasColumnName("ReviewComment")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(s => s.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(s => s.MemberId)
            .HasColumnName("MemberID")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.PtId)
            .HasColumnName("PtID")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.AvailabilityId)
            .HasColumnName("AvailabilityId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        // Foreign Keys
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.MemberId)
            .HasConstraintName("FK_PTTrainingSession_Member")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.PtId)
            .HasConstraintName("FK_PTTrainingSession_PT")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PTAvailability>()
            .WithMany()
            .HasForeignKey(s => s.AvailabilityId)
            .HasConstraintName("FK_PTTrainingSession_Availability")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.completed_at);
        builder.Ignore(s => s.MemberID);
        builder.Ignore(s => s.PtID);
    }
}

