using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.Exceptions;

namespace GymManagement.Desktop.Services.Api;

public class MockApiClient : IApiClient
{
    private readonly List<MemberDto> _members =
    [
        new("USR001", "Nguyễn Văn An", "an.nguyen@example.com", "0901234567", "ACTIVE", DateTime.Now.AddMonths(-3), "Gói 3 Tháng Thể Thao", DateTime.Now.AddDays(45), 28, 5),
        new("USR002", "Trần Thị Bích", "bich.tran@example.com", "0912345678", "ACTIVE", DateTime.Now.AddMonths(-6), "Gói 1 Năm VIP", DateTime.Now.AddDays(180), 95, 12),
        new("USR003", "Lê Hoàng Cường", "cuong.le@example.com", "0987654321", "FROZEN", DateTime.Now.AddMonths(-2), "Gói 1 Tháng Tiêu Chuẩn", DateTime.Now.AddDays(10), 4, 0),
        new("USR004", "Phạm Thị Dung", "dung.pham@example.com", "0933334444", "EXPIRED", DateTime.Now.AddMonths(-5), "Gói 1 Tháng Tiêu Chuẩn", DateTime.Now.AddDays(-5), 0, 0),
        new("USR005", "Vũ Minh Đức", "duc.vu@example.com", "0944445555", "ACTIVE", DateTime.Now.AddMonths(-1), "Gói 6 Tháng Platinum", DateTime.Now.AddDays(140), 62, 8)
    ];

    private readonly List<PackagePlanDto> _packages =
    [
        new("PLAN01", "Gói 1 Tháng Tiêu Chuẩn", "Tập gym không giới hạn giờ trong 30 ngày", 500000m, 30, 30, 0, false, "ACTIVE"),
        new("PLAN02", "Gói 3 Tháng Thể Thao", "Gồm 90 ngày tập gym kèm 5 buổi PT định hướng", 1350000m, 90, 90, 5, true, "ACTIVE"),
        new("PLAN03", "Gói 6 Tháng Platinum", "Tập gym 180 ngày kèm 10 buổi PT và tủ locker riêng", 2500000m, 180, 180, 10, true, "ACTIVE"),
        new("PLAN04", "Gói 1 Năm VIP", "Tập gym 365 ngày kèm 20 buổi PT và phòng xông hơi", 4500000m, 365, 365, 20, true, "ACTIVE")
    ];

    private readonly List<UserSessionDto> _users =
    [
        new("ADMIN01", "Quản Trị Viên Trưởng", "admin@gymsystem.vn", "Admin"),
        new("ADMIN02", "Quản Lý Vận Hành", "manager@gymsystem.vn", "Admin"),
        new("PT001", "HLV Nguyễn Quốc Huy", "huy.pt@gymsystem.vn", "PT"),
        new("PT002", "HLV Hoàng Tuấn Anh", "anh.pt@gymsystem.vn", "PT")
    ];

