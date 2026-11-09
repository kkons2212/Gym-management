namespace GymManagement.Server.Core.DTOs.Billing;

public record CreateInvoiceRequest(
    string UserId,
    string PlanId,
    string PaymentMethod,
    string TransactionType
);

public record InvoiceDto(
    string InvoiceId,
    string InvoiceCode,
    decimal SubtotalAmount,
    decimal TotalAmount,
    string PaymentMethod,
    string TransactionType,
    DateTime? PaidAt,
    string MemberName,
    string PlanName,
    string? CreatedByAdminName,
    string Status = "PENDING",
    decimal? RefundAmount = null
);

public record VietQrPayloadDto(
    string QrDataUrl,
    string BankAccount,
    string BankName,
    decimal Amount,
    string TransferContent,
    int ExpirySeconds,
    string? InvoiceId = null
)
{
    public string QrImageUrl => QrDataUrl;
    public int ExpireSeconds => ExpirySeconds;
}
