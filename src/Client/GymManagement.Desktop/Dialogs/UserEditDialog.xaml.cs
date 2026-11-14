using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Services.Api;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Settings;

namespace GymManagement.Desktop.Dialogs;

public sealed partial class UserEditDialog : ContentDialog
{
    private readonly IApiClient _apiClient;
    private readonly UserSessionDto _targetUser;
    private readonly bool _isSelf;
    private readonly bool _isSuperAdmin;

    internal UserSessionDto? SavedUser { get; private set; }

    public UserEditDialog(IApiClient apiClient, UserSessionDto user, bool isSelf, bool isSuperAdmin = false)
    {
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(user);

        _apiClient = apiClient;
        _targetUser = user;
        _isSelf = isSelf;
        _isSuperAdmin = isSuperAdmin;

        InitializeComponent();

        TxtUserName.Text = user.UserId;
        TxtFullName.Text = user.FullName;
        TxtEmail.Text = user.Email;
        TxtPhone.Text = "0901234567";

        SelectRole(user.Role);

        if (_isSelf)
        {
            CboRole.IsEnabled = false;
            PnlSelfNotice.Visibility = Visibility.Visible;
        }
        else if (!_isSuperAdmin)
        {
            if (CbiAdminRole != null)
            {
                CbiAdminRole.IsEnabled = false;
            }

            bool isTargetAdmin = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(user.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
            if (isTargetAdmin)
            {
                CboRole.IsEnabled = false;
            }
        }

        PrimaryButtonClick += UserEditDialog_PrimaryButtonClick;
    }

    private void SelectRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return;
        }

        foreach (var item in CboRole.Items)
        {
            if (item is ComboBoxItem cbi && cbi.Tag != null)
            {
                if (string.Equals(cbi.Tag.ToString(), role, StringComparison.OrdinalIgnoreCase))
                {
                    CboRole.SelectedItem = cbi;
                    return;
                }
            }
        }
    }

    private async void UserEditDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;

        PnlValidationError.Visibility = Visibility.Collapsed;
        TxtValidationError.Text = string.Empty;

        string fullName = TxtFullName.Text.Trim();
        string phone = TxtPhone.Text.Trim();
        string email = TxtEmail.Text.Trim();

        string roleId = _targetUser.Role;
        if (!_isSelf && CboRole.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
        {
            roleId = selectedItem.Tag.ToString()!;
        }

        // 1. Validation
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
        {
            ShowError("Họ và tên nhân sự phải có ít nhất 3 ký tự.");
            return;
        }

        if (!Regex.IsMatch(phone, @"^0\d{9}$"))
        {
            ShowError("Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số (bắt đầu bằng 0).");
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowError("Địa chỉ email không đúng định dạng.");
            return;
        }

        // 2. Self demotion guardrail check
        if (_isSelf && !string.Equals(roleId, _targetUser.Role, StringComparison.OrdinalIgnoreCase))
        {
            ShowError("Vi phạm quyền quản trị: Bạn không thể tự thay đổi vai trò của tài khoản đang đăng nhập!");
            return;
        }

        if (!_isSuperAdmin && string.Equals(roleId, "Admin", StringComparison.OrdinalIgnoreCase) && !string.Equals(_targetUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            ShowError("Chỉ Quản trị viên cấp cao (SuperAdmin) mới có quyền phân quyền Quản trị viên.");
            return;
        }

        // 3. Idempotency Lock
        IsPrimaryButtonEnabled = false;
        PrSaving.Visibility = Visibility.Visible;
        PrSaving.IsActive = true;

        try
        {
            var request = new UpdateUserRequest(
                fullName,
                phone,
                email,
                roleId
            );

            SavedUser = await _apiClient.UpdateUserAsync(_targetUser.UserId, request);
            Hide();
        }
        catch (Exception ex)
        {
            ShowError($"Không thể cập nhật tài khoản: {ex.Message}");
        }
        finally
        {
            IsPrimaryButtonEnabled = true;
            PrSaving.IsActive = false;
            PrSaving.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowError(string message)
    {
        TxtValidationError.Text = message;
        PnlValidationError.Visibility = Visibility.Visible;
    }
}

