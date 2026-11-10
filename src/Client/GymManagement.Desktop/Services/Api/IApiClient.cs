using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.DTOs.Settings;

namespace GymManagement.Desktop.Services.Api;

public interface IApiClient
{
    // Auth & Session
    Task<LoginResponse> LoginAsync(LoginRequest request);

    // CheckIn & Real-time Monitoring (UC03)
    Task<CheckInResponse> CheckInAsync(CheckInRequest request);
    Task<IReadOnlyList<CheckInLogDto>> GetTodayLogsAsync(string? searchQuery = null, string? statusFilter = null);
    Task<int> GetTodayAttendanceCountAsync();

    // Member Lifecycle (UC02, UC09)
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? query = null);
    Task<MemberProfileResponse> RegisterMemberAsync(RegisterMemberRequest request);
    Task<MemberProfileResponse> UpdateMemberProfileAsync(string userName, UpdateMemberRequest request);
    Task<SubscriptionDetailResponse> RenewSubscriptionAsync(string userName, RenewSubscriptionRequest request);
    Task<SubscriptionDetailResponse> UpgradeSubscriptionAsync(string userName, UpgradeSubscriptionRequest request);
    Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request);
    Task<bool> CancelSubscriptionAsync(CancelSubscriptionRequest request);
    Task<InBodyMetricDto> RecordInBodyMetricAsync(InBodyEntryRequest request);

    // Packages & POS Billing (UC05, UC07)
    Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync();
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? searchQuery = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request);
    Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceCode, decimal amount, string transferContent);
    Task<InvoiceDto> ConfirmPaymentAsync(string invoiceCode);
    Task<InvoiceDto> RefundInvoiceAsync(string invoiceCode, decimal refundAmount, string reason);

    // PT Services (UC04)
    Task<IReadOnlyList<PtSessionSlotDto>> GetPtScheduleSlotsAsync(DateTime startDate, DateTime endDate);
    Task<PtSessionSlotDto> BookPtSlotAsync(BookPtSlotRequest request);
    Task<bool> AssignPtAsync(AssignPtRequest request);

    // Reports & BI (UC06)
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<byte[]> ExportExcelReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> ExportPdfReportAsync(DateTime? fromDate = null, DateTime? toDate = null);

    // Staff RBAC (UC07)
    Task<IReadOnlyList<UserSessionDto>> GetUsersAsync();
    Task<bool> ToggleUserLockAsync(string userId);
    Task<PackagePlanDto> CreatePackagePlanAsync(PackagePlanDto plan);
    Task<UserSessionDto> CreateUserAsync(string fullName, string email, string phone, string username, string role);
}
