using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Data.Adapters;
using GymManagement.Server.Data.Persistence;
using GymManagement.Server.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Server.Data;

/// <summary>
/// Mở rộng IServiceCollection để đăng ký các thành phần tầng Data, Repositories và External Adapters.
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

    /// <summary>
    /// Đăng ký Database API bao gồm GymDbContext, GenericRepository và UnitOfWork vào hệ thống DI.
    /// </summary>
    public static IServiceCollection AddServerDataRepositories(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // 1. Cấu hình GymDbContext với SQL Server (Hỗ trợ cấu hình Docker SQL Server linh hoạt)
        string? connectionString = null;

        if (configuration != null)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=localhost,1433;Database=GymManagementDb;User Id=sa;Password=SqlServer@2026;TrustServerCertificate=True;MultipleActiveResultSets=true";
        }

        services.AddDbContext<GymDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // 2. Đăng ký Generic Repository và Unit of Work
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
