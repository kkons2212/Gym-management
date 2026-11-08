using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.Enums;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// MemberService Component — Kết nối Authenticated + Database API + SMS Gateway.
/// Triển khai toàn bộ nghiệp vụ quản lý hội viên và vòng đời gói tập:
///   UC02: Admin đăng ký, cập nhật, gia hạn, nâng cấp, bảo lưu, hủy gói tập.
///   UC09: Hội viên xem thông tin cá nhân và lịch sử tập luyện.
/// </summary>
public sealed class MemberService : IMemberService
{
    // =========================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // =========================================================================

    /// <summary>RoleID dùng để nhận diện tài khoản hội viên trong bảng Role.</summary>
    private const string RoleIdMember = "MEMBER";

    /// <summary>Trạng thái tài khoản / gói tập đang hoạt động.</summary>
    private const string StatusActive = "ACTIVE";

    /// <summary>Trạng thái gói tập đang bảo lưu.</summary>
    private const string StatusFrozen = "FROZEN";

    /// <summary>Trạng thái gói tập đã hủy.</summary>
    private const string StatusCancelled = "CANCELLED";

    /// <summary>Tiền tố ID hội viên (UserName).</summary>
    private const string MemberIdPrefix = "MBR";

    /// <summary>Tiền tố SubscriptionId.</summary>
    private const string SubscriptionIdPrefix = "SUB";

    // =========================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationAdapter _notification;
    private readonly ILogger<MemberService> _logger;

    // =========================================================================
    // 3. CONSTRUCTOR
    // =========================================================================

    public MemberService(
        IUnitOfWork unitOfWork,
        INotificationAdapter notification,
        ILogger<MemberService> logger)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(logger);

