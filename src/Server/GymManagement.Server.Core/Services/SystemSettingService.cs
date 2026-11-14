using System.Security.Cryptography;
using System.Text;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.Settings;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// SystemSettingService Component — Kết nối Authenticated + Database API (Task 2.6).
/// Quản trị tài khoản nhân sự (Staff RBAC) và danh mục gói tập phòng gym (Package Catalog):
///   UC07 (US 07.1, 07.2, 07.3): Quản trị tài khoản, phân quyền, chốt chặn tự khóa / tự tước quyền.
///   UC07 (US 07.4, 07.5, 07.6): Quản lý danh mục gói tập, kiểm tra toàn vẹn trước khi ngừng kinh doanh.
/// </summary>
public sealed class SystemSettingService : ISystemSettingService
{
    // =========================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // =========================================================================

    private const int MaxUserNameLength = 10;
    private const int MaxPlanIdLength = 10;

    private const string RoleIdAdmin = "ADMIN";
    private const string StatusActive = "ACTIVE";
    private const string StatusLocked = "LOCKED";
    private const string StatusInactive = "INACTIVE";
    private const string StatusDiscontinued = "DISCONTINUED";

    // =========================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SystemSettingService> _logger;

    // =========================================================================
    // 3. CONSTRUCTOR
    // =========================================================================

    public SystemSettingService(
        IUnitOfWork unitOfWork,
        ILogger<SystemSettingService> logger)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(logger);

        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // =========================================================================
    // 4. PUBLIC METHODS — NHÓM 1: QUẢN TRỊ TÀI KHOẢN & PHÂN QUYỀN (UC 7.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<UserDto> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        string cleanUserName = request.UserName.Trim();
        if (cleanUserName.Length > MaxUserNameLength)
        {
            throw new ValidationException($"Tên đăng nhập không được vượt quá {MaxUserNameLength} ký tự.");
        }

