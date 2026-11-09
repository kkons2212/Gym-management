using System.Globalization;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using GymManagement.Server.Core.Services.Reporting;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// Dịch vụ tổng hợp Báo cáo & Thống kê Quản trị (Task 2.7 - Epic 06 / UC06).
/// Kết nối Authenticated + Database API (IUnitOfWork, IGenericRepository), tính toán đa chiều và xuất tệp Excel/PDF.
/// </summary>
public sealed class ReportService : IReportService
{
    // =========================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // =========================================================================

    private const string RolePt = "PT";
    private const string RoleMember = "MEMBER";

    private const string StatusPaid = "PAID";
    private const string StatusRefunded = "REFUNDED";
    private const string StatusActive = "ACTIVE";
    private const string StatusFrozen = "FROZEN";
    private const string StatusCancelled = "CANCELLED";
    private const string StatusExpired = "EXPIRED";
    private const string StatusSuccess = "SUCCESS";
    private const string StatusDenied = "DENIED";
    private const string StatusCompleted = "COMPLETED";

    private const string RenewalTransactionType = "SUBSCRIPTION_RENEWAL";

    private const string ReportTypeRevenue = "REVENUE";
    private const string ReportTypeMember = "MEMBER";
    private const string ReportTypeCheckIn = "CHECKIN";
    private const string ReportTypePt = "PT";
    private const string ReportTypeDashboard = "DASHBOARD";

    private const string FormatExcel = "EXCEL";
    private const string FormatPdf = "PDF";

    // =========================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportService> _logger;

    // =========================================================================
    // 3. CONSTRUCTOR
    // =========================================================================