        _unitOfWork = unitOfWork;
        _notification = notification;
        _logger = logger;
    }

    // =========================================================================
    // 4. PUBLIC METHODS — NHÓM 1: HỒ SƠ HỘI VIÊN (UC02.1, UC02.2, UC09.1)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MemberSearchResponse>> SearchMembersAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<MemberUser>();
        var subRepo = _unitOfWork.Repository<MemberSubscriptionStub>();

        var users = await userRepo.FindAsync(
            u => u.RoleId == RoleIdMember &&
                 (keyword == null ||
                  (u.FullName != null && u.FullName.Contains(keyword)) ||
                  (u.Phone != null && u.Phone.Contains(keyword)) ||
                  u.UserName.Contains(keyword)),
            cancellationToken);

        var result = new List<MemberSearchResponse>(users.Count);
        foreach (var u in users)
        {
            var activeSub = await subRepo.FindSingleOrDefaultAsync(
                s => s.UserName == u.UserName && s.Status == StatusActive,
                cancellationToken);

            result.Add(new MemberSearchResponse(
                UserName: u.UserName,
                FullName: u.FullName ?? string.Empty,
                Phone: u.Phone ?? string.Empty,
                Email: u.Email ?? string.Empty,
                Status: u.Status ?? string.Empty,
                ActiveSubscriptionId: activeSub?.SubscriptionId,
                ActivePlanName: activeSub?.PlanId,
                SubscriptionEndDate: activeSub?.EndDate,
                RemainingCheckInSessions: activeSub?.RemainingCheckInSessions ?? 0,
                RemainingPtSessions: activeSub?.RemainingPtSessions ?? 0));
        }

        _logger.LogInformation(
            "MemberService.SearchMembers: Tìm thấy {Count} hội viên (keyword={Keyword})",
            result.Count, keyword);

        return result;
    }

    /// <inheritdoc/>
    public async Task<MemberProfileResponse> GetMemberProfileAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userName);

        var user = await _unitOfWork.Repository<MemberUser>()
            .FindSingleOrDefaultAsync(
                u => u.UserName == userName && u.RoleId == RoleIdMember,
                cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy hội viên với mã: {userName}");

        var activeSub = await _unitOfWork.Repository<MemberSubscriptionStub>()
            .FindSingleOrDefaultAsync(
                s => s.UserName == userName && s.Status == StatusActive,
                cancellationToken);

        return BuildMemberProfileResponse(user, activeSub);
    }

    /// <inheritdoc/>
    public async Task<MemberProfileResponse> RegisterMemberAsync(
        RegisterMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRegisterMemberRequest(request);

        var userRepo = _unitOfWork.Repository<MemberUser>();

        // Kiểm tra trùng Phone (UC02.1 — phụ 5.2)
        bool phoneExists = await userRepo.ExistsAsync(
            u => u.Phone == request.Phone,
            cancellationToken);

        if (phoneExists)
            throw new ValidationException("Số điện thoại đã được sử dụng bởi hội viên khác.");

        // Kiểm tra gói tập tồn tại và đang hoạt động
        var plan = await _unitOfWork.Repository<PackagePlanStub>()
            .FindSingleOrDefaultAsync(
                p => p.PlanId == request.PlanId && p.IsActive == 1,
                cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy gói tập với mã: {request.PlanId}");

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // Sinh UserName mới theo định dạng MBR-XXXX
            int memberCount = await userRepo.CountAsync(
                u => u.RoleId == RoleIdMember, cancellationToken);
            string newUserName = $"{MemberIdPrefix}-{memberCount + 1:D4}";

            // Tạo tài khoản hội viên trong bảng [User]
            var newUser = new MemberUser
            {
                UserName = newUserName,
                RoleId = RoleIdMember,
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email,
                Status = StatusActive,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            await userRepo.AddAsync(newUser, cancellationToken);

            // Sinh SubscriptionId và QrCode duy nhất
            int subCount = await _unitOfWork.Repository<MemberSubscriptionStub>()
                .CountAsync(cancellationToken: cancellationToken);
            string newSubId = $"{SubscriptionIdPrefix}-{subCount + 1:D5}";
            string qrCode = GenerateQrCode(newUserName);

            // Tính ngày hết hạn = StartDate + DurationDays của gói
            var endDate = request.StartDate.AddDays(plan.DurationDays ?? 30);

            // Tạo subscription trong bảng MemberSubscription
            var newSub = new MemberSubscriptionStub
            {
                SubscriptionId = newSubId,
                UserName = newUserName,
                PlanId = request.PlanId,
                QrCode = qrCode,
                StartDate = request.StartDate,
                EndDate = endDate,
                RemainingCheckInSessions = plan.MaxCheckInSessions ?? 0,
                RemainingPtSessions = plan.TotalPtSessions ?? 0,
                Status = StatusActive,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            await _unitOfWork.Repository<MemberSubscriptionStub>().AddAsync(newSub, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MemberService.RegisterMember: Tạo hội viên {UserName} với gói {PlanId}",
                newUserName, request.PlanId);

            // Gửi Email chào mừng kèm QR code (fire-and-forget)
            await SendWelcomeNotificationSafeAsync(
                newUser.Email ?? string.Empty,
                newUser.FullName ?? newUserName,
                qrCode,
                cancellationToken);

            return BuildMemberProfileResponse(newUser, newSub);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MemberProfileResponse> UpdateMemberAsync(
        UpdateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateUpdateMemberRequest(request);

        var userRepo = _unitOfWork.Repository<MemberUser>();

        var user = await userRepo.FindSingleOrDefaultAsync(
            u => u.UserName == request.UserName,
            cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy hội viên với mã: {request.UserName}");

        // Kiểm tra Phone trùng với hội viên khác
        bool phoneConflict = await userRepo.ExistsAsync(
            u => u.Phone == request.Phone && u.UserName != request.UserName,
            cancellationToken);

        if (phoneConflict)
            throw new ValidationException("Số điện thoại đã được sử dụng bởi hội viên khác.");

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.Email = request.Email;
        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "MemberService.UpdateMember: Cập nhật thông tin hội viên {UserName}",
            request.UserName);

        var activeSub = await _unitOfWork.Repository<MemberSubscriptionStub>()
            .FindSingleOrDefaultAsync(
                s => s.UserName == request.UserName && s.Status == StatusActive,
                cancellationToken);

        return BuildMemberProfileResponse(user, activeSub);
    }

    // =========================================================================
    // 5. PUBLIC METHODS — NHÓM 2: GÓI TẬP (UC02.3, UC02.4, UC02.5)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<SubscriptionDetailResponse?> GetActiveSubscriptionAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userName);

        var sub = await _unitOfWork.Repository<MemberSubscriptionStub>()
            .FindSingleOrDefaultAsync(
                s => s.UserName == userName && s.Status == StatusActive,
                cancellationToken);

        return sub == null ? null : MapToSubscriptionDetailResponse(sub);
    }

    /// <inheritdoc/>
    public async Task<SubscriptionDetailResponse> RenewSubscriptionAsync(
        RenewSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var plan = await _unitOfWork.Repository<PackagePlanStub>()
            .FindSingleOrDefaultAsync(
                p => p.PlanId == request.NewPlanId && p.IsActive == 1,
                cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy gói tập với mã: {request.NewPlanId}");

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // Lấy subscription cũ để lấy UserName
            var oldSub = await _unitOfWork.Repository<MemberSubscriptionStub>()
                .FindSingleOrDefaultAsync(
                    s => s.SubscriptionId == request.SubscriptionId,
                    cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy gói tập với mã: {request.SubscriptionId}");

            // Tạo subscription mới với gói được chọn
            int subCount = await _unitOfWork.Repository<MemberSubscriptionStub>()
                .CountAsync(cancellationToken: cancellationToken);
            string newSubId = $"{SubscriptionIdPrefix}-{subCount + 1:D5}";
            string qrCode = GenerateQrCode($"{request.SubscriptionId}-RENEW");

            var endDate = request.NewStartDate.AddDays(plan.DurationDays ?? 30);

            var newSub = new MemberSubscriptionStub
            {
                SubscriptionId = newSubId,
                UserName = oldSub.UserName,
                PlanId = request.NewPlanId,
                QrCode = qrCode,
                StartDate = request.NewStartDate,
                EndDate = endDate,
                RemainingCheckInSessions = plan.MaxCheckInSessions ?? 0,
                RemainingPtSessions = plan.TotalPtSessions ?? 0,
                Status = StatusActive,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            await _unitOfWork.Repository<MemberSubscriptionStub>().AddAsync(newSub, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MemberService.RenewSubscription: Gia hạn gói {OldSub} → {NewSub} (PlanId={PlanId})",
                request.SubscriptionId, newSubId, request.NewPlanId);

            return MapToSubscriptionDetailResponse(newSub);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SubscriptionDetailResponse> UpgradeSubscriptionAsync(
        UpgradeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newPlan = await _unitOfWork.Repository<PackagePlanStub>()
            .FindSingleOrDefaultAsync(
                p => p.PlanId == request.UpgradeToPlanId && p.IsActive == 1,
                cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy gói tập nâng cấp với mã: {request.UpgradeToPlanId}");

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var sub = await _unitOfWork.Repository<MemberSubscriptionStub>()
                .FindSingleOrDefaultAsync(
                    s => s.SubscriptionId == request.SubscriptionId && s.Status == StatusActive,
                    cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy gói tập đang hoạt động: {request.SubscriptionId}");

            // Nâng cấp: giữ nguyên EndDate, đổi PlanId, cộng thêm sessions từ gói mới
            sub.PlanId = request.UpgradeToPlanId;
            sub.RemainingCheckInSessions += newPlan.MaxCheckInSessions ?? 0;
            sub.RemainingPtSessions += newPlan.TotalPtSessions ?? 0;
            _unitOfWork.Repository<MemberSubscriptionStub>().Update(sub);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MemberService.UpgradeSubscription: Nâng cấp gói {SubId} → PlanId={NewPlan}",
                request.SubscriptionId, request.UpgradeToPlanId);

            return MapToSubscriptionDetailResponse(sub);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SubscriptionDetailResponse> FreezeSubscriptionAsync(
        FreezeSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateFreezeRequest(request);

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var sub = await _unitOfWork.Repository<MemberSubscriptionStub>()
                .FindSingleOrDefaultAsync(
                    s => s.SubscriptionId == request.SubscriptionId,
                    cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy gói tập với mã: {request.SubscriptionId}");

            // Chỉ bảo lưu gói đang ACTIVE (UC02.4)
            if (sub.Status != StatusActive)
                throw new BusinessRuleException(
                    $"Chỉ có thể bảo lưu gói tập đang hoạt động. Trạng thái hiện tại: {sub.Status}");

            // Tính số ngày bảo lưu và kéo dài EndDate
            int freezeDays = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
            var newEndDate = sub.EndDate.AddDays(freezeDays);

            sub.Status = StatusFrozen;
            sub.FreezeStartDate = request.FreezeStartDate;
            sub.FreezeEndDate = request.FreezeEndDate;
            sub.EndDate = newEndDate;
            _unitOfWork.Repository<MemberSubscriptionStub>().Update(sub);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MemberService.FreezeSubscription: Bảo lưu gói {SubId} ({FreezeDays} ngày), EndDate mới = {NewEnd}",
                request.SubscriptionId, freezeDays, newEndDate);

            return MapToSubscriptionDetailResponse(sub);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CancelSubscriptionAsync(
        CancelSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Lý do hủy là bắt buộc (UC02.5 — phụ 3.1)
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ValidationException("Lý do hủy gói tập là bắt buộc.");

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var sub = await _unitOfWork.Repository<MemberSubscriptionStub>()
                .FindSingleOrDefaultAsync(
                    s => s.SubscriptionId == request.SubscriptionId,
                    cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy gói tập với mã: {request.SubscriptionId}");

            sub.Status = StatusCancelled;
            _unitOfWork.Repository<MemberSubscriptionStub>().Update(sub);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MemberService.CancelSubscription: Hủy gói {SubId}, lý do: {Reason}",
                request.SubscriptionId, request.Reason);
        }, cancellationToken);
    }

    // =========================================================================
    // 6. PUBLIC METHODS — NHÓM 3: LỊCH SỬ TẬP LUYỆN (UC09.2)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkoutHistoryResponse>> GetWorkoutHistoryAsync(
        string userName,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userName);

        var logs = await _unitOfWork.Repository<CheckInLogStub>()
            .FindAsync(
                c => c.UserName == userName &&
                     (fromDate == null || c.CheckInAt >= fromDate) &&
                     (toDate == null || c.CheckInAt <= toDate),
                cancellationToken);

        return logs
            .OrderByDescending(c => c.CheckInAt)
            .Select(c => new WorkoutHistoryResponse(
                CheckInId: c.CheckInId,
                CheckInDate: c.CheckInAt ?? DateOnly.MinValue,
                Status: c.Status ?? string.Empty,
                DenialReason: c.DenialReason,
                AssignedPtName: null))    // PT name resolved khi có EF Core JOIN
            .ToList();
    }

    // =========================================================================
    // 7. PRIVATE HELPER METHODS — Validation
    // =========================================================================

    private static void ValidateRegisterMemberRequest(RegisterMemberRequest req)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(req.FullName))
            errors.Add("Họ tên không được để trống.");

        if (string.IsNullOrWhiteSpace(req.Phone) || req.Phone.Length < 9)
            errors.Add("Số điện thoại không hợp lệ.");

        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
            errors.Add("Email không hợp lệ.");

        if (string.IsNullOrWhiteSpace(req.PlanId))
            errors.Add("Gói tập không được để trống.");

        if (errors.Count > 0)
            throw new ValidationException(string.Join(" ", errors));
    }

    private static void ValidateUpdateMemberRequest(UpdateMemberRequest req)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(req.FullName))
            errors.Add("Họ tên không được để trống.");

        if (string.IsNullOrWhiteSpace(req.Phone) || req.Phone.Length < 9)
            errors.Add("Số điện thoại không hợp lệ.");

        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
            errors.Add("Email không hợp lệ.");

        if (errors.Count > 0)
            throw new ValidationException(string.Join(" ", errors));
    }

    private static void ValidateFreezeRequest(FreezeSubscriptionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Reason))
            throw new ValidationException("Lý do bảo lưu là bắt buộc.");

        if (req.FreezeEndDate <= req.FreezeStartDate)
            throw new ValidationException("Ngày kết thúc bảo lưu phải sau ngày bắt đầu.");

        if (req.FreezeStartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ValidationException("Ngày bắt đầu bảo lưu không được ở quá khứ.");
    }

    // =========================================================================
    // 8. PRIVATE HELPER METHODS — Business Logic
    // =========================================================================

    /// <summary>Sinh QrCode duy nhất cho subscription.</summary>
    private static string GenerateQrCode(string seed)
        => $"GYM-{seed}-{DateTime.UtcNow.Ticks:X}";

    /// <summary>
    /// Gửi Email chào mừng hội viên mới (fire-and-forget).
    /// Lỗi gửi email không làm rollback transaction nghiệp vụ chính.
    /// </summary>
    private async Task SendWelcomeNotificationSafeAsync(
        string email,
        string fullName,
        string qrCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email)) return;

        try
        {
            await _notification.SendWelcomeEmailAsync(email, fullName, qrCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "MemberService.SendWelcome: Gửi email chào mừng thất bại cho {Email}",
                email);
        }
    }

    // =========================================================================
    // 9. PRIVATE HELPER METHODS — Mapping
    // =========================================================================

    private static MemberProfileResponse BuildMemberProfileResponse(
        MemberUser user,
        MemberSubscriptionStub? sub)
        => new(
            UserName: user.UserName,
            FullName: user.FullName ?? string.Empty,
            Phone: user.Phone ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Status: user.Status ?? string.Empty,
            ActiveSubscription: sub == null ? null : MapToSubscriptionDetailResponse(sub));

    private static SubscriptionDetailResponse MapToSubscriptionDetailResponse(MemberSubscriptionStub sub)
    {
        Enum.TryParse<SubscriptionStatus>(sub.Status, ignoreCase: true, out var statusEnum);
        return new SubscriptionDetailResponse(
            SubscriptionId: sub.SubscriptionId,
            PlanId: sub.PlanId ?? string.Empty,
            PlanName: sub.PlanId ?? string.Empty,     // placeholder — sẽ resolve JOIN khi có EF Core entity
            QrCode: sub.QrCode ?? string.Empty,
            StartDate: sub.StartDate ?? DateOnly.MinValue,
            EndDate: sub.EndDate,
            RemainingCheckInSessions: sub.RemainingCheckInSessions,
            RemainingPtSessions: sub.RemainingPtSessions,
            Status: statusEnum,
            FreezeStartDate: sub.FreezeStartDate,
            FreezeEndDate: sub.FreezeEndDate,
            AssignedPtId: sub.AssignedPtId);
    }
}

