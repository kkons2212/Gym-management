using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.DTOs.Settings;
using GymManagement.Server.Core.Enums;
using GymManagement.Server.Core.Exceptions;

namespace GymManagement.Desktop.Services.Api;

public class MockApiClient : IApiClient
{
    private readonly List<MemberDto> _members =
    [
        new("USR001", "Nguyễn Văn An", "0901234567", "an.nguyen@example.com", "ACTIVE", DateTime.Now.AddMonths(-3), "Gói 3 Tháng Thể Thao", DateTime.Now.AddDays(45), 28, 5),
        new("USR002", "Trần Thị Bích", "0912345678", "bich.tran@example.com", "ACTIVE", DateTime.Now.AddMonths(-6), "Gói 1 Năm VIP", DateTime.Now.AddDays(180), 95, 12),
        new("USR003", "Lê Hoàng Cường", "0987654321", "cuong.le@example.com", "FROZEN", DateTime.Now.AddMonths(-2), "Gói 1 Tháng Tiêu Chuẩn", DateTime.Now.AddDays(10), 4, 0),
        new("USR004", "Phạm Thị Dung", "0933334444", "dung.pham@example.com", "EXPIRED", DateTime.Now.AddMonths(-5), "Gói 1 Tháng Tiêu Chuẩn", DateTime.Now.AddDays(-5), 0, 0),
        new("USR005", "Vũ Minh Đức", "0944445555", "duc.vu@example.com", "ACTIVE", DateTime.Now.AddMonths(-1), "Gói 6 Tháng Platinum", DateTime.Now.AddDays(140), 62, 8)
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
        {
            throw new ValidationException("Tên đăng nhập và mật khẩu không được để trống.");
        }

        if (request.Password != "123456" && request.Password != "admin")
        {
            throw new BusinessRuleException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

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
            var qr = request.QrCode.Replace("GYM_MEMBER_", "").Trim();
            member = _members.FirstOrDefault(m => m.UserName == qr || request.QrCode.Contains(m.UserName));
        }

        if (member == null)
        {
            throw new NotFoundException("Không tìm thấy thông tin hội viên tương ứng.");
        }

        if (member.Status == "EXPIRED" || (member.EndDate.HasValue && member.EndDate.Value < DateTime.Today))
        {
            throw new BusinessRuleException($"Gói tập trong tài khoản của hội viên {member.FullName} đã hết hạn vào ngày {member.EndDate:dd/MM/yyyy}.");
        }

        if (member.Status == "FROZEN")
        {
            throw new BusinessRuleException($"Gói tập của hội viên {member.FullName} đang tạm ngưng bảo lưu (FROZEN).");
        }

        if (member.RemainingCheckInSessions <= 0)
        {
            throw new BusinessRuleException($"Gói tập của hội viên {member.FullName} đã hết số lượt vào phòng khả dụng.");
        }

        var index = _members.FindIndex(m => m.UserName == member.UserName);
        _members[index] = member with { RemainingCheckInSessions = member.RemainingCheckInSessions - 1 };

        return Task.FromResult(new CheckInResponse(
            true,
            member.FullName,
            member.ActivePlanName ?? "Gói Tiêu Chuẩn",
            member.RemainingCheckInSessions - 1,
            member.RemainingPtSessions,
            DateTime.Now,
            $"CHECK-IN THÀNH CÔNG • HỘI VIÊN {member.FullName.ToUpper()}"
        ));
    }

    public Task<IReadOnlyList<CheckInLogDto>> GetTodayLogsAsync(string? searchQuery = null, string? statusFilter = null)
    {
        var logs = new List<CheckInLogDto>
        {
            new("LOG001", DateTime.Now.AddMinutes(-15), "QR_CODE", null, "Nguyễn Văn An", "admin", "SUCCESS"),
            new("LOG002", DateTime.Now.AddMinutes(-45), "QR_CODE", null, "Trần Thị Bích", "admin", "SUCCESS"),
            new("LOG003", DateTime.Now.AddHours(-1), "MANUAL", "Override: Quên điện thoại", "Vũ Minh Đức", "admin", "SUCCESS"),
            new("LOG004", DateTime.Now.AddHours(-2), "QR_CODE", null, "Phạm Thị Dung", "admin", "FAILED", "Gói tập trong tài khoản đã hết hạn")
        };

        return Task.FromResult<IReadOnlyList<CheckInLogDto>>(logs);
    }

    public Task<int> GetTodayAttendanceCountAsync()
    {
        return Task.FromResult(3);
    }

