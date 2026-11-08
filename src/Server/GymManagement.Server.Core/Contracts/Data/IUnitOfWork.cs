namespace GymManagement.Server.Core.Contracts.Data;

/// <summary>
/// Giao diện Unit of Work điều phối lưu trữ dữ liệu và kiểm soát toàn vẹn giao dịch ACID đa bảng.
/// Cho phép các tầng nghiệp vụ quản lý repository và transaction một cách nhất quán.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Lấy hoặc khởi tạo một thể hiện Generic Repository tương ứng với kiểu thực thể.
    /// </summary>
    /// <typeparam name="T">Kiểu thực thể dữ liệu.</typeparam>
    /// <returns>Thể hiện của IGenericRepository cho thực thể T.</returns>
    IGenericRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Lưu toàn bộ các thay đổi đang được theo dõi vào cơ sở dữ liệu bất đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Số lượng bản ghi bị tác động trong CSDL.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bắt đầu một Transaction cơ sở dữ liệu mới bất đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Xác nhận (Commit) Transaction đang hoạt động bất đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Hoàn tác (Rollback) các thay đổi trong Transaction đang hoạt động bất đồng bộ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Thực thi một hành động có kết quả trả về bên trong một Transaction tự động an toàn (tự động Commit/Rollback).
    /// </summary>
    /// <typeparam name="TResult">Kiểu dữ liệu trả về của hành động.</typeparam>
    /// <param name="action">Hàm nghiệp vụ cần thực thi trong Transaction.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả trả về của hàm nghiệp vụ.</returns>
    Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thực thi một hành động không có kết quả trả về bên trong một Transaction tự động an toàn (tự động Commit/Rollback).
    /// </summary>
    /// <param name="action">Hàm nghiệp vụ cần thực thi trong Transaction.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
