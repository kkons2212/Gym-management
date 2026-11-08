using System.Linq.Expressions;

namespace GymManagement.Server.Core.Contracts.Data;

/// <summary>
/// Giao diện kho lưu trữ tổng quát (Generic Repository Pattern) định nghĩa các thao tác CRUD cơ bản và nâng cao.
/// Tuân thủ nguyên tắc Dependency Inversion (DIP) - tầng Core độc lập hoàn toàn với EF Core.
/// </summary>
/// <typeparam name="T">Kiểu thực thể dữ liệu (class).</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Lấy một thực thể theo khóa chính bất đồng bộ.
    /// </summary>
    /// <param name="id">Giá trị khóa chính.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thực thể nếu tìm thấy, ngược lại trả về null.</returns>
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy toàn bộ danh sách thực thể bất đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách chỉ đọc chứa toàn bộ thực thể.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm danh sách thực thể thỏa mãn biểu thức điều kiện (Predicate).
    /// </summary>
    /// <param name="predicate">Biểu thức lọc điều kiện.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách thực thể phù hợp.</returns>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm một thực thể duy nhất hoặc trả về mặc định thỏa mãn biểu thức điều kiện.
    /// </summary>
    /// <param name="predicate">Biểu thức lọc điều kiện.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thực thể duy nhất hoặc null nếu không tìm thấy.</returns>
    Task<T?> FindSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy dữ liệu phân trang kết hợp sắp xếp và lọc điều kiện.
    /// </summary>
    /// <param name="pageIndex">Số thứ tự trang (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số lượng bản ghi trên một trang.</param>
    /// <param name="predicate">Biểu thức lọc tùy chọn.</param>
    /// <param name="orderBy">Hàm sắp xếp tùy chọn.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Bộ dữ liệu gồm danh sách Items và tổng số bản ghi TotalCount.</returns>
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra sự tồn tại của ít nhất một bản ghi thỏa mãn điều kiện.
    /// </summary>
    /// <param name="predicate">Biểu thức lọc điều kiện.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>True nếu tồn tại bản ghi, ngược lại False.</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm tổng số bản ghi trong bảng hoặc theo điều kiện lọc.
    /// </summary>
    /// <param name="predicate">Biểu thức lọc tùy chọn.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Số lượng bản ghi đếm được.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một thực thể vào kho dữ liệu bất đồng bộ.
    /// </summary>
    /// <param name="entity">Thực thể cần thêm.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một danh sách thực thể vào kho dữ liệu bất đồng bộ.
    /// </summary>
    /// <param name="entities">Danh sách thực thể cần thêm.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu cập nhật một thực thể trong bộ theo dõi trạng thái.
    /// </summary>
    /// <param name="entity">Thực thể cần cập nhật.</param>
    void Update(T entity);

    /// <summary>
    /// Đánh dấu xóa một thực thể khỏi bộ theo dõi trạng thái.
    /// </summary>
    /// <param name="entity">Thực thể cần xóa.</param>
    void Remove(T entity);

    /// <summary>
    /// Đánh dấu xóa một tập hợp thực thể khỏi bộ theo dõi trạng thái.
    /// </summary>
    /// <param name="entities">Danh sách thực thể cần xóa.</param>
    void RemoveRange(IEnumerable<T> entities);
}