        if (string.IsNullOrWhiteSpace(request.RoleId))
        {
            throw new ValidationException("Vai trò không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ValidationException("Họ và tên không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ValidationException("Số điện thoại không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Mật khẩu không được để trống.");
        }

        string cleanRoleId = request.RoleId.Trim().ToUpperInvariant();
        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(cleanRoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Vai trò '{cleanRoleId}' không tồn tại trong hệ thống.");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var existingUser = await userRepo.GetByIdAsync(cleanUserName, cancellationToken);
        if (existingUser != null)
        {
            throw new BusinessRuleException($"Tên đăng nhập '{cleanUserName}' đã tồn tại trong hệ thống.");
        }

        string cleanPhone = request.Phone.Trim();
        var duplicatePhone = await userRepo.FindSingleOrDefaultAsync(
            u => u.Phone == cleanPhone,
            cancellationToken);

        if (duplicatePhone != null)
        {
            throw new BusinessRuleException("Số điện thoại đã được sử dụng bởi tài khoản khác.");
        }

        string cleanEmail = request.Email.Trim();
        var duplicateEmail = await userRepo.FindSingleOrDefaultAsync(
            u => u.Email == cleanEmail,
            cancellationToken);

        if (duplicateEmail != null)
        {
            throw new BusinessRuleException("Email đã được sử dụng bởi tài khoản khác.");
        }

        var newUser = new User();
        newUser.UserName = cleanUserName;
        newUser.RoleId = cleanRoleId;
        newUser.FullName = request.FullName.Trim();
        newUser.Phone = cleanPhone;
        newUser.Email = cleanEmail;
        newUser.PasswordHash = HashPassword(request.Password);
        newUser.Gender = request.Gender;
        newUser.Status = StatusActive;
        newUser.CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

        await userRepo.AddAsync(newUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tạo mới tài khoản người dùng thành công: UserName={UserName}, Role={Role}",
            newUser.UserName,
            cleanRoleId);

        return MapToUserDto(newUser, role.RoleName);
    }

    /// <inheritdoc/>
    public async Task<UserDto> UpdateUserAsync(
        string userName,
        UpdateUserRequest request,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            throw new ValidationException("Tài khoản đang đăng nhập không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ValidationException("Họ và tên không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ValidationException("Số điện thoại không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.RoleId))
        {
            throw new ValidationException("Vai trò không được để trống.");
        }

        string cleanUserName = userName.Trim();
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(cleanUserName, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"Không tìm thấy tài khoản '{cleanUserName}'.");
        }

        string newRoleId = request.RoleId.Trim().ToUpperInvariant();

        // Chốt chặn an toàn: Super Admin không thể tự tước quyền Quản trị của chính mình
        bool isSelf = string.Equals(cleanUserName, currentUserName.Trim(), StringComparison.OrdinalIgnoreCase);
        if (isSelf)
        {
            bool wasAdmin = string.Equals(user.RoleId, RoleIdAdmin, StringComparison.OrdinalIgnoreCase);
            bool isStillAdmin = string.Equals(newRoleId, RoleIdAdmin, StringComparison.OrdinalIgnoreCase);
            if (wasAdmin && !isStillAdmin)
            {
                throw new BusinessRuleException("Không thể tự thay đổi quyền Quản trị của tài khoản đang đăng nhập.");
            }
        }

        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(newRoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Vai trò '{newRoleId}' không tồn tại trong hệ thống.");
        }

        string cleanPhone = request.Phone.Trim();
        var duplicatePhone = await userRepo.FindSingleOrDefaultAsync(
            u => u.Phone == cleanPhone && u.UserName != cleanUserName,
            cancellationToken);

        if (duplicatePhone != null)
        {
            throw new BusinessRuleException("Số điện thoại đã được sử dụng bởi tài khoản khác.");
        }

        string cleanEmail = request.Email.Trim();
        var duplicateEmail = await userRepo.FindSingleOrDefaultAsync(
            u => u.Email == cleanEmail && u.UserName != cleanUserName,
            cancellationToken);

        if (duplicateEmail != null)
        {
            throw new BusinessRuleException("Email đã được sử dụng bởi tài khoản khác.");
        }

        user.FullName = request.FullName.Trim();
        user.Phone = cleanPhone;
        user.Email = cleanEmail;
        user.RoleId = newRoleId;
        user.Gender = request.Gender;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cập nhật tài khoản thành công: UserName={UserName}, Role={Role}",
            user.UserName,
            newRoleId);

        return MapToUserDto(user, role.RoleName);
    }

    /// <inheritdoc/>
    public async Task<UserDto> SetUserStatusAsync(
        string userName,
        string status,
        string currentUserName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ValidationException("Trạng thái không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            throw new ValidationException("Tài khoản đang đăng nhập không hợp lệ.");
        }

        string cleanUserName = userName.Trim();
        string cleanStatus = status.Trim().ToUpperInvariant();

        if (cleanStatus != StatusActive && cleanStatus != StatusLocked && cleanStatus != StatusInactive)
        {
            throw new ValidationException($"Trạng thái '{cleanStatus}' không hợp lệ.");
        }

        // Chốt chặn an toàn: Ngăn Admin tự khóa hoặc vô hiệu hóa tài khoản của chính mình
        bool isSelf = string.Equals(cleanUserName, currentUserName.Trim(), StringComparison.OrdinalIgnoreCase);
        if (isSelf)
        {
            if (cleanStatus == StatusLocked || cleanStatus == StatusInactive)
            {
                throw new BusinessRuleException("Không thể tự khóa hoặc vô hiệu hóa tài khoản đang đăng nhập.");
            }
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(cleanUserName, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"Không tìm thấy tài khoản '{cleanUserName}'.");
        }

        user.Status = cleanStatus;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(user.RoleId, cancellationToken);
        string? roleName = null;
        if (role != null)
        {
            roleName = role.RoleName;
        }

        _logger.LogInformation(
            "Cập nhật trạng thái tài khoản: UserName={UserName}, Status={Status}",
            user.UserName,
            cleanStatus);

        return MapToUserDto(user, roleName);
    }

    /// <inheritdoc/>
    public async Task<UserDto> GetUserByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        string cleanUserName = userName.Trim();
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(cleanUserName, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"Không tìm thấy tài khoản '{cleanUserName}'.");
        }

        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(user.RoleId, cancellationToken);
        string? roleName = null;
        if (role != null)
        {
            roleName = role.RoleName;
        }

        return MapToUserDto(user, roleName);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(
        string? roleId = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var allUsers = await userRepo.GetAllAsync(cancellationToken);

        var roleRepo = _unitOfWork.Repository<Role>();
        var allRoles = await roleRepo.GetAllAsync(cancellationToken);
        var roleDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in allRoles)
        {
            if (!string.IsNullOrWhiteSpace(r.RoleID))
            {
                roleDict[r.RoleID] = r.RoleName ?? string.Empty;
            }
        }

        var results = new List<UserDto>();
        foreach (var u in allUsers)
        {
            bool matchRole = true;
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                matchRole = string.Equals(u.RoleId, roleId.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            bool matchStatus = true;
            if (!string.IsNullOrWhiteSpace(status))
            {
                matchStatus = string.Equals(u.Status, status.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            if (matchRole && matchStatus)
            {
                string? roleName = null;
                if (roleDict.TryGetValue(u.RoleId, out var foundName))
                {
                    roleName = foundName;
                }

                results.Add(MapToUserDto(u, roleName));
            }
        }

        return results;
    }

    // =========================================================================
    // 5. PUBLIC METHODS — NHÓM 2: QUẢN LÝ DANH MỤC GÓI TẬP (UC 7.2)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<PackagePlanDto> CreatePackagePlanAsync(
        CreatePackagePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PlanId))
        {
            throw new ValidationException("Mã gói tập không được để trống.");
        }

