using System.Linq.Expressions;
using GymManagement.Server.Core.Contracts.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Server.Data.Repositories;

/// <summary>
/// Hiện thực kho lưu trữ tổng quát Generic Repository sử dụng Entity Framework Core.
/// Cung cấp các thao tác CRUD cơ bản và nâng cao cho mọi thực thể class.
/// </summary>
/// <typeparam name="T">Kiểu thực thể dữ liệu.</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    // ====================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS (Khai báo đầu lớp)
    // ====================================================================
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    // ====================================================================
    // 2. CONSTRUCTOR
    // ====================================================================
    /// <summary>
    /// Khởi tạo một GenericRepository gắn với DbContext tương ứng.
    /// </summary>
    /// <param name="context">Ngữ cảnh cơ sở dữ liệu EF Core.</param>
    public GenericRepository(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ====================================================================
    // 3. READ / QUERY METHODS
    // ====================================================================
    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> FindSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.AsNoTracking().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        int validPageIndex = pageIndex < 1 ? 1 : pageIndex;
        int validPageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<T> query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        List<T> items = await query
            .Skip((validPageIndex - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await _dbSet.AsNoTracking().CountAsync(cancellationToken)
            : await _dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);
    }

    // ====================================================================
    // 4. WRITE / MUTATION METHODS
    // ====================================================================
    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public void Remove(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public void RemoveRange(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
    }
}
