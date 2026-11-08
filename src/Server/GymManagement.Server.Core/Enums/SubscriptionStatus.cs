namespace GymManagement.Server.Core.Enums;

/// <summary>
/// Trạng thái gói tập hội viên — ánh xạ với cột Status trong bảng MemberSubscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Đang hoạt động — hội viên có thể check-in và sử dụng dịch vụ.</summary>
    Active,

    /// <summary>Đang bảo lưu — hội viên không thể check-in, ngày hết hạn được dời lại.</summary>
    Frozen,

    /// <summary>Đã hết hạn — qua ngày EndDate mà chưa gia hạn.</summary>
    Expired,

    /// <summary>Đã hủy bởi Admin — giữ lịch sử, không cho phép check-in.</summary>
    Cancelled
}
