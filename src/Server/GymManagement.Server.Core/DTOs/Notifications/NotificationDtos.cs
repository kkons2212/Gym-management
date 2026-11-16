namespace GymManagement.Server.Core.DTOs.Notifications;

/// <summary>
/// DTO thông tin bản ghi thông báo trong ứng dụng (In-App Notification).
/// </summary>
public record NotificationDto(
    string NotificationId,
    string Title,
    string Message,
    bool IsRead,
    DateOnly CreatedAt,
    string UserName
);

/// <summary>
/// DTO yêu cầu tạo mới một thông báo trong ứng dụng.
/// </summary>
public record CreateNotificationRequest(
    string UserName,
    string Title,
    string Message
);
