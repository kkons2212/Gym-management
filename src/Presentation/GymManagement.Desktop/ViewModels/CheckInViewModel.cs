using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;

namespace GymManagement.Desktop.ViewModels;

public partial class CheckInViewModel : ObservableObject
{
    private readonly ICheckInService _checkInService;

    [ObservableProperty]
    private string _inputScanCode = string.Empty;

    [ObservableProperty]
    private CheckInRecordDto? _latestCheckIn;

    [ObservableProperty]
    private string _alertMessage = "Sẵn sàng quét mã QR hoặc nhập SĐT hội viên";

    [ObservableProperty]
    private bool _isSuccessAlert = true;

    [ObservableProperty]
    private bool _showAlert;

    public ObservableCollection<CheckInRecordDto> TodayLogs { get; } = [];

    public CheckInViewModel(ICheckInService checkInService)
    {
        _checkInService = checkInService;
        _ = LoadLogsAsync();
    }

    [RelayCommand]
    public async Task LoadLogsAsync()
    {
        var logs = await _checkInService.GetTodayCheckInLogsAsync();
        TodayLogs.Clear();
        foreach (var log in logs)
        {
            TodayLogs.Add(log);
        }
    }

    [RelayCommand]
    public async Task ProcessCheckInAsync()
    {
        if (string.IsNullOrWhiteSpace(InputScanCode)) return;

        var result = await _checkInService.ManualCheckInAsync(InputScanCode.Trim());
        LatestCheckIn = result;
        TodayLogs.Insert(0, result);

        IsSuccessAlert = result.IsSuccess;
        AlertMessage = result.IsSuccess
            ? $"Check-in thành công: {result.MemberName} ({result.PackageName})"
            : $"Từ chối check-in: {result.Reason}";
        ShowAlert = true;

        InputScanCode = string.Empty;
    }
}
