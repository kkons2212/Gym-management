using GymManagement.Core.DTOs;
using GymManagement.Core.Enums;

namespace GymManagement.Core.Contracts;

public interface IAuthService
{
    Task<(bool Success, UserAccountDto? User, string? ErrorMessage)> LoginAsync(string username, string password);
    Task LogoutAsync();
}

public interface IMemberService
{
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? searchTerm = null, MemberStatus? status = null);
    Task<MemberDto?> GetMemberByCodeOrPhoneAsync(string query);
    Task<bool> RegisterMemberAsync(MemberDto member, string packageId);
    Task<bool> RenewOrUpgradeCardAsync(string memberCode, string newPackageId, int extraMonths);
    Task<bool> FreezeCardAsync(string cardCode, DateOnly startDate, DateOnly endDate, string reason);
    Task<bool> LockOrCancelCardAsync(string cardCode, CardStatus newStatus, string reason);
    Task<bool> UpdateMemberProfileAsync(string memberCode, string phone, string email, string address, string? avatarUrl);
}

public interface ICheckInService
{
    Task<CheckInRecordDto> CheckInWithQrAsync(string qrCode);
    Task<CheckInRecordDto> ManualCheckInAsync(string phoneOrMemberCode);
    Task<IReadOnlyList<CheckInRecordDto>> GetTodayCheckInLogsAsync(string? shift = null);
}

public interface IPtService
{
    Task<IReadOnlyList<PtProfileDto>> GetTrainersAsync();
    Task<IReadOnlyList<PtSessionDto>> GetPtScheduleAsync(string? ptId, DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<PtSessionDto>> GetMemberSessionsAsync(string memberCode);
    Task<bool> BookSessionAsync(string memberCode, string ptId, DateTime startTime, DateTime endTime);
    Task<bool> CancelOrRescheduleSessionAsync(string bookingId, DateTime? newTime, string reason);
    Task<bool> CompleteSessionAsync(string bookingId);
    Task<bool> SubmitRatingAsync(string bookingId, int rating, string? comment);
    Task<IReadOnlyList<InBodyRecordDto>> GetInBodyHistoryAsync(string memberCode);
    Task<bool> RecordInBodyAsync(InBodyRecordDto record);

    // UC08 – Giao diện PT: lịch rảnh & học viên phụ trách
    Task<IReadOnlyList<PtSlotDto>> GetPtSlotsAsync(string ptId, DateOnly weekStart);
    Task<bool> SavePtSlotsAsync(string ptId, IReadOnlyList<PtSlotDto> slots);
    Task<IReadOnlyList<MemberDto>> GetPtStudentsAsync(string ptId, string? searchTerm = null);
}

public interface IBillingService
{
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<InvoiceDto> CreateInvoiceAsync(string memberCode, string itemsSummary, decimal total, decimal discount, PaymentMethod method);
    Task<bool> RefundInvoiceAsync(string invoiceCode, decimal refundAmount, string reason);
}

public interface IReportService
{
    Task<IDictionary<string, decimal>> GetRevenueByTimeRangeAsync(DateTime fromDate, DateTime toDate);
    Task<IDictionary<string, int>> GetCheckInTrafficByHourAsync(DateTime date);
    Task<IDictionary<string, int>> GetMemberGrowthAsync(int months);
}

public interface ISystemSettingService
{
    Task<SystemConfigDto> GetConfigAsync();
    Task<bool> UpdateConfigAsync(SystemConfigDto config);
    Task<IReadOnlyList<PackageDto>> GetPackagesAsync(bool includeInactive = false);
    Task<bool> SavePackageAsync(PackageDto package);
    Task<bool> TogglePackageStatusAsync(string packageId, bool isActive);
}
