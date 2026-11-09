using System.Net;
using System.Net.Http.Headers;
using GymManagement.Desktop.Services.Session;

namespace GymManagement.Desktop.Services.Api;

/// <summary>
/// DelegatingHandler tự động đính kèm JWT Bearer Token vào header request và lắng nghe phản hồi 401 Unauthorized.
/// </summary>
public sealed class JwtAuthenticationHeaderHandler : DelegatingHandler
{
    private readonly IAdminSessionService _adminSessionService;

    public JwtAuthenticationHeaderHandler(IAdminSessionService adminSessionService)
    {
        ArgumentNullException.ThrowIfNull(adminSessionService);
        _adminSessionService = adminSessionService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? token = _adminSessionService.Token;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _adminSessionService.NotifySessionExpired();
        }

        return response;
    }
}