    public Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? query = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<MemberDto>>(_members);
        }

        var q = query.Trim();
        var filtered = _members.Where(m =>
            m.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            m.Phone.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            m.UserName.Contains(q, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        return Task.FromResult<IReadOnlyList<MemberDto>>(filtered);
    }

    public Task<MemberProfileResponse> RegisterMemberAsync(RegisterMemberRequest request)
    {
        var member = new MemberDto(
            $"USR{DateTime.Now:mmss}",
            request.FullName,
            request.Phone,
            request.Email,
            "ACTIVE",
            DateTime.Now,
            "Gói Tập Mới",
            DateTime.Today.AddDays(30),
            30,
            0
        );
        _members.Add(member);

        return Task.FromResult(new MemberProfileResponse(
            member.UserName,
            member.FullName,
            member.Phone,
            member.Email,
            member.Status,
            null
        ));
    }

    public Task<MemberProfileResponse> UpdateMemberProfileAsync(string userName, UpdateMemberRequest request)
    {
        var idx = _members.FindIndex(m => m.UserName == userName);
        if (idx >= 0)
        {
            _members[idx] = _members[idx] with
            {
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email
            };
        }

        return Task.FromResult(new MemberProfileResponse(
            userName,
            request.FullName,
            request.Phone,
            request.Email,
            "ACTIVE",
            null
        ));
    }

    public Task<SubscriptionDetailResponse> RenewSubscriptionAsync(string userName, RenewSubscriptionRequest request)
    {
        return Task.FromResult(new SubscriptionDetailResponse(
            request.SubscriptionId,
            request.NewPlanId,
            "Gói 1 Tháng Tiêu Chuẩn",
            "QR_MOCK",
            request.NewStartDate,
            request.NewStartDate.AddDays(30),
            30,
            0,
            SubscriptionStatus.Active,
            null,
            null,
            null
        ));
    }

    public Task<SubscriptionDetailResponse> UpgradeSubscriptionAsync(string userName, UpgradeSubscriptionRequest request)
    {
        return Task.FromResult(new SubscriptionDetailResponse(
            request.SubscriptionId,
            request.UpgradeToPlanId,
            "Gói Nâng Cấp VIP",
            "QR_MOCK",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(90)),
            90,
            5,
            SubscriptionStatus.Active,
            null,
            null,
            null
        ));
    }

    public Task<bool> CancelSubscriptionAsync(CancelSubscriptionRequest request)
    {
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        return Task.FromResult<IReadOnlyList<PackagePlanDto>>(_packages);
    }

    public Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var pkg = _packages.FirstOrDefault(p => p.PlanId == request.PlanId) ?? _packages[0];
        var member = _members.FirstOrDefault(m => m.UserName == request.UserId) ?? _members[0];

        var invoice = new InvoiceDto(
            $"INV{DateTime.Now:mmss}",
            $"INV-20261108-{DateTime.Now:fff}",
            pkg.Price,
            pkg.Price,
            request.PaymentMethod,
            request.TransactionType,
            DateTime.Now,
            member.FullName,
            pkg.Name,
            "Admin Quầy",
            "PAID"
        );

        return Task.FromResult(invoice);
    }

    public Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceCode, decimal amount, string transferContent)
    {
        return Task.FromResult(new VietQrPayloadDto(
            $"https://api.vietqr.io/image/970422-0901234567-print.jpg?amount={amount}&addInfo={Uri.EscapeDataString(transferContent)}",
            "0901234567",
            "MB Bank (Napas 247)",
            amount,
            transferContent,
            600
        ));
    }

    public Task<InvoiceDto> ConfirmPaymentAsync(string invoiceCode)
    {
        return Task.FromResult(new InvoiceDto(
            "INV001",
            invoiceCode,
            500000m,
            500000m,
            "VIETQR",
            "SUBSCRIPTION_PURCHASE",
            DateTime.Now,
            "Nguyễn Văn An",
            "Gói Tập Tiêu Chuẩn",
            "Admin Quầy",
            "PAID"
        ));
    }

    public Task<InvoiceDto> RefundInvoiceAsync(string invoiceCode, decimal refundAmount, string reason)
    {
        return Task.FromResult(new InvoiceDto(
            "INV001",
            invoiceCode,
            500000m,
            500000m,
            "VIETQR",
            "SUBSCRIPTION_PURCHASE",
            DateTime.Now,
            "Nguyễn Văn An",
            "Gói Tập Tiêu Chuẩn",
            "Admin Quầy",
            "REFUNDED",
            refundAmount
        ));
    }

    public Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request)
    {
        var member = _members.FirstOrDefault(m => m.UserName == request.SubscriptionId || m.UserName == "USR001");
        if (member == null)
        {
            return Task.FromResult(false);
        }

        var days = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
        var index = _members.FindIndex(m => m.UserName == member.UserName);
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
        var collision = _ptSlots.Any(s =>
            s.PtId == request.PtId &&
            s.ScheduledDate.Date == request.ScheduledDate.Date &&
            s.StartTime < request.EndTime &&
            request.StartTime < s.EndTime &&
            s.SessionStatus != "CANCELLED");

        if (collision)
        {
            throw new BusinessRuleException("Xung đột thời gian: Huấn luyện viên đã có lịch dạy ca khác trong khung giờ này!");
        }

        var member = _members.FirstOrDefault(m => m.UserName == request.MemberId);
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

    public Task<bool> AssignPtAsync(AssignPtRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(true);
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

    public Task<byte[]> ExportExcelReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return Task.FromResult(new byte[] { 0x50, 0x4B, 0x03, 0x04 });
    }

    public Task<byte[]> ExportPdfReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 });
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
