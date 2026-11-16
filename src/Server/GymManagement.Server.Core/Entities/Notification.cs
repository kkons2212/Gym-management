namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng Notification trong CSDL (Hệ thống thông báo trong ứng dụng).
/// </summary>
public sealed class Notification
{
    public string NotificationId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateOnly CreatedAt { get; set; }

    public DateOnly created_at
    {
        get
        {
            return CreatedAt;
        }
        set
        {
            CreatedAt = value;
        }
    }

    public string UserName { get; set; } = string.Empty;
}
