namespace GymManagement.Server.Core.DTOs.Settings;

/// <summary>
/// DTO yêu cầu tạo mới gói tập vào danh mục kinh doanh (UC 7.2.1).
/// </summary>
public record CreatePackagePlanRequest(
    string PlanId,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    int? MaxCheckInSessions = null,
    bool IsPtIncluded = false,
    int? TotalPtSessions = null
);

/// <summary>
/// DTO yêu cầu chỉnh sửa thông tin gói tập hiện có (UC 7.2.2).
/// </summary>
public record UpdatePackagePlanRequest(
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    int? MaxCheckInSessions = null,
    bool IsPtIncluded = false,
    int? TotalPtSessions = null
);

/// <summary>
/// DTO kết quả xử lý xóa hoặc chuyển trạng thái ngừng kinh doanh gói tập (UC 7.2.3).
/// </summary>
public record DeletePackageResultDto(
    string PlanId,
    bool IsHardDeleted,
    bool IsDiscontinued,
    string Message
);

