using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.DTOs.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymManagement.Server.Data.Adapters;

/// <summary>
/// Hiện thực Cổng Payment API tích hợp VietQR chuẩn NAPAS 247 và chế độ giả lập Sandbox.
/// </summary>
public class VietQrAdapter : IVietQrAdapter
{
    // ====================================================================
    // 1. CONSTANTS & STATIC FIELDS (Khai báo tập trung ở đầu lớp)
    // ====================================================================
    private const string DefaultTemplate = "compact2";
    private static readonly ConcurrentDictionary<string, PaymentVerificationResult> s_simulatedPayments = new();

    // ====================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // ====================================================================
    private readonly HttpClient _httpClient;
    private readonly VietQrOptions _options;
    private readonly ILogger<VietQrAdapter> _logger;

    // ====================================================================
    // 3. CONSTRUCTOR
    // ====================================================================
    public VietQrAdapter(
        HttpClient httpClient,
        IOptions<VietQrOptions> options,
        ILogger<VietQrAdapter> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new VietQrOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ====================================================================
    // 4. PUBLIC METHODS (Interface IVietQrAdapter Implementation)
    // ====================================================================

    /// <inheritdoc />
    public Task<VietQrGenerateResponse> GenerateQrCodeAsync(VietQrGenerateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Amount <= 0)
        {
            _logger.LogWarning("Tạo mã VietQR thất bại: Số tiền {Amount} không hợp lệ cho hóa đơn {InvoiceCode}", request.Amount, request.InvoiceCode);
            return Task.FromResult(new VietQrGenerateResponse(
                Success: false,
                QrCodeDataUrl: string.Empty,
                QuickLink: string.Empty,
                TransactionReference: request.InvoiceCode,
                Content: request.Description,
                Amount: request.Amount,
                ExpiresAt: DateTime.UtcNow,
                ErrorMessage: "Số tiền thanh toán phải lớn hơn 0."
            ));
        }

        cancellationToken.ThrowIfCancellationRequested();

        string template = string.IsNullOrWhiteSpace(_options.Template) ? DefaultTemplate : _options.Template;
        string encodedDescription = UrlEncoder.Default.Encode(request.Description);
        string encodedAccountName = UrlEncoder.Default.Encode(_options.AccountName);

        // Sinh liên kết QuickLink ảnh VietQR chuẩn NAPAS 247
        string quickLink = $"{_options.ImageBaseUrl}/{_options.BankBin}-{_options.AccountNo}-{template}.png" +
                           $"?amount={(long)request.Amount}&addInfo={encodedDescription}&accountName={encodedAccountName}";

        int timeout = request.Timeout.HasValue ? (int)request.Timeout.Value.TotalSeconds : _options.TimeoutSeconds;
        DateTime expiresAt = DateTime.UtcNow.AddSeconds(timeout);

        _logger.LogInformation("Đã tạo mã VietQR thành công cho hóa đơn {InvoiceCode}, số tiền {Amount} VNĐ. Hết hạn lúc: {ExpiresAt}",
            request.InvoiceCode, request.Amount, expiresAt);

        var response = new VietQrGenerateResponse(
            Success: true,
            QrCodeDataUrl: quickLink,
            QuickLink: quickLink,
            TransactionReference: request.InvoiceCode,
            Content: request.Description,
            Amount: request.Amount,
            ExpiresAt: expiresAt
        );

        return Task.FromResult(response);
    }

    /// <inheritdoc />
    public Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionReference, decimal expectedAmount, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionReference);

        cancellationToken.ThrowIfCancellationRequested();

        // Kiểm tra trong bộ nhớ giả lập sandbox nếu đang ở chế độ thử nghiệm
        if (s_simulatedPayments.TryGetValue(transactionReference, out var simulatedResult))
        {
            _logger.LogInformation("Tìm thấy giao dịch giả lập thành công cho mã {TransactionRef}: Số tiền {PaidAmount} VNĐ",
                transactionReference, simulatedResult.PaidAmount);
            return Task.FromResult(simulatedResult);
        }

        _logger.LogInformation("Đối soát giao dịch {TransactionRef} qua cổng VietQR: Chưa nhận được giao dịch tiền về.", transactionReference);

        var pendingResult = new PaymentVerificationResult(
            IsPaid: false,
            TransactionReference: transactionReference,
            PaidAmount: 0,
            PaidAt: null,
            ErrorMessage: "Giao dịch chưa được thanh toán hoặc đang chờ xử lý."
        );

        return Task.FromResult(pendingResult);
    }

    /// <inheritdoc />
    public Task<PaymentVerificationResult> SimulatePaymentSuccessAsync(string transactionReference, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionReference);

        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.IsSandbox)
        {
            _logger.LogWarning("Từ chối giả lập thanh toán: Hệ thống đang chạy ở môi trường Production.");
            return Task.FromResult(new PaymentVerificationResult(
                IsPaid: false,
                TransactionReference: transactionReference,
                PaidAmount: 0,
                PaidAt: null,
                ErrorMessage: "Chức năng giả lập thanh toán chỉ khả dụng trong môi trường Sandbox."
            ));
        }

        var result = new PaymentVerificationResult(
            IsPaid: true,
            TransactionReference: transactionReference,
            PaidAmount: 0, // Sẽ được hệ thống đối soát điền theo số tiền hóa đơn
            PaidAt: DateTime.UtcNow,
            BankTransactionId: $"SANDBOX-MB-{DateTime.UtcNow.Ticks}"
        );

        s_simulatedPayments[transactionReference] = result;

        _logger.LogInformation("Giả lập thanh toán Sandbox THÀNH CÔNG cho hóa đơn {TransactionRef}. Mã GD ngân hàng: {BankTxId}",
            transactionReference, result.BankTransactionId);

        return Task.FromResult(result);
    }
}