// =============================================================================
// ENTITY STUBS
// Placeholder types ánh xạ trực tiếp với 4 bảng SQL Server liên quan đến Member.
// Tên đặt theo đúng Ma trận 3.2 dev_rule (tên bảng vật lý).
// Sẽ được thay bằng EF Core entity chính thức khi hoàn thành Task 1.1.x
// =============================================================================

#pragma warning disable CS8618

/// <summary>Stub Entity — ánh xạ bảng [User] (hội viên).</summary>
internal sealed class MemberUser
{
    public string UserName { get; set; }
    public string RoleId { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Status { get; set; }
    public DateOnly? CreatedAt { get; set; }
}

/// <summary>Stub Entity — ánh xạ bảng MemberSubscription.</summary>
internal sealed class MemberSubscriptionStub
{
    public string SubscriptionId { get; set; }
    public string UserName { get; set; }
    public string? PlanId { get; set; }
    public string? QrCode { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int RemainingCheckInSessions { get; set; }
    public int RemainingPtSessions { get; set; }
    public string? Status { get; set; }
    public DateOnly? FreezeStartDate { get; set; }
    public DateOnly? FreezeEndDate { get; set; }
    public string? AssignedPtId { get; set; }
    public DateOnly? CreatedAt { get; set; }
}

/// <summary>Stub Entity — ánh xạ bảng PackagePlan.</summary>
internal sealed class PackagePlanStub
{
    public string PlanId { get; set; }
    public string? Name { get; set; }
    public int? DurationDays { get; set; }
    public decimal? Price { get; set; }
    public int? MaxCheckInSessions { get; set; }
    public byte? IsPtIncluded { get; set; }
    public int? TotalPtSessions { get; set; }
    public byte IsActive { get; set; }
}

/// <summary>Stub Entity — ánh xạ bảng CheckInLog (đọc-only bởi MemberService).</summary>
internal sealed class CheckInLogStub
{
    public string CheckInId { get; set; }
    public DateOnly? CheckInAt { get; set; }
    public string? Status { get; set; }
    public string? DenialReason { get; set; }
    public string UserName { get; set; }
    public string? SubscriptionId { get; set; }
}

#pragma warning restore CS8618
