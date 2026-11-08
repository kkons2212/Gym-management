namespace GymManagement.Server.Core.DTOs.Packages;

public record PackagePlanDto(
    string PlanId,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    int? MaxCheckInSessions,
    int? TotalPtSessions,
    bool IsPtIncluded,
    string Status
);

