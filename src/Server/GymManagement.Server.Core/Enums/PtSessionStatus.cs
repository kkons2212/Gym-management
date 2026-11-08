namespace GymManagement.Server.Core.Enums;

/// <summary>
/// Trạng thái ca tập PT — ánh xạ với cột Status trong bảng PTTrainingSession.
/// </summary>
public enum PtSessionStatus
{
    /// <summary>Đã đặt lịch, chờ diễn ra.</summary>
    Booked,

    /// <summary>Đã hoàn thành — PT xác nhận sau khi ca tập kết thúc.</summary>
    Completed,

    /// <summary>Đã hủy bởi hội viên hoặc Admin.</summary>
    Cancelled,

    /// <summary>Hội viên vắng mặt không báo trước.</summary>
    AbsentMember,

    /// <summary>PT vắng mặt — hội viên được hoàn lại buổi tập.</summary>
    AbsentPt,

    /// <summary>Hoãn — hội viên được giữ lại buổi tập để đặt lại.</summary>
    Postponed
}
