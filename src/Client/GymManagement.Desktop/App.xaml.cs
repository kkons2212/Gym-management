using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using GymManagement.Desktop.Services.Api;
using GymManagement.Desktop.Services.Session;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop;

public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static IServiceProvider Services { get; private set; } = null!;

    public static nint WindowHandle
    {
        get
        {
            return WinRT.Interop.WindowNative.GetWindowHandle(Window);
        }
    }

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

        // 1. Core Infrastructure & Session Services
        services.AddSingleton<IAdminSessionService, AdminSessionService>();
        services.AddTransient<JwtAuthenticationHeaderHandler>();

        // 2. HTTP Client & API Client Registration
        string apiBaseUrl = Environment.GetEnvironmentVariable("GYM_API_BASE_URL") ?? "http://localhost:5000";
        bool useMockApi = string.Equals(Environment.GetEnvironmentVariable("USE_MOCK_API"), "true", StringComparison.OrdinalIgnoreCase);

        if (useMockApi)
        {
            services.AddSingleton<IApiClient, MockApiClient>();
        }
        else
        {
            services.AddSingleton<HttpClient>(sp =>
            {
                var handler = new JwtAuthenticationHeaderHandler(sp.GetRequiredService<IAdminSessionService>())
                {
                    InnerHandler = new HttpClientHandler()
                };
                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(apiBaseUrl),
                    Timeout = TimeSpan.FromSeconds(15)
                };
                return client;
            });
            services.AddSingleton<IApiClient, HttpApiClient>();
        }

        // 3. ViewModels
        services.AddSingleton<LoginViewModel>();
        services.AddTransient<CheckInCounterViewModel>();
        services.AddTransient<PosBillingViewModel>();
        services.AddTransient<InvoiceHistoryViewModel>();
        services.AddTransient<MembersViewModel>();
        services.AddTransient<PtScheduleViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PackagePlansViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<ShellViewModel>();

        // 4. Views
        services.AddTransient<Views.InvoiceHistoryPage>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        Window.Activate();
    }
}
