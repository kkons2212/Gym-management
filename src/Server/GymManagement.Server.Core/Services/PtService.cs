using GymManagement.Server.Core.Contracts;
using GymManagement.Server.Core.Contracts.Adapters;
using GymManagement.Server.Core.Contracts.Data;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.Entities;
using GymManagement.Server.Core.Enums;
using GymManagement.Server.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace GymManagement.Server.Core.Services;

/// <summary>
/// PtService Component — Kết nối Authenticated + Database API + SMS Gateway.
/// Triển khai toàn bộ nghiệp vụ liên quan đến Huấn luyện viên cá nhân (PT):
///   UC04: Admin quản lý hồ sơ PT và điều phối lịch dạy.
///   UC08: PT đăng ký lịch rảnh, xem học viên, ghi InBody, xác nhận ca tập.
///   UC10: Hội viên đặt/hủy/đổi lịch tập và đánh giá PT.
/// </summary>
public sealed class PtService : IPtService
{
    // =========================================================================
    // 1. CONSTANTS & STATIC FIELDS
    // =========================================================================

    /// <summary>Ngưỡng tính phạt hủy ca muộn (giờ). Hủy trước ngưỡng này = mất buổi tập.</summary>
    private const int CancellationGracePeriodHours = 2;

    /// <summary>Số học viên tối đa một PT có thể phụ trách đồng thời.</summary>
    private const int MaxStudentsPerPt = 10;

    /// <summary>Tiền tố RoleID trong bảng Role dùng để nhận diện PT.</summary>
    private const string RoleIdTrainer = "TRAINER";
    private const string RoleIdPt = "PT";

    /// <summary>Trạng thái tài khoản đang hoạt động trong bảng [User].</summary>
    private const string StatusActive = "ACTIVE";

    /// <summary>Trạng thái tài khoản không hoạt động trong bảng [User].</summary>
    private const string StatusInactive = "INACTIVE";

    // =========================================================================
    // 2. INJECTED DEPENDENCIES & PRIVATE FIELDS
    // =========================================================================

    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationAdapter _notification;
    private readonly ILogger<PtService> _logger;

    // =========================================================================
    // 3. CONSTRUCTOR
    // =========================================================================

    public PtService(
        IUnitOfWork unitOfWork,
        INotificationAdapter notification,
        ILogger<PtService> logger)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(logger);

