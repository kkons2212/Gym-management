namespace GymManagement.Core.Enums;

public enum UserRole
{
    Admin,
    Receptionist,
    Cashier,
    Trainer,
    Member
}

public enum MemberStatus
{
    Active,
    Frozen,
    Expired,
    Locked
}

public enum CardStatus
{
    Active,
    Frozen,
    Expired,
    Cancelled,
    Locked
}

public enum BookingStatus
{
    Confirmed,
    Completed,
    Cancelled,
    NoShow
}

/// <summary>
/// Trạng thái khung giờ trong lịch rảnh của PT (UC 8.1.1).
/// </summary>
public enum PtSlotStatus
{
    Free,        // Trống: PT chưa đăng ký rảnh
    Registered,  // Đã đăng ký rảnh, hội viên có thể đặt
    Booked       // Đã có hội viên đặt lịch, không thể hủy
}

public enum PaymentMethod
{
    Cash,
    VietQR,
    CardPOS
}

public enum InvoiceStatus
{
    Pending,
    Paid,
    Refunded,
    Cancelled
}
