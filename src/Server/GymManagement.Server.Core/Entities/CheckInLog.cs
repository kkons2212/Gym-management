namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng CheckInLog trong CSDL.
/// </summary>
public sealed class CheckInLog
{
    public string CheckInId { get; set; } = string.Empty;

    public DateOnly? CheckInAt { get; set; }

    public DateOnly? check_in_at
    {
        get
        {
            return CheckInAt;
        }
        set
        {
            CheckInAt = value;
        }
    }

    public string? Status { get; set; }

    public string? DenialReason { get; set; }

    public string SubscriptionId { get; set; } = string.Empty;

    public string? VerifiedByAdminId { get; set; }

    public string? VerifiedByAdminID
    {
        get
        {
            return VerifiedByAdminId;
        }
        set
        {
            VerifiedByAdminId = value;
        }
    }

    public string UserName { get; set; } = string.Empty;
}

