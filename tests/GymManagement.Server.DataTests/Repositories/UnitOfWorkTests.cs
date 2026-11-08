using FluentAssertions;
using GymManagement.Server.Data.Persistence;
using GymManagement.Server.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymManagement.Server.DataTests.Repositories;

/// <summary>
/// Bộ kiểm thử đơn vị cho UnitOfWork quản lý Transaction và Repository.
/// </summary>
public class UnitOfWorkTests : IDisposable
{
    private readonly TestGymDbContext _context;
    private readonly Mock<ILogger<UnitOfWork>> _loggerMock;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseInMemoryDatabase(databaseName: $"UnitOfWorkTestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestGymDbContext(options);
        _loggerMock = new Mock<ILogger<UnitOfWork>>();
        _unitOfWork = new UnitOfWork(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        try
        {
            _context.Database.EnsureDeleted();
        }
        catch (ObjectDisposedException)
        {
            // Context có thể đã được giải phóng trong bài test dispose
        }

        try
        {
            _unitOfWork.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // UnitOfWork đã được giải phóng
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Repository_WhenRequestedMultipleTimes_ShouldReturnSameCachedInstance()
    {
        // Act
        var repo1 = _unitOfWork.Repository<TestEntity>();
        var repo2 = _unitOfWork.Repository<TestEntity>();

        // Assert
        repo1.Should().NotBeNull();
        repo2.Should().NotBeNull();
        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenChangesMade_ShouldPersistToDatabase()
    {
        // Arrange
        var repo = _unitOfWork.Repository<TestEntity>();
        await repo.AddAsync(new TestEntity { Id = 1, Name = "Persisted Plan" });

        // Act
        var affectedRows = await _unitOfWork.SaveChangesAsync();

        // Assert
        affectedRows.Should().Be(1);
        var found = await repo.GetByIdAsync(1);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Persisted Plan");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenActionSucceeds_ShouldCommitTransaction()
    {
        // Act
        var result = await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var repo = _unitOfWork.Repository<TestEntity>();
            await repo.AddAsync(new TestEntity { Id = 10, Name = "Transaction Item" });
            return "SUCCESS";
        });

        // Assert
        result.Should().Be("SUCCESS");
        var saved = await _context.TestEntities.FindAsync(10);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Transaction Item");
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenActionThrows_ShouldRollbackAndNotPersist()
    {
        // Act
        Func<Task> act = async () =>
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var repo = _unitOfWork.Repository<TestEntity>();
                await repo.AddAsync(new TestEntity { Id = 99, Name = "Will Rollback" });
                throw new InvalidOperationException("Mô phỏng lỗi nghiệp vụ");
            });
        };

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Mô phỏng lỗi nghiệp vụ");

        var notFound = await _context.TestEntities.FindAsync(99);
        notFound.Should().BeNull();

        // Verify that error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transaction nghiệp vụ thất bại")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_VoidAction_WhenSucceeds_ShouldCommit()
    {
        // Act
        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var repo = _unitOfWork.Repository<TestEntity>();
            await repo.AddAsync(new TestEntity { Id = 20, Name = "Void Action Item" });
        });

        // Assert
        var saved = await _context.TestEntities.FindAsync(20);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Void Action Item");
    }

    [Fact]
    public async Task BeginTransactionAsync_WhenAlreadyActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Act
        Func<Task> act = async () => await _unitOfWork.BeginTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Một Transaction khác đang hoạt động*");

        // Cleanup
        await _unitOfWork.RollbackTransactionAsync();
    }

    [Fact]
    public async Task CommitTransactionAsync_WhenNoActiveTransaction_ShouldThrowInvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await _unitOfWork.CommitTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Không có Transaction nào đang hoạt động*");
    }

    [Fact]
    public void Dispose_WhenCalled_ShouldCleanUpResources()
    {
        // Act
        _unitOfWork.Dispose();

        // Assert
        Action act = () => _unitOfWork.Repository<TestEntity>();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task DisposeAsync_WhenCalled_ShouldCleanUpResources()
    {
        // Act
        await _unitOfWork.DisposeAsync();

        // Assert
        Action act = () => _unitOfWork.Repository<TestEntity>();
        act.Should().Throw<ObjectDisposedException>();
    }
}
