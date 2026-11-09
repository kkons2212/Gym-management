using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

    public async void OnToggleLockClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UserSessionDto user && ViewModel != null)
        {
            await ViewModel.ToggleLockUserCommand.ExecuteAsync(user);
        }
    }

    public void OnRoleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel != null && RoleComboBox?.SelectedItem is ComboBoxItem item && item.Tag is string role)
        {
            ViewModel.NewRole = role;
        }
    }
}
