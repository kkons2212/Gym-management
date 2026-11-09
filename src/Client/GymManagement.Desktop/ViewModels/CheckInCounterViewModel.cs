using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Hardware;
using GymManagement.Server.Core.DTOs.CheckIn;

namespace GymManagement.Desktop.ViewModels;

public partial class CheckInCounterViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly IAudioFeedbackService? _audioService;

    [ObservableProperty]
    private bool _isCameraActive = true;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public string SearchPhone
    {
        get => SearchQuery;
        set => SearchQuery = value;
    }

    [ObservableProperty]
    private bool _hasResult;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private string _statusBanner = string.Empty;

    [ObservableProperty]
    private string _memberName = string.Empty;

    [ObservableProperty]
    private string _planName = string.Empty;

    [ObservableProperty]
    private int _remainingGymSessions;

    [ObservableProperty]
    private int _remainingPtSessions;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public CheckInCounterViewModel(IApiClient apiClient, IAudioFeedbackService? audioService = null)
    {
        _apiClient = apiClient;
        _audioService = audioService;
    }

    [RelayCommand]
    private void ToggleCamera()
    {
        IsCameraActive = !IsCameraActive;
    }

    [RelayCommand]
    public async Task CheckInWebcamQrAsync(string qrCode)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
        {
            return;
        }

        await ProcessCheckInAsync(new CheckInRequest(QrCode: qrCode.Trim()));
    }

    [RelayCommand]
    private async Task CheckInManualAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        string input = SearchQuery.Trim();
        await ProcessCheckInAsync(new CheckInRequest(
            MemberCode: input,
            Phone: input));
    }

    [RelayCommand]
    private async Task CheckInByPhoneAsync()
    {
        await CheckInManualAsync();
    }

    [RelayCommand]
    private async Task ManualOverrideAsync(string reason)
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        string input = SearchQuery.Trim();
        await ProcessCheckInAsync(new CheckInRequest(
            MemberCode: input,
            Phone: input,
            Notes: $"Override thủ công: {reason}"));
    }

    private async Task ProcessCheckInAsync(CheckInRequest request)
    {
        IsLoading = true;
        HasResult = false;
        ErrorMessage = null;

        try
        {
            var response = await _apiClient.CheckInAsync(request);

            IsSuccess = true;
            StatusBanner = response.Message;
            MemberName = response.MemberName;
            PlanName = response.PlanName;
            RemainingGymSessions = response.RemainingCheckInSessions;
            RemainingPtSessions = response.RemainingPtSessions;
            HasResult = true;
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            StatusBanner = "TỪ CHỐI CHECK-IN • CỔNG KHÓA";
            ErrorMessage = ex.Message;
            HasResult = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
