namespace GymManagement.Server.Core.DTOs.Common;

public sealed record ApiErrorResponse(
    bool Success,
    object? Data,
    string? ErrorCode,
    string? Message,
    IReadOnlyDictionary<string, string[]>? Errors = null);
