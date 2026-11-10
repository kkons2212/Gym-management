namespace GymManagement.Server.Core.DTOs.Adapters;

/// <summary>
/// Yêu cầu tạo mã thanh toán VietQR động chuẩn NAPAS 247.
/// </summary>
public record VietQrGenerateRequest(
    string InvoiceCode,
    decimal Amount,
    string Description,
    string CustomerName,
    TimeSpan? Timeout = null
);

/// <summary>
/// Kết quả sinh mã VietQR bao gồm chuỗi mã, đường dẫn ảnh và thời gian hết hạn.
/// </summary>
public record VietQrGenerateResponse(
    bool Success,
    string QrCodeDataUrl,
    string QuickLink,
    string TransactionReference,
    string Content,
    decimal Amount,
    DateTime ExpiresAt,
    string? ErrorMessage = null,
    string? BankAccount = null,
    string? BankName = null
);

/// <summary>
/// Kết quả đối soát trạng thái giao dịch thanh toán VietQR.
/// </summary>
public record PaymentVerificationResult(
    bool IsPaid,
    string TransactionReference,
    decimal PaidAmount,
    DateTime? PaidAt,
    string? BankTransactionId = null,
    string? ErrorMessage = null
);

/// <summary>
/// Cấu hình thông số tích hợp cổng VietQR NAPAS 247.
/// </summary>
public record VietQrOptions
{
    public const string SectionName = "VietQr";

    public string BankBin { get; init; } = "970422"; // MBBank mặc định
    public string BankCode { get; init; } = "MB";
    public string BankName { get; init; } = "MB Bank";
    public string AccountNo { get; init; } = "0912345678";
    public string AccountName { get; init; } = "CONG TY TNHH PHONG TAP THE THAO";
    public string Template { get; init; } = "compact2";
    public string ApiBaseUrl { get; init; } = "https://api.vietqr.io/v2";
    public string ImageBaseUrl { get; init; } = "https://img.vietqr.io/image";
    public string TestCallbackUrl { get; init; } = "https://dev.vietqr.org/vqr/bank/api/test/transaction-callback";
    public string TokenUrl { get; init; } = "https://dev.vietqr.org/vqr/api/token_generate";
    public string? BearerToken { get; init; }
    public bool IsSandbox { get; init; } = true;
    public int TimeoutSeconds { get; init; } = 600; // 10 phút đếm ngược
}

/// <summary>
/// Request gọi API Test Callback trên môi trường VietQR Sandbox (https://api.vietqr.vn/vi/api-vietqr-callback/goi-api-test-callback).
/// </summary>
public record VietQrTestCallbackRequest(
    string BankAccount,
    string Content,
    long Amount,
    string BankCode = "MB",
    string TransType = "C"
);

/// <summary>
/// Webhook DTO nhận thông báo biến động số dư (Transaction Sync) từ VietQR Gateway.
/// </summary>
public record VietQrTransactionSyncRequest(
    string? BankAccount,
    decimal Amount,
    string TransType,
    string Content,
    string? TransactionId = null,
    long? TransactionTime = null,
    string? ReferenceNumber = null,
    string? OrderId = null,
    string? TerminalCode = null
);

/// <summary>
/// Phản hồi chuẩn cho VietQR sau khi tiếp nhận biến động số dư.
/// </summary>
public record VietQrCallbackResponse(
    bool Error,
    string ErrorReason,
    string MessageError
);
