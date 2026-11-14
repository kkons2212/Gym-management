using Windows.Graphics.Imaging;

namespace GymManagement.Desktop.Services.Camera;

/// <summary>
/// Giao diện dịch vụ thu nhận hình ảnh từ Camera/Webcam vật lý phục vụ quét mã QR.
/// </summary>
public interface ICameraCaptureService : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Kiểm tra trạng thái camera đang hoạt động thu nhận frame hay không.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Kiểm tra hệ thống có phát hiện ít nhất một thiết bị camera khả dụng hay không.
    /// </summary>
    bool HasCamera { get; }

    /// <summary>
    /// Định danh thiết bị camera hiện tại đang được kích hoạt.
    /// </summary>
    string? CurrentDeviceId { get; }

    /// <summary>
    /// Sự kiện phát ra mỗi khi có một frame hình ảnh mới từ luồng video camera.
    /// </summary>
    event EventHandler<SoftwareBitmap>? FrameArrived;

    /// <summary>
    /// Liệt kê danh sách toàn bộ các thiết bị camera/webcam khả dụng trên máy tính.
    /// </summary>
    Task<IReadOnlyList<CameraDeviceInfo>> GetAvailableCamerasAsync();

    /// <summary>
    /// Khởi tạo camera với thiết bị được chỉ định hoặc thiết bị mặc định đầu tiên.
    /// </summary>
    Task<bool> InitializeAsync(string? deviceId = null);

    /// <summary>
    /// Bắt đầu luồng thu nhận hình ảnh từ camera.
    /// </summary>
    Task<bool> StartPreviewAsync();

    /// <summary>
    /// Dừng luồng thu nhận hình ảnh và giải phóng tạm thời cảm biến camera.
    /// </summary>
    Task StopPreviewAsync();
}
