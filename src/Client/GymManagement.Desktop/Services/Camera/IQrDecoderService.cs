using Windows.Graphics.Imaging;
using Windows.Storage;

namespace GymManagement.Desktop.Services.Camera;

/// <summary>
/// Giao diện dịch vụ giải mã mã QR từ ảnh/frame camera và kiểm soát hàng rào chống quét trùng.
/// </summary>
public interface IQrDecoderService
{
    /// <summary>
    /// Giải mã mã QR từ đối tượng SoftwareBitmap nhận được từ luồng camera.
    /// Hỗ trợ cờ isMirrored để tự động đảo ngược pixel nếu hình ảnh camera bị lật gương.
    /// </summary>
    string? DecodeFromSoftwareBitmap(SoftwareBitmap bitmap, bool isMirrored = false);

    /// <summary>
    /// Giải mã mã QR từ file hình ảnh do người dùng chọn trên máy tính.
    /// </summary>
    Task<string?> DecodeFromFileAsync(StorageFile file);

    /// <summary>
    /// Thẩm định mã vừa quét qua hàng rào Cooldown. Trả về true nếu được chấp nhận xử lý.
    /// </summary>
    bool TryProcessScannedCode(string rawCode, out string? acceptedCode);

    /// <summary>
    /// Đặt lại bộ đệm thời gian chống quét trùng.
    /// </summary>
    void ResetCooldown();
}
