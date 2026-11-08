namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng PTAvailability trong CSDL.
/// </summary>
public sealed class PTAvailability
{
    public string AvailabilityId { get; set; } = string.Empty;

    public DateOnly AvailableDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public byte IsBooked { get; set; }

    public byte is_Booked
    {
        get
        {
            return IsBooked;
        }
        set
        {
            IsBooked = value;
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
}

