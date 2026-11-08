namespace GymManagement.Server.Core.DTOs.CheckIn;

public record CheckInRequest(
    string? QrCode = null,
    string? Phone = null,
    string? Notes = null
);

public record CheckInResponse(
    bool Success,
    string MemberName,
    string PlanName,
    int RemainingCheckInSessions,
    int RemainingPtSessions,
    DateTime CheckInTime,
    string Message
);

public record CheckInLogDto(
    string CheckInId,
    DateTime CheckInTime,
    string CheckInMethod,
    string? Notes,
    string MemberName,
    string? VerifiedByAdminName
);

