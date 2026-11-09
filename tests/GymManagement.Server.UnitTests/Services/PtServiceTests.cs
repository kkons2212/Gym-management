using FluentAssertions;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Enums;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace GymManagement.Server.UnitTests.Services;

/// <summary>
/// Unit Tests cho PtService — UC04, UC08, UC10.
/// Tuân thủ dev_rule Ch.6: Naming = [MethodUnderTest]_[StateUnderTest]_[ExpectedBehavior]
/// Mô hình: Arrange – Act – Assert (AAA)
/// </summary>
public class PtServiceTests
{
    // =========================================================================
    // TEST FIXTURES — Shared setup
    // =========================================================================

    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<INotificationAdapter> _notifyMock;
    private readonly Mock<ILogger<PtService>> _loggerMock;
    private readonly PtService _sut;           // System Under Test

    public PtServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _notifyMock = new Mock<INotificationAdapter>();
        _loggerMock = new Mock<ILogger<PtService>>();
        _sut = new PtService(_uowMock.Object, _notifyMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // NHÓM 1 — QUẢN LÝ HỒ SƠ PT
    // =========================================================================

    [Fact]
    public async Task SaveTrainerAsync_WhenPhoneAlreadyUsedByAnotherPt_ThrowsValidationException()
    {
        // Arrange
        var request = new SavePtProfileRequest(
            PtId: null, FullName: "Nguyễn Văn A", Phone: "0909999999",
            Email: "a@gym.vn", Specialty: "Gym", ExperienceYears: 3, IsActive: true);

        SetupUserRepoExists(phoneMatch: true);

        // Act
        Func<Task> act = () => _sut.SaveTrainerAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Số điện thoại*");
    }

    [Fact]
    public async Task SaveTrainerAsync_WhenEmailAlreadyUsedByAnotherPt_ThrowsValidationException()
    {
        // Arrange
        var request = new SavePtProfileRequest(
            PtId: null, FullName: "Nguyễn Văn B", Phone: "0900000001",
            Email: "dup@gym.vn", Specialty: "Yoga", ExperienceYears: 2, IsActive: true);

        SetupUserRepoExists(phoneMatch: false, emailMatch: true);

        // Act
        Func<Task> act = () => _sut.SaveTrainerAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public async Task SetTrainerStatusAsync_WhenPtNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var userRepoMock = new Mock<IGenericRepository<User>>();
        userRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync((User?)null);

        _uowMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);

        SetupSubscriptionRepoCount(0);
        SetupSessionRepoCount(0);

        // Act
        Func<Task> act = () => _sut.SetTrainerStatusAsync("PT-99", isActive: false);

        // Assert — CheckDeactivationEligibilityAsync trả CanDeactivate=true, nhưng GetByUserName null → NotFoundException
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CheckDeactivationEligibilityAsync_WhenPtHasActiveStudents_ReturnsCannotDeactivate()
    {
        // Arrange
        SetupSubscriptionRepoCount(3);   // 3 học viên đang phụ trách
        SetupSessionRepoCount(0);

        // Act
        var (canDeactivate, reason, students, pending) =
            await _sut.CheckDeactivationEligibilityAsync("PT-01");

        // Assert
        canDeactivate.Should().BeFalse();
        students.Should().Be(3);
        reason.Should().Contain("học viên");
    }

    [Fact]
    public async Task CheckDeactivationEligibilityAsync_WhenPtHasNoStudentsOrSessions_ReturnsCanDeactivate()
    {
        // Arrange
        SetupSubscriptionRepoCount(0);
        SetupSessionRepoCount(0);

        // Act
        var (canDeactivate, reason, students, pending) =
            await _sut.CheckDeactivationEligibilityAsync("PT-01");

        // Assert
        canDeactivate.Should().BeTrue();
        reason.Should().BeNull();
        students.Should().Be(0);
        pending.Should().Be(0);
    }

    // =========================================================================
    // NHÓM 2 — PHÂN CÔNG PT (UC 4.4)
    // =========================================================================

    [Fact]
    public async Task AssignPtToMemberAsync_WhenPtReachesMaxStudentLimit_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new AssignPtRequest("SUB-001", "PT-01", "admin01");

        // PT tồn tại
        var userRepoMock = new Mock<IGenericRepository<User>>();
        userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(true);
        _uowMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);

