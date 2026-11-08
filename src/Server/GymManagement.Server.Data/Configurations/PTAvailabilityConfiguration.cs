using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể PTAvailability.
/// </summary>
public class PTAvailabilityConfiguration : IEntityTypeConfiguration<PTAvailability>
{
    public void Configure(EntityTypeBuilder<PTAvailability> builder)
    {
        builder.ToTable("PTAvailability");

        builder.HasKey(a => a.AvailabilityId)
            .HasName("PK_PTAvailability");

        builder.Property(a => a.AvailabilityId)
            .HasColumnName("AvailabilityId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(a => a.AvailableDate)
            .HasColumnName("AvailableDate")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(a => a.StartTime)
            .HasColumnName("StartTime")
            .HasColumnType("time(7)")
            .IsRequired();

        builder.Property(a => a.EndTime)
            .HasColumnName("EndTime")
            .HasColumnType("time(7)")
            .IsRequired();

        builder.Property(a => a.IsBooked)
            .HasColumnName("is_Booked")
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(a => a.PtId)
            .HasColumnName("PtID")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        // Foreign Key
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.PtId)
            .HasConstraintName("FK_PTAvailability_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.is_Booked);
        builder.Ignore(a => a.PtID);
    }
}