    private readonly List<PtSessionSlotDto> _ptSlots =
    [
        new("SESS01", "PT001", "HLV Nguyễn Quốc Huy", "USR001", "Nguyễn Văn An", DateTime.Today, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), "SCHEDULED"),
        new("SESS02", "PT001", "HLV Nguyễn Quốc Huy", "USR002", "Trần Thị Bích", DateTime.Today, new TimeSpan(14, 0, 0), new TimeSpan(15, 0, 0), "SCHEDULED"),
        new("SESS03", "PT002", "HLV Hoàng Tuấn Anh", "USR005", "Vũ Minh Đức", DateTime.Today.AddDays(1), new TimeSpan(16, 0, 0), new TimeSpan(17, 0, 0), "SCHEDULED")
    ];

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("Tên đăng nhập và mật khẩu không được để trống.");

        if (request.Password != "123456" && request.Password != "admin")
            throw new BusinessRuleException("Tên đăng nhập hoặc mật khẩu không chính xác.");

        var user = _users.FirstOrDefault(u => u.Email.StartsWith(request.Username, StringComparison.OrdinalIgnoreCase))
                   ?? _users[0];

        return Task.FromResult(new LoginResponse(
            "mock-jwt-token-for-admin-session",
            "mock-refresh-token",
            user.UserId,
            user.FullName,
            user.Role
        ));
    }

    public Task<CheckInResponse> CheckInAsync(CheckInRequest request)
    {
        MemberDto? member = null;

        if (!string.IsNullOrWhiteSpace(request.MemberCode))
        {
            var code = request.MemberCode.Trim();
            member = _members.FirstOrDefault(m => m.UserName == code || m.Phone == code);
        }

        if (member == null && !string.IsNullOrWhiteSpace(request.Phone))
        {
            var phone = request.Phone.Trim();
            member = _members.FirstOrDefault(m => m.Phone == phone || m.UserName == phone);
        }

        if (member == null && !string.IsNullOrWhiteSpace(request.QrCode))
        {
            // Mã QR quét từ webcam
            var qr = request.QrCode.Replace("GYM_MEMBER_", "").Trim();
            member = _members.FirstOrDefault(m => m.UserName == qr || request.QrCode.Contains(m.UserName));
        }

        if (member == null)
            throw new NotFoundException("Không tìm thấy thông tin hội viên tương ứng.");

        if (member.Status == "EXPIRED" || (member.EndDate.HasValue && member.EndDate.Value < DateTime.Today))
            throw new BusinessRuleException($"Gói tập trong tài khoản của hội viên {member.FullName} đã hết hạn vào ngày {member.EndDate:dd/MM/yyyy}.");

        if (member.Status == "FROZEN")
            throw new BusinessRuleException($"Gói tập của hội viên {member.FullName} đang tạm ngưng bảo lưu (FROZEN).");

        if (member.RemainingCheckInSessions <= 0)
            throw new BusinessRuleException($"Gói tập của hội viên {member.FullName} đã hết số lượt vào phòng khả dụng.");

        // Trừ 1 lượt
        var updatedRemaining = member.RemainingCheckInSessions - 1;
        var index = _members.FindIndex(m => m.UserId == member.UserId);
        _members[index] = member with { RemainingCheckInSessions = updatedRemaining };

        return Task.FromResult(new CheckInResponse(
            true,
            member.FullName,
            member.ActivePlanName ?? "Gói Tiêu Chuẩn",
            updatedRemaining,
            member.RemainingPtSessions,
            DateTime.Now,
            "CHECK-IN THÀNH CÔNG • CỔNG TURNSTILE ĐÃ MỞ"
        ));
    }

    public Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? query = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult<IReadOnlyList<MemberDto>>(_members);

        var q = query.Trim().ToLowerInvariant();
        var filtered = _members.Where(m =>
            m.FullName.ToLowerInvariant().Contains(q) ||
            m.Phone.Contains(q) ||
            m.UserId.ToLowerInvariant().Contains(q)).ToList();

        return Task.FromResult<IReadOnlyList<MemberDto>>(filtered);
    }

    public Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        return Task.FromResult<IReadOnlyList<PackagePlanDto>>(_packages);
    }

    public Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var member = _members.FirstOrDefault(m => m.UserId == request.UserId)
                     ?? throw new NotFoundException("Hội viên không tồn tại.");
        var plan = _packages.FirstOrDefault(p => p.PlanId == request.PlanId)
                   ?? throw new NotFoundException("Gói tập không tồn tại.");

        var invoice = new InvoiceDto(
            $"INV{DateTime.Now:yyyyMMddHHmmss}",
            $"HD-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
            plan.Price,
            plan.Price, // TotalAmount == SubtotalAmount == Price (0 discount)
            request.PaymentMethod,
            request.TransactionType,
            DateTime.Now,
            member.FullName,
            plan.Name,
            "Quản Trị Viên"
        );

        // Kích hoạt hoặc gia hạn gói cho hội viên
        var index = _members.FindIndex(m => m.UserId == member.UserId);
        var newEndDate = DateTime.Today.AddDays(plan.DurationDays);

        int additionalGymSessions = 0;
        int additionalPtSessions = 0;
        if (request.TransactionType == "SUBSCRIPTION_RENEWAL")
        {
            additionalGymSessions = member.RemainingCheckInSessions;
            additionalPtSessions = member.RemainingPtSessions;
        }

        var newGymSessions = (plan.MaxCheckInSessions ?? 30) + additionalGymSessions;
        var newPtSessions = (plan.TotalPtSessions ?? 0) + additionalPtSessions;

        _members[index] = member with
        {
            Status = "ACTIVE",
            ActivePlanName = plan.Name,
            EndDate = newEndDate,
            RemainingCheckInSessions = newGymSessions,
            RemainingPtSessions = newPtSessions
        };

        return Task.FromResult(invoice);
    }

    public Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceId, decimal amount, string transferContent)
    {
        // Sinh link VietQR chuẩn Napas 247 QuickLink
        var encodedContent = Uri.EscapeDataString(transferContent);
        var qrUrl = $"https://img.vietqr.io/image/MB-0901234567-compact2.png?amount={amount}&addInfo={encodedContent}&accountName=GYM%20MANAGEMENT";

        return Task.FromResult(new VietQrPayloadDto(
            qrUrl,
            "0901234567",
            "MB Bank (Ngân hàng Quân Đội)",
            amount,
            transferContent,
            600
        ));
    }

    public Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request)
    {
        var member = _members.FirstOrDefault(m => m.UserId == request.SubscriptionId || m.UserId == "USR001");
        if (member == null) return Task.FromResult(false);

        var days = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
        var index = _members.FindIndex(m => m.UserId == member.UserId);
        _members[index] = member with
        {
            Status = "FROZEN",
            EndDate = (member.EndDate ?? DateTime.Today).AddDays(days)
        };

        return Task.FromResult(true);
    }

    public Task<InBodyMetricDto> RecordInBodyMetricAsync(InBodyEntryRequest request)
    {
        var hM = request.HeightCm / 100m;
        var bmi = Math.Round(request.WeightKg / (hM * hM), 1);

        var dto = new InBodyMetricDto(
            $"MET{DateTime.Now:mmss}",
            request.HeightCm,
            request.WeightKg,
            request.BodyFatPercentage,
            request.SkeletalMuscleMass,
            bmi,
            request.Notes,
            DateTime.Now,
            "HLV Nguyễn Quốc Huy"
        );

        return Task.FromResult(dto);
    }

    public Task<IReadOnlyList<PtSessionSlotDto>> GetPtScheduleSlotsAsync(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult<IReadOnlyList<PtSessionSlotDto>>(_ptSlots);
    }

    public Task<PtSessionSlotDto> BookPtSlotAsync(BookPtSlotRequest request)
    {
        // Chống trùng lịch (Collision check)
        var collision = _ptSlots.Any(s =>
            s.PtId == request.PtId &&
            s.ScheduledDate.Date == request.ScheduledDate.Date &&
            s.StartTime < request.EndTime &&
            request.StartTime < s.EndTime &&
            s.SessionStatus != "CANCELLED");

        if (collision)
            throw new BusinessRuleException("Xung đột thời gian: Huấn luyện viên đã có lịch dạy ca khác trong khung giờ này!");

        var member = _members.FirstOrDefault(m => m.UserId == request.MemberId);
        var pt = _users.FirstOrDefault(u => u.UserId == request.PtId);

        var newSlot = new PtSessionSlotDto(
            $"SESS{DateTime.Now:mmss}",
            request.PtId,
            pt?.FullName ?? "HLV Phụ Trách",
            request.MemberId,
            member?.FullName ?? "Hội Viên",
            request.ScheduledDate,
            request.StartTime,
            request.EndTime,
            "SCHEDULED"
        );

        _ptSlots.Add(newSlot);
        return Task.FromResult(newSlot);
    }

    public Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var todayRevenue = 15850000m;
        var todayCheckIns = 142;
        var newMembers = 38;
        var completedPt = 24;

        var peakHours = new List<PeakHourTrafficDto>
        {
            new(6, 12), new(7, 18), new(8, 15), new(9, 8), new(10, 6),
            new(11, 10), new(12, 14), new(14, 9), new(15, 16), new(16, 25),
            new(17, 48), new(18, 55), new(19, 52), new(20, 34), new(21, 12)
        };

        return Task.FromResult(new DashboardSummaryDto(
            todayRevenue,
            todayCheckIns,
            newMembers,
            completedPt,
            peakHours
        ));
    }

    public Task<IReadOnlyList<UserSessionDto>> GetUsersAsync()
    {
        return Task.FromResult<IReadOnlyList<UserSessionDto>>(_users);
    }

    public Task<bool> ToggleUserLockAsync(string userId)
    {
        return Task.FromResult(true);
    }

    public Task<PackagePlanDto> CreatePackagePlanAsync(PackagePlanDto plan)
    {
        _packages.Add(plan);
        return Task.FromResult(plan);
    }

    public Task<UserSessionDto> CreateUserAsync(string fullName, string email, string phone, string username, string role)
    {
        var newUser = new UserSessionDto($"USR{DateTime.Now:mmss}", fullName, email, role);
        _users.Add(newUser);
        return Task.FromResult(newUser);
    }
}

