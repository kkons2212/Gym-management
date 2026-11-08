namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng PackagePlan trong CSDL.
/// </summary>
public sealed class PackagePlan
{
    public string PlanId { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? DurationDays { get; set; }

    public decimal? Price { get; set; }

    public int? MaxCheckInSessions { get; set; }

    public byte? IsPtIncluded { get; set; }

    public byte? is_PtIncluded
    {
        get
        {
            return IsPtIncluded;
        }
        set
        {
            IsPtIncluded = value;
        }
    }

    public int? TotalPtSessions { get; set; }

    public byte? IsActive { get; set; }

    public byte? is_Active
    {
        get
        {
            return IsActive;
        }
        set
        {
            IsActive = value;
        }
    }
}

