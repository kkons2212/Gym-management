using GymManagement.Server.Core.DTOs.Billing;

namespace GymManagement.Server.Core.Contracts;

public interface IBillingService
{
    Task<InvoiceDto> CreateInvoiceAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken = default);

    Task<VietQrPayloadDto> GeneratePaymentQrAsync(
        string invoiceCode,
        CancellationToken cancellationToken = default);

    Task<InvoiceDto> ConfirmPaymentAsync(
        string invoiceCode,
        CancellationToken cancellationToken = default);

    Task<InvoiceDto> RefundInvoiceAsync(
        string invoiceCode,
        string reason,
        CancellationToken cancellationToken = default);
}
