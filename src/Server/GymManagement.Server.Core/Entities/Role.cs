namespace GymManagement.Server.Core.Entities;

/// <summary>
/// Đại diện cho bảng Role trong CSDL.
/// </summary>
public sealed class Role
{
    public string RoleID { get; set; } = string.Empty;

    public string RoleId
    {
        get
        {
            return RoleID;
        }
        set
        {
            RoleID = value;
        }
    }

    public string? RoleName { get; set; }
}

