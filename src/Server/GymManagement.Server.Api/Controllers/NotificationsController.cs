using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller quản lý thông báo trong ứng dụng (In-App Notification Center).
/// </summary>
[Authorize]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        _notificationService = notificationService;
    }

    /// <summary>
    /// Lấy danh sách thông báo của người dùng hiện tại (sắp xếp mới nhất lên đầu).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        string userName = GetCurrentUserName();
        var notifications = await _notificationService.GetUserNotificationsAsync(userName, cancellationToken);
        return OkResponse(notifications, "Lấy danh sách thông báo thành công.");
    }

    /// <summary>
    /// Lấy số lượng thông báo chưa đọc của người dùng hiện tại.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        string userName = GetCurrentUserName();
        int unreadCount = await _notificationService.GetUnreadCountAsync(userName, cancellationToken);
        return OkResponse(unreadCount, "Lấy số lượng thông báo chưa đọc thành công.");
    }

    /// <summary>
    /// Đánh dấu một thông báo cụ thể là đã đọc.
    /// </summary>
    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead([FromRoute] string id, CancellationToken cancellationToken)
    {
        string userName = GetCurrentUserName();
        await _notificationService.MarkAsReadAsync(id, userName, cancellationToken);
        return OkResponse(true, "Đánh dấu thông báo đã đọc thành công.");
    }

    /// <summary>
    /// Đánh dấu toàn bộ thông báo của người dùng hiện tại là đã đọc.
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        string userName = GetCurrentUserName();
        await _notificationService.MarkAllAsReadAsync(userName, cancellationToken);
        return OkResponse(true, "Đánh dấu toàn bộ thông báo đã đọc thành công.");
    }

    /// <summary>
    /// Tạo thông báo mới (dành cho Admin / Quản trị viên).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotification(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _notificationService.CreateNotificationAsync(request, cancellationToken);
        return CreatedResponse($"api/v1/notifications/{created.NotificationId}", created, "Tạo thông báo thành công.");
    }
}
