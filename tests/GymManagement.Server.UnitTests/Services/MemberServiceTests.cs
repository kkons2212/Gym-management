using FluentAssertions;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace GymManagement.Server.UnitTests.Services;

/// <summary>
/// Unit Tests cho MemberService — UC02, UC09.
/// Naming: [MethodUnderTest]_[StateUnderTest]_[ExpectedBehavior]
/// Pattern: Arrange – Act – Assert (AAA)
/// </summary>
public class MemberServiceTests
{
    // =========================================================================
    // TEST FIXTURES
    // =========================================================================

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<INotificationAdapter> _notifyMock;
    private readonly Mock<ILogger<MemberService>> _loggerMock;
    private readonly MemberService _sut;

    public MemberServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _notifyMock = new Mock<INotificationAdapter>();
        _loggerMock = new Mock<ILogger<MemberService>>();
        _sut = new MemberService(_uowMock.Object, _notifyMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // NHÓM 1 — ĐĂNG KÝ HỘI VIÊN (UC02.1)
    // =========================================================================

    [Fact]
    public async Task RegisterMemberAsync_WhenPhoneAlreadyExists_ThrowsValidationException()
    {
        // Arrange
        var request = new RegisterMemberRequest(
            FullName: "Nguyễn Văn A",
            Phone: "0901234567",
            Email: "a@gym.vn",
            PlanId: "PKG-01",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            AdminUserName: "admin01");

        SetupUserRepoPhoneExists(exists: true);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.RegisterMemberAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*điện thoại*");
    }

    [Fact]
    public async Task RegisterMemberAsync_WhenPlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new RegisterMemberRequest(
            FullName: "Nguyễn Văn B",
            Phone: "0911111111",
            Email: "b@gym.vn",
            PlanId: "PKG-INVALID",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            AdminUserName: "admin01");

        SetupUserRepoPhoneExists(exists: false);

        var planRepoMock = new Mock<IGenericRepository<PackagePlanStub>>();
        planRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<PackagePlanStub, bool>>>(), default))
            .ReturnsAsync((PackagePlanStub?)null);
        _uowMock.Setup(u => u.Repository<PackagePlanStub>()).Returns(planRepoMock.Object);

        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.RegisterMemberAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*gói tập*");
    }

    [Fact]
    public async Task RegisterMemberAsync_WhenRequestHasEmptyFullName_ThrowsValidationException()
    {
        // Arrange
        var request = new RegisterMemberRequest(
            FullName: "",
            Phone: "0922222222",
            Email: "c@gym.vn",
            PlanId: "PKG-01",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            AdminUserName: "admin01");

        // Act
        Func<Task> act = () => _sut.RegisterMemberAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Họ tên*");
    }

    // =========================================================================
    // NHÓM 2 — CẬP NHẬT HỘI VIÊN (UC02.2)
    // =========================================================================

    [Fact]
    public async Task UpdateMemberAsync_WhenMemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdateMemberRequest(
            UserName: "MBR-9999",
            FullName: "Test",
            Phone: "0933333333",
            Email: "t@gym.vn");

        SetupUserRepoFindNull();

        // Act
        Func<Task> act = () => _sut.UpdateMemberAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateMemberAsync_WhenPhoneConflictWithAnotherMember_ThrowsValidationException()
    {
        // Arrange
        var request = new UpdateMemberRequest(
            UserName: "MBR-0001",
            FullName: "Nguyễn Văn C",
            Phone: "0901234567",
            Email: "c@gym.vn");

        SetupUserRepoFindExisting("MBR-0001");
        SetupUserRepoPhoneConflict(exists: true);

        // Act
        Func<Task> act = () => _sut.UpdateMemberAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*điện thoại*");
    }

    // =========================================================================
    // NHÓM 3 — BẢO LƯU GÓI TẬP (UC02.4)
    // =========================================================================

    [Fact]
    public async Task FreezeSubscriptionAsync_WhenSubscriptionNotActive_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new FreezeSubscriptionRequest(
            SubscriptionId: "SUB-00001",
            FreezeStartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            FreezeEndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            Reason: "Đi công tác",
            AdminUserName: "admin01");

        var sub = BuildSubscription("SUB-00001", "MBR-0001", status: "FROZEN");
        SetupSubscriptionRepo(sub);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.FreezeSubscriptionAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đang hoạt động*");
    }

    [Fact]
    public async Task FreezeSubscriptionAsync_WhenReasonIsEmpty_ThrowsValidationException()
    {
        // Arrange
        var request = new FreezeSubscriptionRequest(
            SubscriptionId: "SUB-00001",
            FreezeStartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            FreezeEndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            Reason: "",
            AdminUserName: "admin01");

        // Act
        Func<Task> act = () => _sut.FreezeSubscriptionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Lý do*");
    }

    [Fact]
    public async Task FreezeSubscriptionAsync_WhenEndDateBeforeStartDate_ThrowsValidationException()
    {
        // Arrange
        var request = new FreezeSubscriptionRequest(
            SubscriptionId: "SUB-00001",
            FreezeStartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            FreezeEndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),  // trước StartDate
            Reason: "Test",
            AdminUserName: "admin01");

        // Act
        Func<Task> act = () => _sut.FreezeSubscriptionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*kết thúc*");
    }

    // =========================================================================
    // NHÓM 4 — HỦY GÓI TẬP (UC02.5)
    // =========================================================================

    [Fact]
    public async Task CancelSubscriptionAsync_WhenReasonIsEmpty_ThrowsValidationException()
    {
        // Arrange
        var request = new CancelSubscriptionRequest(
            SubscriptionId: "SUB-00001",
            Reason: "",
            AdminUserName: "admin01");

        // Act
        Func<Task> act = () => _sut.CancelSubscriptionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Lý do*");
    }

    [Fact]
    public async Task CancelSubscriptionAsync_WhenSubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CancelSubscriptionRequest(
            SubscriptionId: "SUB-99999",
            Reason: "Vi phạm nội quy",
            AdminUserName: "admin01");

        SetupSubscriptionRepo(null);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.CancelSubscriptionAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // =========================================================================
    // NHÓM 5 — XEM THÔNG TIN (UC09.1)
    // =========================================================================

    [Fact]
    public async Task GetMemberProfileAsync_WhenMemberNotFound_ThrowsNotFoundException()
    {
        // Arrange
        SetupUserRepoFindNull();

        // Act
        Func<Task> act = () => _sut.GetMemberProfileAsync("MBR-9999");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // =========================================================================
    // PRIVATE SETUP HELPERS
    // =========================================================================

    private void SetupUserRepoPhoneExists(bool exists)
    {
        var userRepoMock = new Mock<IGenericRepository<MemberUser>>();
        userRepoMock
            .Setup(r => r.ExistsAsync(
                It.IsAny<Expression<Func<MemberUser, bool>>>(), default))
            .ReturnsAsync(exists);
        userRepoMock
            .Setup(r => r.CountAsync(
                It.IsAny<Expression<Func<MemberUser, bool>>>(), default))
            .ReturnsAsync(5);
        userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<MemberUser>(), default))
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.Repository<MemberUser>()).Returns(userRepoMock.Object);
    }

    private void SetupUserRepoFindNull()
    {
        var userRepoMock = new Mock<IGenericRepository<MemberUser>>();
        userRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<MemberUser, bool>>>(), default))
            .ReturnsAsync((MemberUser?)null);
        _uowMock.Setup(u => u.Repository<MemberUser>()).Returns(userRepoMock.Object);
    }

    private void SetupUserRepoFindExisting(string userName)
    {
        var userRepoMock = new Mock<IGenericRepository<MemberUser>>();
        userRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<MemberUser, bool>>>(), default))
            .ReturnsAsync(new MemberUser
            {
                UserName = userName,
                RoleId = "MEMBER",
                FullName = "Test Member",
                Phone = "0900000000",
                Email = "old@gym.vn",
                Status = "ACTIVE"
            });
        _uowMock.Setup(u => u.Repository<MemberUser>()).Returns(userRepoMock.Object);
    }

    private void SetupUserRepoPhoneConflict(bool exists)
    {
        var userRepoMock = new Mock<IGenericRepository<MemberUser>>();
        userRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<MemberUser, bool>>>(), default))
            .ReturnsAsync(new MemberUser
            {
                UserName = "MBR-0001",
                RoleId = "MEMBER",
                FullName = "Old Name",
                Phone = "0900000000",
                Status = "ACTIVE"
            });
        userRepoMock
            .Setup(r => r.ExistsAsync(
                It.IsAny<Expression<Func<MemberUser, bool>>>(), default))
            .ReturnsAsync(exists);
        userRepoMock.Setup(r => r.Update(It.IsAny<MemberUser>()));
        _uowMock.Setup(u => u.Repository<MemberUser>()).Returns(userRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    private void SetupSubscriptionRepo(MemberSubscriptionStub? sub)
    {
        var subRepoMock = new Mock<IGenericRepository<MemberSubscriptionStub>>();
        subRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<MemberSubscriptionStub, bool>>>(), default))
            .ReturnsAsync(sub);
        subRepoMock.Setup(r => r.Update(It.IsAny<MemberSubscriptionStub>()));
        _uowMock.Setup(u => u.Repository<MemberSubscriptionStub>()).Returns(subRepoMock.Object);
    }

    private void SetupExecuteTransaction()
    {
        _uowMock
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _uowMock
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<MemberProfileResponse>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<Task<MemberProfileResponse>>, CancellationToken>(
                async (action, _) => await action());

        _uowMock
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<SubscriptionDetailResponse>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<Task<SubscriptionDetailResponse>>, CancellationToken>(
                async (action, _) => await action());

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    // ─── Object builders ────────────────────────────────────────────────────

    private static MemberSubscriptionStub BuildSubscription(
        string subscriptionId,
        string userName,
        string status = "ACTIVE",
        DateOnly? endDate = null)
        => new()
        {
            SubscriptionId = subscriptionId,
            UserName = userName,
            PlanId = "PKG-01",
            Status = status,
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            RemainingCheckInSessions = 30,
            RemainingPtSessions = 12
        };
}
