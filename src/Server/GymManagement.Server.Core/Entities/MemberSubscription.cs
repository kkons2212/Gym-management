namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng MemberSubscription trong CSDL.
/// </summary>
public sealed class MemberSubscription
{
    public string SubscriptionId { get; set; } = string.Empty;

    public string? QrCode { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int RemainingCheckInSessions { get; set; }

    public int RemainingPtSessions { get; set; }

    public DateOnly? FreezeStartDate { get; set; }

    public DateOnly? FreezeEndDate { get; set; }

    public string? Status { get; set; }

    public DateOnly? CreatedAt { get; set; }

    public DateOnly? created_at
    {
        get
        {
            return CreatedAt;
        }
        set
        {
            CreatedAt = value;
        }
    }

    public string UserName { get; set; } = string.Empty;

    public string? AssignedPtId { get; set; }

    public string PlanId { get; set; } = string.Empty;

    public string? PTSessionId { get; set; }
}

