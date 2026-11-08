using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.ViewModels;

public partial class InBodyEntryViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _memberId = string.Empty;

    [ObservableProperty]
    private string _memberName = string.Empty;

    [ObservableProperty]
    private decimal _heightCm = 170m;

    [ObservableProperty]
    private decimal _weightKg = 65m;

    [ObservableProperty]
    private decimal? _bodyFatPercentage = 18.5m;

    [ObservableProperty]
    private decimal? _skeletalMuscleMass = 32.0m;

    [ObservableProperty]
    private decimal _bmi = 22.5m;

    [ObservableProperty]
    private string _bmiClassification = "Bình thường (Chuẩn)";

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    public event Action? OnSaveSuccess;

    public InBodyEntryViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        CalculateBmi();
    }

    public void Setup(MemberDto member)
    {
        MemberId = member.UserId;
        MemberName = member.FullName;
        CalculateBmi();
    }

    partial void OnHeightCmChanged(decimal value) => CalculateBmi();
    partial void OnWeightKgChanged(decimal value) => CalculateBmi();

    private void CalculateBmi()
    {
        if (HeightCm <= 0 || WeightKg <= 0) return;

        var hM = HeightCm / 100m;
        Bmi = Math.Round(WeightKg / (hM * hM), 1);

        BmiClassification = Bmi switch
        {
            < 18.5m => "Thiếu cân (Gầy)",
            < 25.0m => "Bình thường (Chuẩn)",
            < 30.0m => "Thừa cân",
            _ => "Béo phì"
        };
    }

    [RelayCommand]
    private async Task SaveInBodyAsync()
    {
        IsLoading = true;
        StatusMessage = null;

        try
        {
            var req = new InBodyEntryRequest(
                MemberId,
                HeightCm,
                WeightKg,
                BodyFatPercentage,
                SkeletalMuscleMass,
                Notes
            );

            await _apiClient.RecordInBodyMetricAsync(req);
            StatusMessage = "Đã lưu chỉ số InBody thành công!";
            OnSaveSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

