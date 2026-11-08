using GymManagement.Server.Core.DTOs.Adapters;

namespace GymManagement.Server.Core.Contracts.Adapters;

/// <summary>
/// Hợp đồng Adapter giải mã hình ảnh mã QR từ thiết bị quét hoặc luồng webcam quầy lễ tân.
/// </summary>
public interface ICameraScannerAdapter
{
    /// <summary>
    /// Giải mã chuỗi mã QR từ mảng byte hình ảnh (PNG, JPEG, BMP).
    /// </summary>
    Task<QrDecodeResult> DecodeQrCodeFromBytesAsync(byte[] imageBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Giải mã chuỗi mã QR từ luồng Stream hình ảnh.
    /// </summary>
    Task<QrDecodeResult> DecodeQrCodeFromStreamAsync(Stream imageStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thẩm định cấu trúc và thời hạn hiệu lực của chuỗi mã QR điểm danh (chu kỳ 60 giây).
    /// </summary>
    QrTokenValidationResult ValidateCheckInToken(string qrPayload, int validDurationSeconds = 60);
}

