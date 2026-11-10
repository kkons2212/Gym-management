using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Adapters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller xử lý thanh toán ngoại vi và Webhook từ cổng VietQR (UC05).
/// </summary>
public sealed class PaymentsController : ApiControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IBillingService billingService,
        ILogger<PaymentsController> logger)
    {
        ArgumentNullException.ThrowIfNull(billingService);
        ArgumentNullException.ThrowIfNull(logger);

        _billingService = billingService;
        _logger = logger;
    }

    /// <summary>
    /// Webhook tiếp nhận thông báo biến động số dư (Transaction Sync) từ cổng thanh toán VietQR.
    /// URL: POST /api/v1/payments/vietqr/callback
    /// </summary>
    [HttpPost("vietqr/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> VietQrCallback(
        [FromBody] VietQrTransactionSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Tiếp nhận Webhook VietQR: Account={Account}, Amount={Amount}, Content={Content}, TransType={TransType}",
            request?.BankAccount,
            request?.Amount,
            request?.Content,
            request?.TransType);

        if (request == null)
        {
            return Ok(new VietQrCallbackResponse(
                Error: true,
                ErrorReason: "400",
                MessageError: "Dữ liệu yêu cầu webhook không được để trống."
            ));
        }

        try
        {
            await _billingService.ProcessVietQrCallbackAsync(request, cancellationToken);

            return Ok(new VietQrCallbackResponse(
                Error: false,
                ErrorReason: "000",
                MessageError: "Success"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi đối soát webhook VietQR Callback: {Message}", ex.Message);

            return Ok(new VietQrCallbackResponse(
                Error: true,
                ErrorReason: "500",
                MessageError: ex.Message
            ));
        }
    }
}

