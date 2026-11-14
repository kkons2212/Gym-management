using GymManagement.Server.Core.Enums;

namespace GymManagement.Server.Core.DTOs.Members;

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 1 — HỒ SƠ HỘI VIÊN (Member Profile)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Thông tin tóm tắt hội viên dùng nội bộ và chia sẻ giữa các service.
/// Internal DTO — hậu tố *Dto.
/// </summary>
public record MemberDto(
    string UserName,
    string FullName,
    string Phone,
    string Email,
    string Status,
    DateTime CreatedAt,
    string? ActivePlanName = null,
    DateTime? EndDate = null,
    int RemainingCheckInSessions = 0,
    int RemainingPtSessions = 0
)
{
    public string UserId => UserName;
}

/// <summary>
/// Kết quả tìm kiếm hội viên trong danh sách.
/// Outgoing response — hậu tố *Response.
/// </summary>
public record MemberSearchResponse(
    string UserName,
    string FullName,
    string Phone,
    string Email,
    string Status,
    string? ActiveSubscriptionId,
    string? ActivePlanName,
    DateOnly? SubscriptionEndDate,
    int RemainingCheckInSessions,
    int RemainingPtSessions
);

/// <summary>
/// Thông tin đầy đủ hội viên kèm gói tập hiện tại (UC09.1).
/// Outgoing response — hậu tố *Response.
/// </summary>
public record MemberProfileResponse(
    string UserName,
    string FullName,
    string Phone,
    string Email,
    string Status,
    SubscriptionDetailResponse? ActiveSubscription
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 2 — GÓI TẬP (MemberSubscription)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Chi tiết gói tập hội viên.
/// Outgoing response — hậu tố *Response.
/// </summary>
public record SubscriptionDetailResponse(
    string SubscriptionId,
    string PlanId,
    string PlanName,
    string QrCode,
    DateOnly StartDate,
    DateOnly EndDate,
    int RemainingCheckInSessions,
    int RemainingPtSessions,
    SubscriptionStatus Status,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate,
    string? AssignedPtId
);

/// <summary>
/// Dữ liệu nội bộ gói tập dùng trong service-to-service.
/// Internal DTO — hậu tố *Dto.
/// </summary>
public record SubscriptionDto(
    string SubscriptionId,
    string UserName,
    string PlanId,
    SubscriptionStatus Status,
    DateOnly EndDate,
    int RemainingCheckInSessions,
    int RemainingPtSessions
);

/// <summary>
/// Chi tiết gói tập hội viên (phiên bản mở rộng dùng cho Auth/Billing context).
/// Internal DTO — hậu tố *Dto.
/// </summary>
public record MemberSubscriptionDto(
    string SubscriptionId,
    string PlanId,
    string PlanName,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int? RemainingCheckInSessions,
    int? RemainingPtSessions,
    DateTime? FreezeStartDate,
    DateTime? FreezeEndDate,
    string? AssignedPtId,
    string? AssignedPtName
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 3 — INCOMING REQUESTS (Hậu tố *Request)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Payload đăng ký hội viên mới cùng gói tập (UC02.1).
/// </summary>
public record RegisterMemberRequest(
    string FullName,
    string Phone,
    string Email,
    string PlanId,
    DateOnly StartDate,
    string AdminUserName,
    string? CustomMemberCode = null
);

/// <summary>
/// Payload cập nhật thông tin hội viên (UC02.2).
/// </summary>
public record UpdateMemberRequest(
    string UserName,
    string FullName,
    string Phone,
    string Email,
    string? NewUserName = null
);

/// <summary>
/// Payload gia hạn gói tập cho hội viên (UC02.3 — gia hạn).
/// </summary>
public record RenewSubscriptionRequest(
    string SubscriptionId,
    string NewPlanId,
    DateOnly NewStartDate,
    string AdminUserName
);

/// <summary>
/// Payload nâng cấp gói tập lên hạng cao hơn (UC02.3 — nâng cấp).
/// </summary>
public record UpgradeSubscriptionRequest(
    string SubscriptionId,
    string UpgradeToPlanId,
    string AdminUserName
);

/// <summary>
/// Payload bảo lưu gói tập hội viên (UC02.4).
/// </summary>
public record FreezeSubscriptionRequest(
    string SubscriptionId,
    DateOnly FreezeStartDate,
    DateOnly FreezeEndDate,
    string Reason,
    string AdminUserName = "admin"
)
{
    public FreezeSubscriptionRequest(string subscriptionId, DateTime freezeStartDate, DateTime freezeEndDate, string reason, string adminUserName = "admin")
        : this(subscriptionId, DateOnly.FromDateTime(freezeStartDate), DateOnly.FromDateTime(freezeEndDate), reason, adminUserName)
    {
    }
}

/// <summary>
/// Payload hủy gói tập hội viên (UC02.5).
/// </summary>
public record CancelSubscriptionRequest(
    string SubscriptionId,
    string Reason,
    string AdminUserName
);

/// <summary>
/// Payload nhập chỉ số InBody (dùng chung với PT context).
/// </summary>
public record InBodyEntryRequest(
    string UserId,
    decimal HeightCm,
    decimal WeightKg,
    decimal? BodyFatPercentage,
    decimal? SkeletalMuscleMass,
    string? Notes
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 4 — INBODY
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Một bản ghi chỉ số InBody.
/// Internal DTO — hậu tố *Dto.
/// </summary>
public record InBodyMetricDto(
    string MetricId,
    decimal HeightCm,
    decimal WeightKg,
    decimal? BodyFatPercentage,
    decimal? SkeletalMuscleMass,
    decimal Bmi,
    string? Notes,
    DateTime MeasuredAt,
    string? RecordedByPtName
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 5 — LỊCH SỬ TẬP LUYỆN (UC09.2)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Một bản ghi trong lịch sử tập luyện của hội viên.
/// Outgoing response — hậu tố *Response.
/// </summary>
public record WorkoutHistoryResponse(
    string CheckInId,
    DateOnly CheckInDate,
    string Status,
    string? DenialReason,
    string? AssignedPtName
);
