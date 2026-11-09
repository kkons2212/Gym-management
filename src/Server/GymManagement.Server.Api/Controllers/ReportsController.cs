using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller tổng hợp báo cáo và thống kê quản trị BI (UC06).
/// Cung cấp số liệu đa chiều: Doanh thu thực thu, biến động hội viên, lưu lượng check-in, hiệu suất PT và xuất Excel/PDF.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public sealed class ReportsController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        ArgumentNullException.ThrowIfNull(reportService);
        ArgumentNullException.ThrowIfNull(logger);

        _reportService = reportService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS — BÁO CÁO THỜI GIAN THỰC (UC06 / US 06.1)
    // =========================================================================

    /// <summary>
    /// Lấy dữ liệu tổng quan cho Dashboard thời gian thực (UC 06.1).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken = default)
    {
        var summary = await _reportService.GetDashboardSummaryAsync(cancellationToken);
        return OkResponse(summary, "Lấy dữ liệu Dashboard tổng quan thành công.");
    }

    /// <summary>
    /// Lấy báo cáo doanh thu thực thu theo thời gian, hình thức và gói tập (UC 06.1).
    /// </summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? paymentMethod = null,
        [FromQuery] string? planId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RevenueReportRequest(fromDate, toDate, paymentMethod, planId);
        var report = await _reportService.GetRevenueReportAsync(request, cancellationToken);
        return OkResponse(report, "Lấy báo cáo doanh thu thành công.");
    }

    /// <summary>
    /// Lấy báo cáo biến động hội viên, tỷ lệ duy trì và phân bổ gói tập (UC 06.1).
    /// </summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetMemberFluctuations(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var request = new MemberFluctuationRequest(fromDate, toDate);
        var report = await _reportService.GetMemberFluctuationReportAsync(request, cancellationToken);
        return OkResponse(report, "Lấy báo cáo biến động hội viên thành công.");
    }

    /// <summary>
    /// Lấy báo cáo lưu lượng check-in, 24 khung giờ cao điểm và lý do từ chối (UC 06.1).
    /// </summary>
    [HttpGet("checkin")]
    public async Task<IActionResult> GetCheckInTraffic(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var request = new CheckInTrafficRequest(fromDate, toDate);
        var report = await _reportService.GetCheckInTrafficReportAsync(request, cancellationToken);
        return OkResponse(report, "Lấy báo cáo lưu lượng check-in thành công.");
    }

    /// <summary>
    /// Lấy báo cáo hiệu suất và điểm đánh giá huấn luyện viên PT (UC 06.1).
    /// </summary>
    [HttpGet("pt")]
    public async Task<IActionResult> GetPtPerformance(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? trainerId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new PtPerformanceRequest(fromDate, toDate, trainerId);
        var report = await _reportService.GetPtPerformanceReportAsync(request, cancellationToken);
        return OkResponse(report, "Lấy báo cáo hiệu suất PT thành công.");
    }

    // =========================================================================
    // 4. ACTION METHODS — XUẤT TỆP EXCEL / PDF (UC06 / US 06.2)
    // =========================================================================

    /// <summary>
    /// Xuất dữ liệu báo cáo sang tệp tin bảng tính Excel (.xlsx) hoặc tài liệu PDF (.pdf) (UC 06.2).
    /// </summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportReport(
        [FromBody] ExportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ReportsController: Xuat bao cao {Type} theo dinh dang {Format}",
            request.ReportType, request.Format);

        var result = await _reportService.ExportReportAsync(request, cancellationToken);
        return File(result.FileData, result.ContentType, result.FileName);
    }
}
