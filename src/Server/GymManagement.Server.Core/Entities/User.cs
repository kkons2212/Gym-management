namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng [User] trong CSDL.
/// </summary>
public sealed class User
{
    public string UserName { get; set; } = string.Empty;

    public string RoleId { get; set; } = string.Empty;

    public string RoleID
    {
        get
        {
            return RoleId;
        }
        set
        {
            RoleId = value;
        }
    }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public byte? Gender { get; set; }

    public string? Status { get; set; }

    public DateOnly? CreatedAt { get; set; }

    public DateOnly? created_at
    {
        get
        {
            return CreatedAt;
        }
        set
        {
            CreatedAt = value;
        }
    }
}