        string cleanPlanId = request.PlanId.Trim().ToUpperInvariant();
        if (cleanPlanId.Length > MaxPlanIdLength)
        {
            throw new ValidationException($"Mã gói tập không được vượt quá {MaxPlanIdLength} ký tự.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Tên gói tập không được để trống.");
        }

        if (request.Price <= 0)
        {
            throw new ValidationException("Đơn giá gói tập phải lớn hơn 0.");
        }

        if (request.DurationDays <= 0)
        {
            throw new ValidationException("Thời hạn gói tập phải lớn hơn 0 ngày.");
        }

        var packageRepo = _unitOfWork.Repository<PackagePlan>();
        var existingPlan = await packageRepo.GetByIdAsync(cleanPlanId, cancellationToken);
        if (existingPlan != null)
        {
            throw new BusinessRuleException($"Mã gói tập '{cleanPlanId}' đã tồn tại trong hệ thống.");
        }

        string cleanName = request.Name.Trim();
        var duplicateName = await packageRepo.FindSingleOrDefaultAsync(
            p => p.Name == cleanName && p.IsActive == 1,
            cancellationToken);

        if (duplicateName != null)
        {
            throw new BusinessRuleException($"Tên gói tập '{cleanName}' đang trùng với gói tập đang kinh doanh.");
        }

        byte ptIncludedByte = 0;
        if (request.IsPtIncluded)
        {
            ptIncludedByte = 1;
        }

        var newPlan = new PackagePlan();
        newPlan.PlanId = cleanPlanId;
        newPlan.Name = cleanName;
        newPlan.Description = request.Description;
        newPlan.Price = request.Price;
        newPlan.DurationDays = request.DurationDays;
        newPlan.MaxCheckInSessions = request.MaxCheckInSessions;
        newPlan.IsPtIncluded = ptIncludedByte;
        newPlan.TotalPtSessions = request.TotalPtSessions;
        newPlan.IsActive = 1;

        await packageRepo.AddAsync(newPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tạo mới gói tập thành công: PlanId={PlanId}, Name={Name}, Price={Price}",
            newPlan.PlanId,
            newPlan.Name,
            newPlan.Price);

