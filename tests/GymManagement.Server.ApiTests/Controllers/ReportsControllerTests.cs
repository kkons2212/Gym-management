using System.Text;
using FluentAssertions;
using GymManagement.Server.Api.Controllers;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GymManagement.Server.ApiTests.Controllers;

public sealed class ReportsControllerTests
{
    private readonly Mock<IReportService> _reportServiceMock = new();
    private readonly Mock<ILogger<ReportsController>> _loggerMock = new();
    private readonly ReportsController _sut;

    public ReportsControllerTests()
    {
        _sut = new ReportsController(_reportServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboardSummary_WhenCalled_ReturnsSummaryDto()
    {
        // Arrange
        var summary = new DashboardSummaryDto(
            TodayRevenue: 5000000m,
            TodayCheckIns: 45,
            NewMembersThisMonth: 12,
            CompletedPtSessions: 8,
            PeakHours: new List<PeakHourTrafficDto> { new(17, 20), new(18, 25) }
        );
        _reportServiceMock.Setup(s => s.GetDashboardSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _sut.GetDashboardSummary(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<DashboardSummaryDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.TodayRevenue.Should().Be(5000000m);
        response.Data.TodayCheckIns.Should().Be(45);
    }

    [Fact]
    public async Task ExportReport_WhenCalled_ReturnsFileContentResult()
    {
        // Arrange
        var request = new ExportReportRequest("REVENUE", "EXCEL");
        var exportResult = new ExportReportResultDto(
            FileName: "BaoCao_REVENUE_20261109.xlsx",
            ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileData: Encoding.UTF8.GetBytes("fake-excel-content")
        );

        _reportServiceMock.Setup(s => s.ExportReportAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exportResult);

        // Act
        var result = await _sut.ExportReport(request, CancellationToken.None);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileDownloadName.Should().Be("BaoCao_REVENUE_20261109.xlsx");
        fileResult.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fileResult.FileContents.Should().NotBeEmpty();
    }
}
