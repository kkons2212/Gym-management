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

        try
        {
            var appWindow = AppWindow;
            appWindow.Resize(new Windows.Graphics.SizeInt32(1366, 820));

            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(appWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var centeredPosition = appWindow.Position;
                centeredPosition.X = (displayArea.WorkArea.Width - 1366) / 2;
                centeredPosition.Y = (displayArea.WorkArea.Height - 820) / 2;
                if (centeredPosition.X < 0)
                {
                    centeredPosition.X = 0;
                }
                if (centeredPosition.Y < 0)
                {
                    centeredPosition.Y = 0;
                }
                appWindow.Move(centeredPosition);
            }

            if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = appWindow.TitleBar;
                titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(255, 248, 250, 252);
                titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 30, 41, 59);
                titleBar.ButtonHoverForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(255, 14, 165, 233);
                titleBar.ButtonPressedForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 100, 116, 139);
            }
        }
        catch
        {
            // Fallback if windowing APIs are constrained in testing
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
