using System.Collections.Concurrent;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Data.Repositories;

/// <summary>
/// Hiện thực Unit of Work điều phối kho lưu trữ và đảm bảo tính toàn vẹn giao dịch ACID đa bảng.
/// Tự động lưu vết lỗi và hoàn tác khi có exception phát sinh trong transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    // ====================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS (Khai báo đầu lớp)
    // ====================================================================
    private readonly GymDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    // ====================================================================
    // 2. CONSTRUCTOR
    // ====================================================================
    /// <summary>
    /// Khởi tạo một thể hiện UnitOfWork với DbContext và Logger.
    /// </summary>
    /// <param name="context">Ngữ cảnh cơ sở dữ liệu GymDbContext.</param>
    /// <param name="logger">Trình ghi log chẩn đoán lỗi.</param>
    public UnitOfWork(GymDbContext context, ILogger<UnitOfWork> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        _context = context;
        _logger = logger;
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    // ====================================================================
    // 3. REPOSITORY ACCESSOR
    // ====================================================================
    /// <inheritdoc />
    public IGenericRepository<T> Repository<T>() where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(T),
            _ => new GenericRepository<T>(_context));
    }

    // ====================================================================
    // 4. PERSISTENCE & TRANSACTION MANAGEMENT
    // ====================================================================
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Một Transaction khác đang hoạt động trên UnitOfWork này.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogDebug("Đã mở Transaction cơ sở dữ liệu mới.");
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Không có Transaction nào đang hoạt động để Commit.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogDebug("Đã Commit Transaction thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình Commit Transaction: {Message}. Đang tự động Rollback...", ex.Message);
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await CleanUpCurrentTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_currentTransaction == null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            _logger.LogWarning("Đã hoàn tác (Rollback) Transaction cơ sở dữ liệu.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi Rollback Transaction: {Message}", ex.Message);
            throw;
        }
        finally
        {
            await CleanUpCurrentTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ObjectDisposedException.ThrowIf(_disposed, this);

        await BeginTransactionAsync(cancellationToken);
        try
        {
            TResult result = await action();
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction nghiệp vụ thất bại và đã được hoàn tác: {Message}", ex.Message);
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        async Task<bool> ActionWrapperAsync()
        {
            await action();
            return true;
        }

        return ExecuteTransactionAsync<bool>(ActionWrapperAsync, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default, params object[] parameters)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(sql);
        return _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    // ====================================================================
    // 5. DISPOSE PATTERN
    // ====================================================================
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Giải phóng tài nguyên unmanaged và managed.
    /// </summary>
    /// <param name="disposing">Cờ cho biết có giải phóng tài nguyên managed hay không.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
                _context.Dispose();
                _repositories.Clear();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Giải phóng tài nguyên bất đồng bộ.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await _context.DisposeAsync();
        _repositories.Clear();
    }

    // ====================================================================
    // 6. PRIVATE HELPER METHODS
    // ====================================================================
    private async Task CleanUpCurrentTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
