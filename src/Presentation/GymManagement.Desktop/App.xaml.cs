using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using GymManagement.Core.Contracts;
using GymManagement.Core.Services;
using GymManagement.Desktop.ViewModels;

namespace GymManagement.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core Mock Services (sau này thay bằng EF Core Services của tầng 2 & 3 mà không đổi UI)
        var mockService = new MockGymService();
        services.AddSingleton<IAuthService>(mockService);
        services.AddSingleton<IMemberService>(mockService);
        services.AddSingleton<ICheckInService>(mockService);
        services.AddSingleton<IPtService>(mockService);
        services.AddSingleton<IBillingService>(mockService);
        services.AddSingleton<IReportService>(mockService);
        services.AddSingleton<ISystemSettingService>(mockService);

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MembersViewModel>();
        services.AddTransient<CheckInViewModel>();
        services.AddTransient<PtScheduleViewModel>();
        services.AddTransient<BillingViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
