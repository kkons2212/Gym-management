using System.IO.Compression;
using System.Text;
using FluentAssertions;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.UnitTests.Services;

/// <summary>
/// Bộ kiểm thử đơn vị cho ReportService Component (Task 2.7 - Epic 06 / UC06).
/// Tuân thủ quy chuẩn AAA (Arrange - Act - Assert), không sử dụng toán tử 3 ngôi hay expression-bodied method.
/// </summary>
public sealed class ReportServiceTests
{
    // =========================================================================
    // 1. CONSTANTS & PRIVATE FIELDS (Đầu file)
    // =========================================================================

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<ReportService>> _loggerMock = new();

    private readonly Mock<IGenericRepository<Invoice>> _invoiceRepoMock = new();
    private readonly Mock<IGenericRepository<MemberSubscription>> _subscriptionRepoMock = new();
    private readonly Mock<IGenericRepository<CheckInLog>> _checkInRepoMock = new();
    private readonly Mock<IGenericRepository<PTTrainingSession>> _ptSessionRepoMock = new();
    private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
    private readonly Mock<IGenericRepository<PackagePlan>> _planRepoMock = new();

    private readonly List<Invoice> _invoices = [];
    private readonly List<MemberSubscription> _subscriptions = [];
    private readonly List<CheckInLog> _checkIns = [];
    private readonly List<PTTrainingSession> _ptSessions = [];
    private readonly List<User> _users = [];
    private readonly List<PackagePlan> _plans = [];

    private readonly ReportService _sut;

    // =========================================================================
    // 2. CONSTRUCTOR & SETUP
    // =========================================================================

