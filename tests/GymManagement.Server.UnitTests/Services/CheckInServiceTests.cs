using System.Linq.Expressions;
using FluentAssertions;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.UnitTests.Services;

/// <summary>
/// Unit Tests cho CheckInService — UC03 (US 03.1, 03.2, 03.3).
/// Tuân thủ dev_rule Ch.6: Naming = [MethodUnderTest]_[StateUnderTest]_[ExpectedBehavior]
/// Mô hình: Arrange – Act – Assert (AAA)
/// </summary>
public class CheckInServiceTests
{
    // =========================================================================
    // 1. TEST FIXTURES & SUT
    // =========================================================================
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IGenericRepository<User>> _userRepoMock;
    private readonly Mock<IGenericRepository<MemberSubscription>> _subRepoMock;
    private readonly Mock<IGenericRepository<CheckInLog>> _checkInRepoMock;
    private readonly Mock<IGenericRepository<PackagePlan>> _planRepoMock;
    private readonly Mock<ICameraScannerAdapter> _cameraAdapterMock;
    private readonly Mock<ILogger<CheckInService>> _loggerMock;
    private readonly CheckInService _sut;

    public CheckInServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IGenericRepository<User>>();
        _subRepoMock = new Mock<IGenericRepository<MemberSubscription>>();
        _checkInRepoMock = new Mock<IGenericRepository<CheckInLog>>();
        _planRepoMock = new Mock<IGenericRepository<PackagePlan>>();
        _cameraAdapterMock = new Mock<ICameraScannerAdapter>();
        _loggerMock = new Mock<ILogger<CheckInService>>();

