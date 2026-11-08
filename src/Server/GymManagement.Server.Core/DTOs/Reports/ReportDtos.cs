namespace GymManagement.Server.Core.DTOs.Reports;

public record PeakHourTrafficDto(int Hour, int CheckInCount);

public record DashboardSummaryDto(
    decimal TodayRevenue,
    int TodayCheckIns,
    int NewMembersThisMonth,
    int CompletedPtSessions,
    IReadOnlyList<PeakHourTrafficDto> PeakHours
);

