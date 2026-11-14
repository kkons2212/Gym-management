using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller quản trị tài khoản nhân sự và danh mục gói tập (UC07).
/// Phục vụ Admin thiết lập tài khoản nhân viên, phân quyền và quản lý danh mục gói dịch vụ.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/v1/admin")]
public sealed class SettingsController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly ISystemSettingService _systemSettingService;
    private readonly ILogger<SettingsController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public SettingsController(
        ISystemSettingService systemSettingService,
        ILogger<SettingsController> logger)
    {
        ArgumentNullException.ThrowIfNull(systemSettingService);
        ArgumentNullException.ThrowIfNull(logger);

        _systemSettingService = systemSettingService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS — QUẢN TRỊ TÀI KHOẢN & PHÂN QUYỀN (UC 7.1)
    // =========================================================================

    /// <summary>
    /// Lấy danh sách vai trò sẵn có trong hệ thống (để populate role dropdown).
    /// </summary>
    [HttpGet("roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken = default)
    {
        var roles = await _systemSettingService.GetRolesAsync(cancellationToken);
        return OkResponse(roles, "Lấy danh sách vai trò thành công.");
    }

    /// <summary>
    /// Lấy danh sách tài khoản theo bộ lọc vai trò và trạng thái (UC 7.1).
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? roleId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var users = await _systemSettingService.GetUsersAsync(roleId, status, cancellationToken);
        return OkResponse(users, "Lấy danh sách người dùng thành công.");
    }

    /// <summary>
    /// Tra cứu chi tiết tài khoản theo tên đăng nhập (UC 7.1).
    /// </summary>
    [HttpGet("users/{userName}")]
    public async Task<IActionResult> GetUserByUserName(
        string userName,
        CancellationToken cancellationToken = default)
    {
        var user = await _systemSettingService.GetUserByUserNameAsync(userName, cancellationToken);
        return OkResponse(user, "Lấy thông tin người dùng thành công.");
    }

    /// <summary>
    /// Tạo mới tài khoản và phân quyền nhân sự (UC 7.1.1).
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SettingsController: Tao tai khoan moi {UserName} voi quyen {RoleId}",
            request.UserName, request.RoleId);

        var user = await _systemSettingService.CreateUserAsync(request, cancellationToken);
        return CreatedResponse($"/api/v1/admin/users/{user.UserName}", user, "Tạo tài khoản người dùng thành công.");
    }

    /// <summary>
    /// Cập nhật thông tin tài khoản và điều chỉnh phân quyền (UC 7.1.2).
    /// </summary>
    [HttpPut("users/{userName}")]
    public async Task<IActionResult> UpdateUser(
        string userName,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        string currentUserName = GetCurrentUserName();
        var user = await _systemSettingService.UpdateUserAsync(userName, request, currentUserName, cancellationToken);
        return OkResponse(user, "Cập nhật thông tin tài khoản thành công.");
    }

    /// <summary>
    /// Khóa hoặc Mở khóa tài khoản nhân sự có chốt chặn chống tự khóa (UC 7.1.3).
    /// </summary>
    [HttpPost("users/{userName}/status")]
    public async Task<IActionResult> SetUserStatus(
        string userName,
        [FromBody] SetUserStatusApiRequest request,
        CancellationToken cancellationToken = default)
    {
        string currentUserName = GetCurrentUserName();
        var user = await _systemSettingService.SetUserStatusAsync(userName, request.Status, currentUserName, cancellationToken);
        return OkResponse(user, "Cập nhật trạng thái tài khoản thành công.");
    }

    // =========================================================================
    // 4. ACTION METHODS — QUẢN LÝ DANH MỤC GÓI TẬP (UC 7.2)
    // =========================================================================

    /// <summary>
    /// Lấy danh sách các gói tập trong danh mục kinh doanh (UC 7.2).
    /// </summary>
    [HttpGet("packages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackagePlans(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var packages = await _systemSettingService.GetPackagePlansAsync(includeInactive, cancellationToken);
        return OkResponse(packages, "Lấy danh mục gói tập thành công.");
    }

    /// <summary>
    /// Lấy thông tin chi tiết một gói tập theo ID (UC 7.2).
    /// </summary>
    [HttpGet("packages/{planId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackagePlanById(
        string planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _systemSettingService.GetPackagePlanByIdAsync(planId, cancellationToken);
        return OkResponse(plan, "Lấy thông tin gói tập thành công.");
    }

    /// <summary>
    /// Thêm mới gói tập vào danh mục kinh doanh (UC 7.2.1).
    /// </summary>
    [HttpPost("packages")]
    public async Task<IActionResult> CreatePackagePlan(
        [FromBody] CreatePackagePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _systemSettingService.CreatePackagePlanAsync(request, cancellationToken);
        return CreatedResponse($"/api/v1/admin/packages/{plan.PlanId}", plan, "Thêm gói tập mới thành công.");
    }

    /// <summary>
    /// Chỉnh sửa thông số gói tập hiện có (UC 7.2.2).
    /// </summary>
    [HttpPut("packages/{planId}")]
    public async Task<IActionResult> UpdatePackagePlan(
        string planId,
        [FromBody] UpdatePackagePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _systemSettingService.UpdatePackagePlanAsync(planId, request, cancellationToken);
        return OkResponse(plan, "Cập nhật gói tập thành công.");
    }

    /// <summary>
    /// Xóa hoặc ngừng kinh doanh gói tập có kiểm tra ràng buộc toàn vẹn (UC 7.2.3).
    /// </summary>
    [HttpDelete("packages/{planId}")]
    public async Task<IActionResult> DeletePackagePlan(
        string planId,
        CancellationToken cancellationToken = default)
    {
        var result = await _systemSettingService.DeletePackagePlanAsync(planId, cancellationToken);
        return OkResponse(result, result.Message);
    }
}

public record SetUserStatusApiRequest(string Status);
