using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Settings;

namespace GymManagement.Server.Core.Contracts;

/// <summary>
/// Hợp đồng dịch vụ quản trị hệ thống (SystemSettingService Component - Task 2.6).
/// Phụ trách các Use Cases thuộc Epic 02 (UC07):
/// - UC 7.1 (US 07.1, 07.2, 07.3): Quản trị tài khoản nhân sự và phân quyền (Staff RBAC).
/// - UC 7.2 (US 07.4, 07.5, 07.6): Quản lý danh mục gói tập phòng gym (Package Catalog).
/// </summary>
public interface ISystemSettingService
{
    // =========================================================================
    // NHÓM 1: QUẢN TRỊ TÀI KHOẢN VÀ PHÂN QUYỀN (UC 7.1.1, UC 7.1.2, UC 7.1.3)
    // =========================================================================

    /// <summary>
    /// Tạo mới tài khoản và phân quyền nhân sự (UC 7.1.1).
    /// Chỉ SuperAdmin mới có quyền tạo tài khoản có vai trò Quản trị viên (Admin/SuperAdmin).
    /// </summary>
    Task<UserDto> CreateUserAsync(CreateUserRequest request, string? currentUserName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin tài khoản và điều chỉnh phân quyền nhân sự (UC 7.1.2).
    /// Có chốt chặn an toàn: Ngăn Super Admin tự tước quyền Quản trị của chính mình.
    /// </summary>
    Task<UserDto> UpdateUserAsync(string userName, UpdateUserRequest request, string currentUserName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Khóa hoặc Mở khóa tài khoản nhân sự (UC 7.1.3).
    /// Có chốt chặn an toàn: Ngăn Quản trị viên tự khóa tài khoản đang đăng nhập.
    /// </summary>
    Task<UserDto> SetUserStatusAsync(string userName, string status, string currentUserName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tra cứu thông tin tài khoản theo tên đăng nhập (UserName).
    /// </summary>
    Task<UserDto> GetUserByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tài khoản theo bộ lọc vai trò và trạng thái hoạt động.
    /// </summary>
    Task<IReadOnlyList<UserDto>> GetUsersAsync(string? roleId = null, string? status = null, CancellationToken cancellationToken = default);

    // =========================================================================
    // NHÓM 2: QUẢN LÝ DANH MỤC GÓI TẬP (UC 7.2.1, UC 7.2.2, UC 7.2.3)
    // =========================================================================

    /// <summary>
    /// Thêm mới gói tập vào danh mục kinh doanh (UC 7.2.1).
    /// </summary>
    Task<PackagePlanDto> CreatePackagePlanAsync(CreatePackagePlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chỉnh sửa thông số gói tập hiện có (UC 7.2.2).
    /// </summary>
    Task<PackagePlanDto> UpdatePackagePlanAsync(string planId, UpdatePackagePlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa hoặc Chuyển trạng thái ngừng kinh doanh gói tập (UC 7.2.3).
    /// Nếu gói tập đã có hội viên đăng ký hoặc có hóa đơn: Ngăn xóa vật lý, chuyển sang DISCONTINUED.
    /// Nếu chưa từng có đăng ký: Thực hiện xóa vật lý (Hard delete).
    /// </summary>
    Task<DeletePackageResultDto> DeletePackagePlanAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một gói tập theo PlanId.
    /// </summary>
    Task<PackagePlanDto> GetPackagePlanByIdAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các gói tập trong danh mục (mặc định chỉ lấy các gói đang hoạt động).
    /// </summary>
    Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
}
