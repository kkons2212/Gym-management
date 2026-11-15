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
)
{
    public bool CanReview
    {
        get
        {
            if (string.Equals(SessionStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(SessionStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var endDateTime = ScheduledDate.Date.Add(EndTime);
            if (DateTime.Now >= endDateTime)
            {
                return true;
            }
            return false;
        }
    }
}

public record BookPtSlotRequest(
    string PtId,
    string MemberId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime
);

