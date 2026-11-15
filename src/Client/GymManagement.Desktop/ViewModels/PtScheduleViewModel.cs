using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Session;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.Enums;

namespace GymManagement.Desktop.ViewModels;

public partial class PtScheduleViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly IAdminSessionService _sessionService;

    [ObservableProperty]
    private DateTimeOffset _selectedDate = DateTimeOffset.Now;

    [ObservableProperty]
    private UserSessionDto? _selectedPt;

    [ObservableProperty]
    private MemberDto? _selectedMember;

    [ObservableProperty]
    private TimeSpan _startTime = new(9, 0, 0);

    [ObservableProperty]
    private TimeSpan _endTime = new(10, 0, 0);

    [ObservableProperty]
    private TimeSlotOption? _selectedStartTime;

    [ObservableProperty]
    private TimeSlotOption? _selectedEndTime;

    partial void OnSelectedStartTimeChanged(TimeSlotOption? value)
    {
        if (value != null)
        {
            StartTime = value.Time;
            if (EndTime <= StartTime || SelectedEndTime == null)
            {
                var targetEnd = StartTime.Add(TimeSpan.FromHours(1));
                var matched = AvailableTimeSlots.FirstOrDefault(s => s.Time == targetEnd);
                if (matched != null)
                {
                    SelectedEndTime = matched;
                }
            }
        }
        CheckForCollisions();
    }

    partial void OnSelectedEndTimeChanged(TimeSlotOption? value)
    {
        if (value != null)
        {
            EndTime = value.Time;
        }
        CheckForCollisions();
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    public bool HasStatusMessage
    {
        get
        {
            return !string.IsNullOrWhiteSpace(StatusMessage);
        }
    }

    partial void OnStatusMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasStatusMessage));
    }

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBookSlot))]
    private bool _hasCollision;

    public bool CanBookSlot
    {
        get
        {
            return !HasCollision;
        }
    }

    [ObservableProperty]
    private string? _collisionWarning;

    private readonly List<MemberDto> _allMembers = [];

    public ObservableCollection<UserSessionDto> AvailablePts { get; } = [];
    public ObservableCollection<MemberDto> AvailableMembers { get; } = [];
    public ObservableCollection<PtSessionSlotDto> ScheduledSlots { get; } = [];
    public ObservableCollection<TimeSlotOption> AvailableTimeSlots { get; } = [];

    public PtScheduleViewModel(IApiClient apiClient, IAdminSessionService sessionService)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(sessionService);
        _apiClient = apiClient;
        _sessionService = sessionService;
        PopulateTimeSlots();
    }

    private void PopulateTimeSlots()
    {
        AvailableTimeSlots.Clear();
        var current = new TimeSpan(6, 0, 0);
        var end = new TimeSpan(22, 0, 0);
        while (current <= end)
        {
            string display = $"{current.Hours:D2}:{current.Minutes:D2}";
            var option = new TimeSlotOption(current, display);
            AvailableTimeSlots.Add(option);
            current = current.Add(TimeSpan.FromMinutes(30));
        }

        var defaultStart = AvailableTimeSlots.FirstOrDefault(s => s.Time == StartTime);
        if (defaultStart == null)
        {
            defaultStart = AvailableTimeSlots.FirstOrDefault(s => s.Time == new TimeSpan(9, 0, 0));
        }
        SelectedStartTime = defaultStart;
        if (defaultStart != null)
        {
            StartTime = defaultStart.Time;
        }

        var defaultEnd = AvailableTimeSlots.FirstOrDefault(s => s.Time == EndTime);
        if (defaultEnd == null)
        {
            defaultEnd = AvailableTimeSlots.FirstOrDefault(s => s.Time == new TimeSpan(10, 0, 0));
        }
        SelectedEndTime = defaultEnd;
        if (defaultEnd != null)
        {
            EndTime = defaultEnd.Time;
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            var trainers = await _apiClient.GetTrainersAsync(activeOnly: true);
            AvailablePts.Clear();
            foreach (var trainer in trainers)
            {
                AvailablePts.Add(new UserSessionDto(trainer.PtId, trainer.FullName, trainer.Email, "PT"));
            }

            if (AvailablePts.Count == 0)
            {
                var users = await _apiClient.GetUsersAsync();
                foreach (var pt in users)
                {
                    bool isPt = string.Equals(pt.Role, "PT", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(pt.Role, "TRAINER", StringComparison.OrdinalIgnoreCase) ||
                                pt.FullName.Contains("PT", StringComparison.OrdinalIgnoreCase);
                    if (isPt)
                    {
                        AvailablePts.Add(pt);
                    }
                }
            }

            if (AvailablePts.Count > 0 && SelectedPt == null)
            {
                SelectedPt = AvailablePts[0];
            }

            var members = await _apiClient.GetMembersAsync();
            _allMembers.Clear();
            foreach (var m in members)
            {
                if (m.Status == "ACTIVE" && m.RemainingPtSessions > 0)
                {
                    _allMembers.Add(m);
                }
            }

            FilterMembersForSelectedPt();

            await LoadScheduleAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khởi tạo: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> ConfirmSessionResultAsync(string sessionId, PtSessionStatus outcome, string? notes)
    {
        try
        {
            IsLoading = true;
            string adminUser = "admin";
            if (_sessionService.CurrentUser != null && !string.IsNullOrWhiteSpace(_sessionService.CurrentUser.Email))
            {
                adminUser = _sessionService.CurrentUser.Email;
            }

            var request = new ConfirmSessionResultRequest(sessionId, adminUser, outcome, notes);
            await _apiClient.ConfirmSessionResultAsync(request);
            StatusMessage = "Đã cập nhật kết quả ca tập thành công!";
            IsSuccess = true;
            await LoadScheduleAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể cập nhật kết quả: {ex.Message}";
            IsSuccess = false;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadScheduleAsync()
    {
        try
        {
            IsLoading = true;
            var start = SelectedDate.Date;
            var end = start.AddDays(7);

            var slots = await _apiClient.GetPtScheduleSlotsAsync(start, end);
            ScheduledSlots.Clear();
            foreach (var slot in slots.Where(s => SelectedPt == null || s.PtId == SelectedPt.UserId))
            {
                ScheduledSlots.Add(slot);
            }

            CheckForCollisions();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể tải lịch PT: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnStartTimeChanged(TimeSpan value)
    {
        CheckForCollisions();
    }

    partial void OnEndTimeChanged(TimeSpan value)
    {
        CheckForCollisions();
    }

    public void FilterMembersForSelectedPt()
    {
        AvailableMembers.Clear();
        if (SelectedPt == null)
        {
            SelectedMember = null;
            return;
        }

        // 1. Học viên do PT này phụ trách
        foreach (var m in _allMembers)
        {
            if (m.AssignedPtId == SelectedPt.UserId)
            {
                AvailableMembers.Add(m);
            }
        }

        // 2. Học viên chưa gán PT (để Admin có thể xếp lịch và tự động gán PT luôn)
        foreach (var m in _allMembers)
        {
            if (string.IsNullOrEmpty(m.AssignedPtId))
            {
                AvailableMembers.Add(m);
            }
        }

        if (AvailableMembers.Count > 0)
        {
            if (SelectedMember == null || !AvailableMembers.Contains(SelectedMember))
            {
                SelectedMember = AvailableMembers[0];
            }
        }
        else
        {
            SelectedMember = null;
        }
    }

    partial void OnSelectedPtChanged(UserSessionDto? value)
    {
        FilterMembersForSelectedPt();
        _ = LoadScheduleAsync();
    }

    partial void OnSelectedDateChanged(DateTimeOffset value)
    {
        _ = LoadScheduleAsync();
    }

    public void CheckForCollisions()
    {
        if (SelectedPt == null)
        {
            HasCollision = false;
            CollisionWarning = null;
            return;
        }

        var collision = ScheduledSlots.Any(s =>
            s.PtId == SelectedPt.UserId &&
            s.ScheduledDate.Date == SelectedDate.Date &&
            s.StartTime < EndTime &&
            StartTime < s.EndTime &&
            s.SessionStatus != "CANCELLED");

        if (collision)
        {
            HasCollision = true;
            CollisionWarning = $"Cảnh báo xung đột: HLV {SelectedPt.FullName} đã có lịch dạy trong khoảng thời gian {StartTime:hh\\:mm} - {EndTime:hh\\:mm}!";
        }
        else
        {
            HasCollision = false;
            CollisionWarning = null;
        }
    }

    [RelayCommand]
    public async Task BookSlotAsync()
    {
        if (SelectedPt == null)
        {
            StatusMessage = "Vui lòng chọn Huấn luyện viên.";
            IsSuccess = false;
            return;
        }

        if (SelectedMember == null)
        {
            StatusMessage = "Vui lòng chọn Hội viên có buổi tập PT.";
            IsSuccess = false;
            return;
        }

        if (EndTime <= StartTime)
        {
            StatusMessage = "Giờ kết thúc phải lớn hơn giờ bắt đầu.";
            IsSuccess = false;
            return;
        }

        CheckForCollisions();
        if (HasCollision)
        {
            StatusMessage = CollisionWarning;
            IsSuccess = false;
            return;
        }

        try
        {
            IsLoading = true;
            var request = new BookPtSlotRequest(
                SelectedPt.UserId,
                SelectedMember.UserId,
                SelectedDate.Date,
                StartTime,
                EndTime
            );

            var newSlot = await _apiClient.BookPtSlotAsync(request);
            ScheduledSlots.Add(newSlot);
            StatusMessage = $"Đã xếp lịch thành công cho HLV {SelectedPt.FullName} và Hội viên {SelectedMember.FullName}!";
            IsSuccess = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể xếp lịch: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public sealed class TimeSlotOption
{
    public TimeSpan Time { get; set; }
    public string DisplayText { get; set; } = string.Empty;

    public TimeSlotOption(TimeSpan time, string displayText)
    {
        Time = time;
        DisplayText = displayText;
    }

    public override string ToString()
    {
        return DisplayText;
    }
}
