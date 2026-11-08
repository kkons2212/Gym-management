using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel => (LoginViewModel)DataContext;

    public LoginPage()
    {
        DataContext = App.GetService<LoginViewModel>();
        InitializeComponent();
        TxtPassword.Password = ViewModel.Password;

        Loaded += LoginPage_Loaded;
        Unloaded += LoginPage_Unloaded;
    }

    private void LoginPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.OnLoginSuccess += OnLoginSuccess;
    }

    private void LoginPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.OnLoginSuccess -= OnLoginSuccess;
    }

    private void OnLoginSuccess()
    {
        var shellVm = App.GetService<ShellViewModel>();
        shellVm.UpdateUserInfo();
        Frame?.Navigate(typeof(MainPage));
    }

    private void TxtPassword_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.Password = TxtPassword.Password;
    }

    private void Input_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (ViewModel.LoginCommand.CanExecute(null))
            {
                ViewModel.LoginCommand.Execute(null);
            }
        }
    }
}
