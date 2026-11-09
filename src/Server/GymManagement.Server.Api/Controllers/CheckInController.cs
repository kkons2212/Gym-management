using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.CheckIn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller kiểm soát điểm danh hội viên vào phòng tập (UC03).
/// Phục vụ quét mã QR webcam hoặc tra cứu SĐT/Mã hội viên tại quầy tiếp đón.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public sealed class CheckInController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly ICheckInService _checkInService;
    private readonly ILogger<CheckInController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public CheckInController(
        ICheckInService checkInService,
        ILogger<CheckInController> logger)
    {
        ArgumentNullException.ThrowIfNull(checkInService);
        ArgumentNullException.ThrowIfNull(logger);

        _checkInService = checkInService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS (ENDPOINTS)
    // =========================================================================

    /// <summary>
    /// Điểm danh hội viên theo yêu cầu tổng quát (UC03).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CheckIn(
        [FromBody] CheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        string adminId = GetCurrentUserName();
        var response = await _checkInService.CheckInAsync(request, adminId, cancellationToken);
        return OkResponse(response, response.Message);
    }

    /// <summary>
    /// Điểm danh nhanh bằng mã QR từ thiết bị / webcam (UC 3.1).
    /// </summary>
    [HttpPost("qr")]
    public async Task<IActionResult> CheckInWithQr(
        [FromBody] QrCheckInApiRequest request,
        CancellationToken cancellationToken = default)
    {
        string adminId = GetCurrentUserName();
        var response = await _checkInService.CheckInWithQrAsync(request.QrCode, adminId, cancellationToken);
        return OkResponse(response, response.Message);
    }

    /// <summary>
    /// Điểm danh thủ công bằng SĐT hoặc Mã hội viên (UC 3.2).
    /// </summary>
    [HttpPost("manual")]
    public async Task<IActionResult> ManualCheckIn(
        [FromBody] ManualCheckInApiRequest request,
        CancellationToken cancellationToken = default)
    {
        string adminId = GetCurrentUserName();
        var response = await _checkInService.ManualCheckInAsync(
            request.MemberCodeOrPhone,
            request.Notes,
            adminId,
            cancellationToken);

        return OkResponse(response, response.Message);
    }

    /// <summary>
    /// Tra cứu danh sách nhật ký điểm danh trong ngày hôm nay kèm bộ lọc (UC 3.3).
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayLogs(
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _checkInService.GetTodayCheckInLogsAsync(searchQuery, statusFilter, cancellationToken);
        return OkResponse(logs, "Lấy danh sách nhật ký điểm danh thành công.");
    }

    /// <summary>
    /// Lấy tổng số lượng hội viên đã điểm danh thành công vào phòng tập hôm nay (UC 3.3).
    /// </summary>
    [HttpGet("attendance-count")]
    public async Task<IActionResult> GetAttendanceCount(CancellationToken cancellationToken = default)
    {
        int count = await _checkInService.GetCurrentAttendanceCountAsync(cancellationToken);
        return OkResponse(count, "Lấy tổng lượt điểm danh hôm nay thành công.");
    }
}

public record QrCheckInApiRequest(string QrCode);
public record ManualCheckInApiRequest(string MemberCodeOrPhone, string? Notes = null);
