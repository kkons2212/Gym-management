using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Server.Core.Entities;

[Table("CheckInLog")]
public sealed class CheckInLog
{
    [Key]
    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string CheckInId { get; set; } = string.Empty;

    [Column("check_in_at", TypeName = "date")]
    public DateOnly? check_in_at { get; set; }

    [Column(TypeName = "varchar(20)")]
    [MaxLength(20)]
    public string? Status { get; set; }

    [Column(TypeName = "varchar(255)")]
    [MaxLength(255)]
    public string? DenialReason { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string SubscriptionId { get; set; } = string.Empty;

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string? VerifiedByAdminId { get; set; }

    [Column(TypeName = "varchar(10)")]
    [MaxLength(10)]
    public string UserName { get; set; } = string.Empty;
}
