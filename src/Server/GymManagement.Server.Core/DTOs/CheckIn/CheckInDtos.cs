namespace GymManagement.Server.Core.DTOs.CheckIn;

/// <summary>
/// DTO yêu cầu điểm danh vào phòng tập bằng mã QR từ webcam, hoặc mã hội viên / số điện thoại tra cứu thủ công.
/// </summary>
public record CheckInRequest(
    string? QrCode = null,
    string? MemberCode = null,
    string? Phone = null,
    string? Notes = null,
    string? VerifiedByAdminId = null
);

/// <summary>
/// DTO phản hồi kết quả điểm danh vào phòng tập.
/// </summary>
public record CheckInResponse(
    bool Success,
    string MemberName,
    string PlanName,
    int RemainingCheckInSessions,
    int RemainingPtSessions,
    DateTime CheckInTime,
    string Message
);

/// <summary>
/// DTO lịch sử điểm danh phục vụ hiển thị và tra cứu nhật ký ra/vào phòng tập (UC 3.3).
/// </summary>
public record CheckInLogDto(
    string CheckInId,
    DateTime CheckInTime,
    string CheckInMethod,
    string? Notes,
    string MemberName,
    string? VerifiedByAdminName,
    string Status = "SUCCESS",
    string? DenialReason = null
);
