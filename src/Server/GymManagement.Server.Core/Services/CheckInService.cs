using System.Security.Cryptography;
using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.Adapters;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// Dịch vụ kiểm soát điểm danh hội viên vào phòng tập (UC03 / US 03.1, 03.2, 03.3).
/// Kết nối xác thực điều kiện gói tập, trừ buổi và ghi nhận lịch sử CheckInLog.
/// </summary>
public sealed class CheckInService : ICheckInService
{
    // ====================================================================
    // 1. CONSTANTS & STATIC FIELDS (Khai báo đầu tiên)
    // ====================================================================
    public const string CHECKIN_ID_PREFIX = "CK";
    public const string STATUS_SUCCESS = "SUCCESS";
    public const string STATUS_DENIED = "DENIED";

    // ====================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS (Ngay sau constants)
    // ====================================================================
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICameraScannerAdapter _cameraScannerAdapter;
    private readonly ILogger<CheckInService> _logger;

    // ====================================================================
    // 3. CONSTRUCTOR (Khởi tạo và inject dependencies)
    // ====================================================================
    public CheckInService(
        IUnitOfWork unitOfWork,
        ICameraScannerAdapter cameraScannerAdapter,
        ILogger<CheckInService> logger)
    {
        _unitOfWork = unitOfWork;
        _cameraScannerAdapter = cameraScannerAdapter;
        _logger = logger;
    }

    // ====================================================================
    // 4. PUBLIC METHODS / INTERFACE IMPLEMENTATIONS (Nghiệp vụ chính)
    // ====================================================================

