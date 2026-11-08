using FluentAssertions;
using GymManagement.Server.Data.Persistence;
using GymManagement.Server.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Server.DataTests.Repositories;

/// <summary>
/// Bộ kiểm thử đơn vị cho GenericRepository&lt;T&gt; sử dụng EF Core InMemory.
/// </summary>
public class GenericRepositoryTests : IDisposable
{
    private readonly TestGymDbContext _context;
    private readonly GenericRepository<TestEntity> _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: $"GenericRepoTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new TestGymDbContext(options);
        _repository = new GenericRepository<TestEntity>(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Gold Plan", IsActive = true };
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Gold Plan");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenEntitiesExist_ShouldReturnAllEntities()
    {
        // Arrange
        await _context.TestEntities.AddRangeAsync(
            new TestEntity { Id = 1, Name = "Plan A", IsActive = true },
            new TestEntity { Id = 2, Name = "Plan B", IsActive = false }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Contain(["Plan A", "Plan B"]);
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        await _context.TestEntities.AddRangeAsync(
            new TestEntity { Id = 1, Name = "Active Plan", IsActive = true },
            new TestEntity { Id = 2, Name = "Inactive Plan", IsActive = false },
            new TestEntity { Id = 3, Name = "Another Active", IsActive = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(x => x.IsActive);

        // Assert
        result.Should().HaveCount(2);
        result.All(x => x.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task FindSingleOrDefaultAsync_WhenMatchingPredicate_ShouldReturnEntity()
    {
        // Arrange
        await _context.TestEntities.AddRangeAsync(
            new TestEntity { Id = 10, Name = "Unique Plan", IsActive = true },
            new TestEntity { Id = 20, Name = "Other Plan", IsActive = false }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindSingleOrDefaultAsync(x => x.Name == "Unique Plan");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedAsync_WhenPagingApplied_ShouldReturnCorrectPageSizeAndTotal()
    {
        // Arrange
        var entities = Enumerable.Range(1, 15)
            .Select(i => new TestEntity { Id = i, Name = $"Plan {i:D2}", IsActive = i % 2 == 0 })
            .ToList();
        await _context.TestEntities.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act: Lấy trang 2, mỗi trang 5 phần tử, chỉ lấy IsActive = true, sắp xếp giảm dần theo Id
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageIndex: 2,
            pageSize: 5,
            predicate: x => x.IsActive,
            orderBy: q => q.OrderByDescending(x => x.Id)
        );

        // Assert
        totalCount.Should().Be(7); // 2, 4, 6, 8, 10, 12, 14
        items.Should().HaveCount(2); // Trang 1 có 5, trang 2 có 2
        items[0].Id.Should().Be(4);
        items[1].Id.Should().Be(2);
    }

    [Fact]
    public async Task ExistsAsync_WhenMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        await _context.TestEntities.AddAsync(new TestEntity { Id = 1, Name = "Existing", IsActive = true });
        await _context.SaveChangesAsync();

        // Act & Assert
        (await _repository.ExistsAsync(x => x.Name == "Existing")).Should().BeTrue();
        (await _repository.ExistsAsync(x => x.Name == "NonExistent")).Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_WithFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        await _context.TestEntities.AddRangeAsync(
            new TestEntity { Id = 1, Name = "A", IsActive = true },
            new TestEntity { Id = 2, Name = "B", IsActive = true },
            new TestEntity { Id = 3, Name = "C", IsActive = false }
        );
        await _context.SaveChangesAsync();

        // Act & Assert
        (await _repository.CountAsync()).Should().Be(3);
        (await _repository.CountAsync(x => x.IsActive)).Should().Be(2);
    }

    [Fact]
    public async Task AddAsync_WhenValidEntity_ShouldPersistEntity()
    {
        // Arrange
        var newEntity = new TestEntity { Id = 50, Name = "New Plan", IsActive = true };

        // Act
        await _repository.AddAsync(newEntity);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.TestEntities.FindAsync(50);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Plan");
    }

    [Fact]
    public async Task AddRangeAsync_WhenValidEntities_ShouldPersistAllEntities()
    {
        // Arrange
        var list = new[]
        {
            new TestEntity { Id = 101, Name = "Batch 1", IsActive = true },
            new TestEntity { Id = 102, Name = "Batch 2", IsActive = true }
        };

        // Act
        await _repository.AddRangeAsync(list);
        await _context.SaveChangesAsync();

        // Assert
        var count = await _context.TestEntities.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task Update_WhenEntityModified_ShouldUpdateEntityState()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Old Name", IsActive = false };
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        entity.Name = "Updated Name";
        entity.IsActive = true;
        _repository.Update(entity);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.TestEntities.FindAsync(1);
        updated!.Name.Should().Be("Updated Name");
        updated.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_WhenEntityDeleted_ShouldRemoveFromDatabase()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "To Delete", IsActive = true };
        await _context.TestEntities.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(entity);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.TestEntities.FindAsync(1);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRange_WhenEntitiesDeleted_ShouldRemoveAllFromDatabase()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Del 1" },
            new TestEntity { Id = 2, Name = "Del 2" }
        };
        await _context.TestEntities.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        _repository.RemoveRange(entities);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.TestEntities.CountAsync()).Should().Be(0);
    }
}
