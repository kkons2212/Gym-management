using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using GymManagement.Desktop.Services.Session;
using GymManagement.Desktop.ViewModels;
using GymManagement.Desktop.Views;

namespace GymManagement.Desktop;

public sealed partial class MainWindow : Window
{
    private readonly IAdminSessionService _sessionService;

    public MainWindow()
    {
        InitializeComponent();

        _sessionService = App.GetService<IAdminSessionService>();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        try
        {
            AppWindow.SetIcon("Assets/AppIcon.ico");
        }
        catch
        {
            // Fallback if icon path is not loaded in sandbox
        }

        // Hook login viewmodel success event
        var loginVm = App.GetService<LoginViewModel>();
        loginVm.OnLoginSuccess += OnLoginSuccessful;

        if (_sessionService.IsAuthenticated)
        {
            RootFrame.Navigate(typeof(MainPage));
        }
        else
        {
            RootFrame.Navigate(typeof(LoginPage));
        }

        // Global keyboard accelerators for F1, F2, F3, F5
        if (Content is FrameworkElement rootElement)
        {
            rootElement.KeyDown += OnWindowKeyDown;
        }
    }

    private void OnLoginSuccessful()
    {
        var shellVm = App.GetService<ShellViewModel>();
        shellVm.UpdateUserInfo();
        RootFrame.Navigate(typeof(MainPage));
    }

    private void OnWindowKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (RootFrame.Content is MainPage mainPage)
        {
            mainPage.HandleShortcut(e.Key);
        }
    }
}
