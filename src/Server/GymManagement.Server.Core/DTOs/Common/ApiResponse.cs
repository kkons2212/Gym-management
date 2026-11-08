namespace GymManagement.Server.Core.DTOs.Common;

public record ApiResponse<T>(
    bool Success,
    T? Data = default,
    string? ErrorCode = null,
    string? Message = null
);

