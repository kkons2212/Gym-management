using System.Security.Claims;
using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class NotificationsControllerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly NotificationsController _sut;

    public NotificationsControllerTests()
    {
        _sut = new NotificationsController(_notificationServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "member01"),
            new Claim(ClaimTypes.Name, "member01"),
            new Claim(ClaimTypes.Role, "Member")
        ], "TestAuth"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetNotifications_ReturnsOkResponseWithList()
    {
        // Arrange
        IReadOnlyList<NotificationDto> expectedList =
        [
            new("N1", "Title 1", "Msg 1", false, new DateOnly(2026, 11, 15), "member01")
        ];

        _notificationServiceMock
            .Setup(s => s.GetUserNotificationsAsync("member01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        // Act
        var result = await _sut.GetNotifications(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<NotificationDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data![0].Title.Should().Be("Title 1");
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOkResponseWithCount()
    {
        // Arrange
        _notificationServiceMock
            .Setup(s => s.GetUnreadCountAsync("member01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _sut.GetUnreadCount(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<int>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().Be(3);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsOkResponse()
    {
        // Arrange
        _notificationServiceMock
            .Setup(s => s.MarkAsReadAsync("N1", "member01", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.MarkAsRead("N1", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsOkResponse()
    {
        // Arrange
        _notificationServiceMock
            .Setup(s => s.MarkAllAsReadAsync("member01", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.MarkAllAsRead(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CreateNotification_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateNotificationRequest("member01", "Thông báo mới", "Nội dung");
        var created = new NotificationDto("N99", "Thông báo mới", "Nội dung", false, new DateOnly(2026, 11, 15), "member01");

        _notificationServiceMock
            .Setup(s => s.CreateNotificationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _sut.CreateNotification(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<NotificationDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.NotificationId.Should().Be("N99");
    }
}