    /// <inheritdoc />
    public async Task<CheckInResponse> CheckInAsync(
        CheckInRequest request,
        string? verifiedByAdminId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.QrCode) && string.IsNullOrWhiteSpace(request.MemberCode) && string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ValidationException("Vui lòng cung cấp mã QR (từ webcam) hoặc mã hội viên / số điện thoại để điểm danh.");
        }

        string? adminId = verifiedByAdminId;
        if (string.IsNullOrWhiteSpace(adminId))
        {
            adminId = request.VerifiedByAdminId;
        }

        // 1. Phân giải thông tin Hội viên và Gói tập tương ứng
        var (user, subscription) = await ResolveUserAndSubscriptionAsync(request, cancellationToken);

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // 2. Kiểm tra các điều kiện nghiêm ngặt của gói tập (Guard Clauses)

        // Quy tắc 1: Gói tập đang bảo lưu
        if (string.Equals(subscription.Status, "FROZEN", StringComparison.OrdinalIgnoreCase))
        {
            await RecordDeniedCheckInSafeAsync(subscription.SubscriptionId, user.UserName, "Gói tập của hội viên đang tạm ngưng bảo lưu (FROZEN).", adminId, cancellationToken);
            throw new BusinessRuleException($"Gói tập của hội viên {user.FullName ?? user.UserName} đang tạm ngưng bảo lưu (FROZEN).");
        }

        // Quy tắc 2: Gói tập đã bị hủy
        if (string.Equals(subscription.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
        {
            await RecordDeniedCheckInSafeAsync(subscription.SubscriptionId, user.UserName, "Gói tập của hội viên đã bị hủy.", adminId, cancellationToken);
            throw new BusinessRuleException($"Gói tập của hội viên {user.FullName ?? user.UserName} đã bị hủy.");
        }

        // Quy tắc 3: Gói tập đã hết hạn sử dụng
        bool isExpired = false;
        if (string.Equals(subscription.Status, "EXPIRED", StringComparison.OrdinalIgnoreCase))
        {
            isExpired = true;
        }

        if (subscription.EndDate.HasValue)
        {
            if (subscription.EndDate.Value < today)
            {
                isExpired = true;
            }
        }

        if (isExpired)
        {
            subscription.Status = "EXPIRED";
            _unitOfWork.Repository<MemberSubscription>().Update(subscription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string expireDateText = string.Empty;
            if (subscription.EndDate.HasValue)
            {
                expireDateText = $" vào ngày {subscription.EndDate.Value:dd/MM/yyyy}";
            }

            await RecordDeniedCheckInSafeAsync(subscription.SubscriptionId, user.UserName, "Gói tập của hội viên đã hết hạn.", adminId, cancellationToken);
            throw new BusinessRuleException($"Gói tập của hội viên {user.FullName ?? user.UserName} đã hết hạn{expireDateText}.");
        }

        // Quy tắc 4: Gói tập không ở trạng thái ACTIVE
        if (!string.Equals(subscription.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            await RecordDeniedCheckInSafeAsync(subscription.SubscriptionId, user.UserName, "Gói tập của hội viên không ở trạng thái hoạt động.", adminId, cancellationToken);
            throw new BusinessRuleException($"Gói tập của hội viên {user.FullName ?? user.UserName} không ở trạng thái hoạt động.");
        }

        // Quy tắc 5: Gói tập đã hết số lượt vào phòng khả dụng
        if (subscription.RemainingCheckInSessions <= 0)
        {
            await RecordDeniedCheckInSafeAsync(subscription.SubscriptionId, user.UserName, "Gói tập của hội viên đã hết số lượt vào phòng khả dụng.", adminId, cancellationToken);
            throw new BusinessRuleException($"Gói tập của hội viên {user.FullName ?? user.UserName} đã hết số lượt vào phòng khả dụng.");
        }

        // 3. Thực thi giao dịch ACID: Trừ 1 lượt check-in và lưu CheckInLog thành công
        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            subscription.RemainingCheckInSessions -= 1;
            _unitOfWork.Repository<MemberSubscription>().Update(subscription);

            string checkInId = GenerateCheckInId();
            var checkInLog = new CheckInLog
            {
                CheckInId = checkInId,
                check_in_at = today,
                Status = STATUS_SUCCESS,
                DenialReason = null,
                SubscriptionId = subscription.SubscriptionId,
                VerifiedByAdminId = adminId,
                UserName = user.UserName
            };

            await _unitOfWork.Repository<CheckInLog>().AddAsync(checkInLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string planName = "Gói tập hội viên";
            if (!string.IsNullOrWhiteSpace(subscription.PlanId))
            {
                var plan = await _unitOfWork.Repository<PackagePlan>().GetByIdAsync(subscription.PlanId, cancellationToken);
                if (plan != null)
                {
                    if (!string.IsNullOrWhiteSpace(plan.Name))
                    {
                        planName = plan.Name;
                    }
                }
            }

            string? memberName = user.FullName;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                memberName = user.UserName;
            }

            _logger.LogInformation(
                "CheckInService.CheckInAsync: Điểm danh thành công hội viên {UserName} ({MemberName}) - Gói tập {PlanName}, còn lại {Remaining} lượt.",
                user.UserName,
                memberName,
                planName,
                subscription.RemainingCheckInSessions);

            return new CheckInResponse(
                Success: true,
                MemberName: memberName,
                PlanName: planName,
                RemainingCheckInSessions: subscription.RemainingCheckInSessions,
                RemainingPtSessions: subscription.RemainingPtSessions,
                CheckInTime: DateTime.UtcNow,
                Message: $"Điểm danh thành công! Chào mừng hội viên {memberName}."
            );
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CheckInResponse> CheckInWithQrAsync(
        string qrCode,
        string? verifiedByAdminId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
        {
            throw new ValidationException("Mã QR điểm danh không được để trống.");
        }

        var request = new CheckInRequest(QrCode: qrCode.Trim(), VerifiedByAdminId: verifiedByAdminId);
        return await CheckInAsync(request, verifiedByAdminId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CheckInResponse> ManualCheckInAsync(
        string memberCodeOrPhone,
        string? notes = null,
        string? verifiedByAdminId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(memberCodeOrPhone))
        {
            throw new ValidationException("Số điện thoại hoặc mã hội viên không được để trống.");
        }

        var request = new CheckInRequest(
            MemberCode: memberCodeOrPhone.Trim(),
            Phone: memberCodeOrPhone.Trim(),
            Notes: notes,
            VerifiedByAdminId: verifiedByAdminId);
        return await CheckInAsync(request, verifiedByAdminId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CheckInLogDto>> GetTodayCheckInLogsAsync(
        string? searchQuery = null,
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        var logs = await _unitOfWork.Repository<CheckInLog>()
            .FindAsync(l => l.check_in_at == today, cancellationToken);

        var users = await _unitOfWork.Repository<User>().GetAllAsync(cancellationToken);
        var userDict = users.ToDictionary(u => u.UserName, u => u.FullName ?? u.UserName);

        var result = new List<CheckInLogDto>();
        foreach (var log in logs)
        {
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (!string.Equals(log.Status, statusFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            string memberName = log.UserName;
            if (userDict.TryGetValue(log.UserName, out var name))
            {
                memberName = name;
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string query = searchQuery.Trim();
                bool matchUser = log.UserName.Contains(query, StringComparison.OrdinalIgnoreCase);
                bool matchName = memberName.Contains(query, StringComparison.OrdinalIgnoreCase);
                if (!matchUser && !matchName)
                {
                    continue;
                }
            }

            string adminName = "Hệ thống tự động";
            if (!string.IsNullOrWhiteSpace(log.VerifiedByAdminId))
            {
                if (userDict.TryGetValue(log.VerifiedByAdminId, out var aName))
                {
                    adminName = aName;
                }
                else
                {
                    adminName = log.VerifiedByAdminId;
                }
            }

            DateTime checkInTime = DateTime.UtcNow;
            if (log.check_in_at.HasValue)
            {
                checkInTime = log.check_in_at.Value.ToDateTime(TimeOnly.MinValue);
            }

            string status = STATUS_SUCCESS;
            if (!string.IsNullOrWhiteSpace(log.Status))
            {
                status = log.Status;
            }

            result.Add(new CheckInLogDto(
                CheckInId: log.CheckInId,
                CheckInTime: checkInTime,
                CheckInMethod: "Tại quầy",
                Notes: log.DenialReason,
                MemberName: memberName,
                VerifiedByAdminName: adminName,
                Status: status,
                DenialReason: log.DenialReason));
        }

        return result.OrderByDescending(r => r.CheckInTime).ToList();
    }

    /// <inheritdoc />
    public async Task<int> GetCurrentAttendanceCountAsync(CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        var logs = await _unitOfWork.Repository<CheckInLog>()
            .FindAsync(l => l.check_in_at == today && l.Status == STATUS_SUCCESS, cancellationToken);

        return logs.Count;
    }

    // ====================================================================
    // 5. PRIVATE HELPER METHODS (Luôn đặt ở cuối cùng của file)
    // ====================================================================

    private async Task<(User User, MemberSubscription Subscription)> ResolveUserAndSubscriptionAsync(
        CheckInRequest request,
        CancellationToken cancellationToken)
    {
        User? user = null;
        MemberSubscription? subscription = null;

        // Trường hợp A: Quét qua chuỗi mã QR
        if (!string.IsNullOrWhiteSpace(request.QrCode))
        {
            string qr = request.QrCode.Trim();

            // 1. Thử thẩm định mã QR động (chu kỳ 60s)
            QrTokenValidationResult? validationResult = null;
            if (qr.StartsWith("GYM-", StringComparison.OrdinalIgnoreCase))
            {
                validationResult = _cameraScannerAdapter.ValidateCheckInToken(qr);
                if (validationResult != null)
                {
                    if (validationResult.IsExpired)
                    {
                        throw new BusinessRuleException("Mã QR điểm danh đã hết hạn, vui lòng mở lại mã mới trên ứng dụng.");
                    }

                    if (validationResult.IsValid && !string.IsNullOrWhiteSpace(validationResult.UserId))
                    {
                        user = await _unitOfWork.Repository<User>().GetByIdAsync(validationResult.UserId, cancellationToken);
                    }
                }
            }

            // 2. Thử khớp mã QR tĩnh trong bảng MemberSubscription
            if (subscription == null)
            {
                var subs = await _unitOfWork.Repository<MemberSubscription>()
                    .FindAsync(s => s.QrCode == qr, cancellationToken);
                if (subs != null)
                {
                    subscription = subs.FirstOrDefault();
                }
            }

            // 3. Hỗ trợ định dạng tiền tố GYM_MEMBER_{UserId}
            if (user == null && subscription == null && qr.StartsWith("GYM_MEMBER_", StringComparison.OrdinalIgnoreCase))
            {
                string userId = qr.Replace("GYM_MEMBER_", "").Trim();
                user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            }

            // 4. Nếu có tiền tố GYM- mà không phải QR tĩnh hợp lệ và thẩm định token thất bại
            if (user == null && subscription == null && validationResult != null && !validationResult.IsValid)
            {
                string failureReason = "Mã QR điểm danh không hợp lệ.";
                if (!string.IsNullOrWhiteSpace(validationResult.FailureReason))
                {
                    failureReason = validationResult.FailureReason;
                }

                throw new BusinessRuleException(failureReason);
            }
        }

        // Trường hợp B: Tra cứu thủ công bằng Mã hội viên hoặc Số điện thoại
        if (user == null && !string.IsNullOrWhiteSpace(request.MemberCode))
        {
            string memberCode = request.MemberCode.Trim();
            user = await _unitOfWork.Repository<User>().GetByIdAsync(memberCode, cancellationToken);
            if (user == null)
            {
                var users = await _unitOfWork.Repository<User>()
                    .FindAsync(u => u.Phone == memberCode, cancellationToken);
                if (users != null && users.Count > 0)
                {
                    user = users.FirstOrDefault();
                }
            }
        }

        if (user == null && !string.IsNullOrWhiteSpace(request.Phone))
        {
            string phone = request.Phone.Trim();
            var users = await _unitOfWork.Repository<User>()
                .FindAsync(u => u.Phone == phone, cancellationToken);
            if (users != null && users.Count > 0)
            {
                user = users.FirstOrDefault();
            }

            if (user == null)
            {
                user = await _unitOfWork.Repository<User>().GetByIdAsync(phone, cancellationToken);
            }
        }

        // Nếu đã có subscription nhưng chưa có user, lấy user theo subscription.UserName
        if (user == null && subscription != null)
        {
            user = await _unitOfWork.Repository<User>().GetByIdAsync(subscription.UserName, cancellationToken);
        }

        if (user == null)
        {
            throw new NotFoundException("Không tìm thấy thông tin hội viên tương ứng.");
        }

        // Nếu chưa có subscription, tìm gói tập của hội viên
        if (subscription == null)
        {
            var userSubs = await _unitOfWork.Repository<MemberSubscription>()
                .FindAsync(s => s.UserName == user.UserName, cancellationToken);

            if (userSubs == null || userSubs.Count == 0)
            {
                throw new NotFoundException($"Hội viên {user.FullName ?? user.UserName} chưa đăng ký gói tập nào.");
            }

            // Ưu tiên chọn gói tập đang ACTIVE
            subscription = userSubs.FirstOrDefault(s => string.Equals(s.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase));
            if (subscription == null)
            {
                // Nếu không có gói ACTIVE, lấy gói gần nhất để báo lỗi chính xác
                subscription = userSubs.Last();
            }
        }

        return (user, subscription);
    }

    private async Task RecordDeniedCheckInSafeAsync(
        string subscriptionId,
        string userName,
        string reason,
        string? verifiedByAdminId,
        CancellationToken cancellationToken)
    {
        try
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            string checkInId = GenerateCheckInId();

            var deniedLog = new CheckInLog
            {
                CheckInId = checkInId,
                check_in_at = today,
                Status = STATUS_DENIED,
                DenialReason = reason,
                SubscriptionId = subscriptionId,
                VerifiedByAdminId = verifiedByAdminId,
                UserName = userName
            };

            await _unitOfWork.Repository<CheckInLog>().AddAsync(deniedLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "CheckInService: Từ chối điểm danh hội viên {UserName} (Gói tập: {SubscriptionId}) - Lý do: {Reason}",
                userName,
                subscriptionId,
                reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckInService: Không thể ghi nhận nhật ký điểm danh bị từ chối.");
        }
    }

    private static string GenerateCheckInId()
    {
        byte[] randomBytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        uint randomInt = BitConverter.ToUInt32(randomBytes, 0) % 100000000;
        return $"{CHECKIN_ID_PREFIX}{randomInt:D8}";
    }
}
