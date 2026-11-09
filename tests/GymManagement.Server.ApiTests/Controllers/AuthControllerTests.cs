using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<ILogger<AuthController>> _loggerMock = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WhenValidCredentials_ReturnsOkResponseWithToken()
    {
        // Arrange
        var request = new LoginRequest("admin", "Admin@123");
        var expectedResponse = new LoginResponse("mock-jwt-token", "mock-refresh-token", "ADMIN01", "Administrator", "ADMIN");

        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<LoginResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Token.Should().Be("mock-jwt-token");
        response.Data.Role.Should().Be("ADMIN");
    }

    [Fact]
    public async Task GetCurrentUser_WhenValidBearerToken_ReturnsUserSession()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer valid-jwt-token";
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var expectedSession = new UserSessionDto("USER01", "Nguyen Van A", "a@gym.com", "MEMBER");
        _authServiceMock.Setup(s => s.ValidateTokenAsync("valid-jwt-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _sut.GetCurrentUser(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<UserSessionDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.UserId.Should().Be("USER01");
        response.Data.Email.Should().Be("a@gym.com");
    }

    [Fact]
    public async Task GetCurrentUser_WhenMissingBearerToken_ReturnsUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = await _sut.GetCurrentUser(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
