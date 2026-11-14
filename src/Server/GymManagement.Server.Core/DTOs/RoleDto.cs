namespace GymManagement.Server.Core.DTOs;

/// <summary>
/// DTO cho thực thể Role dùng để trả về danh sách vai trò từ API.
/// Được sử dụng để populate role dropdown trong Desktop UI.
/// </summary>
public record RoleDto(
    string RoleId,
    string RoleName);
