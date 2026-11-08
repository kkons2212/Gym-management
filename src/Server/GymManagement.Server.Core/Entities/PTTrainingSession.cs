namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng PTTrainingSession trong CSDL.
/// </summary>
public sealed class PTTrainingSession
{
    public string PTSessionId { get; set; } = string.Empty;

    public DateOnly SessionStartTime { get; set; }

    public DateOnly SessionEndTime { get; set; }

    public string? Status { get; set; }

    public string? WorkoutNotes { get; set; }

    public byte? RatingStars { get; set; }

    public string? ReviewComment { get; set; }

    public DateOnly? CompletedAt { get; set; }

    public DateOnly? completed_at
    {
        get
        {
            return CompletedAt;
        }
        set
        {
            CompletedAt = value;
        }
    }

    public string MemberId { get; set; } = string.Empty;

    public string MemberID
    {
        get
        {
            return MemberId;
        }
        set
        {
            MemberId = value;
        }
    }

    public string PtId { get; set; } = string.Empty;

    public string PtID
    {
        get
        {
            return PtId;
        }
        set
        {
            PtId = value;
        }
    }

    public string? AvailabilityId { get; set; }
}

