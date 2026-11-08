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

    public ObservableCollection<PeakHourTrafficDto> PeakHours { get; } = [];

    public DashboardViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
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

