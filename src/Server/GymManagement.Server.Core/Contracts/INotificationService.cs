using GymManagement.Server.Core.DTOs.Notifications;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Giao diện dịch vụ quản lý trung tâm thông báo trong ứng dụng (In-App Notification Center).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Tạo mới một thông báo gửi tới người dùng.
    /// </summary>
    Task<NotificationDto> CreateNotificationAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách toàn bộ thông báo của người dùng, sắp xếp mới nhất lên đầu.
    /// </summary>
    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm số lượng thông báo chưa đọc của người dùng.
    /// </summary>
    Task<int> GetUnreadCountAsync(
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu một thông báo cụ thể là đã đọc.
    /// </summary>
    Task MarkAsReadAsync(
        string notificationId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu toàn bộ thông báo của người dùng là đã đọc.
    /// </summary>
    Task MarkAllAsReadAsync(
        string userName,
        CancellationToken cancellationToken = default);
}
