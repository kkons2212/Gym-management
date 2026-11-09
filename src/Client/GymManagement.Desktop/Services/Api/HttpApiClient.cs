using System.Net.Http.Json;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.DTOs.Settings;
using GymManagement.Server.Core.Exceptions;

namespace GymManagement.Desktop.Services.Api;

/// <summary>
/// Triển khai HTTP Client chính thức giao tiếp với GymManagement API qua giao thức RESTful (Task 4.1.1).
/// </summary>
public sealed class HttpApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public HttpApiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", request);
        return await HandleResponseAsync<LoginResponse>(response);
    }

    public async Task<CheckInResponse> CheckInAsync(CheckInRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpResponseMessage response;
        if (!string.IsNullOrWhiteSpace(request.QrCode))
        {
            var qrPayload = new { QrCode = request.QrCode.Trim() };
            response = await _httpClient.PostAsJsonAsync("/api/v1/checkin/qr", qrPayload);
        }
        else
        {
            string lookup = string.Empty;
            if (!string.IsNullOrWhiteSpace(request.MemberCode))
            {
                lookup = request.MemberCode.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                lookup = request.Phone.Trim();
            }

            var manualPayload = new { MemberCodeOrPhone = lookup, Notes = request.Notes };
            response = await _httpClient.PostAsJsonAsync("/api/v1/checkin/manual", manualPayload);
        }

        return await HandleResponseAsync<CheckInResponse>(response);
    }

    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(string? query = null)
    {
        string url = "/api/v1/members";
        if (!string.IsNullOrWhiteSpace(query))
        {
            url = $"/api/v1/members?keyword={Uri.EscapeDataString(query.Trim())}";
        }

        var response = await _httpClient.GetAsync(url);
        var searchResults = await HandleResponseAsync<IReadOnlyList<MemberSearchResponse>>(response);

        var mappedList = new List<MemberDto>();
        if (searchResults != null)
        {
            foreach (var m in searchResults)
            {
                DateTime? endDate = null;
                if (m.SubscriptionEndDate.HasValue)
                {
                    endDate = m.SubscriptionEndDate.Value.ToDateTime(TimeOnly.MinValue);
                }

                mappedList.Add(new MemberDto(
                    UserName: m.UserName,
                    FullName: m.FullName,
                    Phone: m.Phone,
                    Email: m.Email,
                    Status: m.Status,
                    CreatedAt: DateTime.UtcNow,
                    ActivePlanName: m.ActivePlanName,
                    EndDate: endDate,
                    RemainingCheckInSessions: m.RemainingCheckInSessions,
                    RemainingPtSessions: m.RemainingPtSessions
                ));
            }
        }

        return mappedList;
    }

    public async Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/admin/packages?includeInactive=false");
        return await HandleResponseAsync<IReadOnlyList<PackagePlanDto>>(response);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/invoices", request);
        return await HandleResponseAsync<InvoiceDto>(response);
    }

    public async Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceId, decimal amount, string transferContent)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            throw new ValidationException("Mã hóa đơn không được để trống.");
        }

        var response = await _httpClient.PostAsync($"/api/v1/invoices/{Uri.EscapeDataString(invoiceId)}/vietqr", null);
        return await HandleResponseAsync<VietQrPayloadDto>(response);
    }

    public async Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/members/freeze", request);
        var result = await HandleResponseAsync<SubscriptionDetailResponse>(response);
        return result != null;
    }

    public async Task<InBodyMetricDto> RecordInBodyMetricAsync(InBodyEntryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pt/inbody", request);
        return await HandleResponseAsync<InBodyMetricDto>(response);
    }

    public async Task<IReadOnlyList<PtSessionSlotDto>> GetPtScheduleSlotsAsync(DateTime startDate, DateTime endDate)
    {
        string url = $"/api/v1/pt/schedule/slots?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        return await HandleResponseAsync<IReadOnlyList<PtSessionSlotDto>>(response);
    }

    public async Task<PtSessionSlotDto> BookPtSlotAsync(BookPtSlotRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pt/sessions/book", request);
        return await HandleResponseAsync<PtSessionSlotDto>(response);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/reports/dashboard");
        return await HandleResponseAsync<DashboardSummaryDto>(response);
    }

    public async Task<IReadOnlyList<UserSessionDto>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/admin/users");
        return await HandleResponseAsync<IReadOnlyList<UserSessionDto>>(response);
    }

    public async Task<bool> ToggleUserLockAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException("Mã người dùng không được để trống.");
        }

        var payload = new { Status = "LOCKED" };
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/v1/admin/users/{Uri.EscapeDataString(userId)}/status",
            payload);

        return await HandleResponseAsync<bool>(response);
    }

    public async Task<PackagePlanDto> CreatePackagePlanAsync(PackagePlanDto plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var request = new CreatePackagePlanRequest(
            PlanId: plan.PlanId,
            Name: plan.Name,
            Description: plan.Description,
            Price: plan.Price,
            DurationDays: plan.DurationDays,
            MaxCheckInSessions: plan.MaxCheckInSessions,
            IsPtIncluded: plan.IsPtIncluded,
            TotalPtSessions: plan.TotalPtSessions
        );

        var response = await _httpClient.PostAsJsonAsync("/api/v1/admin/packages", request);
        return await HandleResponseAsync<PackagePlanDto>(response);
    }

    public async Task<UserSessionDto> CreateUserAsync(string fullName, string email, string phone, string username, string role)
    {
        var request = new CreateUserRequest(
            UserName: username,
            RoleId: role,
            FullName: fullName,
            Phone: phone,
            Email: email,
            Password: "TemporaryPassword123!"
        );

        var response = await _httpClient.PostAsJsonAsync("/api/v1/admin/users", request);
        return await HandleResponseAsync<UserSessionDto>(response);
    }

    private static async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (apiResult != null && apiResult.Success && apiResult.Data != null)
            {
                return apiResult.Data;
            }

            if (apiResult != null && !string.IsNullOrEmpty(apiResult.Message))
            {
                throw new BusinessRuleException(apiResult.Message);
            }
        }

        var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        string errorMessage = "Lỗi xử lý yêu cầu từ máy chủ.";
        if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
        {
            errorMessage = errorResult.Message;
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new NotFoundException(errorMessage);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            throw new ValidationException(errorMessage);
        }

        throw new BusinessRuleException(errorMessage);
    }
}