    public ReportServiceTests()
    {
        _invoiceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_invoices);
        _subscriptionRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_subscriptions);
        _checkInRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_checkIns);
        _ptSessionRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_ptSessions);
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_users);
        _planRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_plans);

        _unitOfWorkMock.Setup(u => u.Repository<Invoice>()).Returns(_invoiceRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<MemberSubscription>()).Returns(_subscriptionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<CheckInLog>()).Returns(_checkInRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<PTTrainingSession>()).Returns(_ptSessionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<PackagePlan>()).Returns(_planRepoMock.Object);

        _sut = new ReportService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // 3. TEST CASES: KHỞI TẠO & KIỂM TRA PHỤ THUỘC (CONSTRUCTOR)
    // =========================================================================

    [Fact]
    public void Constructor_WhenNullArguments_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act1 = () => new ReportService(null!, _loggerMock.Object);
        var act2 = () => new ReportService(_unitOfWorkMock.Object, null!);

        // Assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    // =========================================================================
    // 4. TEST CASES: DASHBOARD TỔNG QUAN
    // =========================================================================

    [Fact]
    public async Task GetDashboardSummaryAsync_WhenCalled_AggregatesTodayRevenueCheckInsNewMembersAndSessionsCorrectly()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);

        _invoices.Add(new Invoice
        {
            InvoiceId = "INV01",
            SubtotalAmount = 1500000m,
            RefundAmount = 100000m,
            Status = "PAID",
            PaidAt = today
        });
        _invoices.Add(new Invoice
        {
            InvoiceId = "INV02",
            SubtotalAmount = 500000m,
            Status = "PENDING",
            PaidAt = today
        });

        _checkIns.Add(new CheckInLog
        {
            CheckInId = "CK00000001",
            CheckInAt = today,
            Status = "SUCCESS"
        });
        _checkIns.Add(new CheckInLog
        {
            CheckInId = "CK00000002",
            CheckInAt = today,
            Status = "DENIED"
        });

        _users.Add(new User
        {
            UserName = "MEMBER01",
            RoleId = "MEMBER",
            CreatedAt = firstDayOfMonth
        });

        _ptSessions.Add(new PTTrainingSession
        {
            PTSessionId = "PTS01",
            SessionStartTime = today,
            CompletedAt = today,
            Status = "COMPLETED"
        });

        // Act
        var result = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TodayRevenue.Should().Be(1400000m);
        result.TodayCheckIns.Should().Be(1);
        result.NewMembersThisMonth.Should().Be(1);
        result.CompletedPtSessions.Should().Be(1);
        result.PeakHours.Should().NotBeEmpty();
    }

    // =========================================================================
    // 5. TEST CASES: BÁO CÁO DOANH THU THỰC THU
    // =========================================================================

    [Fact]
    public async Task GetRevenueReportAsync_WhenFilterApplied_CalculatesNetRevenueAndPercentageBreakdownCorrectly()
    {
        // Arrange
        DateOnly from = new DateOnly(2026, 11, 1);
        DateOnly to = new DateOnly(2026, 11, 10);

        _subscriptions.Add(new MemberSubscription { SubscriptionId = "SUB01", PlanId = "PLAN01" });
        _plans.Add(new PackagePlan { PlanId = "PLAN01", Name = "Goi Vang" });

        _invoices.Add(new Invoice
        {
            InvoiceId = "INV01",
            SubscriptionId = "SUB01",
            SubtotalAmount = 2000000m,
            RefundAmount = 200000m,
            PaymentMethod = "VIETQR",
            Status = "PAID",
            PaidAt = new DateOnly(2026, 11, 5)
        });
        _invoices.Add(new Invoice
        {
            InvoiceId = "INV02",
            SubscriptionId = "SUB01",
            SubtotalAmount = 1000000m,
            RefundAmount = 0m,
            PaymentMethod = "CASH",
            Status = "PAID",
            PaidAt = new DateOnly(2026, 11, 6)
        });

        var request = new RevenueReportRequest(FromDate: from, ToDate: to);

        // Act
        var result = await _sut.GetRevenueReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalGrossRevenue.Should().Be(3000000m);
        result.TotalRefundAmount.Should().Be(200000m);
        result.TotalNetRevenue.Should().Be(2800000m);
        result.InvoiceCount.Should().Be(2);

        result.ByPaymentMethod.Should().HaveCount(2);
        result.ByPackage.Should().HaveCount(1);
        result.ByPackage[0].PlanName.Should().Be("Goi Vang");
        result.Trend.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRevenueReportAsync_WhenFromDateGreaterThanToDate_ThrowsValidationException()
    {
        // Arrange
        var request = new RevenueReportRequest(
            FromDate: new DateOnly(2026, 11, 10),
            ToDate: new DateOnly(2026, 11, 1));

        // Act
        var act = () => _sut.GetRevenueReportAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Ngày bắt đầu không được lớn hơn ngày kết thúc*");
    }

    [Fact]
    public async Task GetRevenueReportAsync_WhenNoInvoicesMatch_ReturnsZeroAggregationsWithoutDivisionByZero()
    {
        // Arrange
        var request = new RevenueReportRequest(
            FromDate: new DateOnly(2026, 1, 1),
            ToDate: new DateOnly(2026, 1, 31));

        // Act
        var result = await _sut.GetRevenueReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalGrossRevenue.Should().Be(0m);
        result.TotalRefundAmount.Should().Be(0m);
        result.TotalNetRevenue.Should().Be(0m);
        result.InvoiceCount.Should().Be(0);
        result.ByPaymentMethod.Should().BeEmpty();
        result.ByPackage.Should().BeEmpty();
        result.Trend.Should().BeEmpty();
    }

    // =========================================================================
    // 6. TEST CASES: BÁO CÁO BIẾN ĐỘNG HỘI VIÊN
    // =========================================================================

    [Fact]
    public async Task GetMemberFluctuationReportAsync_WhenDataExists_CalculatesRetentionRateAndPackageDistribution()
    {
        // Arrange
        DateOnly from = new DateOnly(2026, 11, 1);
        DateOnly to = new DateOnly(2026, 11, 30);

        _plans.Add(new PackagePlan { PlanId = "PLAN01", Name = "Goi Standard" });

        _subscriptions.Add(new MemberSubscription
        {
            SubscriptionId = "SUB01",
            UserName = "MEM01",
            PlanId = "PLAN01",
            Status = "ACTIVE",
            CreatedAt = new DateOnly(2026, 11, 5)
        });
        _subscriptions.Add(new MemberSubscription
        {
            SubscriptionId = "SUB02",
            UserName = "MEM02",
            PlanId = "PLAN01",
            Status = "FROZEN",
            CreatedAt = new DateOnly(2026, 11, 10)
        });
        _subscriptions.Add(new MemberSubscription
        {
            SubscriptionId = "SUB03",
            UserName = "MEM03",
            PlanId = "PLAN01",
            Status = "CANCELLED",
            CreatedAt = new DateOnly(2026, 11, 15)
        });

        _invoices.Add(new Invoice
        {
            InvoiceId = "INV_RENEW",
            TransactionType = "SUBSCRIPTION_RENEWAL",
            Status = "PAID",
            PaidAt = new DateOnly(2026, 11, 20)
        });

        var request = new MemberFluctuationRequest(from, to);

        // Act
        var result = await _sut.GetMemberFluctuationReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalActiveMembers.Should().Be(1);
        result.NewMembers.Should().Be(1);
        result.Renewals.Should().Be(1);
        result.FrozenCount.Should().Be(1);
        result.CancelledCount.Should().Be(1);
        result.RetentionRatePercentage.Should().Be(50m);
        result.PackageDistribution.Should().HaveCount(1);
        result.PackageDistribution[0].ActiveCount.Should().Be(1);
    }

    [Fact]
    public async Task GetMemberFluctuationReportAsync_WhenFromDateGreaterThanToDate_ThrowsValidationException()
    {
        // Arrange
        var request = new MemberFluctuationRequest(
            FromDate: new DateOnly(2026, 12, 1),
            ToDate: new DateOnly(2026, 11, 1));

        // Act
        var act = () => _sut.GetMemberFluctuationReportAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    // =========================================================================
    // 7. TEST CASES: BÁO CÁO LƯU LƯỢNG CHECK-IN
    // =========================================================================

    [Fact]
    public async Task GetCheckInTrafficReportAsync_WhenDataExists_Groups24HourTrafficAndDenialReasons()
    {
        // Arrange
        DateOnly date = new DateOnly(2026, 11, 5);
        _checkIns.Add(new CheckInLog
        {
            CheckInId = "CK10000001",
            CheckInAt = date,
            Status = "SUCCESS"
        });
        _checkIns.Add(new CheckInLog
        {
            CheckInId = "CK10000002",
            CheckInAt = date,
            Status = "DENIED",
            DenialReason = "Gói tập đã hết hạn"
        });

        var request = new CheckInTrafficRequest(FromDate: date, ToDate: date);

        // Act
        var result = await _sut.GetCheckInTrafficReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCheckIns.Should().Be(2);
        result.SuccessfulCheckIns.Should().Be(1);
        result.DeniedCheckIns.Should().Be(1);
        result.SuccessRatePercentage.Should().Be(50m);
        result.HourlyDistribution.Should().HaveCount(24);
        result.TopDenialReasons.Should().HaveCount(1);
        result.TopDenialReasons[0].Reason.Should().Be("Gói tập đã hết hạn");
    }

    [Fact]
    public async Task GetCheckInTrafficReportAsync_WhenFromDateGreaterThanToDate_ThrowsValidationException()
    {
        // Arrange
        var request = new CheckInTrafficRequest(
            FromDate: new DateOnly(2026, 11, 10),
            ToDate: new DateOnly(2026, 11, 5));

        // Act
        var act = () => _sut.GetCheckInTrafficReportAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    // =========================================================================
    // 8. TEST CASES: BÁO CÁO HIỆU SUẤT PT
    // =========================================================================

    [Fact]
    public async Task GetPtPerformanceReportAsync_WhenFilteredByTrainer_ReturnsOnlyTrainerMetrics()
    {
        // Arrange
        DateOnly date = new DateOnly(2026, 11, 5);

        _users.Add(new User
        {
            UserName = "PT01",
            FullName = "Nguyen Van Coach",
            RoleId = "PT"
        });

        _ptSessions.Add(new PTTrainingSession
        {
            PTSessionId = "PTS01",
            PtId = "PT01",
            MemberId = "MEM01",
            SessionStartTime = date,
            Status = "COMPLETED",
            RatingStars = 5
        });
        _ptSessions.Add(new PTTrainingSession
        {
            PTSessionId = "PTS02",
            PtId = "PT01",
            MemberId = "MEM02",
            SessionStartTime = date,
            Status = "CANCELLED"
        });
        _ptSessions.Add(new PTTrainingSession
        {
            PTSessionId = "PTS03",
            PtId = "PT02",
            MemberId = "MEM03",
            SessionStartTime = date,
            Status = "COMPLETED",
            RatingStars = 4
        });

        var request = new PtPerformanceRequest(FromDate: date, ToDate: date, TrainerId: "PT01");

        // Act
        var result = await _sut.GetPtPerformanceReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalSessionsScheduled.Should().Be(2);
        result.TotalSessionsCompleted.Should().Be(1);
        result.TotalSessionsCancelled.Should().Be(1);
        result.CompletionRatePercentage.Should().Be(50m);
        result.OverallAverageRating.Should().Be(5.0m);
        result.Trainers.Should().HaveCount(1);
        result.Trainers[0].TrainerName.Should().Be("Nguyen Van Coach");
        result.Trainers[0].AverageRating.Should().Be(5.0m);
    }

    [Fact]
    public async Task GetPtPerformanceReportAsync_WhenNoRatingRecorded_HandlesZeroAverageCleanly()
    {
        // Arrange
        DateOnly date = new DateOnly(2026, 11, 5);

        _ptSessions.Add(new PTTrainingSession
        {
            PTSessionId = "PTS01",
            PtId = "PT01",
            MemberId = "MEM01",
            SessionStartTime = date,
            Status = "COMPLETED",
            RatingStars = null
        });

        var request = new PtPerformanceRequest(FromDate: date, ToDate: date);

        // Act
        var result = await _sut.GetPtPerformanceReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OverallAverageRating.Should().Be(0m);
        result.Trainers[0].AverageRating.Should().Be(0m);
    }

    // =========================================================================
    // 9. TEST CASES: XUẤT BÁO CÁO (EXCEL / PDF)
    // =========================================================================

    [Fact]
    public async Task ExportReportAsync_WhenExcelRequested_GeneratesValidZipArchiveStreamWithExpectedFileName()
    {
        // Arrange
        _invoices.Add(new Invoice
        {
            InvoiceId = "INV01",
            SubtotalAmount = 1000000m,
            Status = "PAID",
            PaidAt = new DateOnly(2026, 11, 5)
        });

        var request = new ExportReportRequest(
            ReportType: "REVENUE",
            Format: "EXCEL",
            FromDate: new DateOnly(2026, 11, 1),
            ToDate: new DateOnly(2026, 11, 10));

        // Act
        var result = await _sut.ExportReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().StartWith("BaoCao_REVENUE_").And.EndWith(".xlsx");
        result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        result.FileData.Should().NotBeEmpty();

        // Kiểm tra xem FileData có phải là file Zip/OpenXML hợp lệ
        using var stream = new MemoryStream(result.FileData);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.Entries.Should().Contain(e => e.FullName == "[Content_Types].xml");
        archive.Entries.Should().Contain(e => e.FullName == "xl/workbook.xml");
        archive.Entries.Should().Contain(e => e.FullName == "xl/worksheets/sheet1.xml");
    }

    [Fact]
    public async Task ExportReportAsync_WhenPdfRequested_GeneratesValidPdfStreamWithHeaderAndContent()
    {
        // Arrange
        _checkIns.Add(new CheckInLog
        {
            CheckInId = "CK001",
            CheckInAt = new DateOnly(2026, 11, 5),
            Status = "SUCCESS"
        });

        var request = new ExportReportRequest(
            ReportType: "CHECKIN",
            Format: "PDF",
            FromDate: new DateOnly(2026, 11, 1),
            ToDate: new DateOnly(2026, 11, 10));

        // Act
        var result = await _sut.ExportReportAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().StartWith("BaoCao_CHECKIN_").And.EndWith(".pdf");
        result.ContentType.Should().Be("application/pdf");
        result.FileData.Should().NotBeEmpty();

        // Kiểm tra xem FileData có chứa chữ ký tiêu chuẩn của file PDF (%PDF-1.4)
        string pdfContent = Encoding.ASCII.GetString(result.FileData);
        pdfContent.Should().StartWith("%PDF-1.4");
        pdfContent.Should().Contain("%%EOF");
    }

    [Fact]
    public async Task ExportReportAsync_WhenReportTypeInvalid_ThrowsValidationException()
    {
        // Arrange
        var request = new ExportReportRequest(
            ReportType: "INVALID_REPORT",
            Format: "EXCEL");

        // Act
        var act = () => _sut.ExportReportAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Loại báo cáo*không hợp lệ*");
    }

    [Fact]
    public async Task ExportReportAsync_WhenFormatInvalid_ThrowsValidationException()
    {
        // Arrange
        var request = new ExportReportRequest(
            ReportType: "REVENUE",
            Format: "WORD");

        // Act
        var act = () => _sut.ExportReportAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Định dạng xuất không hợp lệ*");
    }

    [Fact]
    public async Task ExportReportAsync_WhenNullRequest_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _sut.ExportReportAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

