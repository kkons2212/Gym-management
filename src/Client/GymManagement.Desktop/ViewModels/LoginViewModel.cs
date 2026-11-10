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
    private bool _rememberMe = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    public event Action? OnLoginSuccess;

    public LoginViewModel(IApiClient apiClient, IAdminSessionService sessionService)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(sessionService);

        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading)
        {
            return;
        }

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
            Password = string.Empty;

            if (ex is HttpRequestException ||
                ex.InnerException is System.Net.Sockets.SocketException ||
                ex.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Không thể kết nối đến máy chủ hệ thống (localhost:5000). Vui lòng kiểm tra dịch vụ Backend hoặc bật chế độ Mock API.";
            }
            else if (ex.Message.Contains("LOCKED", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("khóa", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Tài khoản đã bị khóa. Vui lòng liên hệ Super Admin.";
            }
            else
            {
                ErrorMessage = ex.Message;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}

