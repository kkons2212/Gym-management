using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.Enums;
using GymManagement.Server.Core.Exceptions;

namespace GymManagement.Web.Services;

/// <summary>
/// Mock API Client cung cấp dữ liệu giả lập chuẩn xác cho Cổng Web Blazor Server.
/// Hoạt động độc lập, tuân thủ 100% mô hình dữ liệu vật lý 9 bảng.
/// </summary>
public sealed class MockWebApiClient : IWebApiClient
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    // Dữ liệu giả lập
    private readonly List<UserSessionDto> _users =
    [
        new("member01", "Trần Văn An", "an.tran@example.com", "Member"),
        new("member02", "Lê Thị Bích", "bich.le@example.com", "Member"),
        new("pt01", "Nguyễn Văn Thành", "thanh.pt@gym.com", "PT"),
        new("admin", "Quản Trị Viên", "admin@gym.com", "Admin")
    ];

    private readonly List<PackagePlanDto> _packages =
    [
        new("PLAN01", "Gói Gym Tiêu Chuẩn 1 Tháng", "Tập gym không giới hạn giờ giấc", 800000m, 30, 30, 0, false, "Active"),
        new("PLAN02", "Gói Gym & PT Cao Cấp 3 Tháng", "Tập gym 3 tháng kèm 12 buổi HLV cá nhân 1-1", 3600000m, 90, 90, 12, true, "Active"),
        new("PLAN03", "Gói VIP Toàn Diện 6 Tháng", "Tập gym 6 tháng kèm 24 buổi HLV cá nhân 1-1", 6800000m, 180, 180, 24, true, "Active")
    ];

    private MemberSubscriptionDetailDto _memberSubscription = new(
        SubscriptionId: "SUB001",
        UserName: "member01",
        MemberFullName: "Trần Văn An",
        PlanId: "PLAN02",
        PlanName: "Gói Gym & PT Cao Cấp 3 Tháng",
        StartDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-15)),
        EndDate: DateOnly.FromDateTime(DateTime.Today.AddDays(75)),
        RemainingCheckInSessions: 24,
        RemainingPtSessions: 8,
        Status: "ACTIVE",
        AssignedPtId: "pt01",
        AssignedPtName: "Nguyễn Văn Thành",
        FreezeStartDate: null,
        FreezeEndDate: null
    );

    private readonly List<PtSlotDto> _ptSlots = [];
    private readonly List<PtSessionDto> _ptSessions = [];
    private readonly List<InBodyRecordDto> _inBodyRecords = [];
    private readonly List<CheckInLogDto> _checkInLogs = [];

    public MockWebApiClient(CustomAuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // Khởi tạo slots rảnh cho PT tuần này và tuần sau
        for (int dayOffset = 0; dayOffset < 14; dayOffset++)
        {
            DateOnly slotDate = today.AddDays(dayOffset);
            _ptSlots.Add(new PtSlotDto($"SLOT_{dayOffset}_1", slotDate, new TimeOnly(8, 0), new TimeOnly(9, 0), dayOffset == 1));
            _ptSlots.Add(new PtSlotDto($"SLOT_{dayOffset}_2", slotDate, new TimeOnly(9, 30), new TimeOnly(10, 30), false));
            _ptSlots.Add(new PtSlotDto($"SLOT_{dayOffset}_3", slotDate, new TimeOnly(14, 0), new TimeOnly(15, 0), false));
            _ptSlots.Add(new PtSlotDto($"SLOT_{dayOffset}_4", slotDate, new TimeOnly(17, 30), new TimeOnly(18, 30), dayOffset == 2));
            _ptSlots.Add(new PtSlotDto($"SLOT_{dayOffset}_5", slotDate, new TimeOnly(19, 0), new TimeOnly(20, 0), false));
        }

        // Khởi tạo các ca tập
        _ptSessions.Add(new PtSessionDto(
            SessionId: "SES001",
            PtId: "pt01",
            PtName: "Nguyễn Văn Thành",
            MemberUserName: "member01",
            MemberFullName: "Trần Văn An",
            SessionStartTime: DateTime.Today.AddDays(1).AddHours(8),
            SessionEndTime: DateTime.Today.AddDays(1).AddHours(9),
            Status: PtSessionStatus.Booked,
            WorkoutNotes: "Tập nhóm cơ ngực và tay sau",
            RatingStars: null,
            ReviewComment: null,
            CompletedAt: null
        ));

        _ptSessions.Add(new PtSessionDto(
            SessionId: "SES002",
            PtId: "pt01",
            PtName: "Nguyễn Văn Thành",
            MemberUserName: "member01",
            MemberFullName: "Trần Văn An",
            SessionStartTime: DateTime.Today.AddDays(-3).AddHours(17).AddMinutes(30),
            SessionEndTime: DateTime.Today.AddDays(-3).AddHours(18).AddMinutes(30),
            Status: PtSessionStatus.Completed,
            WorkoutNotes: "Hoàn thành bài tập chân mông (Leg Day), thể lực tốt",
            RatingStars: 5,
            ReviewComment: "HLV hướng dẫn rất nhiệt tình và sửa kỹ tư thế Squat!",
            CompletedAt: DateTime.Today.AddDays(-3).AddHours(18).AddMinutes(35)
        ));

        // Khởi tạo chỉ số InBody
        _inBodyRecords.Add(new InBodyRecordDto(
            BodyMetricId: "INB001",
            MemberUserName: "member01",
            WeightKg: 72.5m,
            HeightCm: 175m,
            SkeletalMuscleMass: 33.2m,
            BodyFatPercentage: 18.5m,
            Bmi: 23.7m,
            Notes: "Lần đo đầu tiên khi bắt đầu gói tập",
            MeasuredAt: today.AddDays(-14),
            RecordedByPtId: "pt01",
            RecordedByPtName: "Nguyễn Văn Thành",
            LinkedSessionId: null
        ));

        _inBodyRecords.Add(new InBodyRecordDto(
            BodyMetricId: "INB002",
            MemberUserName: "member01",
            WeightKg: 71.0m,
            HeightCm: 175m,
            SkeletalMuscleMass: 33.8m,
            BodyFatPercentage: 17.2m,
            Bmi: 23.2m,
            Notes: "Giảm mỡ tốt, cơ bắp tăng nhẹ",
            MeasuredAt: today.AddDays(-1),
            RecordedByPtId: "pt01",
            RecordedByPtName: "Nguyễn Văn Thành",
            LinkedSessionId: "SES002"
        ));

        // Lịch sử check-in điểm danh
        _checkInLogs.Add(new CheckInLogDto("CK001", DateTime.Today.AddDays(-3).AddHours(17).AddMinutes(15), "Quét camera tại quầy", null, "Trần Văn An", "Lễ Tân 01", "SUCCESS"));
        _checkInLogs.Add(new CheckInLogDto("CK002", DateTime.Today.AddDays(-5).AddHours(18).AddMinutes(0), "Quét camera tại quầy", null, "Trần Văn An", "Lễ Tân 01", "SUCCESS"));
        _checkInLogs.Add(new CheckInLogDto("CK003", DateTime.Today.AddDays(-8).AddHours(7).AddMinutes(30), "Quét camera tại quầy", null, "Trần Văn An", "Lễ Tân 02", "SUCCESS"));
    }

    // ==========================================
    // 1. AUTHENTICATION (UC01)
    // ==========================================
    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = _users.FirstOrDefault(u =>
            string.Equals(u.UserId, request.Username, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(u.Email, request.Username, StringComparison.OrdinalIgnoreCase));

        if (user == null || request.Password != "123456")
        {
            throw new BusinessRuleException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        _authStateProvider.SetAuthenticated(user);

        return Task.FromResult(new LoginResponse(
            Token: "mock-jwt-token",
            RefreshToken: "mock-refresh-token",
            UserId: user.UserId,
            FullName: user.FullName,
            Role: user.Role
        ));
    }

    public Task<UserSessionDto?> GetCurrentUserSessionAsync()
    {
        return Task.FromResult(_authStateProvider.CurrentUser);
    }

    // ==========================================
    // 2. MEMBER PROFILE & SUBSCRIPTION (UC09, UC03)
    // ==========================================
    public Task<MemberProfileResponse?> GetMyProfileAsync()
    {
        var current = _authStateProvider.CurrentUser;
        if (current == null) return Task.FromResult<MemberProfileResponse?>(null);

        return Task.FromResult<MemberProfileResponse?>(new MemberProfileResponse(
            UserName: current.UserId,
            FullName: current.FullName,
            Phone: "0901234567",
            Email: current.Email,
            Status: "ACTIVE",
            ActiveSubscription: new SubscriptionDetailResponse(
                SubscriptionId: _memberSubscription.SubscriptionId,
                PlanId: _memberSubscription.PlanId,
                PlanName: _memberSubscription.PlanName,
                QrCode: "GYM_MEMBER_" + current.UserId,
                StartDate: _memberSubscription.StartDate,
                EndDate: _memberSubscription.EndDate,
                RemainingCheckInSessions: _memberSubscription.RemainingCheckInSessions,
                RemainingPtSessions: _memberSubscription.RemainingPtSessions,
                Status: SubscriptionStatus.Active,
                FreezeStartDate: _memberSubscription.FreezeStartDate,
                FreezeEndDate: _memberSubscription.FreezeEndDate,
                AssignedPtId: _memberSubscription.AssignedPtId
            )
        ));
    }

    public Task<MemberSubscriptionDetailDto?> GetMySubscriptionAsync()
    {
        return Task.FromResult<MemberSubscriptionDetailDto?>(_memberSubscription);
    }

    public Task<string> GetMyCheckInQrCodeAsync()
    {
        var user = _authStateProvider.CurrentUser;
        string userName = user?.UserId ?? "member01";
        return Task.FromResult(userName);
    }

    public Task<string> RefreshCheckInQrCodeAsync()
    {
        return GetMyCheckInQrCodeAsync();
    }

    // ==========================================
    // 3. PACKAGES & VIETQR (UC02, UC05)
    // ==========================================
    public Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        return Task.FromResult<IReadOnlyList<PackagePlanDto>>(_packages);
    }

    public Task<VietQrPayloadDto> CreateVietQrInvoiceAsync(string planId)
    {
        var plan = _packages.FirstOrDefault(p => p.PlanId == planId)
            ?? throw new NotFoundException("Gói tập không tồn tại.");

        var user = _authStateProvider.CurrentUser;
        string invoiceId = $"INV_{DateTime.Now:yyyyMMddHHmmss}";
        string content = $"GYM {user?.UserId ?? "member01"} {plan.PlanId}";

        return Task.FromResult(new VietQrPayloadDto(
            QrDataUrl: $"https://api.vietqr.io/image/970422-0901234567-compact2.png?amount={plan.Price:0}&addInfo={content}",
            BankAccount: "0901234567",
            BankName: "MB Bank",
            Amount: plan.Price,
            TransferContent: content,
            ExpirySeconds: 600,
            InvoiceId: invoiceId
        ));
    }

    public Task<bool> SimulateVietQrPaymentAsync(string invoiceId)
    {
        _memberSubscription = _memberSubscription with
        {
            Status = "ACTIVE",
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(90)),
            RemainingCheckInSessions = _memberSubscription.RemainingCheckInSessions + 30,
            RemainingPtSessions = _memberSubscription.RemainingPtSessions + 12
        };
        return Task.FromResult(true);
    }

    // ==========================================
    // 4. PT BOOKING (UC10)
    // ==========================================
    public Task<IReadOnlyList<PtSlotDto>> GetPtAvailableSlotsAsync(string ptUserName, DateOnly startDate, DateOnly endDate)
    {
        var slots = _ptSlots
            .Where(s => s.AvailableDate >= startDate && s.AvailableDate <= endDate)
            .OrderBy(s => s.AvailableDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        return Task.FromResult<IReadOnlyList<PtSlotDto>>(slots);
    }

    public Task<PtSessionDto> BookPtSlotAsync(BookSessionRequest request)
    {
        if (_memberSubscription.RemainingPtSessions <= 0)
        {
            throw new BusinessRuleException("Bạn đã hết số buổi tập PT khả dụng trong gói.");
        }

        var slotIndex = _ptSlots.FindIndex(s => s.AvailabilityId == request.AvailabilityId);
        if (slotIndex < 0 || _ptSlots[slotIndex].IsBooked)
        {
            throw new BusinessRuleException("Khung giờ này vừa được đặt hoặc không còn khả dụng.");
        }

        var slot = _ptSlots[slotIndex];
        _ptSlots[slotIndex] = slot with { IsBooked = true };

        _memberSubscription = _memberSubscription with
        {
            RemainingPtSessions = _memberSubscription.RemainingPtSessions - 1
        };

        var newSession = new PtSessionDto(
            SessionId: $"SES_{DateTime.Now:mmss}",
            PtId: "pt01",
            PtName: "Nguyễn Văn Thành",
            MemberUserName: request.MemberUserName,
            MemberFullName: _memberSubscription.MemberFullName,
            SessionStartTime: slot.AvailableDate.ToDateTime(slot.StartTime),
            SessionEndTime: slot.AvailableDate.ToDateTime(slot.EndTime),
            Status: PtSessionStatus.Booked,
            WorkoutNotes: request.WorkoutNotes,
            RatingStars: null,
            ReviewComment: null,
            CompletedAt: null
        );

        _ptSessions.Add(newSession);
        return Task.FromResult(newSession);
    }

    public Task<bool> CancelPtBookingAsync(CancelSessionRequest request)
    {
        var sessionIndex = _ptSessions.FindIndex(s => s.SessionId == request.SessionId);
        if (sessionIndex < 0) throw new NotFoundException("Ca tập không tồn tại.");

        var session = _ptSessions[sessionIndex];
        var hoursUntilStart = (session.SessionStartTime - DateTime.Now).TotalHours;

        if (hoursUntilStart < 2 && !request.ForceLateCancellation)
        {
            throw new BusinessRuleException("Hủy ca trước dưới 2 giờ sẽ không được hoàn buổi tập. Vui lòng xác nhận hủy trễ.");
        }

        _ptSessions[sessionIndex] = session with { Status = PtSessionStatus.Cancelled };

        // Nếu hủy đúng hạn >= 2 tiếng thì hoàn lại buổi
        if (hoursUntilStart >= 2)
        {
            _memberSubscription = _memberSubscription with
            {
                RemainingPtSessions = _memberSubscription.RemainingPtSessions + 1
            };
        }

        return Task.FromResult(true);
    }

    public Task<bool> ReschedulePtBookingAsync(RescheduleSessionRequest request)
    {
        var sessionIndex = _ptSessions.FindIndex(s => s.SessionId == request.SessionId);
        if (sessionIndex < 0) throw new NotFoundException("Ca tập không tồn tại.");

        var newSlotIndex = _ptSlots.FindIndex(s => s.AvailabilityId == request.NewAvailabilityId);
        if (newSlotIndex < 0 || _ptSlots[newSlotIndex].IsBooked)
        {
            throw new BusinessRuleException("Khung giờ mới đã được đặt hoặc không khả dụng.");
        }

        var newSlot = _ptSlots[newSlotIndex];
        _ptSlots[newSlotIndex] = newSlot with { IsBooked = true };

        var oldSession = _ptSessions[sessionIndex];
        _ptSessions[sessionIndex] = oldSession with
        {
            SessionStartTime = newSlot.AvailableDate.ToDateTime(newSlot.StartTime),
            SessionEndTime = newSlot.AvailableDate.ToDateTime(newSlot.EndTime)
        };

        return Task.FromResult(true);
    }

    public Task<bool> SubmitRatingAsync(SubmitRatingRequest request)
    {
        var sessionIndex = _ptSessions.FindIndex(s => s.SessionId == request.SessionId);
        if (sessionIndex < 0) throw new NotFoundException("Ca tập không tồn tại.");

        var session = _ptSessions[sessionIndex];
        _ptSessions[sessionIndex] = session with
        {
            RatingStars = request.RatingStars,
            ReviewComment = request.ReviewComment
        };

        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<PtSessionDto>> GetMyPtSessionsAsync()
    {
        return Task.FromResult<IReadOnlyList<PtSessionDto>>(_ptSessions.OrderByDescending(s => s.SessionStartTime).ToList());
    }

    // ==========================================
    // 5. INBODY & HISTORY (UC06, UC09)
    // ==========================================
    public Task<IReadOnlyList<InBodyRecordDto>> GetMyInBodyHistoryAsync()
    {
        return Task.FromResult<IReadOnlyList<InBodyRecordDto>>(_inBodyRecords.OrderByDescending(r => r.MeasuredAt).ToList());
    }

    public Task<IReadOnlyList<CheckInLogDto>> GetMyCheckInHistoryAsync()
    {
        return Task.FromResult<IReadOnlyList<CheckInLogDto>>(_checkInLogs.OrderByDescending(l => l.CheckInTime).ToList());
    }

    // ==========================================
    // 6. PT OPERATIONS (UC08)
    // ==========================================
    public Task<IReadOnlyList<PtSlotDto>> GetMyAvailabilitySlotsAsync(DateOnly startDate, DateOnly endDate)
    {
        return GetPtAvailableSlotsAsync("pt01", startDate, endDate);
    }

    public Task<bool> SavePtAvailabilitySlotsAsync(SavePtSlotsRequest request)
    {
        foreach (var update in request.Slots)
        {
            var existing = _ptSlots.FirstOrDefault(s =>
                s.AvailableDate == update.AvailableDate &&
                s.StartTime == update.StartTime &&
                s.EndTime == update.EndTime);

            if (existing != null)
            {
                if (existing.IsBooked && !update.Register)
                {
                    throw new BusinessRuleException($"Khung giờ {update.StartTime} ngày {update.AvailableDate:dd/MM/yyyy} đã có học viên đặt lịch, không thể hủy.");
                }

                if (!update.Register)
                {
                    _ptSlots.Remove(existing);
                }
            }
            else if (update.Register)
            {
                _ptSlots.Add(new PtSlotDto(
                    $"SLOT_{DateTime.Now.Ticks}",
                    update.AvailableDate,
                    update.StartTime,
                    update.EndTime,
                    false
                ));
            }
        }

        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<PtSessionDto>> GetPtMyWeeklyScheduleAsync(DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

        var sessions = _ptSessions
            .Where(s => s.SessionStartTime >= startDateTime && s.SessionStartTime <= endDateTime)
            .OrderBy(s => s.SessionStartTime)
            .ToList();

        return Task.FromResult<IReadOnlyList<PtSessionDto>>(sessions);
    }

    public Task<bool> CompletePtSessionAsync(PtCompleteSessionRequest request)
    {
        var index = _ptSessions.FindIndex(s => s.SessionId == request.SessionId);
        if (index < 0) throw new NotFoundException("Ca tập không tồn tại.");

        var session = _ptSessions[index];
        if (session.SessionStartTime > DateTime.Now)
        {
            throw new BusinessRuleException("Ca tập chưa đến giờ diễn ra, không thể xác nhận hoàn thành.");
        }

        _ptSessions[index] = session with
        {
            Status = PtSessionStatus.Completed,
            CompletedAt = DateTime.Now,
            WorkoutNotes = request.WorkoutNotes ?? session.WorkoutNotes
        };

        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<MemberSummaryDto>> GetPtAssignedStudentsAsync()
    {
        IReadOnlyList<MemberSummaryDto> students =
        [
            new("member01", "Trần Văn An", "0901234567", "an.tran@example.com", _memberSubscription.RemainingPtSessions, "ACTIVE"),
            new("member02", "Lê Thị Bích", "0912345678", "bich.le@example.com", 4, "ACTIVE")
        ];

        return Task.FromResult(students);
    }

    public Task<bool> RecordInBodyAsync(RecordInBodyRequest request)
    {
        var record = new InBodyRecordDto(
            BodyMetricId: $"INB_{DateTime.Now:mmss}",
            MemberUserName: request.MemberUserName,
            WeightKg: request.WeightKg,
            HeightCm: request.HeightCm,
            SkeletalMuscleMass: request.SkeletalMuscleMass,
            BodyFatPercentage: request.BodyFatPercentage,
            Bmi: request.Bmi,
            Notes: request.Notes,
            MeasuredAt: DateOnly.FromDateTime(DateTime.Today),
            RecordedByPtId: request.PtUserName,
            RecordedByPtName: "Nguyễn Văn Thành",
            LinkedSessionId: request.LinkedSessionId
        );

        _inBodyRecords.Insert(0, record);
        return Task.FromResult(true);
    }
}
