using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Reports;

namespace GymManagement.Desktop.ViewModels;

public record ReportTypeOption(string Key, string DisplayName);

public partial class DashboardViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public IReadOnlyList<ReportTypeOption> ReportTypeOptions { get; } = new List<ReportTypeOption>
    {
        new("OVERVIEW", "Tổng quan Dashboard"),
        new("REVENUE", "Báo cáo Doanh thu"),
        new("MEMBER", "Báo cáo Hội viên"),
        new("CHECKIN", "Báo cáo Điểm danh"),
        new("PT", "Hiệu suất HLV (PT)")
    };

    [ObservableProperty]
    private ReportTypeOption _selectedReportTypeOption;

    [ObservableProperty]
    private DateTimeOffset? _selectedFromDate;

    [ObservableProperty]
    private DateTimeOffset? _selectedToDate;

    [ObservableProperty]
    private string _selectedDatePreset = "THIS_MONTH";

    [ObservableProperty]
    private string? _dateValidationError;

    [ObservableProperty]
    private bool _hasDateValidationError;

    [ObservableProperty]
    private decimal _todayRevenue;

    [ObservableProperty]
    private int _todayCheckIns;

    [ObservableProperty]
    private int _newMembersThisMonth;

    [ObservableProperty]
    private int _completedPtSessions;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isExporting;

    public bool CanExport
    {
        get
        {
            if (IsExporting || IsLoading)
            {
                return false;
            }
            if (HasDateValidationError)
            {
                return false;
            }
            return true;
        }
    }

    public string FilterSummaryText
    {
        get
        {
            string fromStr = "---";
            if (SelectedFromDate.HasValue)
            {
                fromStr = SelectedFromDate.Value.ToString("dd/MM/yyyy");
            }

            string toStr = "---";
            if (SelectedToDate.HasValue)
            {
                toStr = SelectedToDate.Value.ToString("dd/MM/yyyy");
            }

            string reportName = "Tổng quan Dashboard";
            if (SelectedReportTypeOption != null)
            {
                reportName = SelectedReportTypeOption.DisplayName;
            }

            return $"Thời gian: {fromStr} - {toStr} | Phân loại: {reportName}";
        }
    }

    public string ActiveReportTypeKey
    {
        get
        {
            if (SelectedReportTypeOption != null)
            {
                return SelectedReportTypeOption.Key;
            }
            return "OVERVIEW";
        }
    }

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _exportStatus = string.Empty;

    // Revenue KPIs & Breakdown
    [ObservableProperty]
    private decimal _totalGrossRevenue;

    [ObservableProperty]
    private decimal _totalNetRevenue;

    [ObservableProperty]
    private decimal _totalRefundAmount;

    [ObservableProperty]
    private int _totalInvoices;

    // Member Fluctuation KPIs & Breakdown
    [ObservableProperty]
    private int _totalActiveMembers;

    [ObservableProperty]
    private int _newMemberSignups;

    [ObservableProperty]
    private int _memberRenewals;

    [ObservableProperty]
    private decimal _memberRetentionRate;

    // CheckIn KPIs
    [ObservableProperty]
    private int _totalCheckInTraffic;

    [ObservableProperty]
    private int _successfulCheckInCount;

    [ObservableProperty]
    private int _deniedCheckInCount;

    [ObservableProperty]
    private decimal _checkInSuccessRate;

    // PT Performance KPIs
    [ObservableProperty]
    private int _totalPtSessionsScheduled;

    [ObservableProperty]
    private int _totalPtSessionsCompleted;

    [ObservableProperty]
    private decimal _ptCompletionRate;

    [ObservableProperty]
    private decimal _ptAverageRating;

    public ObservableCollection<PeakHourTrafficDto> PeakHours { get; } = [];
    public ObservableCollection<PackageRevenueDto> PackageRevenues { get; } = [];
    public ObservableCollection<PaymentMethodRevenueDto> PaymentMethodRevenues { get; } = [];
    public ObservableCollection<PackageDistributionDto> MemberPackageDistributions { get; } = [];
    public ObservableCollection<DenialReasonDto> CheckInDenialReasons { get; } = [];
    public ObservableCollection<PtTrainerPerformanceDto> TrainerPerformances { get; } = [];

    public DashboardViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        _selectedReportTypeOption = ReportTypeOptions[0];

        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, today.Month, 1);
        _selectedFromDate = new DateTimeOffset(firstDay);
        _selectedToDate = new DateTimeOffset(today);
    }

    partial void OnIsExportingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanExport));
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanExport));
    }

    partial void OnSelectedFromDateChanged(DateTimeOffset? value)
    {
        ValidateDates();
    }

    partial void OnSelectedToDateChanged(DateTimeOffset? value)
    {
        ValidateDates();
    }

    partial void OnSelectedReportTypeOptionChanged(ReportTypeOption value)
    {
        OnPropertyChanged(nameof(FilterSummaryText));
        OnPropertyChanged(nameof(ActiveReportTypeKey));
        _ = LoadDashboardAsync();
    }

    private void ValidateDates()
    {
        if (SelectedFromDate.HasValue && SelectedToDate.HasValue)
        {
            if (SelectedToDate.Value.Date < SelectedFromDate.Value.Date)
            {
                DateValidationError = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu.";
                HasDateValidationError = true;
                OnPropertyChanged(nameof(CanExport));
                OnPropertyChanged(nameof(FilterSummaryText));
                return;
            }
        }

        DateValidationError = null;
        HasDateValidationError = false;
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(FilterSummaryText));
    }

    [RelayCommand]
    public void ApplyPreset(string preset)
    {
        var today = DateTime.Today;
        SelectedDatePreset = preset;

        if (preset == "TODAY")
        {
            SelectedFromDate = new DateTimeOffset(today);
            SelectedToDate = new DateTimeOffset(today);
        }
        else if (preset == "THIS_WEEK")
        {
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-1 * diff);
            SelectedFromDate = new DateTimeOffset(monday);
            SelectedToDate = new DateTimeOffset(today);
        }
        else if (preset == "THIS_MONTH")
        {
            var firstDay = new DateTime(today.Year, today.Month, 1);
            SelectedFromDate = new DateTimeOffset(firstDay);
            SelectedToDate = new DateTimeOffset(today);
        }
        else if (preset == "THIS_YEAR")
        {
            var firstDayOfYear = new DateTime(today.Year, 1, 1);
            SelectedFromDate = new DateTimeOffset(firstDayOfYear);
            SelectedToDate = new DateTimeOffset(today);
        }

        _ = LoadDashboardAsync();
    }

    public async Task<byte[]> GetExcelBytesAsync()
    {
        string reportType = "REVENUE";
        if (SelectedReportTypeOption != null && SelectedReportTypeOption.Key != "OVERVIEW")
        {
            reportType = SelectedReportTypeOption.Key;
        }

        DateTime? from = null;
        if (SelectedFromDate.HasValue)
        {
            from = SelectedFromDate.Value.DateTime;
        }

        DateTime? to = null;
        if (SelectedToDate.HasValue)
        {
            to = SelectedToDate.Value.DateTime;
        }

        return await _apiClient.ExportExcelReportAsync(reportType, from, to);
    }

    public async Task<byte[]> GetPdfBytesAsync()
    {
        string reportType = "REVENUE";
        if (SelectedReportTypeOption != null && SelectedReportTypeOption.Key != "OVERVIEW")
        {
            reportType = SelectedReportTypeOption.Key;
        }

        DateTime? from = null;
        if (SelectedFromDate.HasValue)
        {
            from = SelectedFromDate.Value.DateTime;
        }

        DateTime? to = null;
        if (SelectedToDate.HasValue)
        {
            to = SelectedToDate.Value.DateTime;
        }

        return await _apiClient.ExportPdfReportAsync(reportType, from, to);
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        if (HasDateValidationError)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            DateOnly? fromDate = null;
            if (SelectedFromDate.HasValue)
            {
                fromDate = DateOnly.FromDateTime(SelectedFromDate.Value.DateTime);
            }

            DateOnly? toDate = null;
            if (SelectedToDate.HasValue)
            {
                toDate = DateOnly.FromDateTime(SelectedToDate.Value.DateTime);
            }

            // 1. Overview Summary
            var summary = await _apiClient.GetDashboardSummaryAsync();
            TodayRevenue = summary.TodayRevenue;
            TodayCheckIns = summary.TodayCheckIns;
            NewMembersThisMonth = summary.NewMembersThisMonth;
            CompletedPtSessions = summary.CompletedPtSessions;

            PeakHours.Clear();
            foreach (var item in summary.PeakHours)
            {
                PeakHours.Add(item);
            }

            // 2. Revenue Report
            var revenueReport = await _apiClient.GetRevenueReportAsync(fromDate, toDate);
            TotalGrossRevenue = revenueReport.TotalGrossRevenue;
            TotalRefundAmount = revenueReport.TotalRefundAmount;
            TotalNetRevenue = revenueReport.TotalNetRevenue;
            TotalInvoices = revenueReport.InvoiceCount;

            PackageRevenues.Clear();
            foreach (var pkg in revenueReport.ByPackage)
            {
                PackageRevenues.Add(pkg);
            }

            PaymentMethodRevenues.Clear();
            foreach (var pm in revenueReport.ByPaymentMethod)
            {
                PaymentMethodRevenues.Add(pm);
            }

            // 3. Member Fluctuation Report
            var memberReport = await _apiClient.GetMemberFluctuationReportAsync(fromDate, toDate);
            TotalActiveMembers = memberReport.TotalActiveMembers;
            NewMemberSignups = memberReport.NewMembers;
            MemberRenewals = memberReport.Renewals;
            MemberRetentionRate = memberReport.RetentionRatePercentage;

            MemberPackageDistributions.Clear();
            foreach (var dist in memberReport.PackageDistribution)
            {
                MemberPackageDistributions.Add(dist);
            }

            // 4. CheckIn Traffic Report
            var checkInReport = await _apiClient.GetCheckInTrafficReportAsync(fromDate, toDate);
            TotalCheckInTraffic = checkInReport.TotalCheckIns;
            SuccessfulCheckInCount = checkInReport.SuccessfulCheckIns;
            DeniedCheckInCount = checkInReport.DeniedCheckIns;
            CheckInSuccessRate = checkInReport.SuccessRatePercentage;

            CheckInDenialReasons.Clear();
            foreach (var reason in checkInReport.TopDenialReasons)
            {
                CheckInDenialReasons.Add(reason);
            }

            // 5. PT Performance Report
            var ptReport = await _apiClient.GetPtPerformanceReportAsync(fromDate, toDate);
            TotalPtSessionsScheduled = ptReport.TotalSessionsScheduled;
            TotalPtSessionsCompleted = ptReport.TotalSessionsCompleted;
            PtCompletionRate = ptReport.CompletionRatePercentage;
            PtAverageRating = ptReport.OverallAverageRating;

            TrainerPerformances.Clear();
            foreach (var trainer in ptReport.Trainers)
            {
                TrainerPerformances.Add(trainer);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu báo cáo: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