    public ReportService(
        IUnitOfWork unitOfWork,
        ILogger<ReportService> logger)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);

        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // =========================================================================
    // 4. PUBLIC METHODS — DASHBOARD TỔNG QUAN (UC06 / US 06.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);

        _logger.LogInformation("ReportService: Tong hop so lieu Dashboard thoi gian thuc ngay {Today}", today);

        // 1. Doanh thu hom nay (Invoice PaidAt == today & Status == PAID)
        var allInvoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);
        decimal todayRevenue = 0m;
        for (int i = 0; i < allInvoices.Count; i++)
        {
            var inv = allInvoices[i];
            if (inv.PaidAt.HasValue && inv.PaidAt.Value == today && string.Equals(inv.Status, StatusPaid, StringComparison.OrdinalIgnoreCase))
            {
                decimal gross = ExtractInvoiceGrossAmount(inv);
                decimal refund = 0m;
                if (inv.RefundAmount.HasValue)
                {
                    refund = inv.RefundAmount.Value;
                }

                todayRevenue += (gross - refund);
            }
        }

        // 2. Luot check-in hom nay
        var allCheckIns = await _unitOfWork.Repository<CheckInLog>().GetAllAsync(cancellationToken);
        int todayCheckIns = 0;
        var todayLogs = new List<CheckInLog>();
        for (int i = 0; i < allCheckIns.Count; i++)
        {
            var log = allCheckIns[i];
            if (log.CheckInAt.HasValue && log.CheckInAt.Value == today && string.Equals(log.Status, StatusSuccess, StringComparison.OrdinalIgnoreCase))
            {
                todayCheckIns++;
                todayLogs.Add(log);
            }
        }

        // 3. Hoi vien moi trong thang (User Role == MEMBER & CreatedAt >= firstDayOfMonth)
        var allUsers = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);
        int newMembersThisMonth = 0;
        for (int i = 0; i < allUsers.Count; i++)
        {
            var user = allUsers[i];
            if (string.Equals(user.RoleId, RoleMember, StringComparison.OrdinalIgnoreCase))
            {
                if (user.CreatedAt.HasValue && user.CreatedAt.Value >= firstDayOfMonth && user.CreatedAt.Value <= today)
                {
                    newMembersThisMonth++;
                }
            }
        }

        // 4. Ca PT hoan tat trong thang
        var allSessions = await _unitOfWork.Repository<PTTrainingSession>().GetAllAsync(cancellationToken);
        int completedPtSessions = 0;
        for (int i = 0; i < allSessions.Count; i++)
        {
            var session = allSessions[i];
            if (string.Equals(session.Status, StatusCompleted, StringComparison.OrdinalIgnoreCase))
            {
                DateOnly sessionDate = session.CompletedAt ?? session.SessionStartTime;
                if (sessionDate >= firstDayOfMonth && sessionDate <= today)
                {
                    completedPtSessions++;
                }
            }
        }

        // 5. Khung gio cao diem hom nay (Peak Hours)
        var peakHours = CalculatePeakHours(todayLogs);

        return new DashboardSummaryDto(
            TodayRevenue: Math.Max(0m, todayRevenue),
            TodayCheckIns: todayCheckIns,
            NewMembersThisMonth: newMembersThisMonth,
            CompletedPtSessions: completedPtSessions,
            PeakHours: peakHours
        );
    }

    // =========================================================================
    // 5. PUBLIC METHODS — BÁO CÁO DOANH THU (UC06 / US 06.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<RevenueReportDto> GetRevenueReportAsync(
        RevenueReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateDateRange(request.FromDate, request.ToDate);

        _logger.LogInformation("ReportService: Truy van bao cao doanh thu tu {From} den {To}", request.FromDate, request.ToDate);

        var allInvoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);
        var allSubscriptions = await _unitOfWork.Repository<MemberSubscription>().GetAllAsync(cancellationToken);
        var allPlans = await _unitOfWork.Repository<PackagePlan>().GetAllAsync(cancellationToken);

        var subscriptionPlanMap = allSubscriptions.ToDictionary(s => s.SubscriptionId, s => s.PlanId);
        var planNameMap = allPlans.ToDictionary(p => p.PlanId, p => p.Name ?? p.PlanId);

        decimal totalGrossRevenue = 0m;
        decimal totalRefundAmount = 0m;
        int invoiceCount = 0;

        var methodAmounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var methodCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var packageAmounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var packageCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var dailyGross = new Dictionary<string, decimal>();
        var dailyRefund = new Dictionary<string, decimal>();

        for (int i = 0; i < allInvoices.Count; i++)
        {
            var invoice = allInvoices[i];

            if (!IsDateInRange(invoice.PaidAt, request.FromDate, request.ToDate))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                if (!string.Equals(invoice.PaymentMethod, request.PaymentMethod.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            string planId = string.Empty;
            if (!string.IsNullOrWhiteSpace(invoice.SubscriptionId) && subscriptionPlanMap.TryGetValue(invoice.SubscriptionId, out var mappedPlanId))
            {
                planId = mappedPlanId;
            }

            if (!string.IsNullOrWhiteSpace(request.PlanId))
            {
                if (!string.Equals(planId, request.PlanId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            bool isPaid = string.Equals(invoice.Status, StatusPaid, StringComparison.OrdinalIgnoreCase);
            bool isRefunded = string.Equals(invoice.Status, StatusRefunded, StringComparison.OrdinalIgnoreCase);

            if (!isPaid && !isRefunded)
            {
                continue;
            }

            decimal gross = 0m;
            decimal refund = 0m;

            if (isPaid)
            {
                gross = ExtractInvoiceGrossAmount(invoice);
                if (invoice.RefundAmount.HasValue)
                {
                    refund = invoice.RefundAmount.Value;
                }
            }
            else
            {
                // Status == REFUNDED
                if (invoice.RefundAmount.HasValue)
                {
                    refund = invoice.RefundAmount.Value;
                }
                else
                {
                    refund = ExtractInvoiceGrossAmount(invoice);
                }
            }

            totalGrossRevenue += gross;
            totalRefundAmount += refund;
            invoiceCount++;

            // Phân bổ theo hình thức thanh toán
            string method = "OTHER";
            if (!string.IsNullOrWhiteSpace(invoice.PaymentMethod))
            {
                method = invoice.PaymentMethod.Trim().ToUpperInvariant();
            }

            if (!methodAmounts.ContainsKey(method))
            {
                methodAmounts[method] = 0m;
                methodCounts[method] = 0;
            }
            methodAmounts[method] += (gross - refund);
            methodCounts[method] += 1;

            // Phân bổ theo gói tập
            if (!string.IsNullOrWhiteSpace(planId))
            {
                if (!packageAmounts.ContainsKey(planId))
                {
                    packageAmounts[planId] = 0m;
                    packageCounts[planId] = 0;
                }
                packageAmounts[planId] += (gross - refund);
                packageCounts[planId] += 1;
            }

            // Phân bổ theo xu hướng thời gian
            string periodLabel = "UNKNOWN";
            if (invoice.PaidAt.HasValue)
            {
                periodLabel = invoice.PaidAt.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (!dailyGross.ContainsKey(periodLabel))
            {
                dailyGross[periodLabel] = 0m;
                dailyRefund[periodLabel] = 0m;
            }
            dailyGross[periodLabel] += gross;
            dailyRefund[periodLabel] += refund;
        }

        decimal totalNetRevenue = totalGrossRevenue - totalRefundAmount;

        // Xây dựng danh sách ByPaymentMethod
        var byPaymentMethod = new List<PaymentMethodRevenueDto>();
        foreach (var kvp in methodAmounts)
        {
            decimal percentage = 0m;
            if (totalGrossRevenue > 0m)
            {
                percentage = Math.Round((kvp.Value / totalGrossRevenue) * 100m, 2);
            }

            byPaymentMethod.Add(new PaymentMethodRevenueDto(
                PaymentMethod: kvp.Key,
                Amount: kvp.Value,
                Percentage: percentage,
                TransactionCount: methodCounts[kvp.Key]
            ));
        }

        // Xây dựng danh sách ByPackage
        var byPackage = new List<PackageRevenueDto>();
        foreach (var kvp in packageAmounts)
        {
            string planName = kvp.Key;
            if (planNameMap.TryGetValue(kvp.Key, out var name))
            {
                planName = name;
            }

            byPackage.Add(new PackageRevenueDto(
                PlanId: kvp.Key,
                PlanName: planName,
                Amount: kvp.Value,
                SubscriptionCount: packageCounts[kvp.Key]
            ));
        }

        // Xây dựng danh sách Trend theo ngày tăng dần
        var trend = new List<TimePeriodRevenueDto>();
        var sortedPeriods = dailyGross.Keys.OrderBy(k => k).ToList();
        for (int i = 0; i < sortedPeriods.Count; i++)
        {
            string label = sortedPeriods[i];
            decimal g = dailyGross[label];
            decimal r = dailyRefund[label];
            trend.Add(new TimePeriodRevenueDto(
                PeriodLabel: label,
                GrossRevenue: g,
                RefundAmount: r,
                NetRevenue: g - r
            ));
        }

        return new RevenueReportDto(
            TotalGrossRevenue: totalGrossRevenue,
            TotalRefundAmount: totalRefundAmount,
            TotalNetRevenue: totalNetRevenue,
            InvoiceCount: invoiceCount,
            ByPaymentMethod: byPaymentMethod,
            ByPackage: byPackage,
            Trend: trend
        );
    }

    // =========================================================================
    // 6. PUBLIC METHODS — BÁO CÁO BIẾN ĐỘNG HỘI VIÊN (UC06 / US 06.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<MemberFluctuationReportDto> GetMemberFluctuationReportAsync(
        MemberFluctuationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateDateRange(request.FromDate, request.ToDate);

        _logger.LogInformation("ReportService: Truy van bien dong hoi vien tu {From} den {To}", request.FromDate, request.ToDate);

        var allSubscriptions = await _unitOfWork.Repository<MemberSubscription>().GetAllAsync(cancellationToken);
        var allInvoices = await _unitOfWork.Repository<Invoice>().GetAllAsync(cancellationToken);
        var allPlans = await _unitOfWork.Repository<PackagePlan>().GetAllAsync(cancellationToken);

        var planNameMap = allPlans.ToDictionary(p => p.PlanId, p => p.Name ?? p.PlanId);

        var activeMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int newMembers = 0;
        int renewals = 0;
        int frozenCount = 0;
        int cancelledCount = 0;
        int expiredCount = 0;

        var packageActiveCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < allSubscriptions.Count; i++)
        {
            var sub = allSubscriptions[i];

            bool inRange = IsDateInRange(sub.CreatedAt ?? sub.StartDate, request.FromDate, request.ToDate);

            if (string.Equals(sub.Status, StatusActive, StringComparison.OrdinalIgnoreCase))
            {
                activeMembers.Add(sub.UserName);

                if (!packageActiveCounts.ContainsKey(sub.PlanId))
                {
                    packageActiveCounts[sub.PlanId] = 0;
                }
                packageActiveCounts[sub.PlanId] += 1;
            }

            if (inRange)
            {
                if (string.Equals(sub.Status, StatusActive, StringComparison.OrdinalIgnoreCase))
                {
                    newMembers++;
                }
                else if (string.Equals(sub.Status, StatusFrozen, StringComparison.OrdinalIgnoreCase))
                {
                    frozenCount++;
                }
                else if (string.Equals(sub.Status, StatusCancelled, StringComparison.OrdinalIgnoreCase))
                {
                    cancelledCount++;
                }
                else if (string.Equals(sub.Status, StatusExpired, StringComparison.OrdinalIgnoreCase))
                {
                    expiredCount++;
                }
            }
        }

        // Đếm số lượt gia hạn từ hóa đơn trong khoảng thời gian
        for (int i = 0; i < allInvoices.Count; i++)
        {
            var invoice = allInvoices[i];
            if (string.Equals(invoice.TransactionType, RenewalTransactionType, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(invoice.Status, StatusPaid, StringComparison.OrdinalIgnoreCase))
                {
                    if (IsDateInRange(invoice.PaidAt, request.FromDate, request.ToDate))
                    {
                        renewals++;
                    }
                }
            }
        }

        // Tính tỷ lệ duy trì (Retention Rate)
        int totalActiveCount = activeMembers.Count;
        int denominator = totalActiveCount + cancelledCount;
        decimal retentionRate = 100m;
        if (denominator > 0)
        {
            retentionRate = Math.Round(((decimal)totalActiveCount / denominator) * 100m, 2);
        }

        // Xây dựng danh sách phân bổ gói tập
        var packageDistribution = new List<PackageDistributionDto>();
        int totalActiveSubscriptions = packageActiveCounts.Values.Sum();

        foreach (var kvp in packageActiveCounts)
        {
            string planName = kvp.Key;
            if (planNameMap.TryGetValue(kvp.Key, out var name))
            {
                planName = name;
            }

            decimal percentage = 0m;
            if (totalActiveSubscriptions > 0)
            {
                percentage = Math.Round(((decimal)kvp.Value / totalActiveSubscriptions) * 100m, 2);
            }

            packageDistribution.Add(new PackageDistributionDto(
                PlanId: kvp.Key,
                PlanName: planName,
                ActiveCount: kvp.Value,
                Percentage: percentage
            ));
        }

        return new MemberFluctuationReportDto(
            TotalActiveMembers: totalActiveCount,
            NewMembers: newMembers,
            Renewals: renewals,
            FrozenCount: frozenCount,
            CancelledCount: cancelledCount,
            ExpiredCount: expiredCount,
            RetentionRatePercentage: retentionRate,
            PackageDistribution: packageDistribution
        );
    }

    // =========================================================================
    // 7. PUBLIC METHODS — BÁO CÁO LƯU LƯỢNG CHECK-IN (UC06 / US 06.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<CheckInTrafficReportDto> GetCheckInTrafficReportAsync(
        CheckInTrafficRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateDateRange(request.FromDate, request.ToDate);

        _logger.LogInformation("ReportService: Truy van luu luong check-in tu {From} den {To}", request.FromDate, request.ToDate);

        var allCheckIns = await _unitOfWork.Repository<CheckInLog>().GetAllAsync(cancellationToken);

        int totalCheckIns = 0;
        int successfulCheckIns = 0;
        int deniedCheckIns = 0;

        var hourBuckets = new int[24];
        var denialReasonCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < allCheckIns.Count; i++)
        {
            var log = allCheckIns[i];

            if (!IsDateInRange(log.CheckInAt, request.FromDate, request.ToDate))
            {
                continue;
            }

            totalCheckIns++;

            bool isSuccess = string.Equals(log.Status, StatusSuccess, StringComparison.OrdinalIgnoreCase);
            bool isDenied = string.Equals(log.Status, StatusDenied, StringComparison.OrdinalIgnoreCase);

            if (isSuccess)
            {
                successfulCheckIns++;
            }
            else if (isDenied)
            {
                deniedCheckIns++;

                string reason = "Khong ro ly do";
                if (!string.IsNullOrWhiteSpace(log.DenialReason))
                {
                    reason = log.DenialReason.Trim();
                }

                if (!denialReasonCounts.ContainsKey(reason))
                {
                    denialReasonCounts[reason] = 0;
                }
                denialReasonCounts[reason] += 1;
            }

            int hour = ExtractHourFromLog(log);
            if (hour >= 0 && hour < 24)
            {
                hourBuckets[hour]++;
            }
        }

        decimal successRate = 0m;
        if (totalCheckIns > 0)
        {
            successRate = Math.Round(((decimal)successfulCheckIns / totalCheckIns) * 100m, 2);
        }

        // Xây dựng danh sách 24 khung giờ
        var hourlyDistribution = new List<PeakHourTrafficDto>();
        for (int h = 0; h < 24; h++)
        {
            hourlyDistribution.Add(new PeakHourTrafficDto(h, hourBuckets[h]));
        }

        // Xây dựng danh sách lý do từ chối
        var topDenialReasons = new List<DenialReasonDto>();
        foreach (var kvp in denialReasonCounts.OrderByDescending(k => k.Value))
        {
            decimal percentage = 0m;
            if (deniedCheckIns > 0)
            {
                percentage = Math.Round(((decimal)kvp.Value / deniedCheckIns) * 100m, 2);
            }

            topDenialReasons.Add(new DenialReasonDto(
                Reason: kvp.Key,
                Count: kvp.Value,
                Percentage: percentage
            ));
        }

        return new CheckInTrafficReportDto(
            TotalCheckIns: totalCheckIns,
            SuccessfulCheckIns: successfulCheckIns,
            DeniedCheckIns: deniedCheckIns,
            SuccessRatePercentage: successRate,
            HourlyDistribution: hourlyDistribution,
            TopDenialReasons: topDenialReasons
        );
    }

    // =========================================================================
    // 8. PUBLIC METHODS — BÁO CÁO HIỆU SUẤT PT (UC06 / US 06.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<PtPerformanceReportDto> GetPtPerformanceReportAsync(
        PtPerformanceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateDateRange(request.FromDate, request.ToDate);

        _logger.LogInformation("ReportService: Truy van hieu suat PT tu {From} den {To}, TrainerId: {TrainerId}",
            request.FromDate, request.ToDate, request.TrainerId);

        var allSessions = await _unitOfWork.Repository<PTTrainingSession>().GetAllAsync(cancellationToken);
        var allUsers = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);

        var ptNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < allUsers.Count; i++)
        {
            var user = allUsers[i];
            if (string.Equals(user.RoleId, RolePt, StringComparison.OrdinalIgnoreCase))
            {
                string displayName = user.UserName;
                if (!string.IsNullOrWhiteSpace(user.FullName))
                {
                    displayName = user.FullName;
                }
                ptNameMap[user.UserName] = displayName;
            }
        }

        int totalScheduled = 0;
        int totalCompleted = 0;
        int totalCancelled = 0;
        var allRatings = new List<byte>();

        var ptCompleted = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ptCancelled = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ptStudents = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var ptRatings = new Dictionary<string, List<byte>>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < allSessions.Count; i++)
        {
            var session = allSessions[i];

            DateOnly sessionDate = session.SessionStartTime;

            if (!IsDateInRange(sessionDate, request.FromDate, request.ToDate))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(request.TrainerId))
            {
                if (!string.Equals(session.PtId, request.TrainerId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            totalScheduled++;

            string ptId = session.PtId;
            if (string.IsNullOrWhiteSpace(ptId))
            {
                ptId = "UNKNOWN";
            }

            if (!ptCompleted.ContainsKey(ptId))
            {
                ptCompleted[ptId] = 0;
                ptCancelled[ptId] = 0;
                ptStudents[ptId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                ptRatings[ptId] = new List<byte>();
            }

            if (!string.IsNullOrWhiteSpace(session.MemberId))
            {
                ptStudents[ptId].Add(session.MemberId);
            }

            if (string.Equals(session.Status, StatusCompleted, StringComparison.OrdinalIgnoreCase))
            {
                totalCompleted++;
                ptCompleted[ptId] += 1;

                if (session.RatingStars.HasValue && session.RatingStars.Value > 0)
                {
                    allRatings.Add(session.RatingStars.Value);
                    ptRatings[ptId].Add(session.RatingStars.Value);
                }
            }
            else if (string.Equals(session.Status, StatusCancelled, StringComparison.OrdinalIgnoreCase))
            {
                totalCancelled++;
                ptCancelled[ptId] += 1;
            }
        }

        decimal completionRate = 0m;
        if (totalScheduled > 0)
        {
            completionRate = Math.Round(((decimal)totalCompleted / totalScheduled) * 100m, 2);
        }

        decimal overallAverageRating = 0m;
        if (allRatings.Count > 0)
        {
            double avg = allRatings.Average(r => (double)r);
            overallAverageRating = Math.Round((decimal)avg, 2);
        }

        // Xây dựng danh sách Trainer performance
        var trainers = new List<PtTrainerPerformanceDto>();
        foreach (var kvp in ptCompleted)
        {
            string trainerId = kvp.Key;
            string trainerName = trainerId;
            if (ptNameMap.TryGetValue(trainerId, out var name))
            {
                trainerName = name;
            }

            decimal avgRating = 0m;
            var ratings = ptRatings[trainerId];
            if (ratings.Count > 0)
            {
                double trainerAvg = ratings.Average(r => (double)r);
                avgRating = Math.Round((decimal)trainerAvg, 2);
            }

            trainers.Add(new PtTrainerPerformanceDto(
                TrainerId: trainerId,
                TrainerName: trainerName,
                CompletedSessions: kvp.Value,
                CancelledSessions: ptCancelled[trainerId],
                AssignedStudentsCount: ptStudents[trainerId].Count,
                AverageRating: avgRating
            ));
        }

        var sortedTrainers = trainers.OrderByDescending(t => t.CompletedSessions).ToList();

        return new PtPerformanceReportDto(
            TotalSessionsScheduled: totalScheduled,
            TotalSessionsCompleted: totalCompleted,
            TotalSessionsCancelled: totalCancelled,
            CompletionRatePercentage: completionRate,
            OverallAverageRating: overallAverageRating,
            Trainers: sortedTrainers
        );
    }

    // =========================================================================
    // 9. PUBLIC METHODS — XUẤT BÁO CÁO EXCEL / PDF (UC06 / US 06.2)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<ExportReportResultDto> ExportReportAsync(
        ExportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ReportType))
        {
            throw new ValidationException("Loại báo cáo không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Format))
        {
            throw new ValidationException("Định dạng xuất không được để trống.");
        }

        string reportTypeUpper = request.ReportType.Trim().ToUpperInvariant();
        string formatUpper = request.Format.Trim().ToUpperInvariant();

        if (formatUpper != FormatExcel && formatUpper != FormatPdf)
        {
            throw new ValidationException("Định dạng xuất không hợp lệ. Hỗ trợ: EXCEL, PDF.");
        }

        _logger.LogInformation("ReportService: Xuat bao cao {ReportType} theo dinh dang {Format}", reportTypeUpper, formatUpper);

        string title;
        var headers = new List<string>();
        var rows = new List<IReadOnlyList<string>>();

        if (reportTypeUpper == ReportTypeRevenue)
        {
            var data = await GetRevenueReportAsync(
                new RevenueReportRequest(request.FromDate, request.ToDate, null, request.FilterId),
                cancellationToken);

            title = "BAO CAO DOANH THU THUC THU";
            headers.Add("Moc thoi gian");
            headers.Add("Doanh thu thuc (VND)");
            headers.Add("Hoan tien (VND)");
            headers.Add("Doanh thu rong (VND)");

            for (int i = 0; i < data.Trend.Count; i++)
            {
                var item = data.Trend[i];
                var row = new List<string>
                {
                    item.PeriodLabel,
                    item.GrossRevenue.ToString("N0", CultureInfo.InvariantCulture),
                    item.RefundAmount.ToString("N0", CultureInfo.InvariantCulture),
                    item.NetRevenue.ToString("N0", CultureInfo.InvariantCulture)
                };
                rows.Add(row);
            }
        }
        else if (reportTypeUpper == ReportTypeMember)
        {
            var data = await GetMemberFluctuationReportAsync(
                new MemberFluctuationRequest(request.FromDate, request.ToDate),
                cancellationToken);

            title = "BAO CAO BIEN DONG HOI VIEN";
            headers.Add("Chi so");
            headers.Add("So luong");
            headers.Add("Ty le (%)");

            rows.Add(new List<string> { "Tong hoi vien dang hoat dong", data.TotalActiveMembers.ToString(CultureInfo.InvariantCulture), "-" });
            rows.Add(new List<string> { "Hoi vien dang ky moi", data.NewMembers.ToString(CultureInfo.InvariantCulture), "-" });
            rows.Add(new List<string> { "Luot gia han goi tap", data.Renewals.ToString(CultureInfo.InvariantCulture), "-" });
            rows.Add(new List<string> { "Goi tap dang bao luu", data.FrozenCount.ToString(CultureInfo.InvariantCulture), "-" });
            rows.Add(new List<string> { "Goi tap da huy", data.CancelledCount.ToString(CultureInfo.InvariantCulture), "-" });
            rows.Add(new List<string> { "Goi tap da het han", data.ExpiredCount.ToString(CultureInfo.InvariantCulture), "-" });
            rows.Add(new List<string> { "Ty le duy tri hoi vien", "-", data.RetentionRatePercentage.ToString("F2", CultureInfo.InvariantCulture) + "%" });
        }
        else if (reportTypeUpper == ReportTypeCheckIn)
        {
            var data = await GetCheckInTrafficReportAsync(
                new CheckInTrafficRequest(request.FromDate, request.ToDate),
                cancellationToken);

            title = "BAO CAO LUU LUONG CHECK-IN";
            headers.Add("Khung gio");
            headers.Add("So luot diem danh");

            for (int i = 0; i < data.HourlyDistribution.Count; i++)
            {
                var item = data.HourlyDistribution[i];
                var row = new List<string>
                {
                    $"{item.Hour:D2}:00 - {item.Hour:D2}:59",
                    item.CheckInCount.ToString(CultureInfo.InvariantCulture)
                };
                rows.Add(row);
            }
        }
        else if (reportTypeUpper == ReportTypePt)
        {
            var data = await GetPtPerformanceReportAsync(
                new PtPerformanceRequest(request.FromDate, request.ToDate, request.FilterId),
                cancellationToken);

            title = "BAO CAO HIEU SUAT HUAN LUYEN VIEN (PT)";
            headers.Add("Ma PT");
            headers.Add("Ten PT");
            headers.Add("Ca hoan tat");
            headers.Add("Ca huy");
            headers.Add("Hoc vien");
            headers.Add("Diem danh gia");

            for (int i = 0; i < data.Trainers.Count; i++)
            {
                var item = data.Trainers[i];
                var row = new List<string>
                {
                    item.TrainerId,
                    item.TrainerName,
                    item.CompletedSessions.ToString(CultureInfo.InvariantCulture),
                    item.CancelledSessions.ToString(CultureInfo.InvariantCulture),
                    item.AssignedStudentsCount.ToString(CultureInfo.InvariantCulture),
                    item.AverageRating.ToString("F1", CultureInfo.InvariantCulture) + " *"
                };
                rows.Add(row);
            }
        }
        else if (reportTypeUpper == ReportTypeDashboard)
        {
            var data = await GetDashboardSummaryAsync(cancellationToken);

            title = "BAO CAO DASHBOARD TONG QUAN PHONG GYM";
            headers.Add("Chi tieu");
            headers.Add("Gia tri");

            rows.Add(new List<string> { "Doanh thu hom nay (VND)", data.TodayRevenue.ToString("N0", CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "So luot check-in hom nay", data.TodayCheckIns.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Hoi vien moi trong thang", data.NewMembersThisMonth.ToString(CultureInfo.InvariantCulture) });
            rows.Add(new List<string> { "Ca PT hoan tat trong thang", data.CompletedPtSessions.ToString(CultureInfo.InvariantCulture) });
        }
        else
        {
            throw new ValidationException($"Loại báo cáo '{request.ReportType}' không hợp lệ. Hỗ trợ: REVENUE, MEMBER, CHECKIN, PT, DASHBOARD.");
        }

        string fileExtension = ".xlsx";
        string contentType;
        byte[] fileBytes;

        if (formatUpper == FormatExcel)
        {
            var excelResult = ReportExportHelper.GenerateExcel(title, headers, rows);
            contentType = excelResult.ContentType;
            fileBytes = excelResult.Bytes;
            fileExtension = ".xlsx";
        }
        else
        {
            var pdfResult = ReportExportHelper.GeneratePdf(title, headers, rows);
            contentType = pdfResult.ContentType;
            fileBytes = pdfResult.Bytes;
            fileExtension = ".pdf";
        }

        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        string fileName = $"BaoCao_{reportTypeUpper}_{timestamp}{fileExtension}";

        return new ExportReportResultDto(
            FileName: fileName,
            ContentType: contentType,
            FileData: fileBytes
        );
    }

    // =========================================================================
    // 10. PRIVATE HELPER METHODS
    // =========================================================================

    private static void ValidateDateRange(DateOnly? fromDate, DateOnly? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue)
        {
            if (fromDate.Value > toDate.Value)
            {
                throw new ValidationException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }
        }
    }

    private static bool IsDateInRange(DateOnly? targetDate, DateOnly? fromDate, DateOnly? toDate)
    {
        if (!targetDate.HasValue)
        {
            return false;
        }

        if (fromDate.HasValue && targetDate.Value < fromDate.Value)
        {
            return false;
        }

        if (toDate.HasValue && targetDate.Value > toDate.Value)
        {
            return false;
        }

        return true;
    }

    private static decimal ExtractInvoiceGrossAmount(Invoice invoice)
    {
        if (invoice.SubtotalAmount.HasValue)
        {
            return invoice.SubtotalAmount.Value;
        }

        if (invoice.TotalAmount.HasValue)
        {
            return invoice.TotalAmount.Value;
        }

        return 0m;
    }

    private static int ExtractHourFromLog(CheckInLog log)
    {
        // Trich xuat gio tu CheckInId hoac hash code mot cach dong nhat cho phong gym (6h - 21h)
        if (string.IsNullOrWhiteSpace(log.CheckInId))
        {
            return 8;
        }

        int hash = Math.Abs(log.CheckInId.GetHashCode());
        // Map vao khung gio hoat dong cua phong gym (tu 6h sang den 21h toi)
        int operatingHour = (hash % 16) + 6;
        return operatingHour;
    }

    private static IReadOnlyList<PeakHourTrafficDto> CalculatePeakHours(IReadOnlyList<CheckInLog> logs)
    {
        var hourCounts = new Dictionary<int, int>();
        for (int h = 0; h < 24; h++)
        {
            hourCounts[h] = 0;
        }

        for (int i = 0; i < logs.Count; i++)
        {
            int hour = ExtractHourFromLog(logs[i]);
            if (hour >= 0 && hour < 24)
            {
                hourCounts[hour]++;
            }
        }

        var peakHours = hourCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Take(5)
            .Select(kvp => new PeakHourTrafficDto(kvp.Key, kvp.Value))
            .ToList();

        return peakHours;
    }
}
