using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Notifications;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// Hiện thực dịch vụ quản trị thông báo trong ứng dụng (In-App Notification Center).
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);

        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<NotificationDto> CreateNotificationAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ValidationException("Yêu cầu tạo thông báo không được để trống.");
        }

        string userName = RequireText(request.UserName, "Tên tài khoản người nhận");
        string title = RequireText(request.Title, "Tiêu đề thông báo");
        string message = RequireText(request.Message, "Nội dung thông báo");

        var user = await _unitOfWork.Repository<User>()
            .FindSingleOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"Không tìm thấy người dùng có mã: {userName}");
        }

        var notification = new Notification
        {
            NotificationId = NewId(),
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UserName = user.UserName
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Đã tạo thông báo {NotificationId} cho người dùng {UserName}: {Title}",
            notification.NotificationId,
            notification.UserName,
            notification.Title);

        return MapToDto(notification);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        userName = RequireText(userName, "Tên tài khoản người dùng");

        var notifications = await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserName == userName, cancellationToken);

        var list = notifications
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.NotificationId)
            .Select(MapToDto)
            .ToList();

        return list;
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        userName = RequireText(userName, "Tên tài khoản người dùng");

        var unreadNotifications = await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserName == userName && !n.IsRead, cancellationToken);

        return unreadNotifications.Count;
    }

    /// <inheritdoc />
    public async Task MarkAsReadAsync(
        string notificationId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        notificationId = RequireText(notificationId, "Mã thông báo");
        userName = RequireText(userName, "Tên tài khoản người dùng");

        var notification = await _unitOfWork.Repository<Notification>()
            .FindSingleOrDefaultAsync(
                n => n.NotificationId == notificationId && n.UserName == userName,
                cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException($"Không tìm thấy thông báo: {notificationId}");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Thông báo {NotificationId} của {UserName} đã được đánh dấu đã đọc.",
                notificationId,
                userName);
        }
    }

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        userName = RequireText(userName, "Tên tài khoản người dùng");

        var unreadNotifications = await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserName == userName && !n.IsRead, cancellationToken);

        if (unreadNotifications.Count == 0)
        {
            return;
        }

        foreach (var item in unreadNotifications)
        {
            item.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(item);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Đã đánh dấu đọc toàn bộ {Count} thông báo cho người dùng {UserName}.",
            unreadNotifications.Count,
            userName);
    }

    // =========================================================================
    // PRIVATE HELPER METHODS
    // =========================================================================

    private static NotificationDto MapToDto(Notification entity)
    {
        return new NotificationDto(
            entity.NotificationId,
            entity.Title,
            entity.Message,
            entity.IsRead,
            entity.CreatedAt,
            entity.UserName);
    }

    private static string NewId()
    {
        return Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
    }

    private static string RequireText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{fieldName} không được để trống.");
        }

        return value.Trim();
    }
}
