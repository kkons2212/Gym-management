using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể MemberBodyMetric.
/// </summary>
public class MemberBodyMetricConfiguration : IEntityTypeConfiguration<MemberBodyMetric>
{
    public void Configure(EntityTypeBuilder<MemberBodyMetric> builder)
    {
        builder.ToTable("MemberBodyMetric");

        builder.HasKey(m => m.BodyMetricId)
            .HasName("PK_MemberBodyMetric");

        builder.Property(m => m.BodyMetricId)
            .HasColumnName("BodyMetricId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.WeightKg)
            .HasColumnName("WeightKg")
            .HasColumnType("decimal(19, 0)")
            .IsRequired();

        builder.Property(m => m.HeightCm)
            .HasColumnName("HeightCm")
            .HasColumnType("decimal(19, 0)")
            .IsRequired();

        builder.Property(m => m.SkeletalMuscleMass)
            .HasColumnName("SkeletalMuscleMass")
            .HasColumnType("decimal(19, 0)")
            .IsRequired();

        builder.Property(m => m.BodyFatPercentage)
            .HasColumnName("BodyFatPercentage")
            .HasColumnType("decimal(19, 0)")
            .IsRequired();

        builder.Property(m => m.Bmi)
            .HasColumnName("Bmi")
            .HasColumnType("decimal(19, 0)")
            .IsRequired();

        builder.Property(m => m.Notes)
            .HasColumnName("Notes")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(m => m.MeasuredAt)
            .HasColumnName("measured_at")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(m => m.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.PTSessionId)
            .HasColumnName("PTSessionId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(m => m.RecordedByPtId)
            .HasColumnName("RecordedByPtId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        // Foreign Keys
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserName)
            .HasConstraintName("FK_MemberBodyMetric_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PTTrainingSession>()
            .WithMany()
            .HasForeignKey(m => m.PTSessionId)
            .HasConstraintName("FK_MemberBodyMetric_PTSession")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.RecordedByPtId)
            .HasConstraintName("FK_MemberBodyMetric_RecordedByPt")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(m => m.measured_at);
        builder.Ignore(m => m.RecordedByPtID);
    }
}

