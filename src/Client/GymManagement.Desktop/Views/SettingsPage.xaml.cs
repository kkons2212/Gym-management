using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.Dialogs;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Session;
using GymManagement.Desktop.ViewModels;
using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Desktop.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get
        {
            return (SettingsViewModel)DataContext;
        }
    }

    public SettingsPage()
    {
        DataContext = App.GetService<SettingsViewModel>();
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadUsersCommand.ExecuteAsync(null);
            }
        };
    }

    public async void OnEditUserClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UserSessionDto user && ViewModel != null)
        {
            var sessionService = App.GetService<IAdminSessionService>();
            bool isSelf = false;
            if (sessionService.CurrentUser != null)
            {
                if (sessionService.CurrentUser.UserId == user.UserId ||
                    string.Equals(sessionService.CurrentUser.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    isSelf = true;
                }
            }

            var dialog = new UserEditDialog(App.GetService<IApiClient>(), user, isSelf, ViewModel.IsSuperAdmin)
            {
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
            if (dialog.SavedUser != null)
            {
                await ViewModel.LoadUsersCommand.ExecuteAsync(null);
            }
        }
    }

    public async void OnToggleLockClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UserSessionDto user && ViewModel != null)
        {
            var sessionService = App.GetService<IAdminSessionService>();
            if (sessionService.CurrentUser != null)
            {
                if (sessionService.CurrentUser.UserId == user.UserId ||
                    string.Equals(sessionService.CurrentUser.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var alert = new ContentDialog
                    {
                        Title = "CẢNH BÁO BẢO MẬT",
                        Content = "Bạn không thể tự khóa tài khoản Quản trị viên đang đăng nhập!",
                        CloseButtonText = "Đã hiểu",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };
                    await alert.ShowAsync();
                    return;
                }
            }

            var confirm = new ContentDialog
            {
                Title = "XÁC NHẬN THAY ĐỔI TRẠNG THÁI",
                Content = $"Bạn có chắc chắn muốn thay đổi trạng thái khóa/mở khóa cho tài khoản '{user.FullName}' ({user.Email}) không?",
                PrimaryButtonText = "Xác nhận",
                CloseButtonText = "Hủy bỏ",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var res = await confirm.ShowAsync();
            if (res == ContentDialogResult.Primary)
            {
                await ViewModel.ToggleLockUserCommand.ExecuteAsync(user);
            }
        }
    }

    public void OnRoleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && RoleComboBox?.SelectedItem is ComboBoxItem item && item.Tag is string role)
        {
            ViewModel.NewRole = role;
        }
    }

    public void OnNewPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && sender is PasswordBox pb)
        {
            ViewModel.NewPassword = pb.Password;
        }
    }
}
