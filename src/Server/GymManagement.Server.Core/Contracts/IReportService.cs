using GymManagement.Server.Core.DTOs.Reports;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Hợp đồng dịch vụ Báo cáo & Thống kê Quản trị (Task 2.7 - Epic 06 / UC06).
/// Cung cấp các phương thức tổng hợp số liệu BI đa chiều và xuất dữ liệu báo cáo sang tệp Excel / PDF.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Lấy dữ liệu tổng quan cho Dashboard thời gian thực (hôm nay và tháng hiện tại).
    /// </summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy báo cáo doanh thu thực thu theo thời gian, hình thức thanh toán và gói tập.
    /// </summary>
    Task<RevenueReportDto> GetRevenueReportAsync(
        RevenueReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy báo cáo biến động hội viên (hội viên mới, gia hạn, bảo lưu, hủy, hết hạn và tỷ trọng gói).
    /// </summary>
    Task<MemberFluctuationReportDto> GetMemberFluctuationReportAsync(
        MemberFluctuationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy báo cáo lưu lượng check-in (phân bổ theo 24 giờ, tỷ lệ thành công và lý do từ chối).
    /// </summary>
    Task<CheckInTrafficReportDto> GetCheckInTrafficReportAsync(
        CheckInTrafficRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy báo cáo hiệu suất và điểm đánh giá chất lượng huấn luyện viên cá nhân (PT).
    /// </summary>
    Task<PtPerformanceReportDto> GetPtPerformanceReportAsync(
        PtPerformanceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trích xuất dữ liệu báo cáo sang tệp tin bảng tính Excel (.xlsx) hoặc tài liệu PDF (.pdf).
    /// </summary>
    Task<ExportReportResultDto> ExportReportAsync(
        ExportReportRequest request,
        CancellationToken cancellationToken = default);
}
