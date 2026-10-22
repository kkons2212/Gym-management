using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;

namespace GymManagement.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISystemSettingService _settingService;

    [ObservableProperty]
    private SystemConfigDto _config = new("05:00", "22:00", 2, 30, 30);

    [ObservableProperty]
    private double _minCancellationHours = 2;

    [ObservableProperty]
    private string _openingTime = "05:00";

    [ObservableProperty]
    private string _closingTime = "22:00";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<PackageDto> PackagesList { get; } = [];

    public SettingsViewModel(ISystemSettingService settingService)
    {
        _settingService = settingService;
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        Config = await _settingService.GetConfigAsync();
        MinCancellationHours = Config.MinCancellationHours;
        OpeningTime = Config.OpeningTime;
        ClosingTime = Config.ClosingTime;
        var pkgs = await _settingService.GetPackagesAsync(includeInactive: true);
        PackagesList.Clear();
        foreach (var p in pkgs)
        {
            PackagesList.Add(p);
        }
    }

    [RelayCommand]
    public async Task SaveConfigAsync()
    {
        Config = Config with
        {
            MinCancellationHours = (int)MinCancellationHours,
            OpeningTime = OpeningTime,
            ClosingTime = ClosingTime
        };
        await _settingService.UpdateConfigAsync(Config);
        StatusMessage = "Đã lưu cài đặt tham số vận hành hệ thống thành công (UC07.3)!";
    }

    [RelayCommand]
    public Task OpenCreatePackageDialogAsync()
    {
        StatusMessage = "Mở popup thêm mới gói tập (UC07.2)...";
        return Task.CompletedTask;
    }
}
