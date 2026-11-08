using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;

namespace GymManagement.Desktop.Services.Api;

public interface IApiClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<CheckInResponse> CheckInAsync(CheckInRequest request);
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? query = null);
    Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync();
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request);
    Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceId, decimal amount, string transferContent);
    Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request);
    Task<InBodyMetricDto> RecordInBodyMetricAsync(InBodyEntryRequest request);
    Task<IReadOnlyList<PtSessionSlotDto>> GetPtScheduleSlotsAsync(DateTime startDate, DateTime endDate);
    Task<PtSessionSlotDto> BookPtSlotAsync(BookPtSlotRequest request);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<IReadOnlyList<UserSessionDto>> GetUsersAsync();
    Task<bool> ToggleUserLockAsync(string userId);
    Task<PackagePlanDto> CreatePackagePlanAsync(PackagePlanDto plan);
    Task<UserSessionDto> CreateUserAsync(string fullName, string email, string phone, string username, string role);
}

