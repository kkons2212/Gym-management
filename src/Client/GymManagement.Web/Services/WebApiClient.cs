using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GymManagement.Server.Core.DTOs.Billing;
using GymManagement.Server.Core.DTOs.CheckIn;
using GymManagement.Server.Core.DTOs.Common;
using GymManagement.Server.Core.DTOs.Identity;
using GymManagement.Server.Core.DTOs.Members;
using GymManagement.Server.Core.DTOs.Notifications;
using GymManagement.Server.Core.DTOs.Packages;
using GymManagement.Server.Core.DTOs.PT;
using GymManagement.Server.Core.DTOs.PTSchedules;
using GymManagement.Server.Core.Exceptions;

namespace GymManagement.Web.Services;

/// <summary>
/// Triển khai HTTP Client chính thức gọi tới Backend REST API cho Cổng Web Portal.
/// </summary>
public sealed class WebApiClient : IWebApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public WebApiClient(
        HttpClient httpClient,
        CustomAuthenticationStateProvider authStateProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(authStateProvider);

        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    // ==========================================
    // 1. AUTHENTICATION (UC01)
    // ==========================================

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
        {
            Content = JsonContent.Create(request)
        };

        var loginRes = await SendAsync<LoginResponse>(httpReq);

        var sessionUser = new UserSessionDto(
            UserId: loginRes.UserId,
            FullName: loginRes.FullName,
            Email: request.Username.Contains('@') ? request.Username : $"{loginRes.UserId}@gym.com",
            Role: loginRes.Role
        );

        _authStateProvider.SetAuthenticated(sessionUser, loginRes.Token);
        return loginRes;
    }

    public Task<UserSessionDto?> GetCurrentUserSessionAsync()
    {
        return Task.FromResult(_authStateProvider.CurrentUser);
    }

    // ==========================================
    // 2. MEMBER PROFILE & SUBSCRIPTION (UC09, UC03)
    // ==========================================

    public async Task<MemberProfileResponse?> GetMyProfileAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return null;
        }

        var httpReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/members/{Uri.EscapeDataString(currentUser.UserId)}");
        return await SendAsync<MemberProfileResponse>(httpReq);
    }

    public async Task<bool> UpdateMyProfileAsync(UpdateMemberProfileRequest request)
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return false;
        }

        var updateReq = new UpdateMemberRequest(
            UserName: currentUser.UserId,
            FullName: request.FullName,
            Phone: request.Phone,
            Email: request.Email,
            NewUserName: null,
            NewPassword: request.NewPassword
        );

        var httpReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/members/{Uri.EscapeDataString(currentUser.UserId)}")
        {
            Content = JsonContent.Create(updateReq)
        };

        await SendAsync<MemberProfileResponse>(httpReq);
        return true;
    }

    public async Task<MemberSubscriptionDetailDto?> GetMySubscriptionAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return null;
        }

        try
        {
            var httpReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/members/{Uri.EscapeDataString(currentUser.UserId)}/active-subscription");
            var sub = await SendAsync<SubscriptionDetailResponse>(httpReq);

            return new MemberSubscriptionDetailDto(
                SubscriptionId: sub.SubscriptionId,
                UserName: currentUser.UserId,
                MemberFullName: currentUser.FullName,
                PlanId: sub.PlanId,
                PlanName: sub.PlanName,
                StartDate: sub.StartDate,
                EndDate: sub.EndDate,
                RemainingCheckInSessions: sub.RemainingCheckInSessions,
                RemainingPtSessions: sub.RemainingPtSessions,
                Status: sub.Status.ToString().ToUpperInvariant(),
                AssignedPtId: sub.AssignedPtId,
                AssignedPtName: null,
                FreezeStartDate: sub.FreezeStartDate,
                FreezeEndDate: sub.FreezeEndDate
            );
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<string> GetMyCheckInQrCodeAsync()
    {
        var sub = await GetMySubscriptionAsync();
        if (sub != null)
        {
            return sub.SubscriptionId;
        }

        var currentUser = _authStateProvider.CurrentUser;
        return currentUser?.UserId ?? "NO_SUBSCRIPTION";
    }

    public Task<string> RefreshCheckInQrCodeAsync()
    {
        return GetMyCheckInQrCodeAsync();
    }

    // ==========================================
    // 3. PACKAGES & VIETQR (UC02, UC05)
    // ==========================================

    public async Task<IReadOnlyList<PackagePlanDto>> GetPackagePlansAsync()
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/settings/packages?includeInactive=false");
        return await SendAsync<IReadOnlyList<PackagePlanDto>>(httpReq);
    }

    public async Task<VietQrPayloadDto> CreateVietQrInvoiceAsync(string planId)
    {
        var currentUser = _authStateProvider.CurrentUser;
        string userName = currentUser?.UserId ?? "member01";

        var createInvoiceReq = new CreateInvoiceRequest(
            UserId: userName,
            PlanId: planId,
            PaymentMethod: "VIETQR",
            TransactionType: "SUBSCRIPTION_PURCHASE"
        );

        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/invoices")
        {
            Content = JsonContent.Create(createInvoiceReq)
        };

        var invoice = await SendAsync<InvoiceDto>(httpReq);

        var qrReq = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/invoices/{Uri.EscapeDataString(invoice.InvoiceCode)}/qr");
        return await SendAsync<VietQrPayloadDto>(qrReq);
    }

    public async Task<bool> SimulateVietQrPaymentAsync(string invoiceId)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/invoices/{Uri.EscapeDataString(invoiceId)}/confirm");
        await SendAsync<InvoiceDto>(httpReq);
        return true;
    }

    // ==========================================
    // 4. PT BOOKING & SESSIONS (UC10)
    // ==========================================

    public async Task<IReadOnlyList<PtSlotDto>> GetPtAvailableSlotsAsync(string ptUserName, DateOnly startDate, DateOnly endDate)
    {
        string url = $"/api/v1/pt/trainers/{Uri.EscapeDataString(ptUserName)}/slots?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<IReadOnlyList<PtSlotDto>>(httpReq);
    }

    public async Task<PtSessionDto> BookPtSlotAsync(BookSessionRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/sessions/book")
        {
            Content = JsonContent.Create(request)
        };
        return await SendAsync<PtSessionDto>(httpReq);
    }

    public async Task<bool> CancelPtBookingAsync(CancelSessionRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/sessions/cancel")
        {
            Content = JsonContent.Create(request)
        };
        await SendAsync<bool>(httpReq);
        return true;
    }

    public async Task<bool> ReschedulePtBookingAsync(RescheduleSessionRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/sessions/reschedule")
        {
            Content = JsonContent.Create(request)
        };
        await SendAsync<PtSessionDto>(httpReq);
        return true;
    }

    public async Task<bool> SubmitRatingAsync(SubmitRatingRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/rating")
        {
            Content = JsonContent.Create(request)
        };
        await SendAsync<bool>(httpReq);
        return true;
    }

    public async Task<IReadOnlyList<PtSessionDto>> GetMyPtSessionsAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return [];
        }

        string url = $"/api/v1/pt/sessions?userName={Uri.EscapeDataString(currentUser.UserId)}";
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<IReadOnlyList<PtSessionDto>>(httpReq);
    }

    // ==========================================
    // 5. INBODY & CHECK-IN HISTORY (UC06, UC09)
    // ==========================================

    public async Task<IReadOnlyList<InBodyRecordDto>> GetMyInBodyHistoryAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return [];
        }

        string url = $"/api/v1/pt/members/{Uri.EscapeDataString(currentUser.UserId)}/inbody";
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<IReadOnlyList<InBodyRecordDto>>(httpReq);
    }

    public async Task<IReadOnlyList<CheckInLogDto>> GetMyCheckInHistoryAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return [];
        }

        string url = $"/api/v1/members/{Uri.EscapeDataString(currentUser.UserId)}/history";
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<IReadOnlyList<CheckInLogDto>>(httpReq);
    }

    // ==========================================
    // 6. PT WORKSPACE (UC08)
    // ==========================================

    public async Task<PtMyProfileDto?> GetMyPtProfileAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return null;
        }

        try
        {
            string url = $"/api/v1/pt/trainers/{Uri.EscapeDataString(currentUser.UserId)}";
            var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
            var trainer = await SendAsync<PtProfileDto>(httpReq);

            string status = "INACTIVE";
            if (trainer.IsActive)
            {
                status = "ACTIVE";
            }

            return new PtMyProfileDto(
                UserName: trainer.PtId,
                FullName: trainer.FullName,
                Phone: trainer.Phone,
                Email: trainer.Email,
                Status: status,
                ActiveStudentsCount: trainer.ActiveStudentsCount,
                RatingAverage: trainer.RatingAverage,
                ReviewCount: trainer.ReviewCount,
                CompletedSessionsCount: 0,
                UpcomingSessionsCount: trainer.PendingSessionsCount
            );
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<PtSlotDto>> GetMyAvailabilitySlotsAsync(DateOnly startDate, DateOnly endDate)
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return [];
        }

        return await GetPtAvailableSlotsAsync(currentUser.UserId, startDate, endDate);
    }

    public async Task<bool> SavePtAvailabilitySlotsAsync(SavePtSlotsRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/slots")
        {
            Content = JsonContent.Create(request)
        };
        await SendAsync<bool>(httpReq);
        return true;
    }

    public async Task<IReadOnlyList<PtSessionDto>> GetPtMyWeeklyScheduleAsync(DateOnly startDate, DateOnly endDate)
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return [];
        }

        string url = $"/api/v1/pt/sessions?ptId={Uri.EscapeDataString(currentUser.UserId)}";
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<IReadOnlyList<PtSessionDto>>(httpReq);
    }

    public async Task<bool> CompletePtSessionAsync(PtCompleteSessionRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/sessions/complete-by-pt")
        {
            Content = JsonContent.Create(request)
        };
        await SendAsync<bool>(httpReq);
        return true;
    }

    public async Task<IReadOnlyList<MemberSummaryDto>> GetPtAssignedStudentsAsync()
    {
        var currentUser = _authStateProvider.CurrentUser;
        if (currentUser == null)
        {
            return [];
        }

        string url = $"/api/v1/pt/trainers/{Uri.EscapeDataString(currentUser.UserId)}/students";
        var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<IReadOnlyList<MemberSummaryDto>>(httpReq);
    }

    public async Task<bool> RecordInBodyAsync(RecordInBodyRequest request)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/pt/inbody")
        {
            Content = JsonContent.Create(request)
        };
        await SendAsync<InBodyRecordDto>(httpReq);
        return true;
    }

    // ==========================================
    // 7. IN-APP NOTIFICATIONS (NOTIFICATION CENTER)
    // ==========================================

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync()
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/notifications");
        return await SendAsync<IReadOnlyList<NotificationDto>>(httpReq);
    }

    public async Task<int> GetUnreadNotificationCountAsync()
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/notifications/unread-count");
        return await SendAsync<int>(httpReq);
    }

    public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/notifications/{Uri.EscapeDataString(notificationId)}/read");
        await SendAsync<bool>(httpReq);
        return true;
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        var httpReq = new HttpRequestMessage(HttpMethod.Put, "/api/v1/notifications/read-all");
        await SendAsync<bool>(httpReq);
        return true;
    }

    // ==========================================
    // PRIVATE HELPER METHODS
    // ==========================================

    private async Task<T> SendAsync<T>(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_authStateProvider.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authStateProvider.Token);
        }

        var response = await _httpClient.SendAsync(request);
        return await HandleResponseAsync<T>(response);
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

        string errorMessage = "Lỗi xử lý yêu cầu từ máy chủ.";
        try
        {
            var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
            {
                errorMessage = errorResult.Message;
            }
        }
        catch (JsonException)
        {
            errorMessage = $"Máy chủ phản hồi mã lỗi {(int)response.StatusCode}: {response.ReasonPhrase}";
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(errorMessage);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new ValidationException(errorMessage);
        }

        throw new BusinessRuleException(errorMessage);
    }
}
