namespace GymManagement.Server.Core.DTOs.Adapters;

/// <summary>
/// Kết quả giải mã chuỗi mã QR từ hình ảnh hoặc luồng dữ liệu camera.
/// </summary>
public record QrDecodeResult(
    bool Success,
    string? RawPayload,
    string? ErrorMessage = null
);

/// <summary>
/// Kết quả thẩm định chuỗi mã token check-in (cấu trúc GYM-{UserId}-{Ticks}-{Checksum}).
/// </summary>
public record QrTokenValidationResult(
    bool IsValid,
    string? UserId,
    DateTime? CreatedAt,
    bool IsExpired,
    string? FailureReason = null
);

