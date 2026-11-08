namespace GymManagement.Server.Core.DTOs.Members;

public record MemberDto(
    string UserId,
    string FullName,
    string Email,
    string Phone,
    string Status,
    DateTime CreatedAt,
    string? ActivePlanName = null,
    DateTime? EndDate = null,
    int RemainingCheckInSessions = 0,
    int RemainingPtSessions = 0
);

public record MemberSubscriptionDto(
    string SubscriptionId,
    string PlanId,
    string PlanName,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int? RemainingCheckInSessions,
    int? RemainingPtSessions,
    DateTime? FreezeStartDate,
    DateTime? FreezeEndDate,
    string? AssignedPtId,
    string? AssignedPtName
);

public record FreezeSubscriptionRequest(
    string SubscriptionId,
    DateTime FreezeStartDate,
    DateTime FreezeEndDate,
    string Reason
);

public record InBodyEntryRequest(
    string UserId,
    decimal HeightCm,
    decimal WeightKg,
    decimal? BodyFatPercentage,
    decimal? SkeletalMuscleMass,
    string? Notes
);

public record InBodyMetricDto(
    string MetricId,
    decimal HeightCm,
    decimal WeightKg,
    decimal? BodyFatPercentage,
    decimal? SkeletalMuscleMass,
    decimal Bmi,
    string? Notes,
    DateTime MeasuredAt,
    string? RecordedByPtName
);

