using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;
using GymManagement.Core.Enums;

namespace GymManagement.Desktop.ViewModels;

public partial class MembersViewModel : ObservableObject
{
    private readonly IMemberService _memberService;

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private MemberStatus? _selectedStatusFilter;

    [ObservableProperty]
    private MemberDto? _selectedMember;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<MemberDto> MembersList { get; } = [];

    public MembersViewModel(IMemberService memberService)
    {
        _memberService = memberService;
        _ = LoadMembersAsync();
    }

    [RelayCommand]
    public async Task LoadMembersAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _memberService.GetMembersAsync(SearchKeyword, SelectedStatusFilter);
            MembersList.Clear();
            foreach (var item in data)
            {
                MembersList.Add(item);
            }
            StatusMessage = $"Tìm thấy {MembersList.Count} hội viên.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        await LoadMembersAsync();
    }

    // Các hàm khung sẵn sàng cho việc mở Dialog & cắm logic sau này
    [RelayCommand]
    public Task OpenRegisterDialogAsync()
    {
        StatusMessage = "Mở form đăng ký hội viên mới (UC02)...";
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task OpenRenewCardDialogAsync(MemberDto member)
    {
        SelectedMember = member;
        StatusMessage = $"Mở form gia hạn/nâng cấp cho: {member.FullName}...";
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task FreezeMemberCardAsync(MemberDto member)
    {
        SelectedMember = member;
        StatusMessage = $"Mở form bảo lưu gói cho: {member.FullName}...";
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LockOrCancelCardAsync(MemberDto member)
    {
        SelectedMember = member;
        StatusMessage = $"Mở form khóa/hủy gói cho: {member.FullName}...";
        return Task.CompletedTask;
    }
}
