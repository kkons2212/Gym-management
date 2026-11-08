using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Data.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Server.Data;

/// <summary>
/// Mở rộng IServiceCollection để đăng ký các thành phần tầng Data và External Adapters.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký toàn bộ 3 External Adapters (VietQR, Notification, CameraScanner) vào hệ thống DI.
    /// </summary>
    public static IServiceCollection AddServerDataAdapters(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // 1. Cấu hình Options nếu có configuration
        if (configuration != null)
        {
            services.Configure<VietQrOptions>(configuration.GetSection(VietQrOptions.SectionName));
            services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));
        }

        // 2. Đăng ký HttpClient
        services.AddHttpClient<IVietQrAdapter, VietQrAdapter>();

        // 3. Đăng ký các Adapters
        services.AddScoped<IVietQrAdapter, VietQrAdapter>();
        services.AddScoped<INotificationAdapter, NotificationAdapter>();
        services.AddSingleton<ICameraScannerAdapter, CameraScannerAdapter>();

        return services;
    }
}

