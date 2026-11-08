using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Session;
using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly IAdminSessionService _sessionService;

    [ObservableProperty]
    private string _username = "admin@gymsystem.vn";

    [ObservableProperty]
    private string _password = "admin";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    public event Action? OnLoginSuccess;

    public LoginViewModel(IApiClient apiClient, IAdminSessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var request = new LoginRequest(Username, Password);
            var response = await _apiClient.LoginAsync(request);

            _sessionService.SetSession(
                response.Token,
                new UserSessionDto(response.UserId, response.FullName, Username, response.Role)
            );

            OnLoginSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

