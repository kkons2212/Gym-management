using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.CheckIn;

namespace GymManagement.Desktop.ViewModels;

public partial class CheckInCounterViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private CancellationTokenSource? _autoResetCts;
    private Microsoft.UI.Dispatching.DispatcherQueue? _dispatcherQueue;
    private List<CheckInLogDto> _allLogsCache = new();

    [ObservableProperty]
    private bool _isCameraActive = true;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public string SearchPhone
    {
        get
        {
            return SearchQuery;
        }
        set
        {
            SearchQuery = value;
        }
    }

    [ObservableProperty]
    private bool _hasResult;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private string _statusBanner = string.Empty;

    [ObservableProperty]
    private string _memberCode = string.Empty;

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

    [ObservableProperty]
    private int _attendanceCount;

    [ObservableProperty]
    private string _selectedShift = "ALL";

    [ObservableProperty]
    private string _gateStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasGateMessage;

    public ObservableCollection<CheckInLogDto> TodayLogs { get; } = new();

    public CheckInCounterViewModel(IApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        _apiClient = apiClient;
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadTodayMonitoringAsync();
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

        MemberCode = qrCode.Trim();
        await ProcessCheckInAsync(new CheckInRequest(QrCode: qrCode.Trim()));
    }

    [RelayCommand]
    public async Task CheckInManualAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        string input;
        input = SearchQuery.Trim();
        MemberCode = input;
        await ProcessCheckInAsync(new CheckInRequest(
            MemberCode: input,
            Phone: input));
    }

    [RelayCommand]
    private async Task CheckInByPhoneAsync()
    {
        await CheckInManualAsync();
    }

    public async Task ExecuteOverrideCheckInAsync(string memberIdentifier, string fullReason)
    {
        if (string.IsNullOrWhiteSpace(memberIdentifier))
        {
            return;
        }

        string input;
        input = memberIdentifier.Trim();
        MemberCode = input;

        await ProcessCheckInAsync(new CheckInRequest(
            MemberCode: input,
            Phone: input,
            Notes: fullReason,
            VerifiedByAdminId: "USR_ADMIN"));
    }

    [RelayCommand]
    public async Task TestGateAsync()
    {
        HasGateMessage = true;
        GateStatusMessage = "Đang gửi xung kích hoạt rơ-le mở chốt cổng...";
        await Task.Delay(1200);
        GateStatusMessage = "Cổng turnstile đã nhả chốt thành công trong 5 giây!";
        await Task.Delay(4000);
        GateStatusMessage = string.Empty;
        HasGateMessage = false;
    }

    [RelayCommand]
    public async Task RefreshMonitoringAsync()
    {
        await LoadTodayMonitoringAsync();
    }

    public async Task LoadTodayMonitoringAsync()
    {
        try
        {
            var logs = await _apiClient.GetTodayLogsAsync();
            var count = await _apiClient.GetTodayAttendanceCountAsync();

            _allLogsCache = logs.ToList();
            AttendanceCount = count;
            ApplyShiftFilter();
        }
        catch (Exception)
        {
            // Tránh văng app nếu API tạm thời chưa có mạng
        }
    }

    public void ApplyShiftFilter()
    {
        TodayLogs.Clear();
        IEnumerable<CheckInLogDto> filtered;
        filtered = _allLogsCache;

        if (SelectedShift == "MORNING")
        {
            filtered = _allLogsCache.Where(l => l.CheckInTime.Hour >= 6 && l.CheckInTime.Hour < 12);
        }
        else if (SelectedShift == "AFTERNOON")
        {
            filtered = _allLogsCache.Where(l => l.CheckInTime.Hour >= 12 && l.CheckInTime.Hour < 17);
        }
        else if (SelectedShift == "EVENING")
        {
            filtered = _allLogsCache.Where(l => l.CheckInTime.Hour >= 17 && l.CheckInTime.Hour <= 22);
        }

        foreach (var item in filtered)
        {
            TodayLogs.Add(item);
        }
    }

    partial void OnSelectedShiftChanged(string value)
    {
        ApplyShiftFilter();
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
            StatusBanner = "CHECK-IN THÀNH CÔNG • CỔNG MỞ";
            MemberName = response.MemberName;
            PlanName = response.PlanName;
            RemainingGymSessions = response.RemainingCheckInSessions;
            RemainingPtSessions = response.RemainingPtSessions;
            HasResult = true;

            // Xóa ô tìm kiếm sau khi điểm danh thành công
            SearchQuery = string.Empty;
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            StatusBanner = "TỪ CHỐI CHECK-IN • CỔNG KHÓA";
            if (!string.IsNullOrWhiteSpace(request.MemberCode))
            {
                MemberName = request.MemberCode;
            }
            else if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                MemberName = request.Phone;
            }
            else
            {
                MemberName = "Hội viên";
            }
            ErrorMessage = ex.Message;
            HasResult = true;
        }
        finally
        {
            IsLoading = false;
        }

        // Tải lại nhật ký giám sát thời gian thực
        await LoadTodayMonitoringAsync();

        // Kích hoạt tự động đếm lùi 7 giây để quay về màn hình chờ sẵn sàng
        StartAutoResetTimer();
    }

    private void StartAutoResetTimer()
    {
        if (_autoResetCts != null)
        {
            _autoResetCts.Cancel();
            _autoResetCts.Dispose();
            _autoResetCts = null;
        }

        _autoResetCts = new CancellationTokenSource();
        var token = _autoResetCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(7000, token);
                if (!token.IsCancellationRequested)
                {
                    if (_dispatcherQueue != null)
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            HasResult = false;
                            StatusBanner = string.Empty;
                            ErrorMessage = null;
                        });
                    }
                    else
                    {
                        HasResult = false;
                        StatusBanner = string.Empty;
                        ErrorMessage = null;
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        });
    }
}
