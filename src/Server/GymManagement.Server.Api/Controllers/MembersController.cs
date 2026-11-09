using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller quản lý hội viên và vòng đời gói tập (UC02, UC09).
/// Phục vụ lễ tân/admin đăng ký, gia hạn, bảo lưu, hủy và hội viên tra cứu hồ sơ/lịch sử tập.
/// </summary>
public sealed class MembersController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IMemberService _memberService;
    private readonly ILogger<MembersController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public MembersController(
        IMemberService memberService,
        ILogger<MembersController> logger)
    {
        ArgumentNullException.ThrowIfNull(memberService);
        ArgumentNullException.ThrowIfNull(logger);

        _memberService = memberService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS — HỒ SƠ HỘI VIÊN (UC02.1, UC02.2, UC09.1)
    // =========================================================================

    /// <summary>
    /// Tìm kiếm hội viên theo từ khóa (SĐT, Họ tên, Mã hội viên) (UC02.2).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SearchMembers(
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var members = await _memberService.SearchMembersAsync(keyword, cancellationToken);
        return OkResponse(members, "Tra cứu danh sách hội viên thành công.");
    }

    /// <summary>
    /// Lấy toàn bộ thông tin chi tiết hồ sơ hội viên (UC09.1).
    /// </summary>
    [HttpGet("{userName}")]
    [Authorize]
    public async Task<IActionResult> GetProfile(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var profile = await _memberService.GetMemberProfileAsync(userName, cancellationToken);
        return OkResponse(profile, "Lấy thông tin hồ sơ hội viên thành công.");
    }

    /// <summary>
    /// Đăng ký hội viên mới kèm gói tập và cấp mã QR (UC02.1).
    /// </summary>
    [HttpPost("register")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RegisterMember(
        [FromBody] RegisterMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MembersController: Tiep nhan dang ky hoi vien moi {FullName}", request.FullName);

        var profile = await _memberService.RegisterMemberAsync(request, cancellationToken);
        return CreatedResponse($"/api/v1/members/{profile.UserName}", profile, "Đăng ký hội viên mới thành công.");
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân của hội viên (UC02.2).
    /// </summary>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateMember(
        [FromBody] UpdateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await _memberService.UpdateMemberAsync(request, cancellationToken);
        return OkResponse(profile, "Cập nhật hồ sơ hội viên thành công.");
    }

    // =========================================================================
    // 4. ACTION METHODS — QUẢN TRỊ VÒNG ĐỜI GÓI TẬP (UC02.3, UC02.4, UC02.5, UC09.1)
    // =========================================================================

    /// <summary>
    /// Lấy thông tin gói tập đang hoạt động của hội viên (UC09.1).
    /// </summary>
    [HttpGet("{userName}/active-subscription")]
    [Authorize]
    public async Task<IActionResult> GetActiveSubscription(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _memberService.GetActiveSubscriptionAsync(userName, cancellationToken);
        return OkResponse(subscription, "Lấy thông tin gói tập thành công.");
    }

    /// <summary>
    /// Gia hạn gói tập cho hội viên (UC02.3).
    /// </summary>
    [HttpPost("renew")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> RenewSubscription(
        [FromBody] RenewSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _memberService.RenewSubscriptionAsync(request, cancellationToken);
        return OkResponse(subscription, "Gia hạn gói tập thành công.");
    }

    /// <summary>
    /// Nâng cấp gói tập lên gói hạng cao hơn (UC02.3).
    /// </summary>
    [HttpPost("upgrade")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpgradeSubscription(
        [FromBody] UpgradeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _memberService.UpgradeSubscriptionAsync(request, cancellationToken);
        return OkResponse(subscription, "Nâng cấp gói tập thành công.");
    }

    /// <summary>
    /// Bảo lưu gói tập và kéo dài thời hạn (UC02.4).
    /// </summary>
    [HttpPost("freeze")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> FreezeSubscription(
        [FromBody] FreezeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _memberService.FreezeSubscriptionAsync(request, cancellationToken);
        return OkResponse(subscription, "Bảo lưu gói tập thành công.");
    }

    /// <summary>
    /// Hủy đăng ký gói tập hội viên (UC02.5).
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CancelSubscription(
        [FromBody] CancelSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        await _memberService.CancelSubscriptionAsync(request, cancellationToken);
        return OkResponse(true, "Hủy gói tập thành công.");
    }

    /// <summary>
    /// Tra cứu lịch sử check-in điểm danh của hội viên (UC09.2).
    /// </summary>
    [HttpGet("{userName}/history")]
    [Authorize]
    public async Task<IActionResult> GetWorkoutHistory(
        string userName,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var history = await _memberService.GetWorkoutHistoryAsync(userName, fromDate, toDate, cancellationToken);
        return OkResponse(history, "Lấy lịch sử tập luyện thành công.");
    }
}
