using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Hardware;
using GymManagement.Desktop.Services.Session;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop;

public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static IServiceProvider Services { get; private set; } = null!;

    public static nint WindowHandle =>
        WinRT.Interop.WindowNative.GetWindowHandle(Window);

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core Infrastructure Services
        services.AddSingleton<IAdminSessionService, AdminSessionService>();
        services.AddSingleton<IAudioFeedbackService, AudioFeedbackService>();
        services.AddSingleton<IApiClient, MockApiClient>();

        // ViewModels
        services.AddSingleton<LoginViewModel>();
        services.AddTransient<CheckInCounterViewModel>();
        services.AddTransient<PosBillingViewModel>();
        services.AddTransient<MembersViewModel>();
        services.AddTransient<PtScheduleViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PackagePlansViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<ShellViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }
}
