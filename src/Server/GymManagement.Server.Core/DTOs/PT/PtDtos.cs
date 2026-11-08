using GymManagement.Server.Core.Enums;

namespace GymManagement.Server.Core.DTOs.PT;

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 1 — HỒ SƠ PT (PtProfile)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Thông tin hồ sơ chuyên môn của Huấn luyện viên cá nhân (PT).
/// Dùng cho API response tra cứu danh sách và xem chi tiết PT.
/// </summary>
public record PtProfileDto(
    string PtId,
    string FullName,
    string Phone,
    string Email,
    string Specialty,
    int ExperienceYears,
    decimal RatingAverage,
    int ReviewCount,
    bool IsActive,
    int ActiveStudentsCount,
    int PendingSessionsCount
);

/// <summary>
/// Tóm tắt thông tin học viên (Member) do PT phụ trách.
/// Dùng thay cho PtProfileDto khi trả danh sách học viên — tránh nhầm lẫn domain.
/// </summary>
public record MemberSummaryDto(
    string UserName,
    string FullName,
    string Phone,
    string Email,
    int RemainingPtSessions,
    string SubscriptionStatus
);

/// <summary>
/// Payload tạo mới / cập nhật hồ sơ PT (UC 4.1, UC 4.3).
/// Incoming request — hậu tố *Request theo dev_rule Ch.3.3.
/// </summary>
public record SavePtProfileRequest(
    string? PtId,           // null = tạo mới, có giá trị = cập nhật
    string FullName,
    string Phone,
    string Email,
    string Specialty,
    int ExperienceYears,
    bool IsActive
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 2 — LỊCH RẢNH PT (PTAvailability)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Một khung giờ trong lịch rảnh của PT.
/// Ánh xạ với bảng PTAvailability — nội bộ chia sẻ, hậu tố *Dto.
/// </summary>
public record PtSlotDto(
    string AvailabilityId,
    DateOnly AvailableDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsBooked       // true = đã có hội viên đặt, không được hủy
);

/// <summary>
/// Một mục slot trong danh sách cần tạo mới hoặc cập nhật.
/// Dùng bên trong SavePtSlotsRequest — hậu tố *Dto vì là internal DTO.
/// </summary>
public record PtSlotUpsertDto(
    string? AvailabilityId, // null = tạo mới, có giá trị = cập nhật/hủy
    DateOnly AvailableDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool Register           // true = đăng ký rảnh, false = hủy đăng ký
);

/// <summary>
/// Payload lưu lịch rảnh PT theo tuần (UC 8.1.1, UC 4.6).
/// Incoming request — hậu tố *Request.
/// </summary>
public record SavePtSlotsRequest(
    string PtId,
    IReadOnlyList<PtSlotUpsertDto> Slots
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 3 — CA TẬP PT (PTTrainingSession)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Chi tiết một ca tập PT, bao gồm trạng thái, ghi chú và đánh giá.
/// Ánh xạ với bảng PTTrainingSession — internal DTO, hậu tố *Dto.
/// </summary>
public record PtSessionDto(
    string SessionId,
    string PtId,
    string PtName,
    string MemberUserName,
    string MemberFullName,
    DateTime SessionStartTime,
    DateTime SessionEndTime,
    PtSessionStatus Status,
    string? WorkoutNotes,
    int? RatingStars,
    string? ReviewComment,
    DateTime? CompletedAt
);

/// <summary>
/// Payload hội viên đặt lịch tập với PT (UC 10.1).
/// Incoming request — hậu tố *Request.
/// </summary>
public record BookSessionRequest(
    string MemberUserName,
    string AvailabilityId,  // slot của PT đã chọn
    string? WorkoutNotes
);

/// <summary>
/// Payload hủy lịch tập (UC 10.2).
/// ForceLateCancellation = true nghĩa là hội viên xác nhận hủy trễ (biết sẽ mất buổi).
/// Incoming request — hậu tố *Request.
/// </summary>
public record CancelSessionRequest(
    string SessionId,
    string RequestedByUserName,
    bool ForceLateCancellation
);

/// <summary>
/// Payload đổi lịch tập sang slot khác của cùng PT phụ trách (UC 10.4).
/// Incoming request — hậu tố *Request.
/// </summary>
public record RescheduleSessionRequest(
    string SessionId,
    string MemberUserName,
    string NewAvailabilityId    // slot mới muốn chuyển sang
);

/// <summary>
/// Payload Admin xác nhận hoặc điều chỉnh kết quả ca tập (UC 4.5).
/// Incoming request — hậu tố *Request.
/// </summary>
public record ConfirmSessionResultRequest(
    string SessionId,
    string AdminUserName,
    PtSessionStatus NewStatus,  // Completed / AbsentMember / AbsentPt / Postponed
    string? Notes
);

/// <summary>
/// Payload PT xác nhận hoàn thành ca tập (UC 8.2.3).
/// Incoming request — hậu tố *Request.
/// </summary>
public record PtCompleteSessionRequest(
    string SessionId,
    string PtUserName,
    string? WorkoutNotes
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 4 — CHỈ SỐ INBODY (MemberBodyMetric)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Một bản ghi đo chỉ số thể hình InBody.
/// Ánh xạ với bảng MemberBodyMetric — internal DTO, hậu tố *Dto.
/// </summary>
public record InBodyRecordDto(
    string BodyMetricId,
    string MemberUserName,
    decimal WeightKg,
    decimal HeightCm,
    decimal SkeletalMuscleMass,
    decimal BodyFatPercentage,
    decimal Bmi,
    string? Notes,
    DateOnly MeasuredAt,
    string RecordedByPtId,
    string RecordedByPtName,
    string? LinkedSessionId
);

/// <summary>
/// Payload PT nhập chỉ số InBody cho học viên (UC 8.2.2).
/// Incoming request — hậu tố *Request.
/// </summary>
public record RecordInBodyRequest(
    string MemberUserName,
    string PtUserName,
    decimal WeightKg,
    decimal HeightCm,
    decimal SkeletalMuscleMass,
    decimal BodyFatPercentage,
    decimal Bmi,
    string? Notes,
    string? LinkedSessionId     // null nếu không gắn với ca tập cụ thể
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 5 — ĐÁNH GIÁ PT (Rating)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Payload hội viên đánh giá sao sau ca tập đã hoàn thành (UC 10.3).
/// Incoming request — hậu tố *Request.
/// </summary>
public record SubmitRatingRequest(
    string SessionId,
    string MemberUserName,
    int RatingStars,            // 1–5
    string? ReviewComment
);

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 6 — PHÂN CÔNG PT (UC 4.4)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Payload Admin phân công PT phụ trách cố định cho hội viên (UC 4.4).
/// Incoming request — hậu tố *Request.
/// </summary>
public record AssignPtRequest(
    string SubscriptionId,      // gói tập hội viên cần gán PT
    string PtId,
    string AdminUserName
);
