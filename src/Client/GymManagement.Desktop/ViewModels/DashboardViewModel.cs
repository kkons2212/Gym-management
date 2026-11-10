using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Reports;

namespace GymManagement.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

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
    private string? _errorMessage;

    [ObservableProperty]
    private string _exportStatus = string.Empty;

    [ObservableProperty]
    private decimal _totalGrossRevenue;

    [ObservableProperty]
    private decimal _totalNetRevenue;

    [ObservableProperty]
    private decimal _totalRefundAmount;

    [ObservableProperty]
    private int _totalInvoices;

    public ObservableCollection<PeakHourTrafficDto> PeakHours { get; } = [];
    public ObservableCollection<PackageRevenueDto> PackageRevenues { get; } = [];
    public ObservableCollection<PaymentMethodRevenueDto> PaymentMethodRevenues { get; } = [];

    public DashboardViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<byte[]> GetExcelBytesAsync()
    {
        return await _apiClient.ExportExcelReportAsync();
    }

    public async Task<byte[]> GetPdfBytesAsync()
    {
        return await _apiClient.ExportPdfReportAsync();
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

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

            var revenueReport = await _apiClient.GetRevenueReportAsync();
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
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu dashboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

