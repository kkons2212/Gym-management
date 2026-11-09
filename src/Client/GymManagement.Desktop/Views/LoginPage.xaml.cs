using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel
    {
        get
        {
            return (LoginViewModel)DataContext;
        }
    }

    public LoginPage()
    {
        DataContext = App.GetService<LoginViewModel>();
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var shellVm = App.GetService<ShellViewModel>();
        shellVm.UpdateUserInfo();
        if (Frame != null)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
