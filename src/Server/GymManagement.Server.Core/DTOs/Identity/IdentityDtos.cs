namespace GymManagement.Server.Core.DTOs.Identity;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string Token,
    string RefreshToken,
    string UserId,
    string FullName,
    string Role
);

public record UserSessionDto(
    string UserId,
    string FullName,
    string Email,
    string Role,
    string? Phone = null
);

