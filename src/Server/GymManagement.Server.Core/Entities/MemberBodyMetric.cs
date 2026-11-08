namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng MemberBodyMetric trong CSDL.
/// </summary>
public sealed class MemberBodyMetric
{
    public string BodyMetricId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public decimal WeightKg { get; set; }

    public decimal HeightCm { get; set; }

    public decimal SkeletalMuscleMass { get; set; }

    public decimal BodyFatPercentage { get; set; }

    public decimal Bmi { get; set; }

    public string? Notes { get; set; }

    public DateOnly MeasuredAt { get; set; }

    public DateOnly measured_at
    {
        get
        {
            return MeasuredAt;
        }
        set
        {
            MeasuredAt = value;
        }
    }

    public string? RecordedByPtId { get; set; }

    public string? RecordedByPtID
    {
        get
        {
            return RecordedByPtId;
        }
        set
        {
            RecordedByPtId = value;
        }
    }

    public string? PTSessionId { get; set; }
}

