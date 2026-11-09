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
        ArgumentNullException.ThrowIfNull(apiClient);
        _apiClient = apiClient;
        CalculateBmi();
    }

    public void Setup(MemberDto member)
    {
        ArgumentNullException.ThrowIfNull(member);

        MemberId = member.UserId;
        MemberName = member.FullName;
        CalculateBmi();
    }

    partial void OnHeightCmChanged(decimal value)
    {
        CalculateBmi();
    }

    partial void OnWeightKgChanged(decimal value)
    {
        CalculateBmi();
    }

    private void CalculateBmi()
    {
        if (HeightCm <= 0m || WeightKg <= 0m)
        {
            return;
        }

        decimal heightMeters = HeightCm / 100m;
        Bmi = Math.Round(WeightKg / (heightMeters * heightMeters), 1);

        if (Bmi < 18.5m)
        {
            BmiClassification = "Thiếu cân (Gầy)";
        }
        else if (Bmi < 25.0m)
        {
            BmiClassification = "Bình thường (Chuẩn)";
        }
        else if (Bmi < 30.0m)
        {
            BmiClassification = "Thừa cân";
        }
        else
        {
            BmiClassification = "Béo phì";
        }
    }

    [RelayCommand]
    private async Task SaveInBodyAsync()
    {
        // 1. Kiểm tra ngưỡng an toàn y tế (ux_rules.md 3.4)
        if (HeightCm < 100m || HeightCm > 250m)
        {
            StatusMessage = "Chiều cao phải nằm trong khoảng từ 100 cm đến 250 cm.";
            return;
        }

        if (WeightKg < 30m || WeightKg > 250m)
        {
            StatusMessage = "Cân nặng phải nằm trong khoảng từ 30 kg đến 250 kg.";
            return;
        }

        if (BodyFatPercentage.HasValue && (BodyFatPercentage.Value < 3m || BodyFatPercentage.Value > 60m))
        {
            StatusMessage = "Tỷ lệ mỡ phải nằm trong khoảng từ 3% đến 60%.";
            return;
        }

        if (SkeletalMuscleMass.HasValue && (SkeletalMuscleMass.Value < 10m || SkeletalMuscleMass.Value > 100m))
        {
            StatusMessage = "Khối lượng cơ xương phải nằm trong khoảng từ 10 kg đến 100 kg.";
            return;
        }

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