        return MapToPackagePlanDto(newPlan);
    }

    /// <inheritdoc/>
    public async Task<PackagePlanDto> UpdatePackagePlanAsync(
        string planId,
        UpdatePackagePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(planId))
        {
            throw new ValidationException("Mã gói tập không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Tên gói tập không được để trống.");
        }

        if (request.Price <= 0)
        {
            throw new ValidationException("Đơn giá gói tập phải lớn hơn 0.");
        }

        if (request.DurationDays <= 0)
        {
            throw new ValidationException("Thời hạn gói tập phải lớn hơn 0 ngày.");
        }

        string cleanPlanId = planId.Trim().ToUpperInvariant();
        var packageRepo = _unitOfWork.Repository<PackagePlan>();
        var plan = await packageRepo.GetByIdAsync(cleanPlanId, cancellationToken);
        if (plan == null)
        {
            throw new NotFoundException($"Không tìm thấy gói tập '{cleanPlanId}'.");
        }

        string cleanName = request.Name.Trim();
        var duplicateName = await packageRepo.FindSingleOrDefaultAsync(
            p => p.Name == cleanName && p.PlanId != cleanPlanId && p.IsActive == 1,
            cancellationToken);

        if (duplicateName != null)
        {
            throw new BusinessRuleException($"Tên gói tập '{cleanName}' đang trùng với gói tập đang kinh doanh khác.");
        }

        byte ptIncludedByte = 0;
        if (request.IsPtIncluded)
        {
            ptIncludedByte = 1;
        }

        plan.Name = cleanName;
        plan.Description = request.Description;
        plan.Price = request.Price;
        plan.DurationDays = request.DurationDays;
        plan.MaxCheckInSessions = request.MaxCheckInSessions;
        plan.IsPtIncluded = ptIncludedByte;
        plan.TotalPtSessions = request.TotalPtSessions;

        packageRepo.Update(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cập nhật gói tập thành công: PlanId={PlanId}, Name={Name}",
            plan.PlanId,
            plan.Name);

        return MapToPackagePlanDto(plan);
    }

    /// <inheritdoc/>
    public async Task<DeletePackageResultDto> DeletePackagePlanAsync(
        string planId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(planId))
        {
            throw new ValidationException("Mã gói tập không được để trống.");
        }

        string cleanPlanId = planId.Trim().ToUpperInvariant();
        var packageRepo = _unitOfWork.Repository<PackagePlan>();
        var plan = await packageRepo.GetByIdAsync(cleanPlanId, cancellationToken);
        if (plan == null)
        {
            throw new NotFoundException($"Không tìm thấy gói tập '{cleanPlanId}'.");
        }

        // Ràng buộc toàn vẹn: Kiểm tra xem gói tập đã có hội viên đăng ký hay chưa
        var subscriptionRepo = _unitOfWork.Repository<MemberSubscription>();
        bool hasSubscriptions = await subscriptionRepo.ExistsAsync(
            s => s.PlanId == cleanPlanId,
            cancellationToken);

        if (hasSubscriptions)
        {
            // Không xóa vật lý — chuyển sang ngừng kinh doanh (DISCONTINUED)
            plan.IsActive = 0;
            packageRepo.Update(plan);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Gói tập {PlanId} đã có hội viên sử dụng. Đã chuyển sang trạng thái Ngừng kinh doanh thay vì xóa vật lý.",
                cleanPlanId);

            return new DeletePackageResultDto(
                cleanPlanId,
                false,
                true,
                "Gói tập đã có hội viên đăng ký. Hệ thống đã tự động chuyển sang trạng thái Ngừng kinh doanh (DISCONTINUED) để bảo toàn quyền lợi của hội viên.");
        }

        // Gói tập chưa từng phát sinh đăng ký: Thực hiện xóa vật lý (Hard delete)
        packageRepo.Remove(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Đã xóa hoàn toàn gói tập chưa từng sử dụng: PlanId={PlanId}",
            cleanPlanId);

        return new DeletePackageResultDto(
            cleanPlanId,
            true,
            false,
            "Đã xóa hoàn toàn gói tập khỏi hệ thống.");
    }

    /// <inheritdoc/>
    public async Task<PackagePlanDto> GetPackagePlanByIdAsync(
        string planId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(planId))
        {
            throw new ValidationException("Mã gói tập không được để trống.");
        }

        string cleanPlanId = planId.Trim().ToUpperInvariant();
        var packageRepo = _unitOfWork.Repository<PackagePlan>();
        var plan = await packageRepo.GetByIdAsync(cleanPlanId, cancellationToken);
        if (plan == null)
        {
            throw new NotFoundException($"Không tìm thấy gói tập '{cleanPlanId}'.");
        }

        return MapToPackagePlanDto(plan);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var roles = await roleRepo.GetAllAsync(cancellationToken);

        var results = roles
            .Select(r => new RoleDto(r.RoleID, r.RoleName ?? string.Empty))
            .ToList();

        return results.AsReadOnly();
    }

    public async Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var packageRepo = _unitOfWork.Repository<PackagePlan>();

        IReadOnlyList<PackagePlan> plans;
        if (includeInactive)
        {
            plans = await packageRepo.GetAllAsync(cancellationToken);
        }
        else
        {
            plans = await packageRepo.FindAsync(p => p.IsActive == 1, cancellationToken);
        }

        var results = new List<PackagePlanDto>();
        foreach (var p in plans)
        {
            results.Add(MapToPackagePlanDto(p));
        }

        return results;
    }

    // =========================================================================
    // 6. PRIVATE HELPER METHODS
    // =========================================================================

    private static UserDto MapToUserDto(User user, string? roleName)
    {
        return new UserDto(
            user.UserName,
            user.RoleId,
            roleName,
            user.FullName,
            user.Phone,
            user.Email,
            user.Gender,
            user.Status,
            user.CreatedAt);
    }

    private static PackagePlanDto MapToPackagePlanDto(PackagePlan plan)
    {
        bool isPt = false;
        if (plan.IsPtIncluded.HasValue)
        {
            if (plan.IsPtIncluded.Value == 1)
            {
                isPt = true;
            }
        }

        string status = StatusDiscontinued;
        if (plan.IsActive.HasValue)
        {
            if (plan.IsActive.Value == 1)
            {
                status = StatusActive;
            }
        }

        decimal price = 0;
        if (plan.Price.HasValue)
        {
            price = plan.Price.Value;
        }

        int durationDays = 0;
        if (plan.DurationDays.HasValue)
        {
            durationDays = plan.DurationDays.Value;
        }

        return new PackagePlanDto(
            plan.PlanId,
            plan.Name ?? string.Empty,
            plan.Description,
            price,
            durationDays,
            plan.MaxCheckInSessions,
            plan.TotalPtSessions,
            isPt,
            status);
    }

    private static string HashPassword(string password)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
