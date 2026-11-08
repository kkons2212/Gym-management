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
    string? ErrorMessage = null
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
    public string AccountNo { get; init; } = "0912345678";
    public string AccountName { get; init; } = "CONG TY TNHH PHONG TAP THE THAO";
    public string Template { get; init; } = "compact2";
    public string ApiBaseUrl { get; init; } = "https://api.vietqr.io/v2";
    public string ImageBaseUrl { get; init; } = "https://img.vietqr.io/image";
    public bool IsSandbox { get; init; } = true;
    public int TimeoutSeconds { get; init; } = 600; // 10 phút đếm ngược
}

