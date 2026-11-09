using GymManagement.Server.Core.DTOs.CheckIn;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Giao diện dịch vụ kiểm soát điểm danh hội viên vào phòng tập (UC03).
/// Kết nối xác thực danh tính, kiểm tra điều kiện gói tập và lưu nhật ký CheckInLog.
/// </summary>
public interface ICheckInService
{
    /// <summary>
    /// Thực hiện điểm danh hội viên theo yêu cầu (quét mã QR hoặc số điện thoại).
    /// </summary>
    /// <param name="request">Thông tin yêu cầu điểm danh.</param>
    /// <param name="verifiedByAdminId">Mã định danh Admin thực hiện xác nhận tại quầy (tùy chọn).</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả điểm danh kèm thông tin hội viên và số lượt tập còn lại.</returns>
    Task<CheckInResponse> CheckInAsync(CheckInRequest request, string? verifiedByAdminId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Điểm danh nhanh bằng chuỗi mã QR (UC 3.1).
    /// </summary>
    /// <param name="qrCode">Chuỗi mã QR quét từ thiết bị của hội viên.</param>
    /// <param name="verifiedByAdminId">Mã định danh Admin xác nhận (tùy chọn).</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả điểm danh.</returns>
    Task<CheckInResponse> CheckInWithQrAsync(string qrCode, string? verifiedByAdminId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Điểm danh thủ công bằng mã hội viên hoặc số điện thoại (UC 3.2).
    /// </summary>
    /// <param name="memberCodeOrPhone">Mã hội viên (UserName) hoặc số điện thoại đăng ký.</param>
    /// <param name="notes">Ghi chú xác nhận của Admin (tùy chọn).</param>
    /// <param name="verifiedByAdminId">Mã định danh Admin xác nhận (tùy chọn).</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Kết quả điểm danh.</returns>
    Task<CheckInResponse> ManualCheckInAsync(string memberCodeOrPhone, string? notes = null, string? verifiedByAdminId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách nhật ký điểm danh trong ngày hôm nay kèm bộ lọc (UC 3.3).
    /// </summary>
    /// <param name="searchQuery">Từ khóa tìm kiếm theo tên hoặc mã hội viên (tùy chọn).</param>
    /// <param name="statusFilter">Bộ lọc trạng thái: SUCCESS hoặc DENIED (tùy chọn).</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách bản ghi điểm danh trong ngày.</returns>
    Task<IReadOnlyList<CheckInLogDto>> GetTodayCheckInLogsAsync(string? searchQuery = null, string? statusFilter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tổng số lượng hội viên đã điểm danh thành công vào phòng tập hôm nay (UC 3.3).
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Số lượng lượt check-in thành công trong ngày.</returns>
    Task<int> GetCurrentAttendanceCountAsync(CancellationToken cancellationToken = default);
}
