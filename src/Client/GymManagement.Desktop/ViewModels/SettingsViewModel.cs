using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Session;
using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly IAdminSessionService _sessionService;

    [ObservableProperty]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    private string _newEmail = string.Empty;

    [ObservableProperty]
    private string _newPhone = string.Empty;

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newRole = "PT";

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isSuccess;

    public string CurrentAdminName
    {
        get
        {
            if (_sessionService.CurrentUser != null && !string.IsNullOrEmpty(_sessionService.CurrentUser.FullName))
            {
                return _sessionService.CurrentUser.FullName;
            }
            return "Quản trị viên";
        }
    }

    public string CurrentAdminRole
    {
        get
        {
            if (_sessionService.CurrentUser != null && !string.IsNullOrEmpty(_sessionService.CurrentUser.Role))
            {
                return _sessionService.CurrentUser.Role;
            }
            return "Admin";
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            if (_sessionService.CurrentUser != null && !string.IsNullOrEmpty(_sessionService.CurrentUser.Role))
            {
                return string.Equals(_sessionService.CurrentUser.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }

    public ObservableCollection<UserSessionDto> Users { get; } = [];

    public SettingsViewModel(IApiClient apiClient, IAdminSessionService sessionService)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(sessionService);

        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            var users = await _apiClient.GetUsersAsync();
            Users.Clear();
            foreach (var u in users)
            {
                Users.Add(u);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể tải danh sách tài khoản: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ToggleLockUserAsync(UserSessionDto user)
    {
        if (user == null)
        {
            return;
        }

        // Guardrail: Không được khóa chính tài khoản admin đang đăng nhập
        if (_sessionService.CurrentUser != null && user.UserId == _sessionService.CurrentUser.UserId)
        {
            StatusMessage = "Cảnh báo bảo mật: Bạn không thể khóa chính tài khoản Quản trị viên đang đăng nhập!";
            IsSuccess = false;
            return;
        }

        try
        {
            IsLoading = true;
            var success = await _apiClient.ToggleUserLockAsync(user.UserId);
            if (success)
            {
                StatusMessage = $"Đã cập nhật trạng thái tài khoản {user.FullName} thành công.";
                IsSuccess = true;
                await LoadUsersAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi cập nhật tài khoản: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CreateUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFullName) || NewFullName.Trim().Length < 3)
        {
            StatusMessage = "Họ và tên nhân sự phải có ít nhất 3 ký tự.";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewUsername))
        {
            StatusMessage = "Vui lòng nhập tên đăng nhập.";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPhone) || !System.Text.RegularExpressions.Regex.IsMatch(NewPhone.Trim(), @"^0\d{9}$"))
        {
            StatusMessage = "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số (bắt đầu bằng 0).";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewEmail) || !System.Text.RegularExpressions.Regex.IsMatch(NewEmail.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            StatusMessage = "Địa chỉ email không đúng định dạng.";
            IsSuccess = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
        {
            StatusMessage = "Mật khẩu nhân sự phải có ít nhất 6 ký tự.";
            IsSuccess = false;
            return;
        }

        if (string.Equals(NewRole, "Admin", StringComparison.OrdinalIgnoreCase) && !IsSuperAdmin)
        {
            StatusMessage = "Chỉ Quản trị viên cấp cao (SuperAdmin) mới có quyền tạo tài khoản Quản trị viên.";
            IsSuccess = false;
            return;
        }

        try
        {
            IsLoading = true;
            var created = await _apiClient.CreateUserAsync(NewFullName.Trim(), NewEmail.Trim(), NewPhone.Trim(), NewUsername.Trim(), NewRole, NewPassword);
            Users.Add(created);

            StatusMessage = $"Đã tạo tài khoản {created.FullName} ({created.Role}) thành công!";
            IsSuccess = true;

            // Reset
            NewFullName = string.Empty;
            NewEmail = string.Empty;
            NewPhone = string.Empty;
            NewUsername = string.Empty;
            NewPassword = string.Empty;
            NewRole = "PT";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Không thể tạo tài khoản: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