        _unitOfWork = unitOfWork;
        _notification = notification;
        _logger = logger;
    }

    // =========================================================================
    // 4. PUBLIC METHODS — NHÓM 1: QUẢN LÝ HỒ SƠ PT (UC 4.1, UC 4.3)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PtProfileDto>> GetTrainersAsync(
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var subscriptionRepo = _unitOfWork.Repository<MemberSubscription>();
        var sessionRepo = _unitOfWork.Repository<PTTrainingSession>();

        var users = await userRepo.FindAsync(
            u => (u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer) &&
                 (activeOnly == null || (u.Status == StatusActive) == activeOnly.Value),
            cancellationToken);

        var result = new List<PtProfileDto>(users.Count);
        foreach (var u in users)
        {
            int studentCount = await subscriptionRepo.CountAsync(
                s => s.AssignedPtId == u.UserName && s.Status == StatusActive,
                cancellationToken);

            int pendingCount = await sessionRepo.CountAsync(
                s => s.PtId == u.UserName && s.Status == nameof(PtSessionStatus.Booked),
                cancellationToken);

            var ratedSessions = await sessionRepo.FindAsync(
                s => s.PtId == u.UserName && s.RatingStars != null,
                cancellationToken);

            decimal avgRating = 0m;
            if (ratedSessions.Count > 0)
            {
                avgRating = (decimal)ratedSessions.Average(s => (double)s.RatingStars!.Value);
            }

            result.Add(MapToProfileDto(u, studentCount, pendingCount, ratedSessions.Count, avgRating));
        }

        _logger.LogInformation(
            "PtService.GetTrainers: Trả về {Count} PT (activeOnly={Filter})",
            result.Count, activeOnly);

        return result;
    }

    /// <inheritdoc/>
    public async Task<PtProfileDto> GetTrainerByIdAsync(
        string ptId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ptId);

        var user = await _unitOfWork.Repository<User>()
            .FindSingleOrDefaultAsync(
                u => u.UserName == ptId && (u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer),
                cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy PT với mã: {ptId}");

        return MapToProfileDto(user, 0, 0, 0, 0m);
    }

    /// <inheritdoc/>
    public async Task<PtProfileDto> SaveTrainerAsync(
        SavePtProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateSavePtProfileRequest(request);

        var userRepo = _unitOfWork.Repository<User>();
        bool isNew = string.IsNullOrWhiteSpace(request.PtId);

        // Kiểm tra trùng Phone với PT khác
        bool phoneConflict = await userRepo.ExistsAsync(
            u => u.Phone == request.Phone &&
                 (u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer) &&
                 (isNew || u.UserName != request.PtId),
            cancellationToken);

        if (phoneConflict)
        {
            throw new ValidationException("Số điện thoại đã được sử dụng bởi PT khác.");
        }

        // Kiểm tra trùng Email với PT khác
        bool emailConflict = await userRepo.ExistsAsync(
            u => u.Email == request.Email &&
                 (u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer) &&
                 (isNew || u.UserName != request.PtId),
            cancellationToken);

        if (emailConflict)
        {
            throw new ValidationException("Email đã được sử dụng bởi PT khác.");
        }

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            User entity;

            if (isNew)
            {
                int count = await userRepo.CountAsync(u => u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer, cancellationToken);
                string newId = $"PT-{count + 1:D2}";

                string status;
                if (request.IsActive)
                {
                    status = StatusActive;
                }
                else
                {
                    status = StatusInactive;
                }

                entity = new User
                {
                    UserName = newId,
                    RoleId = RoleIdPt,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = request.Email,
                    Status = status,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                await userRepo.AddAsync(entity, cancellationToken);

                _logger.LogInformation(
                    "PtService.SaveTrainer: Tạo mới PT {Id} - {Name}",
                    newId, request.FullName);
            }
            else
            {
                entity = await userRepo.FindSingleOrDefaultAsync(
                    u => u.UserName == request.PtId, cancellationToken)
                    ?? throw new NotFoundException($"Không tìm thấy PT với mã: {request.PtId}");

                entity.FullName = request.FullName;
                entity.Phone = request.Phone;
                entity.Email = request.Email;
                if (request.IsActive)
                {
                    entity.Status = StatusActive;
                }
                else
                {
                    entity.Status = StatusInactive;
                }
                userRepo.Update(entity);

                _logger.LogInformation(
                    "PtService.SaveTrainer: Cập nhật PT {Id} - {Name}",
                    request.PtId, request.FullName);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToProfileDto(entity, 0, 0, 0, 0m);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(bool CanDeactivate, string? BlockReason, int ActiveStudents, int PendingSessions)>
        CheckDeactivationEligibilityAsync(
            string ptId,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ptId);

        int studentCount = await _unitOfWork.Repository<MemberSubscription>()
            .CountAsync(s => s.AssignedPtId == ptId && s.Status == StatusActive, cancellationToken);

        int pendingCount = await _unitOfWork.Repository<PTTrainingSession>()
            .CountAsync(
                s => s.PtId == ptId && s.Status == nameof(PtSessionStatus.Booked),
                cancellationToken);

        if (studentCount > 0 || pendingCount > 0)
        {
            string reason = $"PT đang phụ trách {studentCount} học viên và có {pendingCount} ca chưa hoàn tất.";
            return (false, reason, studentCount, pendingCount);
        }

        return (true, null, 0, 0);
    }

    /// <inheritdoc/>
    public async Task SetTrainerStatusAsync(
        string ptId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ptId);

        if (!isActive)
        {
            var (canDeactivate, reason, _, _) =
                await CheckDeactivationEligibilityAsync(ptId, cancellationToken);

            if (!canDeactivate)
            {
                throw new BusinessRuleException(reason!);
            }
        }

        var entity = await _unitOfWork.Repository<User>()
            .FindSingleOrDefaultAsync(u => u.UserName == ptId, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy PT với mã: {ptId}");

        if (isActive)
        {
            entity.Status = StatusActive;
        }
        else
        {
            entity.Status = StatusInactive;
        }
        _unitOfWork.Repository<User>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "PtService.SetTrainerStatus: PT {Id} chuyển sang {Status}",
            ptId, entity.Status);
    }

    // =========================================================================
    // 5. PUBLIC METHODS — NHÓM 2: PHÂN CÔNG PT (UC 4.4)
    // =========================================================================

    /// <inheritdoc/>
    public async Task AssignPtToMemberAsync(
        AssignPtRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Kiểm tra PT tồn tại và đang hoạt động
        bool ptExists = await _unitOfWork.Repository<User>().ExistsAsync(
            u => u.UserName == request.PtId &&
                 (u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer) &&
                 u.Status == StatusActive,
            cancellationToken);

        if (!ptExists)
        {
            throw new NotFoundException($"Không tìm thấy PT đang hoạt động với mã: {request.PtId}");
        }

        var subscription = await _unitOfWork.Repository<MemberSubscription>()
            .FindSingleOrDefaultAsync(
                s => s.SubscriptionId == request.SubscriptionId,
                cancellationToken);

        if (subscription == null)
        {
            var subs = await _unitOfWork.Repository<MemberSubscription>()
                .FindAsync(
                    s => s.UserName == request.SubscriptionId && s.Status == StatusActive,
                    cancellationToken);
            subscription = subs.FirstOrDefault(s => s.RemainingPtSessions > 0);
            if (subscription == null)
            {
                subscription = subs.FirstOrDefault();
            }
        }

        if (subscription == null)
        {
            throw new NotFoundException($"Không tìm thấy gói tập khả dụng với mã: {request.SubscriptionId}");
        }

        if (subscription.RemainingPtSessions <= 0)
        {
            throw new BusinessRuleException("Hội viên không còn buổi tập PT khả dụng trong gói hiện tại.");
        }

        // Kiểm tra giới hạn số học viên của PT (UC 4.4 — phụ 4.1)
        int currentStudents = await _unitOfWork.Repository<MemberSubscription>().CountAsync(
            s => s.AssignedPtId == request.PtId && s.Status == StatusActive,
            cancellationToken);

        if (currentStudents >= MaxStudentsPerPt)
        {
            throw new BusinessRuleException(
                $"PT đã đạt giới hạn {MaxStudentsPerPt} học viên phụ trách tối đa.");
        }

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            subscription.AssignedPtId = request.PtId;
            _unitOfWork.Repository<MemberSubscription>().Update(subscription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.AssignPt: Phân công PT {PtId} → Subscription {SubId}",
                request.PtId, request.SubscriptionId);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MemberSummaryDto>> GetPtStudentsAsync(
        string ptId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ptId);

        // Lấy các gói tập được gán cho PT này
        var subscriptions = await _unitOfWork.Repository<MemberSubscription>()
            .FindAsync(
                s => s.AssignedPtId == ptId && s.Status == StatusActive,
                cancellationToken);

        var memberUserNames = subscriptions.ToDictionary(s => s.UserName, s => s);

        // Lấy thông tin User của từng học viên
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(
                u => memberUserNames.ContainsKey(u.UserName) &&
                     (searchTerm == null ||
                      (u.FullName != null && u.FullName.Contains(searchTerm)) ||
                      u.UserName.Contains(searchTerm)),
                cancellationToken);

        return users.Select(u =>
        {
            memberUserNames.TryGetValue(u.UserName, out var sub);
            return new MemberSummaryDto(
                UserName: u.UserName,
                FullName: u.FullName ?? string.Empty,
                Phone: u.Phone ?? string.Empty,
                Email: u.Email ?? string.Empty,
                RemainingPtSessions: sub?.RemainingPtSessions ?? 0,
                SubscriptionStatus: sub?.Status ?? string.Empty);
        }).ToList();
    }

    // =========================================================================
    // 6. PUBLIC METHODS — NHÓM 3: LỊCH RẢNH PT (UC 4.6, UC 8.1.1, UC 8.1.2)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PtSlotDto>> GetPtSlotsAsync(
        string ptId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ptId);

        var slots = await _unitOfWork.Repository<PTAvailability>()
            .FindAsync(
                s => s.PtId == ptId &&
                     s.AvailableDate >= fromDate &&
                     s.AvailableDate <= toDate,
                cancellationToken);

        return slots
            .OrderBy(s => s.AvailableDate)
            .ThenBy(s => s.StartTime)
            .Select(MapToSlotDto)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task SavePtSlotsAsync(
        SavePtSlotsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var availRepo = _unitOfWork.Repository<PTAvailability>();

            foreach (var item in request.Slots)
            {
                if (!string.IsNullOrWhiteSpace(item.AvailabilityId))
                {
                    // Cập nhật hoặc hủy slot đã tồn tại
                    var existing = await availRepo.FindSingleOrDefaultAsync(
                        s => s.AvailabilityId == item.AvailabilityId, cancellationToken);

                    if (existing == null)
                    {
                        continue;
                    }

                    // Không cho phép hủy slot đã có hội viên đặt (UC 8.1.1 — phụ 5.1)
                    if (!item.Register && existing.IsBooked == 1)
                    {
                        throw new BusinessRuleException(
                            $"Khung giờ {existing.AvailableDate:dd/MM} {existing.StartTime:hh\\:mm} " +
                            "đã có hội viên đặt lịch, không thể hủy.");
                    }

                    if (item.Register)
                    {
                        existing.StartTime = item.StartTime;
                        existing.EndTime = item.EndTime;
                        availRepo.Update(existing);
                    }
                    else
                    {
                        availRepo.Remove(existing);
                    }
                }
                else if (item.Register)
                {
                    // Ràng buộc: Không được đăng ký thời gian đã qua
                    var now = DateTime.Now;
                    if (item.AvailableDate < DateOnly.FromDateTime(now) ||
                        (item.AvailableDate == DateOnly.FromDateTime(now) && item.StartTime <= TimeOnly.FromDateTime(now)))
                    {
                        throw new BusinessRuleException(
                            $"Không thể đăng ký khung giờ rảnh ở thời điểm đã qua ({item.AvailableDate:dd/MM/yyyy} {item.StartTime:hh\\:mm}).");
                    }

                    // Tạo slot mới
                    string newAvailId = await GenerateUniqueAvailabilityIdAsync(cancellationToken);
                    var newSlot = new PTAvailability
                    {
                        AvailabilityId = newAvailId,
                        PtId = request.PtId,
                        AvailableDate = item.AvailableDate,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        IsBooked = 0
                    };
                    await availRepo.AddAsync(newSlot, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.SavePtSlots: PT {PtId} lưu {Count} slot",
                request.PtId, request.Slots.Count);
        }, cancellationToken);
    }

    // =========================================================================
    // 7. PUBLIC METHODS — NHÓM 4: CA TẬP PT (UC 10.1, 10.2, 10.4, 4.5, 8.2.3)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PtSessionDto>> GetSessionsAsync(
        string? ptId = null,
        string? memberUserName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _unitOfWork.Repository<PTTrainingSession>()
            .FindAsync(
                s => (ptId == null || s.PtId == ptId) &&
                     (memberUserName == null || s.MemberId == memberUserName) &&
                     (fromDate == null || s.SessionStartTime >= DateOnly.FromDateTime(fromDate.Value)) &&
                     (toDate == null || s.SessionEndTime <= DateOnly.FromDateTime(toDate.Value)),
                cancellationToken);

        return sessions
            .OrderBy(s => s.SessionStartTime)
            .Select(s => MapToSessionDto(s))
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PtSessionSlotDto>> GetScheduleSlotsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        DateOnly? fromDate = null;
        if (startDate.HasValue)
        {
            fromDate = DateOnly.FromDateTime(startDate.Value.Date);
        }

        DateOnly? toDate = null;
        if (endDate.HasValue)
        {
            toDate = DateOnly.FromDateTime(endDate.Value.Date);
        }

        var sessions = await _unitOfWork.Repository<PTTrainingSession>()
            .FindAsync(
                s => (!fromDate.HasValue || s.SessionStartTime >= fromDate.Value) &&
                     (!toDate.HasValue || s.SessionStartTime <= toDate.Value),
                cancellationToken);

        var availIds = sessions
            .Where(s => !string.IsNullOrEmpty(s.AvailabilityId))
            .Select(s => s.AvailabilityId!)
            .Distinct()
            .ToList();

        var availabilities = new Dictionary<string, PTAvailability>();
        if (availIds.Count > 0)
        {
            var avails = await _unitOfWork.Repository<PTAvailability>()
                .FindAsync(a => availIds.Contains(a.AvailabilityId), cancellationToken);
            foreach (var a in avails)
            {
                availabilities[a.AvailabilityId] = a;
            }
        }

        var userNames = sessions
            .SelectMany(s => new[] { s.PtId, s.MemberId })
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var users = new Dictionary<string, User>();
        if (userNames.Count > 0)
        {
            var userList = await _unitOfWork.Repository<User>()
                .FindAsync(u => userNames.Contains(u.UserName), cancellationToken);
            foreach (var u in userList)
            {
                users[u.UserName] = u;
            }
        }

        var results = new List<PtSessionSlotDto>(sessions.Count);
        foreach (var s in sessions.OrderBy(x => x.SessionStartTime))
        {
            string ptName = s.PtId;
            if (users.TryGetValue(s.PtId, out var ptUser))
            {
                if (!string.IsNullOrWhiteSpace(ptUser.FullName))
                {
                    ptName = ptUser.FullName;
                }
            }

            string memberName = s.MemberId;
            if (users.TryGetValue(s.MemberId, out var memUser))
            {
                if (!string.IsNullOrWhiteSpace(memUser.FullName))
                {
                    memberName = memUser.FullName;
                }
            }

            TimeSpan startTime = new TimeSpan(8, 0, 0);
            TimeSpan endTime = new TimeSpan(9, 0, 0);

            if (!string.IsNullOrEmpty(s.AvailabilityId) && availabilities.TryGetValue(s.AvailabilityId, out var avail))
            {
                startTime = avail.StartTime.ToTimeSpan();
                endTime = avail.EndTime.ToTimeSpan();
            }

            string status = nameof(PtSessionStatus.Booked);
            if (!string.IsNullOrEmpty(s.Status))
            {
                status = s.Status;
            }

            DateTime scheduledDate = s.SessionStartTime.ToDateTime(TimeOnly.MinValue);

            results.Add(new PtSessionSlotDto(
                SessionId: s.PTSessionId,
                PtId: s.PtId,
                PtName: ptName,
                MemberId: s.MemberId,
                MemberName: memberName,
                ScheduledDate: scheduledDate,
                StartTime: startTime,
                EndTime: endTime,
                SessionStatus: status,
                Notes: s.WorkoutNotes
            ));
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<PtSessionSlotDto> BookScheduleSlotAsync(
        BookPtSlotRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var pt = await _unitOfWork.Repository<User>()
                .FindSingleOrDefaultAsync(
                    u => u.UserName == request.PtId &&
                         (u.RoleId == RoleIdPt || u.RoleId == RoleIdTrainer),
                    cancellationToken);

            if (pt == null)
            {
                throw new NotFoundException($"Không tìm thấy HLV với mã: {request.PtId}");
            }

            var member = await _unitOfWork.Repository<User>()
                .FindSingleOrDefaultAsync(
                    u => u.UserName == request.MemberId,
                    cancellationToken);

            if (member == null)
            {
                throw new NotFoundException($"Không tìm thấy hội viên với mã: {request.MemberId}");
            }

            var subscriptions = await _unitOfWork.Repository<MemberSubscription>()
                .FindAsync(
                    s => s.UserName == request.MemberId && s.Status == StatusActive,
                    cancellationToken);

            MemberSubscription? subscription = null;
            if (subscriptions != null && subscriptions.Count > 0)
            {
                subscription = subscriptions.FirstOrDefault(s => s.RemainingPtSessions > 0);
                if (subscription == null)
                {
                    subscription = subscriptions.FirstOrDefault();
                }
            }
            else
            {
                subscription = await _unitOfWork.Repository<MemberSubscription>()
                    .FindSingleOrDefaultAsync(
                        s => s.UserName == request.MemberId && s.Status == StatusActive,
                        cancellationToken);
            }

            if (subscription == null)
            {
                throw new BusinessRuleException("Hội viên chưa có gói tập đang hoạt động.");
            }

            if (subscription.RemainingPtSessions <= 0)
            {
                throw new BusinessRuleException("Hội viên không còn số buổi tập PT khả dụng trong gói hiện tại.");
            }

            if (!string.IsNullOrEmpty(subscription.AssignedPtId) && subscription.AssignedPtId != request.PtId)
            {
                string memberName = member.UserName;
                if (!string.IsNullOrWhiteSpace(member.FullName))
                {
                    memberName = member.FullName;
                }
                throw new BusinessRuleException(
                    $"Hội viên {memberName} đã được phân công cho HLV khác ({subscription.AssignedPtId}).");
            }

            if (string.IsNullOrEmpty(subscription.AssignedPtId))
            {
                subscription.AssignedPtId = request.PtId;
                _unitOfWork.Repository<MemberSubscription>().Update(subscription);
            }

            var targetDate = DateOnly.FromDateTime(request.ScheduledDate);
            var startTime = new TimeOnly(request.StartTime.Hours, request.StartTime.Minutes, request.StartTime.Seconds);
            var endTime = new TimeOnly(request.EndTime.Hours, request.EndTime.Minutes, request.EndTime.Seconds);

            var existingSlots = await _unitOfWork.Repository<PTAvailability>()
                .FindAsync(
                    a => a.PtId == request.PtId &&
                         a.AvailableDate == targetDate &&
                         a.IsBooked == 1 &&
                         a.StartTime < endTime &&
                         a.EndTime > startTime,
                    cancellationToken);

            if (existingSlots.Count > 0)
            {
                throw new BusinessRuleException("HLV đã có ca tập được xếp trong khung giờ này.");
            }

            string availId = await GenerateUniqueAvailabilityIdAsync(cancellationToken);

            var newSlot = new PTAvailability
            {
                AvailabilityId = availId,
                PtId = request.PtId,
                AvailableDate = targetDate,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = 1
            };
            await _unitOfWork.Repository<PTAvailability>().AddAsync(newSlot, cancellationToken);

            string sessionId = await GenerateUniqueSessionIdAsync(cancellationToken);

            var newSession = new PTTrainingSession
            {
                PTSessionId = sessionId,
                PtId = request.PtId,
                MemberId = request.MemberId,
                AvailabilityId = availId,
                SessionStartTime = targetDate,
                SessionEndTime = targetDate,
                Status = nameof(PtSessionStatus.Booked),
                WorkoutNotes = "Admin xếp lịch trực tiếp"
            };
            await _unitOfWork.Repository<PTTrainingSession>().AddAsync(newSession, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.BookScheduleSlot: Admin xếp lịch PT {PtId} cho hội viên {MemberId} vào {Date} {Start}-{End}",
                request.PtId, request.MemberId, targetDate, startTime, endTime);

            string ptDisplayName = pt.FullName ?? pt.UserName;
            string memberDisplayName = member.FullName ?? member.UserName;

            return new PtSessionSlotDto(
                SessionId: sessionId,
                PtId: request.PtId,
                PtName: ptDisplayName,
                MemberId: request.MemberId,
                MemberName: memberDisplayName,
                ScheduledDate: request.ScheduledDate,
                StartTime: request.StartTime,
                EndTime: request.EndTime,
                SessionStatus: nameof(PtSessionStatus.Booked),
                Notes: newSession.WorkoutNotes
            );
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PtSessionDto> BookSessionAsync(
        BookSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            // 1. Tìm slot muốn đặt — kiểm tra còn trống
            var slot = await _unitOfWork.Repository<PTAvailability>()
                .FindSingleOrDefaultAsync(
                    s => s.AvailabilityId == request.AvailabilityId && s.IsBooked == 0,
                    cancellationToken)
                ?? throw new BusinessRuleException(
                    "Khung giờ này của PT vừa được đặt, vui lòng chọn khung giờ khác.");

            // 2. Lấy gói tập của hội viên — phải có AssignedPtId khớp với PtId của slot
            var subscription = await _unitOfWork.Repository<MemberSubscription>()
                .FindSingleOrDefaultAsync(
                    s => s.UserName == request.MemberUserName &&
                         s.AssignedPtId == slot.PtId &&
                         s.Status == StatusActive,
                    cancellationToken)
                ?? throw new BusinessRuleException(
                    "Bạn chưa được phân công PT phụ trách hoặc gói tập không còn hiệu lực.");

            // 3. Kiểm tra còn buổi PT (UC 10.1 — phụ 4.1)
            if (subscription.RemainingPtSessions <= 0)
            {
                throw new BusinessRuleException(
                    "Bạn không còn số buổi tập PT khả dụng để đặt thêm lịch.");
            }

            // 4. Kiểm tra hội viên không trùng lịch trong cùng khung giờ (UC 10.1 — phụ 4.3)
            var sessionStart = slot.AvailableDate.ToDateTime(slot.StartTime);
            var sessionEnd = slot.AvailableDate.ToDateTime(slot.EndTime);

            bool hasConflict = await _unitOfWork.Repository<PTTrainingSession>().ExistsAsync(
                s => s.MemberId == request.MemberUserName &&
                     s.Status == nameof(PtSessionStatus.Booked) &&
                     s.SessionStartTime < DateOnly.FromDateTime(sessionEnd) &&
                     s.SessionEndTime > DateOnly.FromDateTime(sessionStart),
                cancellationToken);

            if (hasConflict)
            {
                throw new BusinessRuleException("Bạn đã có lịch tập trong khung giờ này.");
            }

            // 5. Tạo ca tập mới
            string newSessionId = await GenerateUniqueSessionIdAsync(cancellationToken);

            var newSession = new PTTrainingSession
            {
                PTSessionId = newSessionId,
                PtId = slot.PtId,
                MemberId = request.MemberUserName,
                AvailabilityId = slot.AvailabilityId,
                SessionStartTime = DateOnly.FromDateTime(sessionStart),
                SessionEndTime = DateOnly.FromDateTime(sessionEnd),
                Status = nameof(PtSessionStatus.Booked),
                WorkoutNotes = request.WorkoutNotes
            };
            await _unitOfWork.Repository<PTTrainingSession>().AddAsync(newSession, cancellationToken);

            // 6. Đánh dấu slot đã bị đặt
            slot.IsBooked = 1;
            _unitOfWork.Repository<PTAvailability>().Update(slot);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.BookSession: Hội viên {Member} đặt ca {SessionId} với PT {PtId}",
                request.MemberUserName, newSessionId, slot.PtId);

            // 7. Gửi SMS nhắc lịch hẹn — lỗi gửi SMS không rollback transaction chính
            await SendSessionReminderSafeAsync(
                request.MemberUserName, slot.PtId, sessionStart, cancellationToken);

            return MapToSessionDto(newSession);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CancelSessionAsync(
        CancelSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var session = await _unitOfWork.Repository<PTTrainingSession>()
                .FindSingleOrDefaultAsync(s => s.PTSessionId == request.SessionId, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy ca tập với mã: {request.SessionId}");

            if (session.Status != nameof(PtSessionStatus.Booked))
            {
                throw new BusinessRuleException("Chỉ có thể hủy ca tập đang ở trạng thái Đã đặt.");
            }

            // Xác định có trong ngưỡng phạt không
            bool isLate = await IsLateCancellationAsync(session, cancellationToken);

            // Hủy trễ chưa được xác nhận — trả BusinessRuleException để UI hiển thị cảnh báo xác nhận 2 bước
            if (isLate && !request.ForceLateCancellation)
            {
                throw new BusinessRuleException(
                    $"Lịch tập hủy trễ (dưới {CancellationGracePeriodHours} giờ). " +
                    "Buổi tập sẽ không được hoàn lại. Xác nhận để tiếp tục.");
            }

            // Cập nhật trạng thái ca tập
            session.Status = nameof(PtSessionStatus.Cancelled);
            _unitOfWork.Repository<PTTrainingSession>().Update(session);

            // Hoàn lại buổi tập nếu hủy đúng hạn (UC 10.2 — dòng sự kiện chính bước 6)
            if (!isLate)
            {
                await RestorePtSessionAsync(session.MemberId, cancellationToken);
            }

            // Giải phóng slot PT
            await ReleaseSlotAsync(session.AvailabilityId, cancellationToken);

            // Tạo thông báo In-App thông báo hủy ca tập cho cả PT và Hội viên
            try
            {
                var cancelNotifPt = new Notification
                {
                    NotificationId = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                    Title = "Lịch tập PT đã bị hủy",
                    Message = $"Ca tập ngày {session.SessionStartTime:dd/MM/yyyy} với học viên {session.MemberId} đã bị hủy. Khung giờ này đã được mở lại.",
                    IsRead = false,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    UserName = session.PtId
                };
                await _unitOfWork.Repository<Notification>().AddAsync(cancelNotifPt, cancellationToken);

                var cancelNotifMem = new Notification
                {
                    NotificationId = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                    Title = "Lịch tập PT đã bị hủy",
                    Message = $"Ca tập ngày {session.SessionStartTime:dd/MM/yyyy} với Huấn luyện viên {session.PtId} đã được hủy thành công.",
                    IsRead = false,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    UserName = session.MemberId
                };
                await _unitOfWork.Repository<Notification>().AddAsync(cancelNotifMem, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PtService.CancelSession: Tạo thông báo In-App hủy ca thất bại cho ca {SessionId}", session.PTSessionId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.CancelSession: Ca {SessionId} bị hủy bởi {User}, trễ={IsLate}",
                request.SessionId, request.RequestedByUserName, isLate);

            if (isLate)
            {
                _logger.LogWarning(
                    "PtService.CancelSession: Hủy trễ — hội viên {Member} mất 1 buổi tập",
                    session.MemberId);
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PtSessionDto> RescheduleSessionAsync(
        RescheduleSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var session = await _unitOfWork.Repository<PTTrainingSession>()
                .FindSingleOrDefaultAsync(s => s.PTSessionId == request.SessionId, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy ca tập với mã: {request.SessionId}");

            if (session.Status != nameof(PtSessionStatus.Booked))
            {
                throw new BusinessRuleException("Chỉ có thể đổi lịch ca tập đang ở trạng thái Đã đặt.");
            }

            // Kiểm tra slot mới còn trống và thuộc cùng PT
            var newSlot = await _unitOfWork.Repository<PTAvailability>()
                .FindSingleOrDefaultAsync(
                    s => s.AvailabilityId == request.NewAvailabilityId &&
                         s.PtId == session.PtId &&
                         s.IsBooked == 0,
                    cancellationToken)
                ?? throw new BusinessRuleException(
                    "Ca dạy đã đầy hoặc không thuộc PT phụ trách của bạn.");

            // Kiểm tra hội viên không trùng lịch với slot mới (UC 10.4 — phụ 5.1)
            var newStart = newSlot.AvailableDate.ToDateTime(newSlot.StartTime);
            var newEnd = newSlot.AvailableDate.ToDateTime(newSlot.EndTime);

            bool conflict = await _unitOfWork.Repository<PTTrainingSession>().ExistsAsync(
                s => s.MemberId == request.MemberUserName &&
                     s.PTSessionId != request.SessionId &&
                     s.Status == nameof(PtSessionStatus.Booked) &&
                     s.SessionStartTime < DateOnly.FromDateTime(newEnd) &&
                     s.SessionEndTime > DateOnly.FromDateTime(newStart),
                cancellationToken);

            if (conflict)
            {
                throw new BusinessRuleException("Bạn đã có lịch tập trong khung giờ mới.");
            }

            // Giải phóng slot cũ
            await ReleaseSlotAsync(session.AvailabilityId, cancellationToken);

            // Cập nhật ca tập sang slot mới
            session.AvailabilityId = newSlot.AvailabilityId;
            session.SessionStartTime = DateOnly.FromDateTime(newStart);
            session.SessionEndTime = DateOnly.FromDateTime(newEnd);
            _unitOfWork.Repository<PTTrainingSession>().Update(session);

            // Đánh dấu slot mới đã đặt
            newSlot.IsBooked = 1;
            _unitOfWork.Repository<PTAvailability>().Update(newSlot);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.RescheduleSession: Ca {SessionId} đổi sang slot {NewSlot}",
                request.SessionId, request.NewAvailabilityId);

            return MapToSessionDto(session);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PtSessionDto> CompleteSessionByPtAsync(
        PtCompleteSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var session = await _unitOfWork.Repository<PTTrainingSession>()
                .FindSingleOrDefaultAsync(
                    s => s.PTSessionId == request.SessionId && s.PtId == request.PtUserName,
                    cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy ca tập với mã: {request.SessionId}");

            // Chỉ xác nhận ca đang Booked và đã đến giờ diễn ra (UC 8.2.3 — phụ 4.1)
            if (session.Status != nameof(PtSessionStatus.Booked))
            {
                throw new BusinessRuleException("Ca tập chưa diễn ra hoặc đã hoàn thành.");
            }

            await EnsureSessionStartedAsync(session, cancellationToken);

            session.Status = nameof(PtSessionStatus.Completed);
            session.WorkoutNotes = request.WorkoutNotes ?? session.WorkoutNotes;
            session.CompletedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            _unitOfWork.Repository<PTTrainingSession>().Update(session);

            // Trừ 1 buổi RemainingPtSessions của học viên
            await DeductPtSessionAsync(session.MemberId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.CompleteSession: PT {PtId} xác nhận hoàn thành ca {SessionId}",
                request.PtUserName, request.SessionId);

            return MapToSessionDto(session);
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PtSessionDto> ConfirmSessionResultAsync(
        ConfirmSessionResultRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var allowedStatuses = new[]
        {
            PtSessionStatus.Completed,
            PtSessionStatus.AbsentMember,
            PtSessionStatus.AbsentPt,
            PtSessionStatus.Postponed
        };

        if (!allowedStatuses.Contains(request.NewStatus))
        {
            throw new ValidationException(
                "Trạng thái không hợp lệ. Chỉ chấp nhận: Completed, AbsentMember, AbsentPt, Postponed.");
        }

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var session = await _unitOfWork.Repository<PTTrainingSession>()
                .FindSingleOrDefaultAsync(s => s.PTSessionId == request.SessionId, cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy ca tập với mã: {request.SessionId}");

            Enum.TryParse<PtSessionStatus>(session.Status, out var previousStatus);

            // Xử lý trừ / hoàn buổi tập dựa trên chuyển đổi trạng thái (UC 4.5)
            bool wasCompleted = previousStatus == PtSessionStatus.Completed;
            bool nowCompleted = request.NewStatus == PtSessionStatus.Completed;
            bool nowReturnable = request.NewStatus is PtSessionStatus.AbsentPt
                                                   or PtSessionStatus.Postponed;

            if (!wasCompleted && nowCompleted)
            {
                await DeductPtSessionAsync(session.MemberId, cancellationToken);       // Hoàn thành lần đầu → trừ 1 buổi
            }
            else if (wasCompleted && nowReturnable)
            {
                await RestorePtSessionAsync(session.MemberId, cancellationToken);      // Admin đổi từ Completed → hoàn lại 1 buổi (UC 4.5 — phụ 5.2)
            }

            session.Status = request.NewStatus.ToString();
            session.WorkoutNotes = request.Notes ?? session.WorkoutNotes;
            if (request.NewStatus == PtSessionStatus.Completed)
            {
                session.CompletedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            _unitOfWork.Repository<PTTrainingSession>().Update(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.ConfirmResult: Admin {Admin} chuyển ca {SessionId}: {Old} → {New}",
                request.AdminUserName, request.SessionId, previousStatus, request.NewStatus);

            return MapToSessionDto(session);
        }, cancellationToken);
    }

    // =========================================================================
    // 8. PUBLIC METHODS — NHÓM 5: INBODY (UC 8.2.2)
    // =========================================================================

    /// <inheritdoc/>
    public async Task<IReadOnlyList<InBodyRecordDto>> GetInBodyHistoryAsync(
        string memberUserName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memberUserName);

        var records = await _unitOfWork.Repository<MemberBodyMetric>()
            .FindAsync(m => m.UserName == memberUserName, cancellationToken);

        return records
            .OrderByDescending(m => m.MeasuredAt)
            .Select(MapToInBodyDto)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<InBodyRecordDto> RecordInBodyAsync(
        RecordInBodyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRecordInBodyRequest(request);

        return await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            int count = await _unitOfWork.Repository<MemberBodyMetric>()
                .CountAsync(cancellationToken: cancellationToken);
            string newId = $"BMI-{count + 1:D5}";

            var entity = new MemberBodyMetric
            {
                BodyMetricId = newId,
                UserName = request.MemberUserName,
                WeightKg = request.WeightKg,
                HeightCm = request.HeightCm,
                SkeletalMuscleMass = request.SkeletalMuscleMass,
                BodyFatPercentage = request.BodyFatPercentage,
                Bmi = request.Bmi,
                Notes = request.Notes,
                MeasuredAt = DateOnly.FromDateTime(DateTime.UtcNow),
                RecordedByPtId = request.PtUserName,
                PTSessionId = request.LinkedSessionId
            };

            await _unitOfWork.Repository<MemberBodyMetric>().AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.RecordInBody: PT {PtId} ghi InBody {Id} cho hội viên {Member}",
                request.PtUserName, newId, request.MemberUserName);

            return MapToInBodyDto(entity);
        }, cancellationToken);
    }

    // =========================================================================
    // 9. PUBLIC METHODS — NHÓM 6: ĐÁNH GIÁ PT (UC 10.3)
    // =========================================================================

    /// <inheritdoc/>
    public async Task SubmitRatingAsync(
        SubmitRatingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RatingStars is < 1 or > 5)
        {
            throw new ValidationException("Điểm đánh giá phải từ 1 đến 5 sao.");
        }

        await _unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var session = await _unitOfWork.Repository<PTTrainingSession>()
                .FindSingleOrDefaultAsync(
                    s => s.PTSessionId == request.SessionId && s.MemberId == request.MemberUserName,
                    cancellationToken)
                ?? throw new NotFoundException($"Không tìm thấy ca tập với mã: {request.SessionId}");

            // Chỉ đánh giá được ca đã hoàn thành (UC 10.3 — điều kiện tiên quyết)
            if (session.Status != nameof(PtSessionStatus.Completed))
            {
                throw new BusinessRuleException("Chỉ có thể đánh giá ca tập đã hoàn thành.");
            }

            // Mỗi ca chỉ được đánh giá 1 lần
            if (session.RatingStars.HasValue)
            {
                throw new BusinessRuleException("Ca tập này đã được đánh giá, không thể đánh giá lại.");
            }

            session.RatingStars = (byte)request.RatingStars;
            session.ReviewComment = request.ReviewComment;
            _unitOfWork.Repository<PTTrainingSession>().Update(session);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PtService.SubmitRating: Hội viên {Member} đánh giá {Stars} sao cho ca {SessionId}",
                request.MemberUserName, request.RatingStars, request.SessionId);
        }, cancellationToken);
    }

    // =========================================================================
    // 10. PRIVATE HELPER METHODS — Validation
    // =========================================================================

    private static void ValidateSavePtProfileRequest(SavePtProfileRequest req)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(req.FullName))
        {
            errors.Add("Họ tên không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(req.Phone) || req.Phone.Length < 9)
        {
            errors.Add("Số điện thoại không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
        {
            errors.Add("Email không hợp lệ.");
        }

        if (req.ExperienceYears < 0)
        {
            errors.Add("Số năm kinh nghiệm không được âm.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join(" ", errors));
        }
    }

    private static void ValidateRecordInBodyRequest(RecordInBodyRequest req)
    {
        var errors = new List<string>();

        if (req.WeightKg <= 0)
        {
            errors.Add("Cân nặng phải lớn hơn 0.");
        }

        if (req.HeightCm <= 0)
        {
            errors.Add("Chiều cao phải lớn hơn 0.");
        }

        if (req.BodyFatPercentage < 0)
        {
            errors.Add("% mỡ không được âm.");
        }

        if (req.SkeletalMuscleMass <= 0)
        {
            errors.Add("Khối lượng cơ phải lớn hơn 0.");
        }

        if (req.Bmi <= 0)
        {
            errors.Add("Chỉ số BMI phải lớn hơn 0.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join(" ", errors));
        }
    }

    // =========================================================================
    // 11. PRIVATE HELPER METHODS — Business Logic
    // =========================================================================

    /// <summary>Kiểm tra ca tập đã đến giờ diễn ra chưa.</summary>
    private async Task EnsureSessionStartedAsync(
        PTTrainingSession session,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(session.AvailabilityId))
        {
            return;
        }

        var slot = await _unitOfWork.Repository<PTAvailability>()
            .FindSingleOrDefaultAsync(
                s => s.AvailabilityId == session.AvailabilityId, cancellationToken);

        if (slot == null)
        {
            return;
        }

        var sessionStart = slot.AvailableDate.ToDateTime(slot.StartTime);
        if (DateTime.UtcNow < sessionStart)
        {
            throw new BusinessRuleException("Ca tập chưa đến giờ diễn ra, không thể xác nhận hoàn thành.");
        }
    }

    /// <summary>Xác định hủy ca có tính phạt không (dưới ngưỡng CancellationGracePeriodHours).</summary>
    private async Task<bool> IsLateCancellationAsync(
        PTTrainingSession session,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(session.AvailabilityId))
        {
            return false;
        }

        var slot = await _unitOfWork.Repository<PTAvailability>()
            .FindSingleOrDefaultAsync(
                s => s.AvailabilityId == session.AvailabilityId, cancellationToken);

        if (slot == null)
        {
            return false;
        }

        var sessionStart = slot.AvailableDate.ToDateTime(slot.StartTime);
        double hoursUntilStart = (sessionStart - DateTime.UtcNow).TotalHours;
        return hoursUntilStart < CancellationGracePeriodHours;
    }

    /// <summary>Trừ 1 buổi RemainingPtSessions của hội viên (gói ACTIVE đầu tiên tìm được).</summary>
    private async Task DeductPtSessionAsync(string memberUserName, CancellationToken cancellationToken)
    {
        var subs = await _unitOfWork.Repository<MemberSubscription>()
            .FindAsync(
                s => s.UserName == memberUserName && s.Status == StatusActive,
                cancellationToken);

        MemberSubscription? sub = null;
        if (subs != null && subs.Count > 0)
        {
            sub = subs.FirstOrDefault(s => s.RemainingPtSessions > 0);
            if (sub == null)
            {
                sub = subs.FirstOrDefault();
            }
        }
        else
        {
            sub = await _unitOfWork.Repository<MemberSubscription>()
                .FindSingleOrDefaultAsync(
                    s => s.UserName == memberUserName && s.Status == StatusActive,
                    cancellationToken);
        }

        if (sub == null || sub.RemainingPtSessions <= 0)
        {
            return;
        }

        sub.RemainingPtSessions--;
        _unitOfWork.Repository<MemberSubscription>().Update(sub);
    }

    /// <summary>Hoàn lại 1 buổi RemainingPtSessions của hội viên (gói ACTIVE đầu tiên tìm được).</summary>
    private async Task RestorePtSessionAsync(string memberUserName, CancellationToken cancellationToken)
    {
        var subs = await _unitOfWork.Repository<MemberSubscription>()
            .FindAsync(
                s => s.UserName == memberUserName && s.Status == StatusActive,
                cancellationToken);

        MemberSubscription? sub = null;
        if (subs != null && subs.Count > 0)
        {
            sub = subs.FirstOrDefault();
        }
        else
        {
            sub = await _unitOfWork.Repository<MemberSubscription>()
                .FindSingleOrDefaultAsync(
                    s => s.UserName == memberUserName && s.Status == StatusActive,
                    cancellationToken);
        }

        if (sub == null)
        {
            return;
        }

        sub.RemainingPtSessions++;
        _unitOfWork.Repository<MemberSubscription>().Update(sub);
    }

    /// <summary>Giải phóng slot PTAvailability (is_Booked → 0).</summary>
    private async Task ReleaseSlotAsync(string? availabilityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(availabilityId))
        {
            return;
        }

        var slot = await _unitOfWork.Repository<PTAvailability>()
            .FindSingleOrDefaultAsync(s => s.AvailabilityId == availabilityId, cancellationToken);

        if (slot == null)
        {
            return;
        }

        slot.IsBooked = 0;
        _unitOfWork.Repository<PTAvailability>().Update(slot);
    }

    /// <summary>
    /// Gửi SMS nhắc lịch hẹn tập — lỗi gửi không làm rollback nghiệp vụ chính.
    /// </summary>
    private async Task SendSessionReminderSafeAsync(
        string memberUserName,
        string ptId,
        DateTime sessionStart,
        CancellationToken cancellationToken)
    {
        try
        {
            var memberUser = await _unitOfWork.Repository<User>()
                .FindSingleOrDefaultAsync(u => u.UserName == memberUserName, cancellationToken);

            var ptUser = await _unitOfWork.Repository<User>()
                .FindSingleOrDefaultAsync(u => u.UserName == ptId, cancellationToken);

            string memberName = memberUserName;
            if (memberUser != null && !string.IsNullOrWhiteSpace(memberUser.FullName))
            {
                memberName = memberUser.FullName;
            }

            string ptName = ptId;
            if (ptUser != null && !string.IsNullOrWhiteSpace(ptUser.FullName))
            {
                ptName = ptUser.FullName;
            }

            // 1. Tạo thông báo In-App cho Huấn luyện viên PT
            var ptNotif = new Notification
            {
                NotificationId = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                Title = "Lịch tập PT mới",
                Message = $"Học viên {memberName} đã đặt lịch tập vào ngày {sessionStart:dd/MM/yyyy} lúc {sessionStart:HH:mm}. Vui lòng kiểm tra lịch làm việc của bạn.",
                IsRead = false,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UserName = ptId
            };
            await _unitOfWork.Repository<Notification>().AddAsync(ptNotif, cancellationToken);

            // 2. Tạo thông báo In-App cho Hội viên
            var memberNotif = new Notification
            {
                NotificationId = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
                Title = "Đặt lịch PT thành công",
                Message = $"Bạn đã đặt thành công lịch tập cùng Huấn luyện viên {ptName} vào ngày {sessionStart:dd/MM/yyyy} lúc {sessionStart:HH:mm}.",
                IsRead = false,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                UserName = memberUserName
            };
            await _unitOfWork.Repository<Notification>().AddAsync(memberNotif, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Gửi SMS nếu có số điện thoại
            if (memberUser?.Phone != null && ptUser != null)
            {
                await _notification.SendPtSessionReminderAsync(
                    memberUser.Phone,
                    memberName,
                    ptName,
                    sessionStart,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "PtService.SendSessionReminder: Gửi thông báo thất bại cho hội viên {Member}",
                memberUserName);
        }
    }

    // =========================================================================
    // 12. PRIVATE HELPER METHODS — Mapping
    // =========================================================================

    private static PtProfileDto MapToProfileDto(
        User u,
        int students,
        int pending,
        int reviewCount,
        decimal avgRating)
    {
        return new PtProfileDto(
            PtId: u.UserName,
            FullName: u.FullName ?? string.Empty,
            Phone: u.Phone ?? string.Empty,
            Email: u.Email ?? string.Empty,
            Specialty: string.Empty,     // bảng [User] hiện chưa có cột Specialty
            ExperienceYears: 0,
            RatingAverage: avgRating,
            ReviewCount: reviewCount,
            IsActive: u.Status == StatusActive,
            ActiveStudentsCount: students,
            PendingSessionsCount: pending);
    }

    private async Task<string> GenerateUniqueAvailabilityIdAsync(CancellationToken cancellationToken)
    {
        int count = await _unitOfWork.Repository<PTAvailability>().CountAsync(cancellationToken: cancellationToken);
        string id = $"AVL{count + 1:D7}";
        int attempts = 0;
        while (await _unitOfWork.Repository<PTAvailability>().ExistsAsync(a => a.AvailabilityId == id, cancellationToken) && attempts < 100)
        {
            count++;
            id = $"AVL{count + 1:D7}";
            attempts++;
        }
        return id;
    }

    private async Task<string> GenerateUniqueSessionIdAsync(CancellationToken cancellationToken)
    {
        int count = await _unitOfWork.Repository<PTTrainingSession>().CountAsync(cancellationToken: cancellationToken);
        string id = $"SES{count + 1:D7}";
        int attempts = 0;
        while (await _unitOfWork.Repository<PTTrainingSession>().ExistsAsync(s => s.PTSessionId == id, cancellationToken) && attempts < 100)
        {
            count++;
            id = $"SES{count + 1:D7}";
            attempts++;
        }
        return id;
    }

    private static PtSlotDto MapToSlotDto(PTAvailability s)
    {
        return new PtSlotDto(
            AvailabilityId: s.AvailabilityId,
            AvailableDate: s.AvailableDate,
            StartTime: s.StartTime,
            EndTime: s.EndTime,
            IsBooked: s.IsBooked == 1);
    }

    private static PtSessionDto MapToSessionDto(PTTrainingSession s)
    {
        Enum.TryParse<PtSessionStatus>(s.Status, out var status);
        return new PtSessionDto(
            SessionId: s.PTSessionId,
            PtId: s.PtId,
            PtName: s.PtId,              // placeholder — sẽ dùng JOIN khi có EF Core entity thật
            MemberUserName: s.MemberId,
            MemberFullName: s.MemberId,
            SessionStartTime: s.SessionStartTime.ToDateTime(TimeOnly.MinValue),
            SessionEndTime: s.SessionEndTime.ToDateTime(TimeOnly.MaxValue),
            Status: status,
            WorkoutNotes: s.WorkoutNotes,
            RatingStars: s.RatingStars,
            ReviewComment: s.ReviewComment,
            CompletedAt: s.CompletedAt?.ToDateTime(TimeOnly.MinValue));
    }

    private static InBodyRecordDto MapToInBodyDto(MemberBodyMetric m)
    {
        return new InBodyRecordDto(
            BodyMetricId: m.BodyMetricId,
            MemberUserName: m.UserName,
            WeightKg: m.WeightKg,
            HeightCm: m.HeightCm,
            SkeletalMuscleMass: m.SkeletalMuscleMass,
            BodyFatPercentage: m.BodyFatPercentage,
            Bmi: m.Bmi,
            Notes: m.Notes,
            MeasuredAt: m.MeasuredAt,
            RecordedByPtId: m.RecordedByPtId ?? string.Empty,
            RecordedByPtName: m.RecordedByPtId ?? string.Empty,
            LinkedSessionId: m.PTSessionId);
    }
}
