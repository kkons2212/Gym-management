using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.DTOs.Settings;

namespace GymManagement.Desktop.Services.Api;

public class ResilientApiClient : IApiClient
{
    private readonly HttpApiClient _http;
    private readonly MockApiClient _mock;
    private bool _fallbackToMock;

    public bool IsMockActive
    {
        get
        {
            return _fallbackToMock;
        }
        set
        {
            _fallbackToMock = value;
        }
    }

    public ResilientApiClient(HttpApiClient http, MockApiClient mock)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(mock);
        _http = http;
        _mock = mock;

        string? mockEnv;
        mockEnv = Environment.GetEnvironmentVariable("USE_MOCK_API");
        if (string.Equals(mockEnv, "true", StringComparison.OrdinalIgnoreCase))
        {
            _fallbackToMock = true;
        }
    }

    private async Task<T> ExecuteWithFallbackAsync<T>(Func<Task<T>> httpFunc, Func<Task<T>> mockFunc)
    {
        if (_fallbackToMock)
        {
            return await mockFunc();
        }

        try
        {
            return await httpFunc();
        }
        catch (HttpRequestException)
        {
            _fallbackToMock = true;
            return await mockFunc();
        }
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.LoginAsync(request), () => _mock.LoginAsync(request));
    }

    public Task<CheckInResponse> CheckInAsync(CheckInRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.CheckInAsync(request), () => _mock.CheckInAsync(request));
    }

    public Task<IReadOnlyList<CheckInLogDto>> GetTodayLogsAsync(string? searchQuery = null, string? statusFilter = null)
    {
        return ExecuteWithFallbackAsync(() => _http.GetTodayLogsAsync(searchQuery, statusFilter), () => _mock.GetTodayLogsAsync(searchQuery, statusFilter));
    }

    public Task<int> GetTodayAttendanceCountAsync()
    {
        return ExecuteWithFallbackAsync(() => _http.GetTodayAttendanceCountAsync(), () => _mock.GetTodayAttendanceCountAsync());
    }

    public Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? query = null)
    {
        return ExecuteWithFallbackAsync(() => _http.GetMembersAsync(query), () => _mock.GetMembersAsync(query));
    }

    public Task<MemberProfileResponse> RegisterMemberAsync(RegisterMemberRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.RegisterMemberAsync(request), () => _mock.RegisterMemberAsync(request));
    }

    public Task<MemberProfileResponse> UpdateMemberProfileAsync(string userName, UpdateMemberRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.UpdateMemberProfileAsync(userName, request), () => _mock.UpdateMemberProfileAsync(userName, request));
    }

    public Task<SubscriptionDetailResponse> RenewSubscriptionAsync(string userName, RenewSubscriptionRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.RenewSubscriptionAsync(userName, request), () => _mock.RenewSubscriptionAsync(userName, request));
    }

    public Task<SubscriptionDetailResponse> UpgradeSubscriptionAsync(string userName, UpgradeSubscriptionRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.UpgradeSubscriptionAsync(userName, request), () => _mock.UpgradeSubscriptionAsync(userName, request));
    }

    public Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.FreezeSubscriptionAsync(request), () => _mock.FreezeSubscriptionAsync(request));
    }

    public Task<bool> CancelSubscriptionAsync(CancelSubscriptionRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.CancelSubscriptionAsync(request), () => _mock.CancelSubscriptionAsync(request));
    }

    public Task<InBodyMetricDto> RecordInBodyMetricAsync(InBodyEntryRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.RecordInBodyMetricAsync(request), () => _mock.RecordInBodyMetricAsync(request));
    }

    public Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        return ExecuteWithFallbackAsync(() => _http.GetPackagePlansAsync(), () => _mock.GetPackagePlansAsync());
    }

    public Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.CreateInvoiceAsync(request), () => _mock.CreateInvoiceAsync(request));
    }

    public Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceCode, decimal amount, string transferContent)
    {
        return ExecuteWithFallbackAsync(() => _http.GenerateVietQrAsync(invoiceCode, amount, transferContent), () => _mock.GenerateVietQrAsync(invoiceCode, amount, transferContent));
    }

    public Task<InvoiceDto> ConfirmPaymentAsync(string invoiceCode)
    {
        return ExecuteWithFallbackAsync(() => _http.ConfirmPaymentAsync(invoiceCode), () => _mock.ConfirmPaymentAsync(invoiceCode));
    }

    public Task<InvoiceDto> RefundInvoiceAsync(string invoiceCode, decimal refundAmount, string reason)
    {
        return ExecuteWithFallbackAsync(() => _http.RefundInvoiceAsync(invoiceCode, refundAmount, reason), () => _mock.RefundInvoiceAsync(invoiceCode, refundAmount, reason));
    }

    public Task<IReadOnlyList<PtSessionSlotDto>> GetPtScheduleSlotsAsync(DateTime startDate, DateTime endDate)
    {
        return ExecuteWithFallbackAsync(() => _http.GetPtScheduleSlotsAsync(startDate, endDate), () => _mock.GetPtScheduleSlotsAsync(startDate, endDate));
    }

    public Task<PtSessionSlotDto> BookPtSlotAsync(BookPtSlotRequest request)
    {
        return ExecuteWithFallbackAsync(() => _http.BookPtSlotAsync(request), () => _mock.BookPtSlotAsync(request));
    }

    public Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        return ExecuteWithFallbackAsync(() => _http.GetDashboardSummaryAsync(), () => _mock.GetDashboardSummaryAsync());
    }

    public Task<byte[]> ExportExcelReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return ExecuteWithFallbackAsync(() => _http.ExportExcelReportAsync(fromDate, toDate), () => _mock.ExportExcelReportAsync(fromDate, toDate));
    }

    public Task<byte[]> ExportPdfReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return ExecuteWithFallbackAsync(() => _http.ExportPdfReportAsync(fromDate, toDate), () => _mock.ExportPdfReportAsync(fromDate, toDate));
    }

    public Task<IReadOnlyList<UserSessionDto>> GetUsersAsync()
    {
        return ExecuteWithFallbackAsync(() => _http.GetUsersAsync(), () => _mock.GetUsersAsync());
    }

    public Task<bool> ToggleUserLockAsync(string userId)
    {
        return ExecuteWithFallbackAsync(() => _http.ToggleUserLockAsync(userId), () => _mock.ToggleUserLockAsync(userId));
    }

    public Task<PackagePlanDto> CreatePackagePlanAsync(PackagePlanDto plan)
    {
        return ExecuteWithFallbackAsync(() => _http.CreatePackagePlanAsync(plan), () => _mock.CreatePackagePlanAsync(plan));
    }

    public Task<UserSessionDto> CreateUserAsync(string fullName, string email, string phone, string username, string role)
    {
        return ExecuteWithFallbackAsync(() => _http.CreateUserAsync(fullName, email, phone, username, role), () => _mock.CreateUserAsync(fullName, email, phone, username, role));
    }
}

