using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel => (LoginViewModel)DataContext;

    public LoginPage()
    {
        InitializeComponent();
        DataContext = App.GetService<LoginViewModel>();
        TxtPassword.Password = ViewModel.Password;
    }

    private void TxtPassword_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.Password = TxtPassword.Password;
    }
}
