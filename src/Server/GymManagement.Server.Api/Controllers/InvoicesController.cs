using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller quản lý thanh toán, hóa đơn và cổng VietQR (UC05).
/// Phục vụ lễ tân lập hóa đơn, tạo mã VietQR động, xác nhận thanh toán và hoàn tiền.
/// </summary>
public sealed class InvoicesController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IBillingService _billingService;
    private readonly ILogger<InvoicesController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public InvoicesController(
        IBillingService billingService,
        ILogger<InvoicesController> logger)
    {
        ArgumentNullException.ThrowIfNull(billingService);
        ArgumentNullException.ThrowIfNull(logger);

        _billingService = billingService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS (ENDPOINTS)
    // =========================================================================

    /// <summary>
    /// Lập hóa đơn thanh toán mới cho gói tập (POS / VietQR) (UC 5.1).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("InvoicesController: Tao hoa don cho hoi vien {UserId}, goi {PlanId}",
            request.UserId, request.PlanId);

        var invoice = await _billingService.CreateInvoiceAsync(request, cancellationToken);
        return CreatedResponse($"/api/v1/invoices/{invoice.InvoiceCode}", invoice, "Tạo hóa đơn thành công.");
    }

    /// <summary>
    /// Sinh mã VietQR động để thanh toán chuyển khoản qua ngân hàng (UC 5.1).
    /// </summary>
    [HttpGet("{invoiceCode}/qr")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GeneratePaymentQr(
        string invoiceCode,
        CancellationToken cancellationToken = default)
    {
        var qrPayload = await _billingService.GeneratePaymentQrAsync(invoiceCode, cancellationToken);
        return OkResponse(qrPayload, "Tạo mã QR thanh toán thành công.");
    }

    /// <summary>
    /// Xác nhận thanh toán thành công và kích hoạt gói tập trong 1 Transaction ACID (UC 5.2).
    /// </summary>
    [HttpPost("{invoiceCode}/confirm")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ConfirmPayment(
        string invoiceCode,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _billingService.ConfirmPaymentAsync(invoiceCode, cancellationToken);
        return OkResponse(invoice, "Xác nhận thanh toán hóa đơn thành công.");
    }

    /// <summary>
    /// Xử lý hoàn tiền hóa đơn và hủy/thu hồi gói tập (UC 5.3).
    /// </summary>
    [HttpPost("{invoiceCode}/refund")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RefundInvoice(
        string invoiceCode,
        [FromBody] RefundInvoiceApiRequest request,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _billingService.RefundInvoiceAsync(invoiceCode, request.Reason, cancellationToken);
        return OkResponse(invoice, "Hoàn tiền hóa đơn thành công.");
    }
}

public record RefundInvoiceApiRequest(string Reason);
