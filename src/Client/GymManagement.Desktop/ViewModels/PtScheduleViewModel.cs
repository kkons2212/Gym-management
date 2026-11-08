using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.PTSchedules;

namespace GymManagement.Desktop.ViewModels;

public partial class PtScheduleViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

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
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBookSlot))]
    private bool _hasCollision;

    public bool CanBookSlot => !HasCollision;

    [ObservableProperty]
    private string? _collisionWarning;

    public ObservableCollection<UserSessionDto> AvailablePts { get; } = [];
    public ObservableCollection<MemberDto> AvailableMembers { get; } = [];
    public ObservableCollection<PtSessionSlotDto> ScheduledSlots { get; } = [];

    public PtScheduleViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            var users = await _apiClient.GetUsersAsync();
            AvailablePts.Clear();
            foreach (var pt in users.Where(u => u.Role == "PT"))
            {
                AvailablePts.Add(pt);
            }
            if (AvailablePts.Count > 0 && SelectedPt == null)
            {
                SelectedPt = AvailablePts[0];
            }

            var members = await _apiClient.GetMembersAsync();
            AvailableMembers.Clear();
            foreach (var m in members.Where(m => m.Status == "ACTIVE" && m.RemainingPtSessions > 0))
            {
                AvailableMembers.Add(m);
            }
            if (AvailableMembers.Count > 0 && SelectedMember == null)
            {
                SelectedMember = AvailableMembers[0];
            }

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
    partial void OnSelectedPtChanged(UserSessionDto? value)
    {
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
