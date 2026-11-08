using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Session;

namespace GymManagement.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IAdminSessionService _sessionService;

    [ObservableProperty]
    private string _adminDisplayName = "Quản trị viên";

    [ObservableProperty]
    private string _adminRoleName = "Admin";

    [ObservableProperty]
    private bool _isAuthenticated;

    public event Action? LogoutRequested;

    public ShellViewModel(IAdminSessionService sessionService)
    {
        _sessionService = sessionService;
        UpdateUserInfo();
    }

    public void UpdateUserInfo()
    {
        IsAuthenticated = _sessionService.IsAuthenticated;
        AdminDisplayName = _sessionService.CurrentUser?.FullName ?? "Quản trị viên";
        AdminRoleName = _sessionService.CurrentUser?.Role ?? "Admin";
    }

    [RelayCommand]
    public void Logout()
    {
        _sessionService.ClearSession();
        UpdateUserInfo();
        LogoutRequested?.Invoke();
    }
}

