using System.Security.Claims;
using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class SettingsControllerTests
{
    private readonly Mock<ISystemSettingService> _settingServiceMock = new();
    private readonly Mock<ILogger<SettingsController>> _loggerMock = new();
    private readonly SettingsController _sut;

    public SettingsControllerTests()
    {
        _sut = new SettingsController(_settingServiceMock.Object, _loggerMock.Object);

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
    public async Task GetUsers_WhenCalled_ReturnsUsersList()
    {
        // Arrange
        var list = new List<UserDto>
        {
            new("PT01", "PT", "Personal Trainer", "HLV The Hinh", "0901234567", "pt@gym.com", 1, "ACTIVE", new DateOnly(2026, 1, 1))
        };
        _settingServiceMock.Setup(s => s.GetUsersAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _sut.GetUsers(null, null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<UserDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreatePackagePlan_WhenValid_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreatePackagePlanRequest("PLAN01", "Gói VIP", "Mo ta", 2000000m, 30, 30, true, 10);
        var expectedPlan = new PackagePlanDto("PLAN01", "Gói VIP", "Mo ta", 2000000m, 30, 30, 10, true, "ACTIVE");

        _settingServiceMock.Setup(s => s.CreatePackagePlanAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPlan);

        // Act
        var result = await _sut.CreatePackagePlan(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<PackagePlanDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.PlanId.Should().Be("PLAN01");
        response.Data.Price.Should().Be(2000000m);
    }

    [Fact]
    public async Task DeletePackagePlan_WhenCalled_ReturnsSuccessMessage()
    {
        // Arrange
        var deleteResult = new DeletePackageResultDto("PLAN01", false, true, "Đã ngừng kinh doanh gói tập thành công.");
        _settingServiceMock.Setup(s => s.DeletePackagePlanAsync("PLAN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        // Act
        var result = await _sut.DeletePackagePlan("PLAN01", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<DeletePackageResultDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.IsDiscontinued.Should().BeTrue();
        response.Message.Should().Be("Đã ngừng kinh doanh gói tập thành công.");
    }
}
