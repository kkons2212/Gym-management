namespace GymManagement.Server.UnitTests.DTOs;

using FluentAssertions;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PTSchedules;
using Xunit;

public class AdminDesktopDtoTests
{
    [Fact]
    public void PackagePlanDto_WhenConstructed_ShouldStoreValuesCorrectly()
    {
        // Arrange
        const string planId = "PLAN01";
        const string name = "Gói 1 Tháng Thể Thao";
        const string description = "Quyền lợi tập gym không giới hạn";
        const decimal price = 500000m;
        const int durationDays = 30;
        const int maxCheckIn = 30;
        const int ptSessions = 0;
        const bool isPtIncluded = false;
        const string status = "ACTIVE";

        // Act
        var dto = new PackagePlanDto(
            planId,
            name,
            description,
            price,
            durationDays,
            maxCheckIn,
            ptSessions,
            isPtIncluded,
            status
        );

        // Assert
        dto.PlanId.Should().Be(planId);
        dto.Name.Should().Be(name);
        dto.Price.Should().Be(price);
        dto.DurationDays.Should().Be(durationDays);
        dto.MaxCheckInSessions.Should().Be(maxCheckIn);
        dto.IsPtIncluded.Should().BeFalse();
        dto.Status.Should().Be("ACTIVE");
    }

    [Fact]
    public void CheckInRequest_WhenConstructedWithQrCode_ShouldHaveQrCodeAndNullPhone()
    {
        // Arrange
        const string sampleQr = "MEMBER-TOKEN-12345";

        // Act
        var request = new CheckInRequest(QrCode: sampleQr);

        // Assert
        request.QrCode.Should().Be(sampleQr);
        request.Phone.Should().BeNull();
        request.Notes.Should().BeNull();
    }

    [Fact]
    public void CreateInvoiceRequest_WhenConstructed_ShouldRetainMatchingPaymentMethod()
    {
        // Arrange
        const string userId = "USR01";
        const string planId = "PLAN01";
        const string method = "VIETQR";
        const string type = "SUBSCRIPTION_PURCHASE";

        // Act
        var request = new CreateInvoiceRequest(userId, planId, method, type);

        // Assert
        request.UserId.Should().Be(userId);
        request.PlanId.Should().Be(planId);
        request.PaymentMethod.Should().Be("VIETQR");
        request.TransactionType.Should().Be("SUBSCRIPTION_PURCHASE");
    }

    [Fact]
    public void FreezeSubscriptionRequest_WhenConstructed_ShouldRetainDateRange()
    {
        // Arrange
        const string subscriptionId = "SUB01";
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(14);
        const string reason = "Đi công tác";

        // Act
        var request = new FreezeSubscriptionRequest(subscriptionId, startDate, endDate, reason);

        // Assert
        request.SubscriptionId.Should().Be(subscriptionId);
        request.FreezeStartDate.Should().Be(startDate);
        request.FreezeEndDate.Should().Be(endDate);
        request.Reason.Should().Be(reason);
    }

    [Fact]
    public void BookPtSlotRequest_WhenConstructed_ShouldRetainTimeInterval()
    {
        // Arrange
        const string ptId = "PT01";
        const string memberId = "USR01";
        var date = DateTime.Today;
        var start = new TimeSpan(9, 0, 0);
        var end = new TimeSpan(10, 0, 0);

        // Act
        var request = new BookPtSlotRequest(ptId, memberId, date, start, end);

        // Assert
        request.PtId.Should().Be(ptId);
        request.MemberId.Should().Be(memberId);
        request.ScheduledDate.Should().Be(date);
        request.StartTime.Should().Be(start);
        request.EndTime.Should().Be(end);
    }
}

