using GymManagement.Server.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Server.DataTests.Repositories;

/// <summary>
/// Thực thể giả lập phục vụ kiểm thử đơn vị GenericRepository và UnitOfWork.
/// </summary>
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Ngữ cảnh DbContext giả lập phục vụ kiểm thử InMemory cho Data layer.
/// </summary>
public class TestGymDbContext : GymDbContext
{
    public TestGymDbContext(DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
    }
}
