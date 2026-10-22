using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;

namespace GymManagement.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private readonly ICheckInService _checkInService;

    [ObservableProperty]
    private decimal _todayRevenue = 28500000m;

    [ObservableProperty]
    private int _activeMembersCount = 248;

    [ObservableProperty]
    private int _todayCheckInsCount = 86;

    [ObservableProperty]
    private int _activePtSessionsCount = 14;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<CheckInRecordDto> RecentCheckIns { get; } = [];

    public DashboardViewModel(IReportService reportService, ICheckInService checkInService)
    {
        _reportService = reportService;
        _checkInService = checkInService;
        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    public async Task LoadDashboardDataAsync()
    {
        IsLoading = true;
        try
        {
            var logs = await _checkInService.GetTodayCheckInLogsAsync();
            RecentCheckIns.Clear();
            foreach (var log in logs)
            {
                RecentCheckIns.Add(log);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
