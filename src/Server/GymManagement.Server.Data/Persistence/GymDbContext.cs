using GymManagement.Server.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Server.Data.Persistence;

/// <summary>
/// Ngữ cảnh cơ sở dữ liệu Entity Framework Core chính cho hệ thống Gym Management.
/// Quản lý phiên làm việc với SQL Server và theo dõi trạng thái các thực thể.
/// </summary>
public class GymDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MemberSubscription> MemberSubscriptions => Set<MemberSubscription>();
    public DbSet<PackagePlan> PackagePlans => Set<PackagePlan>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<CheckInLog> CheckInLogs => Set<CheckInLog>();

    // ====================================================================
    // 1. DBSETS CHO 9 THỰC THỂ CSDL VẬT LÝ
    // ====================================================================
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PackagePlan> PackagePlans => Set<PackagePlan>();
    public DbSet<MemberSubscription> MemberSubscriptions => Set<MemberSubscription>();
    public DbSet<PTAvailability> PTAvailabilities => Set<PTAvailability>();
    public DbSet<PTTrainingSession> PTTrainingSessions => Set<PTTrainingSession>();
    public DbSet<CheckInLog> CheckInLogs => Set<CheckInLog>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<MemberBodyMetric> MemberBodyMetrics => Set<MemberBodyMetric>();

    // ====================================================================
    // 2. CONSTRUCTOR
    // ====================================================================
    /// <summary>
    /// Khởi tạo một phiên GymDbContext với các tùy chọn cấu hình được truyền vào qua DI.
    /// </summary>
    /// <param name="options">Tùy chọn cấu hình DbContext.</param>
    public GymDbContext(DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    // ====================================================================
    // 3. MODEL CONFIGURATION
    // ====================================================================
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tự động áp dụng tất cả các cấu hình IEntityTypeConfiguration<T> trong assembly Data
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }
}
