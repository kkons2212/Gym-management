using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể PackagePlan.
/// </summary>
public class PackagePlanConfiguration : IEntityTypeConfiguration<PackagePlan>
{
    public void Configure(EntityTypeBuilder<PackagePlan> builder)
    {
        builder.ToTable("PackagePlan");

        builder.HasKey(p => p.PlanId)
            .HasName("PK_PackagePlan");

        builder.Property(p => p.PlanId)
            .HasColumnName("PlanId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("Name")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(p => p.Description)
            .HasColumnName("Description")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(p => p.DurationDays)
            .HasColumnName("DurationDays")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(p => p.Price)
            .HasColumnName("Price")
            .HasColumnType("decimal(19, 0)")
            .IsRequired(false);

        builder.Property(p => p.MaxCheckInSessions)
            .HasColumnName("MaxCheckInSessions")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(p => p.IsPtIncluded)
            .HasColumnName("is_PtIncluded")
            .HasColumnType("tinyint")
            .IsRequired(false);

        builder.Property(p => p.TotalPtSessions)
            .HasColumnName("TotalPtSessions")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_Active")
            .HasColumnType("tinyint")
            .IsRequired(false);

        builder.Ignore(p => p.is_PtIncluded);
        builder.Ignore(p => p.is_Active);
    }
}

