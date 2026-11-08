namespace GymManagement.Server.Core.DTOs.Settings;

/// <summary>
/// DTO yêu cầu tạo mới tài khoản nhân viên / người dùng (UC 7.1.1).
/// </summary>
public record CreateUserRequest(
    string UserName,
    string RoleId,
    string FullName,
    string Phone,
    string Email,
    string Password,
    byte? Gender = null
);

/// <summary>
/// DTO yêu cầu cập nhật thông tin và điều chỉnh phân quyền tài khoản (UC 7.1.2).
/// </summary>
public record UpdateUserRequest(
    string FullName,
    string Phone,
    string Email,
    string RoleId,
    byte? Gender = null
);

/// <summary>
/// DTO yêu cầu thay đổi trạng thái tài khoản (UC 7.1.3: Khóa / Mở khóa).
/// </summary>
public record SetUserStatusRequest(
    string Status
);

/// <summary>
/// DTO thông tin tài khoản người dùng trong hệ thống.
/// </summary>
public record UserDto(
    string UserName,
    string RoleId,
    string? RoleName,
    string? FullName,
    string? Phone,
    string? Email,
    byte? Gender,
    string? Status,
    DateOnly? CreatedAt
);

