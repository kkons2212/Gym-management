namespace GymManagement.Server.Core.DTOs.PTSchedules;

public record PtSessionSlotDto(
    string SessionId,
    string PtId,
    string PtName,
    string MemberId,
    string MemberName,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string SessionStatus,
    string? Notes = null
);

public record BookPtSlotRequest(
    string PtId,
    string MemberId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime
);

