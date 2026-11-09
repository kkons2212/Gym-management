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
        ArgumentNullException.ThrowIfNull(sessionService);
        _sessionService = sessionService;
        UpdateUserInfo();
    }

    public void UpdateUserInfo()
    {
        IsAuthenticated = _sessionService.IsAuthenticated;

        if (_sessionService.CurrentUser != null && !string.IsNullOrEmpty(_sessionService.CurrentUser.FullName))
        {
            AdminDisplayName = _sessionService.CurrentUser.FullName;
        }
        else
        {
            AdminDisplayName = "Quản trị viên";
        }

        if (_sessionService.CurrentUser != null && !string.IsNullOrEmpty(_sessionService.CurrentUser.Role))
        {
            AdminRoleName = _sessionService.CurrentUser.Role;
        }
        else
        {
            AdminRoleName = "Admin";
        }
    }

    [RelayCommand]
    public void Logout()
    {
        _sessionService.ClearSession();
        UpdateUserInfo();
        LogoutRequested?.Invoke();
    }
}
