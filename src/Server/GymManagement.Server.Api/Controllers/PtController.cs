using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.PT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller điều phối dịch vụ Huấn luyện viên cá nhân (PT) (UC04, UC08, UC10).
/// Phục vụ admin quản lý hồ sơ PT, PT đăng ký slot/hoàn thành ca và hội viên đặt/đổi/hủy lịch.
/// </summary>
public sealed class PtController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IPtService _ptService;
    private readonly ILogger<PtController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public PtController(
        IPtService ptService,
        ILogger<PtController> logger)
    {
        ArgumentNullException.ThrowIfNull(ptService);
        ArgumentNullException.ThrowIfNull(logger);

        _ptService = ptService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS — QUẢN LÝ HỒ SƠ & PHÂN CÔNG PT (UC04)
    // =========================================================================

    /// <summary>
    /// Lấy danh sách tất cả PT, có thể lọc theo trạng thái hoạt động (UC 4.2).
    /// </summary>
    [HttpGet("trainers")]
    [Authorize]
    public async Task<IActionResult> GetTrainers(
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var trainers = await _ptService.GetTrainersAsync(activeOnly, cancellationToken);
        return OkResponse(trainers, "Lấy danh sách PT thành công.");
    }

    /// <summary>
    /// Lấy chi tiết hồ sơ một PT theo ID (UC 4.1).
    /// </summary>
    [HttpGet("trainers/{ptId}")]
    [Authorize]
    public async Task<IActionResult> GetTrainerById(
        string ptId,
        CancellationToken cancellationToken = default)
    {
        var trainer = await _ptService.GetTrainerByIdAsync(ptId, cancellationToken);
        return OkResponse(trainer, "Lấy thông tin PT thành công.");
    }

    /// <summary>
    /// Tạo mới hoặc cập nhật hồ sơ PT (UC 4.1, UC 4.3).
    /// </summary>
    [HttpPost("trainers")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SaveTrainer(
        [FromBody] SavePtProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var trainer = await _ptService.SaveTrainerAsync(request, cancellationToken);
        return OkResponse(trainer, "Lưu hồ sơ PT thành công.");
    }

    /// <summary>
    /// Phân công PT phụ trách cố định cho hội viên (UC 4.4).
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AssignPtToMember(
        [FromBody] AssignPtRequest request,
        CancellationToken cancellationToken = default)
    {
        await _ptService.AssignPtToMemberAsync(request, cancellationToken);
        return OkResponse(true, "Phân công PT cho hội viên thành công.");
    }

    /// <summary>
    /// Lấy danh sách học viên đang được một PT phụ trách (UC 8.2.1).
    /// </summary>
    [HttpGet("trainers/{ptId}/students")]
    [Authorize]
    public async Task<IActionResult> GetPtStudents(
        string ptId,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var students = await _ptService.GetPtStudentsAsync(ptId, searchTerm, cancellationToken);
        return OkResponse(students, "Lấy danh sách học viên của PT thành công.");
    }

    // =========================================================================
    // 4. ACTION METHODS — LỊCH RẢNH PT (UC 4.6, UC 8.1.1, UC 8.1.2)
    // =========================================================================

    /// <summary>
    /// Lấy danh sách lịch rảnh của PT trong khoảng thời gian (UC 8.1.2).
    /// </summary>
    [HttpGet("trainers/{ptId}/slots")]
    [Authorize]
    public async Task<IActionResult> GetPtSlots(
        string ptId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var slots = await _ptService.GetPtSlotsAsync(ptId, fromDate, toDate, cancellationToken);
        return OkResponse(slots, "Lấy lịch rảnh PT thành công.");
    }

    /// <summary>
    /// PT đăng ký hoặc hủy đăng ký lịch rảnh theo tuần (UC 8.1.1).
    /// </summary>
    [HttpPost("slots")]
    [Authorize]
    public async Task<IActionResult> SavePtSlots(
        [FromBody] SavePtSlotsRequest request,
        CancellationToken cancellationToken = default)
    {
        await _ptService.SavePtSlotsAsync(request, cancellationToken);
        return OkResponse(true, "Lưu lịch rảnh thành công.");
    }

    // =========================================================================
    // 5. ACTION METHODS — CA TẬP & ĐẶT LỊCH (UC 10.1, UC 10.2, UC 10.4, UC 8.2.3, UC 4.5)
    // =========================================================================

    /// <summary>
    /// Lấy danh sách ca tập của PT hoặc hội viên (UC 4.2, UC 8.1.2).
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetSessions(
        [FromQuery] string? ptId = null,
        [FromQuery] string? memberUserName = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _ptService.GetSessionsAsync(ptId, memberUserName, fromDate, toDate, cancellationToken);
        return OkResponse(sessions, "Lấy danh sách ca tập thành công.");
    }

    /// <summary>
    /// Hội viên đặt lịch tập với PT (UC 10.1).
    /// </summary>
    [HttpPost("sessions/book")]
    [Authorize]
    public async Task<IActionResult> BookSession(
        [FromBody] BookSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _ptService.BookSessionAsync(request, cancellationToken);
        return OkResponse(session, "Đặt lịch tập với PT thành công.");
    }

    /// <summary>
    /// Hội viên hủy lịch tập có áp dụng quy tắc phạt 2h (UC 10.2).
    /// </summary>
    [HttpPost("sessions/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSession(
        [FromBody] CancelSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        await _ptService.CancelSessionAsync(request, cancellationToken);
        return OkResponse(true, "Hủy lịch tập thành công.");
    }

    /// <summary>
    /// Hội viên đổi lịch tập sang slot khác của cùng PT (UC 10.4).
    /// </summary>
    [HttpPost("sessions/reschedule")]
    [Authorize]
    public async Task<IActionResult> RescheduleSession(
        [FromBody] RescheduleSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _ptService.RescheduleSessionAsync(request, cancellationToken);
        return OkResponse(session, "Đổi lịch tập thành công.");
    }

    /// <summary>
    /// PT xác nhận hoàn thành ca tập và trừ buổi (UC 8.2.3).
    /// </summary>
    [HttpPost("sessions/complete-by-pt")]
    [Authorize(Policy = "PtOnly")]
    public async Task<IActionResult> CompleteSessionByPt(
        [FromBody] PtCompleteSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _ptService.CompleteSessionByPtAsync(request, cancellationToken);
        return OkResponse(session, "Xác nhận hoàn tất ca tập thành công.");
    }

    /// <summary>
    /// Admin xác nhận hoặc điều chỉnh kết quả ca tập (UC 4.5).
    /// </summary>
    [HttpPost("sessions/confirm-result")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ConfirmSessionResult(
        [FromBody] ConfirmSessionResultRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _ptService.ConfirmSessionResultAsync(request, cancellationToken);
        return OkResponse(session, "Cập nhật kết quả ca tập thành công.");
    }

    // =========================================================================
    // 6. ACTION METHODS — INBODY & ĐÁNH GIÁ SAO (UC 8.2.2, UC 10.3)
    // =========================================================================

    /// <summary>
    /// Lấy lịch sử chỉ số InBody của học viên (UC 8.2.1).
    /// </summary>
    [HttpGet("members/{userName}/inbody")]
    [Authorize]
    public async Task<IActionResult> GetInBodyHistory(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var history = await _ptService.GetInBodyHistoryAsync(userName, cancellationToken);
        return OkResponse(history, "Lấy lịch sử InBody thành công.");
    }

    /// <summary>
    /// PT ghi nhận chỉ số InBody mới cho học viên (UC 8.2.2).
    /// </summary>
    [HttpPost("inbody")]
    [Authorize]
    public async Task<IActionResult> RecordInBody(
        [FromBody] RecordInBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        var record = await _ptService.RecordInBodyAsync(request, cancellationToken);
        return OkResponse(record, "Ghi nhận chỉ số InBody thành công.");
    }

    /// <summary>
    /// Hội viên đánh giá điểm sao (1–5) sau ca tập đã hoàn thành (UC 10.3).
    /// </summary>
    [HttpPost("rating")]
    [Authorize]
    public async Task<IActionResult> SubmitRating(
        [FromBody] SubmitRatingRequest request,
        CancellationToken cancellationToken = default)
    {
        await _ptService.SubmitRatingAsync(request, cancellationToken);
        return OkResponse(true, "Gửi đánh giá ca tập thành công.");
    }
}
