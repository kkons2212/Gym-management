using GymManagement.Server.Core.DTOs.Identity;

namespace GymManagement.Desktop.Services.Session;

public interface IAdminSessionService
{
    bool IsAuthenticated { get; }
    string? Token { get; }
    UserSessionDto? CurrentUser { get; }
    event Action? OnSessionExpired;

    void SetSession(string token, UserSessionDto user);
    void ClearSession();
    void NotifySessionExpired();
}

public class AdminSessionService : IAdminSessionService
{
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && CurrentUser != null;
    public string? Token { get; private set; }
    public UserSessionDto? CurrentUser { get; private set; }
    public event Action? OnSessionExpired;

    public void SetSession(string token, UserSessionDto user)
    {
        Token = token;
        CurrentUser = user;
    }

    public void ClearSession()
    {
        Token = null;
        CurrentUser = null;
    }

    public void NotifySessionExpired()
    {
        ClearSession();
        OnSessionExpired?.Invoke();
    }
}

