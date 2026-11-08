namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng Invoice trong CSDL.
/// </summary>
public sealed class Invoice
{
    public string InvoiceId { get; set; } = string.Empty;

    public string InvoiceCode { get; set; } = string.Empty;

    public string? TransactionType { get; set; }

    public decimal? SubtotalAmount { get; set; }

    public int? TotalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public decimal? RefundAmount { get; set; }

    public string? RefundReason { get; set; }

    public DateOnly? PaidAt { get; set; }

    public DateOnly? paid_at
    {
        get
        {
            return PaidAt;
        }
        set
        {
            PaidAt = value;
        }
    }

    public string UserName { get; set; } = string.Empty;

    public string SubscriptionId { get; set; } = string.Empty;

    public string? CreatedByAdminId { get; set; }

    public string? CreatedByAdminID
    {
        get
        {
            return CreatedByAdminId;
        }
        set
        {
            CreatedByAdminId = value;
        }
    }

    public string? CheckInLogCheckInId { get; set; }
}

