using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagement.Server.Data.Configurations;

/// <summary>
/// Cấu hình ánh xạ Fluent API cho thực thể Invoice.
/// </summary>
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoice");

        builder.HasKey(i => i.InvoiceId)
            .HasName("PK_Invoice");

        builder.Property(i => i.InvoiceId)
            .HasColumnName("InvoiceId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(i => i.InvoiceCode)
            .HasColumnName("InvoiceCode")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(i => i.TransactionType)
            .HasColumnName("TransactionType")
            .HasColumnType("nvarchar(50)")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(i => i.SubtotalAmount)
            .HasColumnName("SubtotalAmount")
            .HasColumnType("decimal(19, 0)")
            .IsRequired(false);

        builder.Property(i => i.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasColumnType("int")
            .IsRequired(false);

        builder.Property(i => i.PaymentMethod)
            .HasColumnName("PaymentMethod")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(i => i.Status)
            .HasColumnName("Status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(i => i.RefundAmount)
            .HasColumnName("RefundAmount")
            .HasColumnType("decimal(19, 0)")
            .IsRequired(false);

        builder.Property(i => i.RefundReason)
            .HasColumnName("RefundReason")
            .HasColumnType("nvarchar(255)")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(i => i.PaidAt)
            .HasColumnName("paid_at")
            .HasColumnType("date")
            .IsRequired(false);

        builder.Property(i => i.UserName)
            .HasColumnName("UserName")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(i => i.SubscriptionId)
            .HasColumnName("SubscriptionId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(i => i.CreatedByAdminId)
            .HasColumnName("CreatedByAdminId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(i => i.CheckInLogCheckInId)
            .HasColumnName("CheckInLogCheckInId")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false);

        // Unique Constraint
        builder.HasIndex(i => i.InvoiceCode)
            .IsUnique()
            .HasDatabaseName("UQ_Invoice_InvoiceCode");

        // Foreign Keys
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.UserName)
            .HasConstraintName("FK_Invoice_User")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MemberSubscription>()
            .WithMany()
            .HasForeignKey(i => i.SubscriptionId)
            .HasConstraintName("FK_Invoice_Subscription")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.CreatedByAdminId)
            .HasConstraintName("FK_Invoice_Admin")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CheckInLog>()
            .WithMany()
            .HasForeignKey(i => i.CheckInLogCheckInId)
            .HasConstraintName("FK_Invoice_CheckInLog")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(i => i.paid_at);
        builder.Ignore(i => i.CreatedByAdminID);
    }
}

