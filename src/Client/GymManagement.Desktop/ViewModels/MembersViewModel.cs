using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Members;

namespace GymManagement.Desktop.ViewModels;

public partial class MembersViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public ObservableCollection<MemberDto> Members { get; } = [];

    [ObservableProperty]
    private MemberDto? _selectedMember;

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private string _statusFilter = "ALL";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private bool _isFreezeModalOpen;

    public MembersViewModel(IApiClient apiClient)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        _apiClient = apiClient;
    }

    public async Task InitializeAsync()
    {
        await LoadMembersAsync();
    }

    [RelayCommand]
    public async Task LoadMembersAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _apiClient.GetMembersAsync(SearchKeyword);
            Members.Clear();

            foreach (var m in list)
            {
                if (StatusFilter == "ALL" || m.Status == StatusFilter)
                {
                    Members.Add(m);
                }
            }

            if (Members.Count == 0)
            {
                IsEmpty = true;
            }
            else
            {
                IsEmpty = false;
            }

            if (Members.Count > 0 && SelectedMember == null)
            {
                SelectedMember = Members[0];
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenFreezeModal(MemberDto member)
    {
        SelectedMember = member;
        IsFreezeModalOpen = true;
    }
}

