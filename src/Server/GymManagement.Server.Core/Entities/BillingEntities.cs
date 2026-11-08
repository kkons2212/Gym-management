using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Server.Core.Entities;

[Table("User")]
public sealed class User
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string UserName { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string RoleId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? FullName { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? Phone { get; set; }

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    public byte? Gender { get; set; }

    [Column(TypeName = "varchar(20)")]
    [MaxLength(20)]
    public string? Status { get; set; }

    [Column("created_at", TypeName = "date")]
    public DateOnly? CreatedAt { get; set; }
}

[Table("MemberSubscription")]
public sealed class MemberSubscription
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string SubscriptionId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string? QrCode { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? EndDate { get; set; }

    public int RemainingCheckInSessions { get; set; }
    public int RemainingPtSessions { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? FreezeStartDate { get; set; }

    [Column(TypeName = "date")]
    public DateOnly? FreezeEndDate { get; set; }

    [Column(TypeName = "varchar(20)")]
    [MaxLength(20)]
    public string? Status { get; set; }

    [Column("created_at", TypeName = "date")]
    public DateOnly? CreatedAt { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string UserName { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? AssignedPtId { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string PlanId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? PTSessionId { get; set; }
}

[Table("PackagePlan")]
public sealed class PackagePlan
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string PlanId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? Name { get; set; }

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string? Description { get; set; }

    public int? DurationDays { get; set; }

    [Column(TypeName = "decimal(19, 0)")]
    public decimal? Price { get; set; }

    public int? MaxCheckInSessions { get; set; }

    [Column("is_PtIncluded", TypeName = "tinyint")]
    public byte? IsPtIncluded { get; set; }

    public int? TotalPtSessions { get; set; }

    [Column("is_Active", TypeName = "tinyint")]
    public byte? IsActive { get; set; }
}

[Table("Invoice")]
public sealed class Invoice
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string InvoiceId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string InvoiceCode { get; set; } = string.Empty;

    [Column(TypeName = "varchar(50)")]
    [MaxLength(50)]
    public string? TransactionType { get; set; }

    [Column(TypeName = "decimal(19, 0)")]
    public decimal? SubtotalAmount { get; set; }

    public int? TotalAmount { get; set; }

    [Column(TypeName = "varchar(100)")]
    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [Column(TypeName = "varchar(20)")]
    [MaxLength(20)]
    public string? Status { get; set; }

    [Column(TypeName = "decimal(19, 0)")]
    public decimal? RefundAmount { get; set; }

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string? RefundReason { get; set; }

    [Column("paid_at", TypeName = "date")]
    public DateOnly? PaidAt { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string UserName { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string SubscriptionId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? CreatedByAdminId { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? CheckInLogCheckInId { get; set; }
}
