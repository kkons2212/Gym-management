using System.Linq.Expressions;
using FluentAssertions;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Notifications;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.UnitTests.Services;

public sealed class NotificationServiceTests
{
    private readonly List<Notification> _notifications = [];
    private readonly List<User> _users = [];
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IGenericRepository<Notification>> _notificationRepoMock = new();
    private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
    private readonly Mock<ILogger<NotificationService>> _loggerMock = new();

    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        // Setup User repo
        _userRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<User, bool>> predicate, CancellationToken _) =>
                _users.AsQueryable().FirstOrDefault(predicate.Compile()));

        // Setup Notification repo
        _notificationRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((entity, _) => _notifications.Add(entity))
            .Returns(Task.CompletedTask);

        _notificationRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Notification, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Notification, bool>> predicate, CancellationToken _) =>
                _notifications.AsQueryable().Where(predicate.Compile()).ToList());

        _notificationRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<Notification, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Notification, bool>> predicate, CancellationToken _) =>
                _notifications.AsQueryable().FirstOrDefault(predicate.Compile()));

        _notificationRepoMock
            .Setup(r => r.Update(It.IsAny<Notification>()))
            .Callback<Notification>(entity =>
            {
                var existing = _notifications.FirstOrDefault(n => n.NotificationId == entity.NotificationId);
                if (existing != null)
                {
                    _notifications.Remove(existing);
                    _notifications.Add(entity);
                }
            });

        _unitOfWorkMock.Setup(u => u.Repository<Notification>()).Returns(_notificationRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _users.Add(new User
        {
            UserName = "member01",
            FullName = "Nguyễn Văn A",
            Email = "member01@example.com",
            RoleId = "MEMBER",
            Status = "ACTIVE"
        });

        _sut = new NotificationService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithValidRequest_ShouldCreateNotificationAndReturnDto()
    {
        // Arrange
        var request = new CreateNotificationRequest("member01", "Chào mừng", "Chào mừng bạn tham gia phòng gym!");

        // Act
        var result = await _sut.CreateNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserName.Should().Be("member01");
        result.Title.Should().Be("Chào mừng");
        result.Message.Should().Be("Chào mừng bạn tham gia phòng gym!");
        result.IsRead.Should().BeFalse();
        result.CreatedAt.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
        result.NotificationId.Should().NotBeNullOrWhiteSpace();

        _notifications.Should().HaveCount(1);
        _notifications.Single().Title.Should().Be("Chào mừng");
    }

    [Fact]
    public async Task CreateNotificationAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateNotificationRequest("unknown_user", "Tiêu đề", "Nội dung");

        // Act
        Func<Task> act = async () => await _sut.CreateNotificationAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Không tìm thấy người dùng*");
    }

    [Theory]
    [InlineData("", "Nội dung")]
    [InlineData("Tiêu đề", "")]
    public async Task CreateNotificationAsync_WhenTitleOrMessageEmpty_ShouldThrowValidationException(string title, string message)
    {
        // Arrange
        var request = new CreateNotificationRequest("member01", title, message);

        // Act
        Func<Task> act = async () => await _sut.CreateNotificationAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ShouldReturnNotificationsForUser_OrderedDescending()
    {
        // Arrange
        _notifications.Add(new Notification
        {
            NotificationId = "NOTIF001",
            UserName = "member01",
            Title = "Thông báo 1",
            Message = "Nội dung 1",
            IsRead = true,
            CreatedAt = new DateOnly(2026, 11, 1)
        });
        _notifications.Add(new Notification
        {
            NotificationId = "NOTIF002",
            UserName = "member01",
            Title = "Thông báo 2",
            Message = "Nội dung 2",
            IsRead = false,
            CreatedAt = new DateOnly(2026, 11, 10)
        });
        _notifications.Add(new Notification
        {
            NotificationId = "NOTIF003",
            UserName = "other_user",
            Title = "Thông báo khác",
            Message = "Nội dung",
            IsRead = false,
            CreatedAt = new DateOnly(2026, 11, 10)
        });

        // Act
        var result = await _sut.GetUserNotificationsAsync("member01");

        // Assert
        result.Should().HaveCount(2);
        result[0].NotificationId.Should().Be("NOTIF002");
        result[1].NotificationId.Should().Be("NOTIF001");
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnCorrectCountOfUnreadNotifications()
    {
        // Arrange
        _notifications.Add(new Notification
        {
            NotificationId = "N1",
            UserName = "member01",
            Title = "T1",
            Message = "M1",
            IsRead = false,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        _notifications.Add(new Notification
        {
            NotificationId = "N2",
            UserName = "member01",
            Title = "T2",
            Message = "M2",
            IsRead = false,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        _notifications.Add(new Notification
        {
            NotificationId = "N3",
            UserName = "member01",
            Title = "T3",
            Message = "M3",
            IsRead = true,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        // Act
        int unreadCount = await _sut.GetUnreadCountAsync("member01");

        // Assert
        unreadCount.Should().Be(2);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationExists_ShouldSetIsReadToTrue()
    {
        // Arrange
        var notif = new Notification
        {
            NotificationId = "NOTIF001",
            UserName = "member01",
            Title = "T1",
            Message = "M1",
            IsRead = false,
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _notifications.Add(notif);

        // Act
        await _sut.MarkAsReadAsync("NOTIF001", "member01");

        // Assert
        notif.IsRead.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenNotificationNotFound_ShouldThrowNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _sut.MarkAsReadAsync("NONEXIST", "member01");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldMarkAllUnreadNotificationsAsRead()
    {
        // Arrange
        var n1 = new Notification { NotificationId = "N1", UserName = "member01", Title = "T1", Message = "M1", IsRead = false };
        var n2 = new Notification { NotificationId = "N2", UserName = "member01", Title = "T2", Message = "M2", IsRead = false };
        var n3 = new Notification { NotificationId = "N3", UserName = "other", Title = "T3", Message = "M3", IsRead = false };
        _notifications.AddRange([n1, n2, n3]);

        // Act
        await _sut.MarkAllAsReadAsync("member01");

        // Assert
        n1.IsRead.Should().BeTrue();
        n2.IsRead.Should().BeTrue();
        n3.IsRead.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
