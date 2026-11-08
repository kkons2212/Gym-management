using GymManagement.Server.Core.DTOs.Adapters;

namespace GymManagement.Server.Core.Contracts.Adapters;

/// <summary>
/// Hợp đồng Adapter tích hợp cổng thanh toán VietQR NAPAS 247.
/// </summary>
public interface IVietQrAdapter
{
    /// <summary>
    /// Sinh chuỗi mã thanh toán và đường dẫn ảnh VietQR động.
    /// </summary>
    Task<VietQrGenerateResponse> GenerateQrCodeAsync(VietQrGenerateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xác thực trạng thái thanh toán từ ngân hàng hoặc hệ thống đối soát.
    /// </summary>
    Task<PaymentVerificationResult> VerifyPaymentAsync(string transactionReference, decimal expectedAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Giả lập thanh toán thành công (chế độ Sandbox phục vụ phát triển & kiểm thử).
    /// </summary>
    Task<PaymentVerificationResult> SimulatePaymentSuccessAsync(string transactionReference, CancellationToken cancellationToken = default);
}

