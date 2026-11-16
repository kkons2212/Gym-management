using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Web.Services;

/// <summary>
/// DTO lưu trữ thông tin phiên làm việc được mã hóa trong browser local storage.
/// </summary>
public sealed record PersistedUserSession(UserSessionDto User, string? Token);

/// <summary>
/// Quản lý trạng thái xác thực và phân quyền người dùng trong phiên làm việc Blazor Server (UC01).
/// Hỗ trợ phục hồi phiên làm việc qua ProtectedSessionStorage khi người dùng làm mới (F5) trang.
/// Tự động kết thúc phiên khi đóng tab/trình duyệt để ngăn ngừa tự động đăng nhập ngoài ý muốn.
/// </summary>
public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string SessionStorageKey = "gym_user_session";
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private readonly ProtectedSessionStorage? _sessionStorage;
    private readonly ProtectedLocalStorage? _localStorage;
    private ClaimsPrincipal _currentUser = Anonymous;
    private UserSessionDto? _userSession;
    private bool _isInitialized;

    public string? Token { get; private set; }
    public UserSessionDto? CurrentUser => _userSession;
    public bool IsAuthenticated => _userSession != null;
    public bool IsMember => string.Equals(_userSession?.Role, "Member", StringComparison.OrdinalIgnoreCase);
    public bool IsPt => string.Equals(_userSession?.Role, "PT", StringComparison.OrdinalIgnoreCase);

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage? sessionStorage = null,
        ProtectedLocalStorage? localStorage = null)
    {
        _sessionStorage = sessionStorage;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_userSession != null)
        {
            return new AuthenticationState(_currentUser);
        }

        if (!_isInitialized)
        {
            _isInitialized = true;

            // 1. Dọn dẹp legacy session kẹt trong LocalStorage cũ (nếu có)
            if (_localStorage != null)
            {
                try
                {
                    await _localStorage.DeleteAsync(SessionStorageKey);
                }
                catch (Exception)
                {
                    // Bỏ qua lỗi JS interop khi prerender
                }
            }

            // 2. Phục hồi phiên làm việc của tab hiện tại qua SessionStorage (chỉ giữ khi F5)
            if (_sessionStorage != null)
            {
                try
                {
                    var result = await _sessionStorage.GetAsync<PersistedUserSession>(SessionStorageKey);
                    if (result.Success && result.Value?.User != null)
                    {
                        var user = result.Value.User;
                        _userSession = user;
                        Token = result.Value.Token;
                        _currentUser = CreateClaimsPrincipal(user);
                        return new AuthenticationState(_currentUser);
                    }
                }
                catch (Exception)
                {
                    // Fallback nếu storage trống, lỗi giải mã hoặc JS Interop chưa sẵn sàng
                }
            }
        }

        _currentUser = Anonymous;
        _userSession = null;
        Token = null;
        return new AuthenticationState(Anonymous);
    }

    /// <summary>
    /// Thiết lập phiên làm việc sau khi đăng nhập thành công và lưu vào ProtectedSessionStorage.
    /// </summary>
    public async Task SetAuthenticatedAsync(UserSessionDto user, string? token = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        _userSession = user;
        Token = token;
        _currentUser = CreateClaimsPrincipal(user);
        _isInitialized = true;

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

        if (_sessionStorage != null)
        {
            try
            {
                await _sessionStorage.SetAsync(SessionStorageKey, new PersistedUserSession(user, token));
            }
            catch (Exception)
            {
                // Bỏ qua lỗi nếu storage không khả dụng
            }
        }
    }

    /// <summary>
    /// Đồng bộ phiên làm việc (tương thích cho các hàm gọi synchronous).
    /// </summary>
    public void SetAuthenticated(UserSessionDto user, string? token = null)
    {
        _ = SetAuthenticatedAsync(user, token);
    }

    /// <summary>
    /// Xóa phiên làm việc khi người dùng đăng xuất và dọn dẹp storage.
    /// </summary>
    public async Task SetAnonymousAsync()
    {
        _userSession = null;
        Token = null;
        _currentUser = Anonymous;
        _isInitialized = true;

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

        if (_sessionStorage != null)
        {
            try
            {
                await _sessionStorage.DeleteAsync(SessionStorageKey);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi nếu storage không khả dụng
            }
        }

        if (_localStorage != null)
        {
            try
            {
                await _localStorage.DeleteAsync(SessionStorageKey);
            }
            catch (Exception)
            {
                // Bỏ qua lỗi nếu storage không khả dụng
            }
        }
    }

    /// <summary>
    /// Xóa phiên làm việc đồng bộ.
    /// </summary>
    public void SetAnonymous()
    {
        _ = SetAnonymousAsync();
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(UserSessionDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        return new ClaimsPrincipal(identity);
    }
}

