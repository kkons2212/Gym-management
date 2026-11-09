using System.Security.Claims;
using GymManagement.Server.Core.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Server.Api.Controllers;

/// <summary>
/// Lớp Controller cơ sở (Base Controller) định nghĩa các phương thức chuẩn hóa phản hồi ApiResponse.
/// Cung cấp các tiện ích truy xuất thông tin phiên xác thực từ Claims của người dùng.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    // =========================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // =========================================================================

    private const string DefaultSuccessMessage = "Thao tác thành công.";

    // =========================================================================
    // 2. PROTECTED HELPER METHODS (PHẢN HỒI CHUẨN)
    // =========================================================================

    /// <summary>
    /// Trả về kết quả thành công HTTP 200 kèm cấu trúc ApiResponse.
    /// </summary>
    protected IActionResult OkResponse<T>(T data, string? message = null)
    {
        string finalMessage = DefaultSuccessMessage;
        if (!string.IsNullOrWhiteSpace(message))
        {
            finalMessage = message;
        }

        return Ok(new ApiResponse<T>(true, data, null, finalMessage));
    }

    /// <summary>
    /// Trả về kết quả tạo mới thành công HTTP 201 kèm cấu trúc ApiResponse.
    /// </summary>
    protected IActionResult CreatedResponse<T>(string uri, T data, string? message = null)
    {
        string finalMessage = DefaultSuccessMessage;
        if (!string.IsNullOrWhiteSpace(message))
        {
            finalMessage = message;
        }

        return Created(uri, new ApiResponse<T>(true, data, null, finalMessage));
    }

    // =========================================================================
    // 3. PROTECTED CLAIMS HELPERS
    // =========================================================================

    /// <summary>
    /// Lấy định danh tên đăng nhập (UserName) từ Claims của phiên đăng nhập hiện tại.
    /// </summary>
    protected string GetCurrentUserName()
    {
        Claim? nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdentifierClaim != null && !string.IsNullOrWhiteSpace(nameIdentifierClaim.Value))
        {
            return nameIdentifierClaim.Value;
        }

        Claim? subClaim = User.FindFirst("sub");
        if (subClaim != null && !string.IsNullOrWhiteSpace(subClaim.Value))
        {
            return subClaim.Value;
        }

        Claim? nameClaim = User.FindFirst(ClaimTypes.Name);
        if (nameClaim != null && !string.IsNullOrWhiteSpace(nameClaim.Value))
        {
            return nameClaim.Value;
        }

        return string.Empty;
    }

    /// <summary>
    /// Lấy vai trò (Role) của người dùng từ Claims hiện tại.
    /// </summary>
    protected string GetCurrentUserRole()
    {
        Claim? roleClaim = User.FindFirst(ClaimTypes.Role);
        if (roleClaim != null && !string.IsNullOrWhiteSpace(roleClaim.Value))
        {
            return roleClaim.Value;
        }

        Claim? shortRoleClaim = User.FindFirst("role");
        if (shortRoleClaim != null && !string.IsNullOrWhiteSpace(shortRoleClaim.Value))
        {
            return shortRoleClaim.Value;
        }

        return string.Empty;
    }
}
