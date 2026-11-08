using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Packages;

namespace GymManagement.Desktop.ViewModels;

public partial class PackagePlansViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private double _newPrice;

    [ObservableProperty]
    private double _newDurationDays = 30;

    [ObservableProperty]
    private double _newMaxCheckInSessions = 30;

    [ObservableProperty]
    private double _newTotalPtSessions = 0;

    [ObservableProperty]
    private bool _newIsPtIncluded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    public ObservableCollection<PackagePlanDto> Packages { get; } = [];

    public PackagePlansViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    public async Task LoadPackagesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            var packages = await _apiClient.GetPackagePlansAsync();
            Packages.Clear();
            foreach (var p in packages)
            {
                Packages.Add(p);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể tải danh mục gói tập: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreatePackageAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            StatusMessage = "Vui lòng nhập tên gói tập.";
            IsSuccess = false;
            return;
        }

        if (NewPrice <= 0)
        {
            StatusMessage = "Giá gói tập phải lớn hơn 0 VNĐ.";
            IsSuccess = false;
            return;
        }

        if (NewDurationDays <= 0)
        {
            StatusMessage = "Thời hạn gói tập phải lớn hơn 0 ngày.";
            IsSuccess = false;
            return;
        }

        try
        {
            IsLoading = true;

            int totalPtSessions = 0;
            if (NewIsPtIncluded)
            {
                totalPtSessions = (int)NewTotalPtSessions;
            }

            var plan = new PackagePlanDto(
                $"PLAN{DateTime.Now:mmss}",
                NewName.Trim(),
                NewDescription.Trim(),
                (decimal)NewPrice,
                (int)NewDurationDays,
                (int)NewMaxCheckInSessions,
                totalPtSessions,
                NewIsPtIncluded,
                "ACTIVE"
            );

            var created = await _apiClient.CreatePackagePlanAsync(plan);
            Packages.Add(created);

            StatusMessage = $"Tạo gói tập '{created.Name}' thành công!";
            IsSuccess = true;

            // Reset form
            NewName = string.Empty;
            NewDescription = string.Empty;
            NewPrice = 0;
            NewDurationDays = 30;
            NewMaxCheckInSessions = 30;
            NewTotalPtSessions = 0;
            NewIsPtIncluded = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể tạo gói tập: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
