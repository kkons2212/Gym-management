using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.CheckIn;

namespace GymManagement.Web.Services;

/// <summary>
/// Chi tiết gói tập hội viên đang sở hữu dùng cho cổng Web.
/// </summary>
public record MemberSubscriptionDetailDto(
    string SubscriptionId,
    string UserName,
    string MemberFullName,
    string PlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    int RemainingCheckInSessions,
    int RemainingPtSessions,
    string Status, // ACTIVE, FROZEN, EXPIRED
    string? AssignedPtId,
    string? AssignedPtName,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate
);

/// <summary>
/// Hợp đồng giao tiếp dữ liệu cho Cổng thông tin Hội viên & Huấn luyện viên PT.
/// </summary>
public interface IWebApiClient
{
    // Xác thực & Phiên làm việc (UC01)
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserSessionDto?> GetCurrentUserSessionAsync();

    // Hồ sơ & Gói tập số của Hội viên (UC09, UC03)
    Task<MemberProfileResponse?> GetMyProfileAsync();
    Task<MemberSubscriptionDetailDto?> GetMySubscriptionAsync();
    Task<string> GetMyCheckInQrCodeAsync();
    Task<string> RefreshCheckInQrCodeAsync();

    // Danh mục gói tập & Thanh toán VietQR (UC02, UC05)
    Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync();
    Task<VietQrPayloadDto> CreateVietQrInvoiceAsync(string planId);
    Task<bool> SimulateVietQrPaymentAsync(string invoiceId);

    // Đặt lịch, hủy ca & đổi lịch PT (UC10)
    Task<IReadOnlyList<PtSlotDto>> GetPtAvailableSlotsAsync(string ptUserName, DateOnly startDate, DateOnly endDate);
    Task<PtSessionDto> BookPtSlotAsync(BookSessionRequest request);
    Task<bool> CancelPtBookingAsync(CancelSessionRequest request);
    Task<bool> ReschedulePtBookingAsync(RescheduleSessionRequest request);
    Task<bool> SubmitRatingAsync(SubmitRatingRequest request);
    Task<IReadOnlyList<PtSessionDto>> GetMyPtSessionsAsync();

    // Thể hình InBody & Lịch sử tập luyện (UC06, UC09)
    Task<IReadOnlyList<InBodyRecordDto>> GetMyInBodyHistoryAsync();
    Task<IReadOnlyList<CheckInLogDto>> GetMyCheckInHistoryAsync();

    // Cổng tác nghiệp dành cho Huấn luyện viên PT (UC08)
    Task<IReadOnlyList<PtSlotDto>> GetMyAvailabilitySlotsAsync(DateOnly startDate, DateOnly endDate);
    Task<bool> SavePtAvailabilitySlotsAsync(SavePtSlotsRequest request);
    Task<IReadOnlyList<PtSessionDto>> GetPtMyWeeklyScheduleAsync(DateOnly startDate, DateOnly endDate);
    Task<bool> CompletePtSessionAsync(PtCompleteSessionRequest request);
    Task<IReadOnlyList<MemberSummaryDto>> GetPtAssignedStudentsAsync();
    Task<bool> RecordInBodyAsync(RecordInBodyRequest request);
}

