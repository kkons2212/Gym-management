using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Web.Services;

/// <summary>
/// Quản lý trạng thái xác thực và phân quyền người dùng trong phiên làm việc Blazor Server (UC01).
/// </summary>
public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private ClaimsPrincipal _currentUser = Anonymous;
    private UserSessionDto? _userSession;

    public UserSessionDto? CurrentUser => _userSession;
    public bool IsAuthenticated => _userSession != null;
    public bool IsMember => string.Equals(_userSession?.Role, "Member", StringComparison.OrdinalIgnoreCase);
    public bool IsPt => string.Equals(_userSession?.Role, "PT", StringComparison.OrdinalIgnoreCase);

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    /// <summary>
    /// Thiết lập phiên làm việc sau khi đăng nhập thành công.
    /// </summary>
    public void SetAuthenticated(UserSessionDto user)
    {
        ArgumentNullException.ThrowIfNull(user);

        _userSession = user;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        _currentUser = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Xóa phiên làm việc khi người dùng đăng xuất.
    /// </summary>
    public void SetAnonymous()
    {
        _userSession = null;
        _currentUser = Anonymous;

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
