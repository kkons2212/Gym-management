namespace GymManagement.Desktop.Services.Camera;

/// <summary>
/// Đại diện cho thông tin thiết bị Camera/Webcam được phát hiện trên hệ thống.
/// </summary>
public record CameraDeviceInfo(
    string DeviceId,
    string DisplayName,
    bool IsDefault
);
