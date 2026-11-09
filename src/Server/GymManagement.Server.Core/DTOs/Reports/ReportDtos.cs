namespace GymManagement.Server.Core.DTOs.Reports;

public record PeakHourTrafficDto(int Hour, int CheckInCount);

public record DashboardSummaryDto(
    decimal TodayRevenue,
    int TodayCheckIns,
    int NewMembersThisMonth,
    int CompletedPtSessions,
    IReadOnlyList<PeakHourTrafficDto> PeakHours
);

public record RevenueReportRequest(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? PaymentMethod = null,
    string? PlanId = null
);

public record PaymentMethodRevenueDto(
    string PaymentMethod,
    decimal Amount,
    decimal Percentage,
    int TransactionCount
);

public record PackageRevenueDto(
    string PlanId,
    string PlanName,
    decimal Amount,
    int SubscriptionCount
);

public record TimePeriodRevenueDto(
    string PeriodLabel,
    decimal GrossRevenue,
    decimal RefundAmount,
    decimal NetRevenue
);

public record RevenueReportDto(
    decimal TotalGrossRevenue,
    decimal TotalRefundAmount,
    decimal TotalNetRevenue,
    int InvoiceCount,
    IReadOnlyList<PaymentMethodRevenueDto> ByPaymentMethod,
    IReadOnlyList<PackageRevenueDto> ByPackage,
    IReadOnlyList<TimePeriodRevenueDto> Trend
);

public record MemberFluctuationRequest(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null
);

public record PackageDistributionDto(
    string PlanId,
    string PlanName,
    int ActiveCount,
    decimal Percentage
);

public record MemberFluctuationReportDto(
    int TotalActiveMembers,
    int NewMembers,
    int Renewals,
    int FrozenCount,
    int CancelledCount,
    int ExpiredCount,
    decimal RetentionRatePercentage,
    IReadOnlyList<PackageDistributionDto> PackageDistribution
);

public record CheckInTrafficRequest(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null
);

public record DenialReasonDto(
    string Reason,
    int Count,
    decimal Percentage
);

public record CheckInTrafficReportDto(
    int TotalCheckIns,
    int SuccessfulCheckIns,
    int DeniedCheckIns,
    decimal SuccessRatePercentage,
    IReadOnlyList<PeakHourTrafficDto> HourlyDistribution,
    IReadOnlyList<DenialReasonDto> TopDenialReasons
);

public record PtPerformanceRequest(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? TrainerId = null
);

public record PtTrainerPerformanceDto(
    string TrainerId,
    string TrainerName,
    int CompletedSessions,
    int CancelledSessions,
    int AssignedStudentsCount,
    decimal AverageRating
);

public record PtPerformanceReportDto(
    int TotalSessionsScheduled,
    int TotalSessionsCompleted,
    int TotalSessionsCancelled,
    decimal CompletionRatePercentage,
    decimal OverallAverageRating,
    IReadOnlyList<PtTrainerPerformanceDto> Trainers
);

public record ExportReportRequest(
    string ReportType,
    string Format,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? FilterId = null
);

public record ExportReportResultDto(
    string FileName,
    string ContentType,
    byte[] FileData
);
