using GymManagement.Core.Contracts;
using GymManagement.Core.DTOs;
using GymManagement.Core.Enums;

namespace GymManagement.Core.Services;

public class MockGymService : 
    IAuthService, 
    IMemberService, 
    ICheckInService, 
    IPtService, 
    IBillingService, 
    IReportService, 
    ISystemSettingService
{
    private readonly List<MemberDto> _members =
    [
        new("MB-0248", "Nguyễn Minh Anh", "0901 234 567", "minhanh@email.com", "079012340001", new DateOnly(1996, 5, 12), "Quận 1, TP. Hồ Chí Minh", null, MemberStatus.Active, "Premium (12 tháng)", new DateOnly(2027, 4, 18), 8),
        new("MB-0247", "Trần Quốc Bảo", "0903 456 789", "baotran@email.com", "079012340002", new DateOnly(1992, 11, 20), "Quận 3, TP. Hồ Chí Minh", null, MemberStatus.Active, "Gym + PT (12 buổi)", new DateOnly(2027, 3, 2), 5),
        new("MB-0246", "Lê Hoàng Nam", "0905 111 246", "nam.le@email.com", "079012340003", new DateOnly(1990, 8, 15), "Thủ Đức, TP. Hồ Chí Minh", null, MemberStatus.Expired, "Standard (6 tháng)", new DateOnly(2025, 4, 17), 0),
        new("MB-0245", "Phạm Thu Hà", "0908 222 345", "ha.pham@email.com", "079012340004", new DateOnly(1998, 2, 28), "Quận Bình Thạnh, TP. Hồ Chí Minh", null, MemberStatus.Frozen, "Premium (12 tháng)", new DateOnly(2026, 10, 28), 12),
        new("MB-0244", "Đỗ Gia Huy", "0906 333 444", "huy.do@email.com", "079012340005", new DateOnly(1994, 7, 9), "Quận 7, TP. Hồ Chí Minh", null, MemberStatus.Active, "Gym + PT (12 buổi)", new DateOnly(2027, 1, 30), 10)
    ];

    private readonly List<PackageDto> _packages =
    [
        new("PKG-01", "Standard", 3600000m, "6 tháng", 0, 7, "Sử dụng khu vực tập gym, tủ đồ cơ bản", 84, true),
        new("PKG-02", "Premium", 6900000m, "12 tháng", 0, 14, "Gym, lớp nhóm Yoga/Zumba, phòng xông hơi", 126, true),
        new("PKG-03", "Gym + PT 1:1", 4800000m, "12 buổi", 12, 7, "12 buổi tập 1 kèm 1 cùng HLV cá nhân", 23, true),
        new("PKG-04", "Yoga Linh Hoạt", 2400000m, "3 tháng", 12, 0, "12 buổi Yoga nhóm chuyên sâu", 0, false)
    ];

    private readonly List<PtProfileDto> _trainers =
    [
        new("PT-01", "Nguyễn Quốc Bảo", "0903 456 789", "bao.nguyen@gymhouse.vn", "Gym & Tăng cơ chuyên sâu", 5, 4.9m, 48, null, true),
        new("PT-02", "Trần Thu Trang", "0907 111 222", "trang.tran@gymhouse.vn", "Giảm cân & Định hình vóc dáng", 4, 4.8m, 35, null, true),
        new("PT-03", "Lê Thanh Tùng", "0908 123 456", "tung.le@gymhouse.vn", "Boxing & Cardio thể lực", 6, 5.0m, 62, null, true)
    ];

    private readonly List<CheckInRecordDto> _checkInLogs =
    [
        new("CK-001", "CARD-0248", "MB-0248", "Nguyễn Minh Anh", "Premium", DateTime.Now.AddMinutes(-45), true, null, "Ca Sáng"),
        new("CK-002", "CARD-0247", "MB-0247", "Trần Quốc Bảo", "Gym + PT", DateTime.Now.AddMinutes(-20), true, null, "Ca Sáng"),
        new("CK-003", "CARD-0246", "MB-0246", "Lê Hoàng Nam", "Standard", DateTime.Now.AddMinutes(-10), false, "Gói tập đã hết hạn", "Ca Sáng")
    ];

    private readonly List<PtSessionDto> _sessions =
    [
        new("BK-101", "PT-01", "Nguyễn Quốc Bảo", "MB-0248", "Nguyễn Minh Anh", DateTime.Today.AddHours(14), DateTime.Today.AddHours(15), BookingStatus.Confirmed, "Buổi 5/12: Tập cơ lưng và vai", null, null),
        new("BK-102", "PT-01", "Nguyễn Quốc Bảo", "MB-0247", "Trần Quốc Bảo", DateTime.Today.AddHours(16), DateTime.Today.AddHours(17), BookingStatus.Confirmed, "Buổi 7/12: Cardio và chân", null, null),
        new("BK-100", "PT-02", "Trần Thu Trang", "MB-0245", "Phạm Thu Hà", DateTime.Today.AddDays(-1).AddHours(9), DateTime.Today.AddDays(-1).AddHours(10), BookingStatus.Completed, "Buổi 1: Đo InBody và khởi động", 5, "PT hướng dẫn rất tận tình!")
    ];

    private readonly List<InBodyRecordDto> _inBodyRecords =
    [
        new("IB-01", "MB-0248", DateTime.Now.AddMonths(-2), 65.2m, 22.4m, 28.5m, "Giai đoạn bắt đầu", "Nguyễn Quốc Bảo"),
        new("IB-02", "MB-0248", DateTime.Now.AddMonths(-1), 63.8m, 20.1m, 29.2m, "Tiến triển tốt", "Nguyễn Quốc Bảo"),
        new("IB-03", "MB-0248", DateTime.Now.AddDays(-5), 62.5m, 18.5m, 30.1m, "Đạt mục tiêu tăng cơ giảm mỡ", "Nguyễn Quốc Bảo")
    ];

    private readonly List<InvoiceDto> _invoices =
    [
        new("INV-2026-001", "MB-0248", "Nguyễn Minh Anh", "Lê Ngọc Mai", 6900000m, 500000m, 6400000m, PaymentMethod.VietQR, InvoiceStatus.Paid, DateTime.Now.AddDays(-15), "Gói Premium 12 tháng"),
        new("INV-2026-002", "MB-0247", "Trần Quốc Bảo", "Hoàng Thu Hà", 4800000m, 0m, 4800000m, PaymentMethod.CardPOS, InvoiceStatus.Paid, DateTime.Now.AddDays(-10), "Gói Gym + PT 12 buổi")
    ];

    // Phân công PT phụ trách học viên (UC 8.2.1) – dữ liệu demo tĩnh
    private readonly Dictionary<string, List<string>> _ptAssignments = new()
    {
        ["PT-01"] = ["MB-0248", "MB-0247"],
        ["PT-02"] = ["MB-0245"],
        ["PT-03"] = ["MB-0244"]
    };

    // Ghi đè trạng thái lịch rảnh do PT tự đăng ký, khóa theo "{ptId}|{slotId}" (UC 8.1.1)
    private readonly Dictionary<string, bool> _ptSlotOverrides = [];

    // Auth
    public Task<(bool Success, UserAccountDto? User, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        var user = new UserAccountDto("USR-01", username, "Admin Quản Trị", "admin@gymhouse.vn", "0908 123 456", UserRole.Admin, true);
        return Task.FromResult<(bool, UserAccountDto?, string?)>((true, user, null));
    }
    public Task LogoutAsync() => Task.CompletedTask;

    // Members
    public Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? searchTerm = null, MemberStatus? status = null)
    {
        var result = _members.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            result = result.Where(m => m.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || m.Phone.Contains(searchTerm));
        if (status.HasValue)
            result = result.Where(m => m.Status == status.Value);
        return Task.FromResult<IReadOnlyList<MemberDto>>(result.ToList());
    }

    public Task<MemberDto?> GetMemberByCodeOrPhoneAsync(string query)
    {
        var member = _members.FirstOrDefault(m => m.Phone == query || m.MemberCode == query);
        return Task.FromResult(member);
    }

    public Task<bool> RegisterMemberAsync(MemberDto member, string packageId)
    {
        _members.Insert(0, member);
        return Task.FromResult(true);
    }

    public Task<bool> RenewOrUpgradeCardAsync(string memberCode, string newPackageId, int extraMonths) => Task.FromResult(true);
    public Task<bool> FreezeCardAsync(string cardCode, DateOnly startDate, DateOnly endDate, string reason) => Task.FromResult(true);
    public Task<bool> LockOrCancelCardAsync(string cardCode, CardStatus newStatus, string reason) => Task.FromResult(true);
    public Task<bool> UpdateMemberProfileAsync(string memberCode, string phone, string email, string address, string? avatarUrl) => Task.FromResult(true);

    // Check-in
    public Task<CheckInRecordDto> CheckInWithQrAsync(string qrCode)
    {
        var record = new CheckInRecordDto($"CK-{Guid.NewGuid():N}"[..8], "CARD-0248", "MB-0248", "Nguyễn Minh Anh", "Premium", DateTime.Now, true, null, "Ca Sáng");
        _checkInLogs.Insert(0, record);
        return Task.FromResult(record);
    }

    public Task<CheckInRecordDto> ManualCheckInAsync(string phoneOrMemberCode)
    {
        var member = _members.FirstOrDefault(m => m.Phone == phoneOrMemberCode || m.MemberCode == phoneOrMemberCode);
        var record = member != null 
            ? new CheckInRecordDto($"CK-{Guid.NewGuid():N}"[..8], "CARD-MOCK", member.MemberCode, member.FullName, member.ActivePackageName ?? "Gym", DateTime.Now, true, null, "Ca Sáng")
            : new CheckInRecordDto($"CK-{Guid.NewGuid():N}"[..8], "CARD-ERR", phoneOrMemberCode, "Khách lạ", "Không rõ", DateTime.Now, false, "Không tìm thấy thông tin hội viên", "Ca Sáng");
        _checkInLogs.Insert(0, record);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<CheckInRecordDto>> GetTodayCheckInLogsAsync(string? shift = null)
    {
        return Task.FromResult<IReadOnlyList<CheckInRecordDto>>(_checkInLogs);
    }

    // PT
    public Task<IReadOnlyList<PtProfileDto>> GetTrainersAsync() => Task.FromResult<IReadOnlyList<PtProfileDto>>(_trainers);

    public Task<IReadOnlyList<PtSessionDto>> GetPtScheduleAsync(string? ptId, DateTime startDate, DateTime endDate)
    {
        var result = _sessions.AsEnumerable();
        if (!string.IsNullOrEmpty(ptId))
            result = result.Where(s => s.PtId == ptId);
        return Task.FromResult<IReadOnlyList<PtSessionDto>>(result.ToList());
    }

    public Task<IReadOnlyList<PtSessionDto>> GetMemberSessionsAsync(string memberCode)
    {
        var result = _sessions.Where(s => s.MemberCode == memberCode).ToList();
        return Task.FromResult<IReadOnlyList<PtSessionDto>>(result);
    }

    public Task<bool> BookSessionAsync(string memberCode, string ptId, DateTime startTime, DateTime endTime)
    {
        var pt = _trainers.FirstOrDefault(t => t.PtId == ptId);
        var member = _members.FirstOrDefault(m => m.MemberCode == memberCode);
        _sessions.Add(new PtSessionDto($"BK-{Guid.NewGuid():N}"[..6], ptId, pt?.FullName ?? "PT", memberCode, member?.FullName ?? "Hội viên", startTime, endTime, BookingStatus.Confirmed, null, null, null));
        return Task.FromResult(true);
    }

    public Task<bool> CancelOrRescheduleSessionAsync(string bookingId, DateTime? newTime, string reason) => Task.FromResult(true);

    public Task<bool> CompleteSessionAsync(string bookingId)
    {
        var index = _sessions.FindIndex(s => s.BookingId == bookingId);
        if (index < 0) return Task.FromResult(false);

        var session = _sessions[index];

        // UC 8.2.3 – chỉ xác nhận được ca đang chờ và đã/đang diễn ra
        if (session.Status != BookingStatus.Confirmed || session.StartTime > DateTime.Now)
            return Task.FromResult(false);

        _sessions[index] = session with { Status = BookingStatus.Completed };

        // Trừ 1 buổi PT còn lại trong gói của học viên
        var memberIndex = _members.FindIndex(m => m.MemberCode == session.MemberCode);
        if (memberIndex >= 0 && _members[memberIndex].RemainingPtSessions > 0)
        {
            _members[memberIndex] = _members[memberIndex] with
            {
                RemainingPtSessions = _members[memberIndex].RemainingPtSessions - 1
            };
        }

        return Task.FromResult(true);
    }
    public Task<bool> SubmitRatingAsync(string bookingId, int rating, string? comment) => Task.FromResult(true);

    public Task<IReadOnlyList<InBodyRecordDto>> GetInBodyHistoryAsync(string memberCode)
    {
        var list = _inBodyRecords.Where(r => r.MemberCode == memberCode).OrderByDescending(r => r.MeasuredDate).ToList();
        return Task.FromResult<IReadOnlyList<InBodyRecordDto>>(list);
    }

    public Task<bool> RecordInBodyAsync(InBodyRecordDto record)
    {
        _inBodyRecords.Insert(0, record);
        return Task.FromResult(true);
    }

    // UC 8.1.1 – Sinh lịch rảnh của PT cho một tuần (06:00–22:00, mỗi khung 1 giờ)
    public Task<IReadOnlyList<PtSlotDto>> GetPtSlotsAsync(string ptId, DateOnly weekStart)
    {
        var slots = new List<PtSlotDto>();

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var date = weekStart.AddDays(dayOffset);

            for (var hour = 6; hour <= 21; hour++)
            {
                var start = date.ToDateTime(new TimeOnly(hour, 0));
                var end = start.AddHours(1);
                var slotId = $"SLOT-{date:yyyyMMdd}-{hour:D2}";

                // Ưu tiên 1: khung giờ đã có hội viên đặt lịch (không được phép hủy)
                var isBooked = _sessions.Any(s => s.PtId == ptId
                                                  && s.Status != BookingStatus.Cancelled
                                                  && s.StartTime < end && s.EndTime > start);

                // Ưu tiên 2: trạng thái PT đã lưu, nếu chưa có thì lấy mẫu mặc định tất định
                var isRegistered = _ptSlotOverrides.TryGetValue($"{ptId}|{slotId}", out var saved)
                    ? saved
                    : IsSlotRegisteredByDefault(date, hour);

                var status = isBooked ? PtSlotStatus.Booked
                    : isRegistered ? PtSlotStatus.Registered
                    : PtSlotStatus.Free;

                slots.Add(new PtSlotDto(slotId, date, $"{hour:D2}:00", $"{hour + 1:D2}:00", status));
            }
        }

        return Task.FromResult<IReadOnlyList<PtSlotDto>>(slots);
    }

    public Task<bool> SavePtSlotsAsync(string ptId, IReadOnlyList<PtSlotDto> slots)
    {
        foreach (var slot in slots)
        {
            // Khung giờ đã có hội viên đặt thì giữ nguyên, không ghi đè (UC 8.1.1 – phụ 5.1)
            if (slot.Status == PtSlotStatus.Booked) continue;

            _ptSlotOverrides[$"{ptId}|{slot.SlotId}"] = slot.Status == PtSlotStatus.Registered;
        }

        return Task.FromResult(true);
    }

    // UC 8.2.1 – Danh sách học viên do PT trực tiếp phụ trách
    public Task<IReadOnlyList<MemberDto>> GetPtStudentsAsync(string ptId, string? searchTerm = null)
    {
        var assignedCodes = _ptAssignments.TryGetValue(ptId, out var assigned) ? assigned : [];
        var result = _members.Where(m => assignedCodes.Contains(m.MemberCode));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            result = result.Where(m => m.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                                       || m.MemberCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<MemberDto>>(result.ToList());
    }

    /// <summary>
    /// Mẫu lịch rảnh mặc định cho dữ liệu demo: băm tất định theo (ngày, giờ)
    /// để lịch rảnh không đổi giữa các lần render/navigation.
    /// </summary>
    private static bool IsSlotRegisteredByDefault(DateOnly date, int hour)
        => (date.DayNumber * 31 + hour * 17) % 5 is 0 or 1;

    // Billing
    public Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(DateTime? fromDate = null, DateTime? toDate = null) => Task.FromResult<IReadOnlyList<InvoiceDto>>(_invoices);
    public Task<InvoiceDto> CreateInvoiceAsync(string memberCode, string itemsSummary, decimal total, decimal discount, PaymentMethod method)
    {
        var inv = new InvoiceDto($"INV-{DateTime.Now:yyyyMMdd}-{_invoices.Count + 1:D3}", memberCode, "Hội viên", "Thu ngân", total, discount, total - discount, method, InvoiceStatus.Paid, DateTime.Now, itemsSummary);
        _invoices.Insert(0, inv);
        return Task.FromResult(inv);
    }
    public Task<bool> RefundInvoiceAsync(string invoiceCode, decimal refundAmount, string reason) => Task.FromResult(true);

    // Reports
    public Task<IDictionary<string, decimal>> GetRevenueByTimeRangeAsync(DateTime fromDate, DateTime toDate)
    {
        IDictionary<string, decimal> data = new Dictionary<string, decimal>
        {
            ["T2"] = 12500000m,
            ["T3"] = 18200000m,
            ["T4"] = 15400000m,
            ["T5"] = 21000000m,
            ["T6"] = 28500000m,
            ["T7"] = 35000000m,
            ["CN"] = 31200000m
        };
        return Task.FromResult(data);
    }

    public Task<IDictionary<string, int>> GetCheckInTrafficByHourAsync(DateTime date)
    {
        IDictionary<string, int> data = new Dictionary<string, int>
        {
            ["06:00"] = 42,
            ["08:00"] = 28,
            ["11:30"] = 19,
            ["15:00"] = 35,
            ["17:30"] = 86,
            ["19:00"] = 94,
            ["21:00"] = 31
        };
        return Task.FromResult(data);
    }

    public Task<IDictionary<string, int>> GetMemberGrowthAsync(int months)
    {
        IDictionary<string, int> data = new Dictionary<string, int>
        {
            ["T1"] = 120,
            ["T2"] = 145,
            ["T3"] = 180,
            ["T4"] = 210
        };
        return Task.FromResult(data);
    }

    // Settings
    public Task<SystemConfigDto> GetConfigAsync() => Task.FromResult(new SystemConfigDto("05:00", "22:00", 2, 30, 30));
    public Task<bool> UpdateConfigAsync(SystemConfigDto config) => Task.FromResult(true);
    public Task<IReadOnlyList<PackageDto>> GetPackagesAsync(bool includeInactive = false)
    {
        var pkgs = includeInactive ? _packages : _packages.Where(p => p.IsActive).ToList();
        return Task.FromResult<IReadOnlyList<PackageDto>>(pkgs);
    }
    public Task<bool> SavePackageAsync(PackageDto package) => Task.FromResult(true);
    public Task<bool> TogglePackageStatusAsync(string packageId, bool isActive) => Task.FromResult(true);
}
