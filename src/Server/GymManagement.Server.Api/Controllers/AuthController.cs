using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.DTOs.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Controller xử lý xác thực tài khoản và phiên làm việc (UC01).
/// Cung cấp cổng đăng nhập, cấp phát JWT token và tra cứu phiên người dùng.
/// </summary>
public sealed class AuthController : ApiControllerBase
{
    // =========================================================================
    // 1. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    // =========================================================================
    // 2. CONSTRUCTOR
    // =========================================================================

    public AuthController(
        IAuthService _authService,
        ILogger<AuthController> logger)
    {
        ArgumentNullException.ThrowIfNull(_authService);
        ArgumentNullException.ThrowIfNull(logger);

        this._authService = _authService;
        _logger = logger;
    }

    // =========================================================================
    // 3. ACTION METHODS (ENDPOINTS)
    // =========================================================================

    /// <summary>
    /// Đăng nhập hệ thống bằng Tên đăng nhập / Email và Mật khẩu (UC01).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AuthController: Nhan yeu cau dang nhap tu nguoi dung {Username}", request.Username);

        LoginResponse response = await _authService.LoginAsync(request, cancellationToken);
        return OkResponse(response, "Đăng nhập thành công.");
    }

    /// <summary>
    /// Tra cứu thông tin tài khoản phiên làm việc hiện tại từ JWT Token (UC01).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        string authHeader = Request.Headers.Authorization.ToString();
        string token = string.Empty;

        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring(7).Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { Success = false, Message = "Token xác thực không hợp lệ." });
        }

        UserSessionDto session = await _authService.ValidateTokenAsync(token, cancellationToken);
        return OkResponse(session, "Lấy thông tin phiên làm việc thành công.");
    }
}