        _uowMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.Repository<MemberSubscription>()).Returns(_subRepoMock.Object);
        _uowMock.Setup(u => u.Repository<CheckInLog>()).Returns(_checkInRepoMock.Object);
        _uowMock.Setup(u => u.Repository<PackagePlan>()).Returns(_planRepoMock.Object);

        // ExecuteTransactionAsync passes through the action
        _uowMock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task<CheckInResponse>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<CheckInResponse>>, CancellationToken>((action, _) => action());

        _sut = new CheckInService(_uowMock.Object, _cameraAdapterMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // 2. CHECKINASYNC — VALIDATION & GUARD CLAUSES
    // =========================================================================

    [Fact]
    public async Task CheckInAsync_WhenBothQrAndPhoneEmpty_ThrowsValidationException()
    {
        // Arrange
        var request = new CheckInRequest(QrCode: "", Phone: "");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Vui lòng cung cấp mã QR (từ webcam) hoặc mã hội viên / số điện thoại để điểm danh.*");
    }

    [Fact]
    public async Task CheckInAsync_WhenUserNotFoundByPhone_ThrowsNotFoundException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var request = new CheckInRequest(Phone: "0999999999");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Không tìm thấy thông tin hội viên tương ứng.*");
    }

    [Fact]
    public async Task CheckInAsync_WhenNoSubscriptionFound_ThrowsNotFoundException()
    {
        // Arrange
        var user = CreateTestUser("user01", "Nguyễn Văn A", "0901234567");
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([user]);

        _subRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var request = new CheckInRequest(Phone: "0901234567");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*chưa đăng ký gói tập nào.*");
    }

    // =========================================================================
    // 3. CHECKINASYNC — SUBSCRIPTION RULES & EXCEPTIONS
    // =========================================================================

    [Fact]
    public async Task CheckInAsync_WhenSubscriptionIsFrozen_ThrowsBusinessRuleExceptionAndRecordsDeniedLog()
    {
        // Arrange
        var user = CreateTestUser("user01", "Nguyễn Văn A", "0901234567");
        var sub = CreateTestSubscription("sub01", "user01", status: "FROZEN", remainingSessions: 10);

        SetupUserAndSubscription(user, sub);

        var request = new CheckInRequest(Phone: "0901234567", VerifiedByAdminId: "admin01");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đang tạm ngưng bảo lưu (FROZEN).*");

        _checkInRepoMock.Verify(r => r.AddAsync(
            It.Is<CheckInLog>(l => l.Status == "DENIED" && l.DenialReason!.Contains("FROZEN")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenSubscriptionIsCancelled_ThrowsBusinessRuleExceptionAndRecordsDeniedLog()
    {
        // Arrange
        var user = CreateTestUser("user02", "Trần Thị B", "0902345678");
        var sub = CreateTestSubscription("sub02", "user02", status: "CANCELLED", remainingSessions: 5);

        SetupUserAndSubscription(user, sub);

        var request = new CheckInRequest(Phone: "0902345678");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã bị hủy.*");

        _checkInRepoMock.Verify(r => r.AddAsync(
            It.Is<CheckInLog>(l => l.Status == "DENIED" && l.DenialReason!.Contains("đã bị hủy")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenSubscriptionIsExpiredByStatus_ThrowsBusinessRuleExceptionAndRecordsDeniedLog()
    {
        // Arrange
        var user = CreateTestUser("user03", "Lê Văn C", "0903456789");
        var sub = CreateTestSubscription("sub03", "user03", status: "EXPIRED", remainingSessions: 5);

        SetupUserAndSubscription(user, sub);

        var request = new CheckInRequest(Phone: "0903456789");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã hết hạn*");

        _checkInRepoMock.Verify(r => r.AddAsync(
            It.Is<CheckInLog>(l => l.Status == "DENIED" && l.DenialReason!.Contains("hết hạn")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenEndDateIsInPast_ThrowsBusinessRuleExceptionAndUpdatesStatusToExpired()
    {
        // Arrange
        var user = CreateTestUser("user04", "Phạm Văn D", "0904567890");
        DateOnly pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
        var sub = CreateTestSubscription("sub04", "user04", status: "ACTIVE", remainingSessions: 8, endDate: pastDate);

        SetupUserAndSubscription(user, sub);

        var request = new CheckInRequest(Phone: "0904567890");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã hết hạn*");

        sub.Status.Should().Be("EXPIRED");
        _subRepoMock.Verify(r => r.Update(sub), Times.Once);
        _checkInRepoMock.Verify(r => r.AddAsync(
            It.Is<CheckInLog>(l => l.Status == "DENIED"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenRemainingSessionsIsZero_ThrowsBusinessRuleExceptionAndRecordsDeniedLog()
    {
        // Arrange
        var user = CreateTestUser("user05", "Hoàng Văn E", "0905678901");
        var sub = CreateTestSubscription("sub05", "user05", status: "ACTIVE", remainingSessions: 0);

        SetupUserAndSubscription(user, sub);

        var request = new CheckInRequest(Phone: "0905678901");

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã hết số lượt vào phòng khả dụng.*");

        _checkInRepoMock.Verify(r => r.AddAsync(
            It.Is<CheckInLog>(l => l.Status == "DENIED" && l.DenialReason!.Contains("hết số lượt")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================================
    // 4. CHECKINASYNC — SUCCESSFUL CHECK-IN (QR & PHONE)
    // =========================================================================

    [Fact]
    public async Task CheckInAsync_WhenValidPhone_DeductsSessionAndRecordsSuccessLog()
    {
        // Arrange
        var user = CreateTestUser("user06", "Vũ Thị F", "0906789012");
        var sub = CreateTestSubscription("sub06", "user06", status: "ACTIVE", remainingSessions: 10);
        var plan = new PackagePlan { PlanId = "PLAN01", Name = "Gói Gym VIP 1 Tháng" };

        SetupUserAndSubscription(user, sub);
        _planRepoMock.Setup(r => r.GetByIdAsync("PLAN01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var request = new CheckInRequest(Phone: "0906789012", VerifiedByAdminId: "admin99");

        // Act
        var response = await _sut.CheckInAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.MemberName.Should().Be("Vũ Thị F");
        response.PlanName.Should().Be("Gói Gym VIP 1 Tháng");
        response.RemainingCheckInSessions.Should().Be(9);
        response.Message.Should().Contain("Điểm danh thành công");

        sub.RemainingCheckInSessions.Should().Be(9);
        _subRepoMock.Verify(r => r.Update(sub), Times.Once);
        _checkInRepoMock.Verify(r => r.AddAsync(
            It.Is<CheckInLog>(l => l.Status == "SUCCESS" && l.SubscriptionId == "sub06" && l.VerifiedByAdminId == "admin99"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_WhenValidStaticQrCode_DeductsSessionAndRecordsSuccessLog()
    {
        // Arrange
        const string staticQr = "GYM-STATIC-QR-TOKEN-12345";
        var user = CreateTestUser("user07", "Đỗ Văn G", "0907890123");
        var sub = CreateTestSubscription("sub07", "user07", status: "ACTIVE", remainingSessions: 5, qrCode: staticQr);

        _subRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([sub]);
        _userRepoMock.Setup(r => r.GetByIdAsync("user07", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = new CheckInRequest(QrCode: staticQr);

        // Act
        var response = await _sut.CheckInAsync(request);

        // Assert
        response.Success.Should().BeTrue();
        response.MemberName.Should().Be("Đỗ Văn G");
        response.RemainingCheckInSessions.Should().Be(4);
    }

    [Fact]
    public async Task CheckInAsync_WhenDynamicQrCodeIsExpired_ThrowsBusinessRuleException()
    {
        // Arrange
        const string dynamicQr = "GYM-user08-638000000000-INVALID";
        _cameraAdapterMock.Setup(c => c.ValidateCheckInToken(dynamicQr, It.IsAny<int>()))
            .Returns(new QrTokenValidationResult(IsValid: false, UserId: "user08", CreatedAt: DateTime.UtcNow.AddMinutes(-5), IsExpired: true, FailureReason: "Mã QR đã hết hạn."));

        var request = new CheckInRequest(QrCode: dynamicQr);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*hết hạn*");
    }

    [Fact]
    public async Task CheckInAsync_WhenDynamicQrCodeIsInvalid_ThrowsBusinessRuleException()
    {
        // Arrange
        const string dynamicQr = "GYM-CORRUPTED-TOKEN";
        _cameraAdapterMock.Setup(c => c.ValidateCheckInToken(dynamicQr, It.IsAny<int>()))
            .Returns(new QrTokenValidationResult(IsValid: false, UserId: null, CreatedAt: null, IsExpired: false, FailureReason: "Sai định dạng mã QR."));

        var request = new CheckInRequest(QrCode: dynamicQr);

        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInAsync(request);
        };

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Sai định dạng mã QR.*");
    }

    [Fact]
    public async Task CheckInAsync_WhenDynamicQrCodeIsValid_PerformsSuccessfulCheckIn()
    {
        // Arrange
        const string dynamicQr = "GYM-user09-VALID-PAYLOAD";
        var user = CreateTestUser("user09", "Ngô Văn H", "0908901234");
        var sub = CreateTestSubscription("sub09", "user09", status: "ACTIVE", remainingSessions: 20);

        _cameraAdapterMock.Setup(c => c.ValidateCheckInToken(dynamicQr, It.IsAny<int>()))
            .Returns(new QrTokenValidationResult(IsValid: true, UserId: "user09", CreatedAt: DateTime.UtcNow, IsExpired: false));

        _userRepoMock.Setup(r => r.GetByIdAsync("user09", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _subRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([sub]);

        var request = new CheckInRequest(QrCode: dynamicQr);

        // Act
        var response = await _sut.CheckInAsync(request);

        // Assert
        response.Success.Should().BeTrue();
        response.MemberName.Should().Be("Ngô Văn H");
        response.RemainingCheckInSessions.Should().Be(19);
    }

    // =========================================================================
    // 5. CONVENIENCE WRAPPER METHODS (CheckInWithQrAsync & ManualCheckInAsync)
    // =========================================================================

    [Fact]
    public async Task CheckInWithQrAsync_WhenValid_ReturnsSuccessResponse()
    {
        // Arrange
        const string qrCode = "STATIC-QR-TEST";
        var user = CreateTestUser("user10", "Dương Thị K", "0909012345");
        var sub = CreateTestSubscription("sub10", "user10", status: "ACTIVE", remainingSessions: 7, qrCode: qrCode);

        _subRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([sub]);
        _userRepoMock.Setup(r => r.GetByIdAsync("user10", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var response = await _sut.CheckInWithQrAsync(qrCode, verifiedByAdminId: "admin01");

        // Assert
        response.Success.Should().BeTrue();
        response.MemberName.Should().Be("Dương Thị K");
    }

    [Fact]
    public async Task CheckInWithQrAsync_WhenQrEmpty_ThrowsValidationException()
    {
        // Act
        Func<Task> act = async () =>
        {
            await _sut.CheckInWithQrAsync("");
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Mã QR điểm danh không được để trống.*");
    }

    [Fact]
    public async Task ManualCheckInAsync_WhenValid_ReturnsSuccessResponse()
    {
        // Arrange
        var user = CreateTestUser("user11", "Trịnh Văn L", "0910123456");
        var sub = CreateTestSubscription("sub11", "user11", status: "ACTIVE", remainingSessions: 12);

        SetupUserAndSubscription(user, sub);

        // Act
        var response = await _sut.ManualCheckInAsync("0910123456", notes: "Khách quên điện thoại", verifiedByAdminId: "admin01");

        // Assert
        response.Success.Should().BeTrue();
        response.MemberName.Should().Be("Trịnh Văn L");
        response.RemainingCheckInSessions.Should().Be(11);
    }

    [Fact]
    public async Task ManualCheckInAsync_WhenMemberCode_ReturnsSuccessResponse()
    {
        // Arrange
        var user = CreateTestUser("user12", "Lê Thị B", "0910999888");
        var sub = CreateTestSubscription("sub12", "user12", status: "ACTIVE", remainingSessions: 5);

        SetupUserAndSubscription(user, sub);

        // Act
        var response = await _sut.ManualCheckInAsync("user12", notes: "Điểm danh qua mã hội viên", verifiedByAdminId: "admin01");

        // Assert
        response.Success.Should().BeTrue();
        response.MemberName.Should().Be("Lê Thị B");
        response.RemainingCheckInSessions.Should().Be(4);
    }

    [Fact]
    public async Task ManualCheckInAsync_WhenPhoneEmpty_ThrowsValidationException()
    {
        // Act
        Func<Task> act = async () =>
        {
            await _sut.ManualCheckInAsync("");
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Số điện thoại hoặc mã hội viên không được để trống.*");
    }

    // =========================================================================
    // 6. HISTORY & ATTENDANCE (GetTodayCheckInLogsAsync & GetCurrentAttendanceCountAsync)
    // =========================================================================

    [Fact]
    public async Task GetTodayCheckInLogsAsync_WhenLogsExist_ReturnsOrderedLogs()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var users = new List<User>
        {
            CreateTestUser("user01", "Hội Viên Một", "0901111111"),
            CreateTestUser("user02", "Hội Viên Hai", "0902222222"),
            CreateTestUser("admin01", "Admin Quản Trị", "0903333333")
        };

        var logs = new List<CheckInLog>
        {
            new() { CheckInId = "CK00000001", check_in_at = today, Status = "SUCCESS", SubscriptionId = "sub01", UserName = "user01", VerifiedByAdminId = "admin01" },
            new() { CheckInId = "CK00000002", check_in_at = today, Status = "DENIED", DenialReason = "Hết hạn", SubscriptionId = "sub02", UserName = "user02", VerifiedByAdminId = "admin01" }
        };

        _checkInRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CheckInLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetTodayCheckInLogsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Any(r => r.MemberName == "Hội Viên Một" && r.Status == "SUCCESS").Should().BeTrue();
        result.Any(r => r.MemberName == "Hội Viên Hai" && r.Status == "DENIED").Should().BeTrue();
    }

    [Fact]
    public async Task GetTodayCheckInLogsAsync_WhenStatusFilterApplied_ReturnsMatchingLogs()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var users = new List<User>
        {
            CreateTestUser("user01", "Hội Viên Một", "0901111111"),
            CreateTestUser("user02", "Hội Viên Hai", "0902222222")
        };

        var logs = new List<CheckInLog>
        {
            new() { CheckInId = "CK00000001", check_in_at = today, Status = "SUCCESS", SubscriptionId = "sub01", UserName = "user01" },
            new() { CheckInId = "CK00000002", check_in_at = today, Status = "DENIED", DenialReason = "Bảo lưu", SubscriptionId = "sub02", UserName = "user02" }
        };

        _checkInRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CheckInLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetTodayCheckInLogsAsync(statusFilter: "SUCCESS");

        // Assert
        result.Should().HaveCount(1);
        result[0].MemberName.Should().Be("Hội Viên Một");
        result[0].Status.Should().Be("SUCCESS");
    }

    [Fact]
    public async Task GetCurrentAttendanceCountAsync_ReturnsCountOfTodaySuccessfulCheckIns()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var logs = new List<CheckInLog>
        {
            new() { CheckInId = "CK00000001", check_in_at = today, Status = "SUCCESS", SubscriptionId = "sub01", UserName = "user01" },
            new() { CheckInId = "CK00000002", check_in_at = today, Status = "SUCCESS", SubscriptionId = "sub02", UserName = "user02" },
            new() { CheckInId = "CK00000003", check_in_at = today, Status = "SUCCESS", SubscriptionId = "sub03", UserName = "user03" }
        };

        _checkInRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CheckInLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        int count = await _sut.GetCurrentAttendanceCountAsync();

        // Assert
        count.Should().Be(3);
    }

    // =========================================================================
    // 7. PRIVATE HELPER METHODS
    // =========================================================================

    private static User CreateTestUser(string userName, string fullName, string phone)
    {
        return new User
        {
            UserName = userName,
            FullName = fullName,
            Phone = phone,
            RoleId = "MEMBER",
            Status = "ACTIVE",
            CreatedAt = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1))
        };
    }

    private static MemberSubscription CreateTestSubscription(
        string subId,
        string userName,
        string status,
        int remainingSessions,
        DateOnly? endDate = null,
        string? qrCode = null)
    {
        DateOnly validEndDate = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        return new MemberSubscription
        {
            SubscriptionId = subId,
            UserName = userName,
            PlanId = "PLAN01",
            Status = status,
            RemainingCheckInSessions = remainingSessions,
            RemainingPtSessions = 0,
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
            EndDate = validEndDate,
            QrCode = qrCode
        };
    }

    private void SetupUserAndSubscription(User user, MemberSubscription subscription)
    {
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([user]);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _subRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subscription]);
        _subRepoMock.Setup(r => r.GetByIdAsync(subscription.SubscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
    }
}
