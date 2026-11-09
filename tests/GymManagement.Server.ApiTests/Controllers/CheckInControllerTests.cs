using System.Security.Claims;
using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class CheckInControllerTests
{
    private readonly Mock<ICheckInService> _checkInServiceMock = new();
    private readonly Mock<ILogger<CheckInController>> _loggerMock = new();
    private readonly CheckInController _sut;

    public CheckInControllerTests()
    {
        _sut = new CheckInController(_checkInServiceMock.Object, _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "ADMIN01"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "TestAuth"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CheckInWithQr_WhenValid_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new QrCheckInApiRequest("VALID_QR_CODE");
        var expectedResponse = new CheckInResponse(
            Success: true,
            MemberName: "Nguyen Van A",
            PlanName: "Gói Vàng",
            RemainingCheckInSessions: 19,
            RemainingPtSessions: 5,
            CheckInTime: DateTime.UtcNow,
            Message: "Điểm danh thành công."
        );

        _checkInServiceMock.Setup(s => s.CheckInWithQrAsync("VALID_QR_CODE", "ADMIN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.CheckInWithQr(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CheckInResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.MemberName.Should().Be("Nguyen Van A");
        response.Data.RemainingCheckInSessions.Should().Be(19);
    }

    [Fact]
    public async Task ManualCheckIn_WhenValid_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new ManualCheckInApiRequest("0901234567", "Check-in tai quay");
        var expectedResponse = new CheckInResponse(
            Success: true,
            MemberName: "Tran Van B",
            PlanName: "Gói Bạc",
            RemainingCheckInSessions: 10,
            RemainingPtSessions: 0,
            CheckInTime: DateTime.UtcNow,
            Message: "Điểm danh thành công."
        );

        _checkInServiceMock.Setup(s => s.ManualCheckInAsync("0901234567", "Check-in tai quay", "ADMIN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.ManualCheckIn(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<CheckInResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.MemberName.Should().Be("Tran Van B");
    }

    [Fact]
    public async Task GetAttendanceCount_WhenCalled_ReturnsCurrentCount()
    {
        // Arrange
        _checkInServiceMock.Setup(s => s.GetCurrentAttendanceCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        // Act
        var result = await _sut.GetAttendanceCount(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<int>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().Be(42);
    }
}
