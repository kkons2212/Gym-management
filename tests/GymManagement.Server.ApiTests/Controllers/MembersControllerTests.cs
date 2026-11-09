using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class MembersControllerTests
{
    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<ILogger<MembersController>> _loggerMock = new();
    private readonly MembersController _sut;

    public MembersControllerTests()
    {
        _sut = new MembersController(_memberServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchMembers_WhenCalled_ReturnsOkWithList()
    {
        // Arrange
        var list = new List<MemberSearchResponse>
        {
            new("MEM01", "Nguyen Van A", "0901234567", "a@gym.com", "ACTIVE", "SUB01", "GOLD", new DateOnly(2026, 12, 1), 10, 2)
        };
        _memberServiceMock.Setup(s => s.SearchMembersAsync("Nguyen", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _sut.SearchMembers("Nguyen", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<MemberSearchResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProfile_WhenMemberExists_ReturnsProfile()
    {
        // Arrange
        var profile = new MemberProfileResponse(
            UserName: "MEM01",
            FullName: "Nguyen Van A",
            Phone: "0901234567",
            Email: "a@gym.com",
            Status: "ACTIVE",
            ActiveSubscription: null
        );
        _memberServiceMock.Setup(s => s.GetMemberProfileAsync("MEM01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _sut.GetProfile("MEM01", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<MemberProfileResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.UserName.Should().Be("MEM01");
    }

    [Fact]
    public async Task RegisterMember_WhenValid_ReturnsCreatedResponse()
    {
        // Arrange
        var request = new RegisterMemberRequest("Nguyen Van A", "0901234567", "a@gym.com", "PLAN01", new DateOnly(2026, 11, 1), "ADMIN01");
        var profile = new MemberProfileResponse("MEM01", "Nguyen Van A", "0901234567", "a@gym.com", "ACTIVE", null);

        _memberServiceMock.Setup(s => s.RegisterMemberAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _sut.RegisterMember(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<MemberProfileResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.UserName.Should().Be("MEM01");
    }
}
