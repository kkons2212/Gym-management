using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class PtControllerTests
{
    private readonly Mock<IPtService> _ptServiceMock = new();
    private readonly Mock<ILogger<PtController>> _loggerMock = new();
    private readonly PtController _sut;

    public PtControllerTests()
    {
        _sut = new PtController(_ptServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetTrainers_WhenCalled_ReturnsTrainersList()
    {
        // Arrange
        var list = new List<PtProfileDto>
        {
            new("PT01", "HLV The Hinh", "0912345678", "pt@gym.com", "The Hinh", 5, 4.8m, 10, true, 5, 2)
        };
        _ptServiceMock.Setup(s => s.GetTrainersAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _sut.GetTrainers(true, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IReadOnlyList<PtProfileDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data![0].PtId.Should().Be("PT01");
    }

    [Fact]
    public async Task BookSession_WhenValidRequest_ReturnsBookedSession()
    {
        // Arrange
        var request = new BookSessionRequest("MEM01", "SLOT01", "Ghi chu tap luyen");
        var expectedSession = new PtSessionDto(
            SessionId: "PTS01",
            PtId: "PT01",
            PtName: "HLV Nguyen Van C",
            MemberUserName: "MEM01",
            MemberFullName: "Tran Thi D",
            SessionStartTime: DateTime.UtcNow,
            SessionEndTime: DateTime.UtcNow.AddHours(1),
            Status: PtSessionStatus.Booked,
            WorkoutNotes: "Ghi chu tap luyen",
            RatingStars: null,
            ReviewComment: null,
            CompletedAt: null
        );

        _ptServiceMock.Setup(s => s.BookSessionAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _sut.BookSession(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PtSessionDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.SessionId.Should().Be("PTS01");
        response.Data.Status.Should().Be(PtSessionStatus.Booked);
    }
}