        // Subscription cho hội viên tồn tại
        var subscription = BuildSubscription("SUB-001", "MB-01", assignedPtId: null);
        var subRepoMock = new Mock<IGenericRepository<MemberSubscription>>();
        subRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), default))
            .ReturnsAsync(subscription);
        // PT đã có 10 học viên = đạt giới hạn
        subRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), default))
            .ReturnsAsync(10);
        _uowMock.Setup(u => u.Repository<MemberSubscription>()).Returns(subRepoMock.Object);

        // Act
        Func<Task> act = () => _sut.AssignPtToMemberAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*giới hạn*");
    }

    // =========================================================================
    // NHÓM 4 — CA TẬP PT — BookSession (UC 10.1)
    // =========================================================================

    [Fact]
    public async Task BookSessionAsync_WhenSlotAlreadyBooked_ThrowsBusinessRuleException()
    {
        // Arrange — slot is_Booked = 1
        var request = new BookSessionRequest("MB-01", "AVL-001", null);

        var availRepoMock = new Mock<IGenericRepository<PTAvailability>>();
        availRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<PTAvailability, bool>>>(), default))
            .ReturnsAsync((PTAvailability?)null);   // slot đã booked → trả null (vì filter is_Booked == 0)

        _uowMock.Setup(u => u.Repository<PTAvailability>()).Returns(availRepoMock.Object);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.BookSessionAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*vừa được đặt*");
    }

    [Fact]
    public async Task BookSessionAsync_WhenMemberHasNoRemainingPtSessions_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new BookSessionRequest("MB-01", "AVL-001", null);

        var slot = BuildSlot("AVL-001", "PT-01", isBooked: false,
            date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var availRepoMock = new Mock<IGenericRepository<PTAvailability>>();
        availRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<PTAvailability, bool>>>(), default))
            .ReturnsAsync(slot);
        _uowMock.Setup(u => u.Repository<PTAvailability>()).Returns(availRepoMock.Object);

        // Subscription còn 0 buổi PT
        var subscription = BuildSubscription("SUB-001", "MB-01", assignedPtId: "PT-01",
            remainingPtSessions: 0);
        var subRepoMock = new Mock<IGenericRepository<MemberSubscription>>();
        subRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<MemberSubscription, bool>>>(), default))
            .ReturnsAsync(subscription);
        _uowMock.Setup(u => u.Repository<MemberSubscription>()).Returns(subRepoMock.Object);

        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.BookSessionAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*không còn số buổi*");
    }

    // =========================================================================
    // NHÓM 4 — CA TẬP PT — CancelSession (UC 10.2)
    // =========================================================================

    [Fact]
    public async Task CancelSessionAsync_WhenLessThanTwoHoursBeforeStart_AndNotForced_ThrowsBusinessRuleException()
    {
        // Arrange — ca tập diễn ra sau 30 phút (< 2 giờ), chưa force
        var request = new CancelSessionRequest("SES-001", "MB-01", ForceLateCancellation: false);

        var session = BuildSession("SES-001", "PT-01", "MB-01",
            status: nameof(PtSessionStatus.Booked), availabilityId: "AVL-001");

        SetupSessionRepo(session);

        // Slot bắt đầu sau 30 phút
        var slot = BuildSlot("AVL-001", "PT-01", isBooked: true,
            date: DateOnly.FromDateTime(DateTime.UtcNow),
            startTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddMinutes(30)));
        SetupAvailabilityRepo(slot);

        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.CancelSessionAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*hủy trễ*");
    }

    [Fact]
    public async Task CancelSessionAsync_WhenSessionNotInBookedStatus_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new CancelSessionRequest("SES-001", "MB-01", ForceLateCancellation: false);
        var session = BuildSession("SES-001", "PT-01", "MB-01",
            status: nameof(PtSessionStatus.Completed));

        SetupSessionRepo(session);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.CancelSessionAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Đã đặt*");
    }

    // =========================================================================
    // NHÓM 6 — ĐÁNH GIÁ PT (UC 10.3)
    // =========================================================================

    [Fact]
    public async Task SubmitRatingAsync_WhenRatingStarsOutOfRange_ThrowsValidationException()
    {
        // Arrange
        var request = new SubmitRatingRequest("SES-001", "MB-01", RatingStars: 6, null);

        // Act
        Func<Task> act = () => _sut.SubmitRatingAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*1 đến 5*");
    }

    [Fact]
    public async Task SubmitRatingAsync_WhenSessionAlreadyRated_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new SubmitRatingRequest("SES-001", "MB-01", RatingStars: 5, "Tốt");
        var session = BuildSession("SES-001", "PT-01", "MB-01",
            status: nameof(PtSessionStatus.Completed), ratingStars: 4); // đã đánh giá rồi

        SetupSessionRepo(session);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.SubmitRatingAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*đã được đánh giá*");
    }

    [Fact]
    public async Task SubmitRatingAsync_WhenSessionNotCompleted_ThrowsBusinessRuleException()
    {
        // Arrange
        var request = new SubmitRatingRequest("SES-001", "MB-01", RatingStars: 5, null);
        var session = BuildSession("SES-001", "PT-01", "MB-01",
            status: nameof(PtSessionStatus.Booked));

        SetupSessionRepo(session);
        SetupExecuteTransaction();

        // Act
        Func<Task> act = () => _sut.SubmitRatingAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*hoàn thành*");
    }

    // =========================================================================
    // NHÓM 5 — INBODY (UC 8.2.2)
    // =========================================================================

    [Fact]
    public async Task RecordInBodyAsync_WhenWeightKgIsZero_ThrowsValidationException()
    {
        // Arrange
        var request = new RecordInBodyRequest(
            "MB-01", "PT-01",
            WeightKg: 0,            // invalid
            HeightCm: 170,
            SkeletalMuscleMass: 30,
            BodyFatPercentage: 15,
            Bmi: 20,
            Notes: null,
            LinkedSessionId: null);

        // Act
        Func<Task> act = () => _sut.RecordInBodyAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Cân nặng*");
    }

    // =========================================================================
    // PRIVATE HELPERS — Setup mocks
    // =========================================================================

    private void SetupUserRepoExists(bool phoneMatch = false, bool emailMatch = false)
    {
        var userRepoMock = new Mock<IGenericRepository<User>>();
        int callCount = 0;

        userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return phoneMatch;
                }

                return emailMatch;
            });

        userRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(5);

        userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);
        SetupExecuteTransaction();
    }

    private void SetupSubscriptionRepoCount(int count)
    {
        var subRepoMock = new Mock<IGenericRepository<MemberSubscription>>();
        subRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<MemberSubscription, bool>>>(), default))
            .ReturnsAsync(count);
        _uowMock.Setup(u => u.Repository<MemberSubscription>()).Returns(subRepoMock.Object);
    }

    private void SetupSessionRepoCount(int count)
    {
        var sessionRepoMock = new Mock<IGenericRepository<PTTrainingSession>>();
        sessionRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<PTTrainingSession, bool>>>(), default))
            .ReturnsAsync(count);
        _uowMock.Setup(u => u.Repository<PTTrainingSession>()).Returns(sessionRepoMock.Object);
    }

    private void SetupSessionRepo(PTTrainingSession session)
    {
        var sessionRepoMock = new Mock<IGenericRepository<PTTrainingSession>>();
        sessionRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<PTTrainingSession, bool>>>(), default))
            .ReturnsAsync(session);
        sessionRepoMock
            .Setup(r => r.Update(It.IsAny<PTTrainingSession>()));
        _uowMock.Setup(u => u.Repository<PTTrainingSession>()).Returns(sessionRepoMock.Object);
    }

    private void SetupAvailabilityRepo(PTAvailability? slot)
    {
        var availRepoMock = new Mock<IGenericRepository<PTAvailability>>();
        availRepoMock
            .Setup(r => r.FindSingleOrDefaultAsync(
                It.IsAny<Expression<Func<PTAvailability, bool>>>(), default))
            .ReturnsAsync(slot);
        availRepoMock
            .Setup(r => r.Update(It.IsAny<PTAvailability>()));
        _uowMock.Setup(u => u.Repository<PTAvailability>()).Returns(availRepoMock.Object);
    }

    /// <summary>
    /// Mock ExecuteTransactionAsync để thực thi action inline
    /// (không mở thật DbTransaction, chỉ chạy hàm callback).
    /// </summary>
    private void SetupExecuteTransaction()
    {
        _uowMock
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

        _uowMock
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<PtSessionDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<Task<PtSessionDto>>, CancellationToken>(async (action, _) => await action());

        _uowMock
            .Setup(u => u.ExecuteTransactionAsync(
                It.IsAny<Func<Task<PtProfileDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<Task<PtProfileDto>>, CancellationToken>(async (action, _) => await action());

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    // ─── Object builders ────────────────────────────────────────────────────

    private static PTTrainingSession BuildSession(
        string sessionId,
        string ptId,
        string memberId,
        string status,
        string? availabilityId = null,
        byte? ratingStars = null)
        => new()
        {
            PTSessionId = sessionId,
            PtId = ptId,
            MemberId = memberId,
            Status = status,
            AvailabilityId = availabilityId,
            RatingStars = ratingStars,
            SessionStartTime = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            SessionEndTime = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

    private static PTAvailability BuildSlot(
        string availabilityId,
        string ptId,
        bool isBooked,
        DateOnly? date = null,
        TimeOnly? startTime = null)
    {
        byte bookedValue = 0;
        if (isBooked)
        {
            bookedValue = 1;
        }

        return new()
        {
            AvailabilityId = availabilityId,
            PtId = ptId,
            IsBooked = bookedValue,
            AvailableDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = startTime ?? new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
    }

    private static MemberSubscription BuildSubscription(
        string subscriptionId,
        string userName,
        string? assignedPtId,
        int remainingPtSessions = 5)
        => new()
        {
            SubscriptionId = subscriptionId,
            UserName = userName,
            AssignedPtId = assignedPtId,
            Status = "ACTIVE",
            RemainingPtSessions = remainingPtSessions
        };
}
