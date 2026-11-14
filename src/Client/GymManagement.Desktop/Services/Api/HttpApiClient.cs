using System.Net.Http.Json;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.DTOs.Reports;
using GymManagement.Server.Core.DTOs.Settings;
using GymManagement.Server.Core.DTOs;
using GymManagement.Server.Core.Exceptions;

namespace GymManagement.Desktop.Services.Api;

/// <summary>
/// Triển khai HTTP Client chính thức giao tiếp với GymManagement API qua giao thức RESTful (Task 4.1).
/// </summary>
public sealed class HttpApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public HttpApiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. AUTH & SESSION
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", request);
        return await HandleResponseAsync<LoginResponse>(response);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. CHECK-IN & REAL-TIME MONITORING (UC03)
    // ─────────────────────────────────────────────────────────────────────────

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

    public async Task<IReadOnlyList<CheckInLogDto>> GetTodayLogsAsync(string? searchQuery = null, string? statusFilter = null)
    {
        string url = "/api/v1/checkin/today";
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            queryParams.Add($"searchQuery={Uri.EscapeDataString(searchQuery.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "ALL")
        {
            queryParams.Add($"statusFilter={Uri.EscapeDataString(statusFilter.Trim())}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url);
        return await HandleResponseAsync<IReadOnlyList<CheckInLogDto>>(response);
    }

    public async Task<int> GetTodayAttendanceCountAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/checkin/attendance-count");
        return await HandleResponseAsync<int>(response);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. MEMBER LIFECYCLE (UC02, UC09)
    // ─────────────────────────────────────────────────────────────────────────

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

    public async Task<MemberProfileResponse> RegisterMemberAsync(RegisterMemberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/members/register", request);
        return await HandleResponseAsync<MemberProfileResponse>(response);
    }

    public async Task<MemberProfileResponse> UpdateMemberProfileAsync(string userName, UpdateMemberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/members/{Uri.EscapeDataString(userName)}", request);
        return await HandleResponseAsync<MemberProfileResponse>(response);
    }

    public async Task<SubscriptionDetailResponse> RenewSubscriptionAsync(string userName, RenewSubscriptionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync($"/api/v1/members/{Uri.EscapeDataString(userName)}/renew", request);
        return await HandleResponseAsync<SubscriptionDetailResponse>(response);
    }

    public async Task<SubscriptionDetailResponse> UpgradeSubscriptionAsync(string userName, UpgradeSubscriptionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync($"/api/v1/members/{Uri.EscapeDataString(userName)}/upgrade", request);
        return await HandleResponseAsync<SubscriptionDetailResponse>(response);
    }

    public async Task<bool> FreezeSubscriptionAsync(FreezeSubscriptionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/members/freeze", request);
        var result = await HandleResponseAsync<SubscriptionDetailResponse>(response);
        return result != null;
    }

    public async Task<bool> CancelSubscriptionAsync(CancelSubscriptionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/members/cancel", request);
        var result = await HandleResponseAsync<SubscriptionDetailResponse>(response);
        return result != null;
    }

    public async Task<InBodyMetricDto> RecordInBodyMetricAsync(InBodyEntryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pt/inbody", request);
        return await HandleResponseAsync<InBodyMetricDto>(response);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. PACKAGES & POS BILLING (UC05, UC07)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/admin/packages?includeInactive=false");
        return await HandleResponseAsync<IReadOnlyList<PackagePlanDto>>(response);
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? searchQuery = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        string url;
        url = "/api/v1/invoices";

        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            queryParams.Add($"search={Uri.EscapeDataString(searchQuery.Trim())}");
        }

        if (fromDate.HasValue)
        {
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        }

        if (toDate.HasValue)
        {
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url);
        return await HandleResponseAsync<IReadOnlyList<InvoiceDto>>(response);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/invoices", request);
        return await HandleResponseAsync<InvoiceDto>(response);
    }

    public async Task<VietQrPayloadDto> GenerateVietQrAsync(string invoiceCode, decimal amount, string transferContent)
    {
        if (invoiceCode != "TEMP" && !string.IsNullOrWhiteSpace(invoiceCode))
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/invoices/{Uri.EscapeDataString(invoiceCode)}/qr");
                if (response.IsSuccessStatusCode)
                {
                    return await HandleResponseAsync<VietQrPayloadDto>(response);
                }
            }
            catch
            {
                // Fallback to client-side QuickLink generation
            }
        }

        string bankBin = Environment.GetEnvironmentVariable("VIETQR_BANK_BIN") ?? "970422";
        string accountNo = Environment.GetEnvironmentVariable("VIETQR_ACCOUNT_NO") ?? "0901234567";
        string accountName = Environment.GetEnvironmentVariable("VIETQR_ACCOUNT_NAME") ?? "CONG TY TNHH PHONG TAP THE THAO";
        string bankName = Environment.GetEnvironmentVariable("VIETQR_BANK_NAME") ?? "MB Bank (Napas 247)";
        string imageBaseUrl = Environment.GetEnvironmentVariable("VIETQR_IMAGE_BASE_URL") ?? "https://img.vietqr.io/image";

        string encodedContent = Uri.EscapeDataString(transferContent);
        string encodedAccountName = Uri.EscapeDataString(accountName);
        string qrImageUrl = $"{imageBaseUrl}/{bankBin}-{accountNo}-compact2.svg?amount={(long)amount}&addInfo={encodedContent}&accountName={encodedAccountName}";

        var payload = new VietQrPayloadDto(
            qrImageUrl,
            accountNo,
            bankName,
            amount,
            transferContent,
            600
        );

        return payload;
    }

    public async Task<InvoiceDto> ConfirmPaymentAsync(string invoiceCode)
    {
        if (string.IsNullOrWhiteSpace(invoiceCode))
        {
            throw new ValidationException("Mã hóa đơn không được để trống.");
        }

        var response = await _httpClient.PostAsync($"/api/v1/invoices/{Uri.EscapeDataString(invoiceCode)}/confirm", null);
        return await HandleResponseAsync<InvoiceDto>(response);
    }

    public async Task<InvoiceDto> RefundInvoiceAsync(string invoiceCode, decimal refundAmount, string reason)
    {
        if (string.IsNullOrWhiteSpace(invoiceCode))
        {
            throw new ValidationException("Mã hóa đơn không được để trống.");
        }

        var payload = new { Reason = reason };
        var response = await _httpClient.PostAsJsonAsync($"/api/v1/invoices/{Uri.EscapeDataString(invoiceCode)}/refund", payload);
        return await HandleResponseAsync<InvoiceDto>(response);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. PT SERVICES (UC04)
    // ─────────────────────────────────────────────────────────────────────────

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

    public async Task<bool> AssignPtAsync(AssignPtRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pt/assign", request);
        return await HandleResponseAsync<bool>(response);
    }

    public async Task<IReadOnlyList<PtProfileDto>> GetTrainersAsync(bool? activeOnly = null)
    {
        string url = "/api/v1/pt/trainers";
        if (activeOnly.HasValue)
        {
            if (activeOnly.Value)
            {
                url = "/api/v1/pt/trainers?activeOnly=true";
            }
            else
            {
                url = "/api/v1/pt/trainers?activeOnly=false";
            }
        }

        var response = await _httpClient.GetAsync(url);
        return await HandleResponseAsync<IReadOnlyList<PtProfileDto>>(response);
    }

    public async Task<PtProfileDto> SaveTrainerAsync(SavePtProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pt/trainers", request);
        return await HandleResponseAsync<PtProfileDto>(response);
    }

    public async Task<bool> ConfirmSessionResultAsync(ConfirmSessionResultRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("/api/v1/pt/sessions/confirm-result", request);
        return await HandleResponseAsync<bool>(response);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. REPORTS & BI (UC06)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/reports/dashboard");
        return await HandleResponseAsync<DashboardSummaryDto>(response);
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        string url = "/api/v1/reports/revenue";
        var queryParams = new List<string>();

        if (fromDate.HasValue)
        {
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        }

        if (toDate.HasValue)
        {
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url);
        return await HandleResponseAsync<RevenueReportDto>(response);
    }

    public async Task<byte[]> ExportExcelReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        string url = "/api/v1/reports/export/excel";
        var queryParams = new List<string>();

        if (fromDate.HasValue)
        {
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        }

        if (toDate.HasValue)
        {
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        throw new BusinessRuleException("Không thể tải file báo cáo Excel từ máy chủ.");
    }

    public async Task<byte[]> ExportPdfReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        string url = "/api/v1/reports/export/pdf";
        var queryParams = new List<string>();

        if (fromDate.HasValue)
        {
            queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        }

        if (toDate.HasValue)
        {
            queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        }

        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        throw new BusinessRuleException("Không thể tải file báo cáo PDF từ máy chủ.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. STAFF RBAC & PACKAGES (UC07)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<UserSessionDto>> GetUsersAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/admin/users");
        var userDtos = await HandleResponseAsync<IReadOnlyList<UserDto>>(response);
        var list = new List<UserSessionDto>();
        if (userDtos != null)
        {
            foreach (var u in userDtos)
            {
                if (string.Equals(u.RoleId, "MEMBER", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string role = u.RoleId;
                if (!string.IsNullOrWhiteSpace(u.RoleName))
                {
                    role = u.RoleName;
                }

                string fullName = u.UserName;
                if (!string.IsNullOrWhiteSpace(u.FullName))
                {
                    fullName = u.FullName;
                }

                string email = string.Empty;
                if (!string.IsNullOrWhiteSpace(u.Email))
                {
                    email = u.Email;
                }

                string phone = string.Empty;
                if (!string.IsNullOrWhiteSpace(u.Phone))
                {
                    phone = u.Phone;
                }

                list.Add(new UserSessionDto(u.UserName, fullName, email, role, phone));
            }
        }
        return list;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/admin/roles");
        return await HandleResponseAsync<IReadOnlyList<RoleDto>>(response);
    }

    public async Task<bool> ToggleUserLockAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException("Mã người dùng không được để trống.");
        }

        var getRes = await _httpClient.GetAsync($"/api/v1/admin/users/{Uri.EscapeDataString(userId)}");
        var userDto = await HandleResponseAsync<UserDto>(getRes);
        string newStatus = "LOCKED";
        if (userDto != null && string.Equals(userDto.Status, "LOCKED", StringComparison.OrdinalIgnoreCase))
        {
            newStatus = "ACTIVE";
        }

        var payload = new { Status = newStatus };
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/admin/users/{Uri.EscapeDataString(userId)}/status",
            payload);

        var result = await HandleResponseAsync<UserDto>(response);
        return result != null;
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

    public async Task<UserSessionDto> CreateUserAsync(string fullName, string email, string phone, string username, string role, string password)
    {
        var request = new CreateUserRequest(
            UserName: username,
            RoleId: role,
            FullName: fullName,
            Phone: phone,
            Email: email,
            Password: password
        );

        var response = await _httpClient.PostAsJsonAsync("/api/v1/admin/users", request);
        var userDto = await HandleResponseAsync<UserDto>(response);
        string assignedRole = role;
        if (userDto != null && !string.IsNullOrWhiteSpace(userDto.RoleName))
        {
            assignedRole = userDto.RoleName;
        }
        return new UserSessionDto(username, fullName, email, assignedRole);
    }

    public async Task<UserSessionDto> UpdateUserAsync(string userName, UpdateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/admin/users/{Uri.EscapeDataString(userName)}", request);
        var userDto = await HandleResponseAsync<UserDto>(response);
        string role = request.RoleId;
        string fullName = request.FullName;
        string email = request.Email;
        if (userDto != null)
        {
            if (!string.IsNullOrWhiteSpace(userDto.RoleName))
            {
                role = userDto.RoleName;
            }
            if (!string.IsNullOrWhiteSpace(userDto.FullName))
            {
                fullName = userDto.FullName;
            }
            if (!string.IsNullOrWhiteSpace(userDto.Email))
            {
                email = userDto.Email;
            }
        }
        return new UserSessionDto(userName, fullName, email, role);
    }

    public async Task<bool> SetUserStatusAsync(string userName, string status)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ValidationException("Tên đăng nhập không được để trống.");
        }

        var payload = new { Status = status };
        var response = await _httpClient.PostAsJsonAsync($"/api/v1/admin/users/{Uri.EscapeDataString(userName)}/status", payload);
        return await HandleResponseAsync<bool>(response);
    }

    public async Task<PackagePlanDto> UpdatePackagePlanAsync(string planId, UpdatePackagePlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(planId))
        {
            throw new ValidationException("Mã gói tập không được để trống.");
        }

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/admin/packages/{Uri.EscapeDataString(planId)}", request);
        return await HandleResponseAsync<PackagePlanDto>(response);
    }

    public async Task<DeletePackageResultDto> DeletePackagePlanAsync(string planId)
    {
        if (string.IsNullOrWhiteSpace(planId))
        {
            throw new ValidationException("Mã gói tập không được để trống.");
        }

        var response = await _httpClient.DeleteAsync($"/api/v1/admin/packages/{Uri.EscapeDataString(planId)}");
        return await HandleResponseAsync<DeletePackageResultDto>(response);
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
